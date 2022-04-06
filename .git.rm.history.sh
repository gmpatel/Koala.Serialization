# Checkout

git checkout --orphan latest_branch

# Add all the files

git add -A

# Commit the changes

git commit -am "adding repository to git!"

# Delete the branch

git branch -D main

# Rename the current branch to master

git branch -m main

# Finally, force update your repository

git push -f origin main