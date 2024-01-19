/// <binding BeforeBuild='beforeBuildJS, beforeBuildSCSS' ProjectOpened='watch' />
'use strict';

const
    gulp = require('gulp'),
    concat = require('gulp-concat'),
    csso = require('gulp-csso'),
    sass = require('gulp-sass')(require('sass')),
    htmlmin = require('gulp-htmlmin'),
    filter = require('gulp-filter'),
    flatmap = require('gulp-flatmap'),
    terser = require('gulp-terser'),
    sourcemaps = require('gulp-sourcemaps'),
    merge = require('merge-stream'),
    babel = require('gulp-babel'),
    del = require('del'),
    debug = require('gulp-debug'),
    postcss = require('gulp-postcss'),
    rename = require('gulp-rename'),
    flexGapPolyfill = require('flex-gap-polyfill'),
    autoprefixer = require('autoprefixer'),
    gapProperties = require('postcss-gap-properties'),
    bundleconfig = require('./bundleconfig.json'),
    rollup = require('rollup'),
    rollupNodeResolve = require('@rollup/plugin-node-resolve'),
    gulprollup = require('gulp-best-rollup-2');

const regex = {
    css: /(?<!s)\.css$/,
    scss: /\.(scss)$/,
    html: /\.(html|htm)$/,
    rollup: /\.rollup\.js$/,
    js: /(?<!rollup)\.js$/,
};

function minjs() {
    var tasks = getBundles(regex.js).map(function (bundle) {
        if (bundle.sourceMap === false) {
            if (bundle.skipBabel) {
                return gulp.src(bundle.inputFiles, { since: gulp.lastRun(minjs), base: '.' })
                    .pipe(concat(bundle.outputFileName))
                    .pipe(terser())
                    .pipe(gulp.dest('.'));
            } else {
                return gulp.src(bundle.inputFiles, { since: gulp.lastRun(minjs), base: '.' })
                    .pipe(concat(bundle.outputFileName))
                    .pipe(babel({
                        presets: ['@babel/preset-env']
                    }))
                    .pipe(terser())
                    .pipe(gulp.dest('.'));
            }
        } else {
            if (bundle.skipBabel) {
                return gulp.src(bundle.inputFiles, { since: gulp.lastRun(minjs), base: '.' })
                    .pipe(concat(bundle.outputFileName))
                    .pipe(sourcemaps.init())
                    .pipe(terser())
                    .pipe(sourcemaps.write('./'))
                    .pipe(gulp.dest('.'));
            } else {
                return gulp.src(bundle.inputFiles, { since: gulp.lastRun(minjs), base: '.' })
                    .pipe(concat(bundle.outputFileName))
                    .pipe(sourcemaps.init())
                    .pipe(babel({
                        presets: ['@babel/preset-env']
                    }))
                    .pipe(terser())
                    .pipe(sourcemaps.write('./'))
                    .pipe(gulp.dest('.'));
            }
        }
    });
    return merge(tasks);
}

function mincss() {
    var tasks = getBundlesByInputFileNames(regex.css).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { since: gulp.lastRun(mincss), base: '.' })
            .pipe(sourcemaps.init())
            .pipe(csso({
                sourceMap: true
            }))
            .pipe(concat(bundle.outputFileName))
            .pipe(sourcemaps.write('.'))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
}

function mincssSingleFile(bundle) {
    //console.log(bundle);
    return bundle.inputFiles.flatMap(f => {
        return gulp.src(f, { since: gulp.lastRun(mincssTaskArray), base: '.' })
            .pipe(sourcemaps.init())
            .pipe(csso({
                sourceMap: true
            }))
            .pipe(concat(bundle.outputFileName))
            .pipe(sourcemaps.write('.'))
            .pipe(gulp.dest('.'));
    });
}

function mincssTaskArray(cb) {
    let result = getBundlesByInputFileNames(regex.css).map(function (bundle) {
        return mincssSingleFile(bundle);
    });

    //console.trace(result);

    cb();
}

function compilesass() {
    var tasks = getBundlesByInputFileNames(regex.sass).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { since: gulp.lastRun(compilesass), base: '.' })
            .pipe(sourcemaps.init())
            .pipe(sass({
                includePaths: ['node_modules']
            }).on('error', sass.logError))
            .pipe(postcss([gapProperties(), autoprefixer()]))
            .pipe(concat(bundle.inputFiles.length > 1 ? bundle.outputFileName : bundle.inputFiles[0].replace('.scss', '.css')))
            .pipe(sourcemaps.write('.'))
            .pipe(gulp.dest('.'));
    });
    return merge(tasks);
}

function wildcardscss() {
    var tasks = gulp
        .src('css/src/**/*.scss', { since: gulp.lastRun(wildcardscss) })
        .pipe(debug({ title: 'wildcardscss:' }))
        .pipe(sourcemaps.mapSources(function (sourcePath, file) {
            return '/css/dist/' + sourcePath
        }))
        .pipe(sourcemaps.init())
        .pipe(sass({
            includePaths: ['node_modules']
        }).on('error', sass.logError))
        .pipe(sourcemaps.write(''))
        .pipe(gulp.dest('css/dist'))
        .pipe(filter('**/*.css'))
        .pipe(csso({
            sourceMap: true
        }))
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write(''))
        .pipe(gulp.dest('css/dist'));

    return merge(tasks);
}

function handleBabelError(error) {
    console.error(error.toString());
    this.emit('end');
}

function wildcardjs() {
    var tasks = gulp
        .src('js/src/**/!(*.min).js', { since: gulp.lastRun(wildcardjs) })
        .pipe(debug({ title: 'wildcardjs:' }))
        .pipe(sourcemaps.init())
        .pipe(flatmap((stream, file) => {
            console.log(file.path);
            if (file.path.indexOf('models') > -1) {
                console.log('running babel');
                return stream.pipe(babel({
                    presets: ['@babel/preset-env']
                }))
            } else {
                return stream;
            }
        }))
        .on('error', handleBabelError)
        .pipe(terser())
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write(''))
        .pipe(gulp.dest('js/dist'));

    return merge(tasks);
}

function wildcardfonts() {
    var tasks = gulp
        .src('css/src/**/*.{woff,woff2,ttf}', { since: gulp.lastRun(wildcardfonts) })
        .pipe(debug({ title: 'wildcardfonts:' }))
        .pipe(gulp.dest('css/dist'));

    return merge(tasks);
}

function compilerollup() {
    var tasks = getBundlesByInputFileNames(regex.rollup).map((bundle) => {
        return gulp.src(bundle.inputFiles, { since: gulp.lastRun(gulprollup), base: '.' })
            .pipe(debug({ title: 'compilerollup:' }))
            .pipe(gulprollup({
                rollup: rollup,
                plugins: [rollupNodeResolve()]
            }, 'umd'))
            .pipe(concat(bundle.outputFileName))
            .pipe(gulp.dest('.'))
    });
    return merge(tasks);
}

gulp.task('clean', function () {
    var files = bundleconfig.map(function (bundle) {
        return bundle.outputFileName;
    });
    return del(files);
});

gulp.task('css', gulp.series([mincss]));

gulp.task('sass', gulp.series([compilesass]));

gulp.task('js', gulp.series([minjs]));

gulp.task('fonts', gulp.series([wildcardfonts]));

gulp.task('all', gulp.series(['sass', 'css', 'js']));

gulp.task('rollup', gulp.series([compilerollup]));

gulp.task('beforeBuildSCSS', gulp.series([wildcardscss]));

gulp.task('beforeBuildJS', gulp.series([wildcardjs]));

gulp.task('watch', function () {
    getBundles(regex.js).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, minjs);
    });

    getBundlesByInputFileNames(regex.css).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, mincssTaskArray);
    });

    gulp.watch('css/**/*.scss', gulp.series([wildcardscss]));

    gulp.watch('js/**/!(*.min).js', gulp.series([wildcardjs]))

    getBundlesByInputFileNames(regex.rollup).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, compilerollup);
    });
});

function getBundles(regexPattern) {
    return bundleconfig.filter(function (bundle) {
        return regexPattern.test(bundle.outputFileName);
    });
}

function getBundlesByInputFileNames(regexPattern) {
    return bundleconfig.filter(function (bundle) {
        return regexPattern.test(bundle.inputFiles);
    });
}