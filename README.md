<h4>
    2023-03-17 Designers Guild: main www site
</h4>
<section>
    <p>
        Finally migrated this to Git after 10 years using Team Foundation Version Control.
    </p>
    <p>
        As per the TFVC paradigm, previously there were three separate branches each the entirety of the codebase.
        As it feels most natural for this project, I have decided to go with the 'environment branches' approach.
    </p>
</section>
<section>
    <ul>
        <li>
            master was created from the last commit to Production in the previous version control.
        </li>
        <li>
            staging was created from master.
        </li>
        <li>
            development was created from staging.
        </li>
        <li>
            features and bugfixes are created from development, worked, then committed.
        </li>
        <li>
            completed features and bugfixes are merged into development via pull request.
        </li>
        <li>
            once successfully merged into development, feature_# (bugfix_#) are deleted.
        </li>
        <li>
            development is then pull-requested into staging.
        </li>
        <li>
            staging is then pull-requested into master.
        </li>
    </ul>
</section>
