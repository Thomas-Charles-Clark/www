# 2023-03-17 Designers Guild: main www site
# 
# Finally migrated this to Git after 10 years using Team Foundation Version Control.
# As per the TFVC paradigm, previously there were three separate branches each the entirety of the codebase.
# As it feels most natural for this project, I have decided to go with the 'environment branches' approach.
#
# master was created from the last commit to Production in the previous version control.
# staging was created from master.
# development was created from staging.
# Features and bugfixes are created from development, worked, then committed.
# Completed features and bugfixes are merged into development via pull request.
# Once successfully merged into development, feature_# (bugfix_#) are deleted.
# development is then pull-requested into staging.
# staging is then pull-requested into master.
# Repeat.
#
