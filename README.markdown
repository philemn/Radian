# Radian: A file-based content management framework for .NET

I've seen several people attempt to build their own CMS because they're looking for something simple that functions the way they need it to.  Most developers immediately go to their favorite database and start building a schema around the content and page structures that they plan to deal with.  Radian takes care of that aspect and leaves with with a solid platform to build your dynamic site on top of.  **All this is done without a database!**

## Why no database?

There are two primary reasons why I chose to build Radian as a file-based framework.

- **Versioning** - There's already an awesome tool out there for versioning files (git) which works beautifully with content files on disk.
- **Deployment** - Deploying updates is as simple as moving files: 1) git push, 2) xcopy, 3) FTP, etc.