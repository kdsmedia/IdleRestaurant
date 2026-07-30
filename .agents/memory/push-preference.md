---
name: Push-on-every-change preference
description: User requires a git commit and push to GitHub origin after every edit made in this project.
---

# Push-on-every-change preference

Every time any file is edited or changed, immediately commit and push to the GitHub remote (`origin/main`) using `gitPush({})`.

**Why:** User explicitly stated this requirement when setting up the project.

**How to apply:** After any `WriteFile` or `Edit` call, stage all changes with `git add -A`, commit with a descriptive message, then call `gitPush({})`.
