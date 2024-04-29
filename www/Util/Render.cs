using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web;
using System.Web.Optimization;

using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Util
{
    public static class Render
    {
        private static readonly char[] newline = { '\r', '\n' };

        public static IHtmlString Styles(string path)
        {
            return new HtmlString(System.Web.Optimization.Styles.RenderFormat(@"<link href=""{0}"" rel=""stylesheet"" />", path).ToString().TrimEnd(newline));
        }

        public static IHtmlString StylesPreload(string path)
        {
            return new HtmlString(System.Web.Optimization.Styles.RenderFormat(@"<link href=""{0}"" rel=""preload stylesheet"" as=""style"" onload=""this.rel='stylesheet'"" />", path).ToString().TrimEnd(newline));

            // This is necessary to support iOS 10.3 and under, but... need to be able to feature detect a lack of support for preload before knownig
            // we can go ahead and skip it
            //return new HtmlString(System.Web.Optimization.Styles.RenderFormat(@"<link href=""{0}"" rel=""stylesheet"" />", path).ToString().TrimEnd(newline));
        }

        public static IHtmlString ScriptAsync(string path)
        {
#if DEBUG
            var files = new BundleResolver().GetBundleContents(path);

            var bundleBytes = new List<byte>();

            string b64encodedHash = null;

            using (var sha512 = SHA512.Create())
            {
                foreach (var f in files)
                {
                    bundleBytes.AddRange(System.IO.File.ReadAllBytes(HttpContext.Current.Server.MapPath(f)));
                }

                var hashedBytes = sha512.ComputeHash(bundleBytes.ToArray());

                b64encodedHash = Convert.ToBase64String(hashedBytes);
            }

            return new HtmlString(Scripts.RenderFormat($"<script src='{{0}}' integrity='sha512-{b64encodedHash}' async></script>", path).ToString().TrimEnd(newline));

#else
            var bundleContext = new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, path);

            var bundle = BundleTable.Bundles.GetBundleFor(path);

            var bundleResponse = bundle.GenerateBundleResponse(bundleContext);

            var bundleBytes = System.Text.Encoding.UTF8.GetBytes(bundleResponse.Content);

            var b64encodedHash = (string)null;

            using (var sha512 = SHA512.Create())
            {
                var hashedBytes = sha512.ComputeHash(bundleBytes);

                b64encodedHash = Convert.ToBase64String(hashedBytes);
            }

            return new HtmlString(Scripts.RenderFormat($"<script src='{{0}}' integrity='sha512-{b64encodedHash}' async></script>", path).ToString().TrimEnd(newline));
#endif
        }

        public static IHtmlString Script(string path, bool skipDefer = false)
        {
            if (skipDefer)
            {
                return new HtmlString(Scripts.RenderFormat($"<script src='{{0}}'></script>", path).ToString().TrimEnd(newline));
            }
            else
            {
                if (dgFramework.IsMobileDevice())
                {
                    return new HtmlString(Scripts.RenderFormat($"<script src='{{0}}' defer></script>", path).ToString().TrimEnd(newline));
                }
                else
                {
                    return new HtmlString(Scripts.RenderFormat($"<script src='{{0}}'></script>", path).ToString().TrimEnd(newline));
                }
            }
        }

        public static IHtmlString ScriptDefer(string path)
        {
            // Not entirely sure this is worth all the bother - it will add yae much overhead to
            // every single page view...

            return new HtmlString(Scripts.RenderFormat($"<script src='{{0}}' defer></script>", path).ToString().TrimEnd(newline));

#if DEBUG

            //var result = "";

            //var files = new BundleResolver().GetBundleContents(path);

            //string b64encodedHash = null;

            //using (var sha512 = SHA512.Create())
            //{
            //    foreach (var f in files)
            //    {
            //        var filePath = HttpContext.Current.Server.MapPath(f);

            //        var hashedBytes = sha512.ComputeHash(System.IO.File.ReadAllBytes(filePath));

            //        b64encodedHash = Convert.ToBase64String(hashedBytes);

            //        result += $"<script src='{f.TrimStart(tilde)}' integrity='sha512-{b64encodedHash}' defer></script>{Environment.NewLine}";
            //    }
            //}

            //return new HtmlString(result.TrimEnd(newline));

#else
            var bundleContext = new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, path);

            var bundle = BundleTable.Bundles.GetBundleFor(path);

            var bundleResponse = bundle.GenerateBundleResponse(bundleContext);

            var bundleBytes = System.Text.Encoding.UTF8.GetBytes(bundleResponse.Content);

            var b64encodedHash = (string)null;

            using (var sha512 = SHA512.Create())
            {
                var hashedBytes = sha512.ComputeHash(bundleBytes);

                b64encodedHash = Convert.ToBase64String(hashedBytes);
            }

            return new HtmlString(Scripts.RenderFormat($"<script src='{{0}}' integrity='sha512-{b64encodedHash}' defer></script>", path).ToString().TrimEnd(newline));
#endif
        }

        public static IHtmlString Image(
            string src,
            IHtmlString title,
            IHtmlString alt,
            short defaultWidth,
            short defaultHeight,
            string id = null,
            string classes = null,
            string[] srcset = null,
            string[] sizes = null)
        {
            return new HtmlString(
                $@"<img {(String.IsNullOrEmpty(id) ? "" : $@"id=""{id}""")} " +
                    $@"{(String.IsNullOrEmpty(classes) ? "" : $@"class=""{classes}""")} " +
                    $@"width=""{defaultWidth}"" " +
                    $@"height=""{defaultHeight}"" " +
                    $@"{(String.IsNullOrEmpty(title?.ToString()) ? "" : $@"title=""{title}""")} " +
                    $@"{(String.IsNullOrEmpty(alt?.ToString()) ? "" : $@"alt=""{alt}""")} " +
                    $@"src=""{src}"" " +
                    $@"{(srcset == null ? "" : $@"srcset=""{(String.Join($",{Environment.NewLine}", srcset))}""")} " +
                    $@"{(sizes == null ? "" : $@"sizes=""{(String.Join($",{Environment.NewLine}", sizes))}""")} " +
                    $@" />");
        }

        public static IHtmlString LinkingImage(
            string href,
            string target,
            IHtmlString anchorTitle,
            IHtmlString anchorAlt,
            string imgSrc,
            IHtmlString imgTitle,
            IHtmlString imgAlt,
            short defaultWidth,
            short defaultHeight,
            string anchorId = null,
            string anchorClasses = null,
            string rel = null,
            string imgId = null,
            string imgClasses = null,
            string[] srcset = null,
            string[] sizes = null)
        {
            return new HtmlString(
                $@"<a {(String.IsNullOrEmpty(anchorId) ? "" : $@"id=""{anchorId}""")} " +
                    $@"{(String.IsNullOrEmpty(anchorClasses) ? "" : $@"class=""{anchorClasses}""")} " +
                    $@"href=""{href}"" " +
                    $@"{(String.IsNullOrEmpty(anchorTitle?.ToString()) ? "" : $@"title=""{anchorTitle}""")} " +
                    $@"{(String.IsNullOrEmpty(anchorAlt?.ToString()) ? "" : $@"alt=""{anchorAlt}""")} " +
                    $@"{(String.IsNullOrEmpty(rel?.ToString()) ? "" : $@"rel=""{rel}""")} " +
                    $@"target=""{target}"">" + 
                    $"\r\n\t" +
                    $@"<img {(String.IsNullOrEmpty(imgId) ? "" : $@"id=""{imgId}""")} " +
                        $@"{(String.IsNullOrEmpty(imgClasses) ? "" : $@"class=""{imgClasses}""")} " +
                        $@"width=""{defaultWidth}"" " +
                        $@"height=""{defaultHeight}"" " +
                        $@"{(String.IsNullOrEmpty(imgTitle?.ToString()) ? "" : $@"title=""{imgTitle}""")} " +
                        $@"{(String.IsNullOrEmpty(imgAlt?.ToString()) ? "" : $@"alt=""{imgAlt}""")} " +
                        $@"src=""{imgSrc}"" " +
                        $@"{(srcset == null ? "" : $@"srcset=""{(String.Join($",{Environment.NewLine}", srcset))}""")} " +
                        $@"{(sizes == null ? "" : $@"sizes=""{(String.Join($",{Environment.NewLine}", sizes))}""")} />" +
                    $"\r\n" +
                $@"</a>");
        }
    }
}