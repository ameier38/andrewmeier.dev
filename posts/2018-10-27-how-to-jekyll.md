---
layout: post
title: How to Jekyll
cover: /assets/images/how-to-jekyll/cover.png
permalink: how-to-jekyll
date: 2018-10-27 11:37:39 -0400
updated: 2019-05-25 11:00:00 -0400
categories: 
  - jekyll
  - windows
  - blog
  - markdown
comments: true
---

This post will walk through the steps necessary to build
a blog using GitHub Pages and Jekyll on Windows.

## Environment set up
Checkout the [Windows Development Environment post](https://andrewmeier.dev/win-dev)
for a guide on setting up a Windows development environment. The section below covers
only the items necessary for this tutorial.

Install [Scoop](https://scoop.sh). See my [win-dev post](https://andrewmeier.dev/win-dev#scoop)
for quick instructions.

Add extras and versions buckets.
```powershell
scoop bucket add extras
scoop bucket add versions
```

Install [Git](https://git-scm.com/). Git is a version-control system 
for tracking changes in computer files.
```powershell
scoop install git
```

Install [Ruby](https://www.ruby-lang.org/en/). Ruby is a dynamic, 
open source programming language with a focus on simplicity and productivity.
```powershell
scoop install ruby
```

Install [MSYS2](https://www.msys2.org/). MSYS2 is a software distro 
and building platform for Windows.
```powershell
scoop install msys2
msys2
```
> A console will open. Once it finishes running
commands just close the console.

Install the MSYS2 and MINGW development toolchain.
```powershell
ridk install 3
```

Install [Jekyll](https://jekyllrb.com/)
```powershell
gem install bundler jekyll
```

## Create your site repository

Create a new repository for your site.
```powershell
jekyll new my-site
```
> This bootstraps a new Jekyll site in the directory `my-site`.

Initialize Git. 
```powershell
cd my-site
git init
```

Create a new empty repository on GitHub. Navigate to 
`github.com/{username}?tab=repositories` and click the 'New' button.

![create-new-repository](create-new-repository.png)

Copy the repository link.

![clone-repository](clone-repository.png)

Add the repository link as a remote.
```powershell
git remote add origin {copied repo url}
```

Add files to track.
```powershell
git add .
```

Commit changes.
```powershell
git commit -m 'initial commit'
```

Push to GitHub.
```powershell
git push origin master
```

## Host your site on GitHub pages

[GitHub Pages](https://pages.github.com/) allows you to host
your web site for free from a GitHub repository.

Update the `Gemfile` to enable GitHub pages. You need to comment out `gem "jekyll"`
and then uncomment the `gem "github-pages"`. The result should look like below.
```ruby
# This will help ensure the proper Jekyll version is running.
# Happy Jekylling!
# gem "jekyll", "~> 3.8.4"

# This is the default theme for new Jekyll sites. You may change this to anything you like.
gem "minima", "~> 2.0"

# If you want to use GitHub Pages, remove the "gem "jekyll"" above and
# uncomment the line below. To upgrade, run `bundle update github-pages`.
gem "github-pages", group: :jekyll_plugins
```

Push your changes to GitHub.
```powershell
git add Gemfile
git commit -m "update Gemfile"
git push origin master
```

Go to the Settings tab for your repository.

![github-select-page-rule](github-select-settings.png)

Scroll down to the GitHub Pages section and set the source to the `master` branch.

![github-pages](github-pages.png)

Navigate to `{your username}.github.io/{your repository}` and verify your site is working.

## Using a custom domain name

Purchase a domain name. I recommend [Namecheap](https://namecheap.com) but
you can use plenty of other domain name registrars. I bought `andrewcmeier.com` 
for ~$30 and it renews at ~$20 a year.

Create a free [Cloudflare](https://cloudflare.com) account. Cloudflare is used
as the DNS provider so you can route traffic to your site securely using SSL.

Add a site to your Cloudflare account.

![cloudflare-add-site](cloudflare-add-site.png)

Get the Cloudflare nameservers for your site.

![cloudflare-nameservers](cloudflare-nameservers.png)

Add the Cloudflare nameservers to Namecheap (or whatever registrar you use).

![namecheap-custom-dns](namecheap-custom-dns.png)

Add CNAME records to Cloudflare. You will add one apex record
(`andrewcmeier.com` in my case) and another `wwww` record (`www.andrewcmeier.com` in my case).
Both of the values will be `{username}.github.io`. So for my site, the value is
`ameier38.github.io`.

![cloudflare-cname](cloudflare-cname.png)

Add a CNAME file in your repository. This is used to register the domain
name with GitHub pages and point to the correct repository.
```powershell
cd my-site
echo "{your domain}.com" > CNAME
git add CNAME
git commit -m 'added CNAME'
```

Open the Page Rules dashboard and select 'Create Page Rule'.

![cloudflare-select-page-rule](cloudflare-select-page-rule.png)

Add Page Rule to always use HTTPS.

![cloudflare-page-rule-https](cloudflare-page-rule-https.png)

Add Page Rule to forward to the apex.

![cloudflare-page-rule-forward-to-apex](cloudflare-page-rule-forward-to-apex.png)

Now your site is set up to use your custom domain name and you will [get a better SEO
ranking with Google](https://webmasters.googleblog.com/2014/08/https-as-ranking-signal.html) 
since you are using HTTPS :tada:.

## Create a post

To create a post, we need to add a new markdown file into the `_posts` directory.
Each post is a standard Markdown file which uses the 
[kramdown syntax](https://kramdown.gettalong.org/syntax.html). Each post should
have a 'Front Matter' section at the top which contains the Jekyll options
formatted in YAML. Below is the 'Front Matter' for this post.
```yaml
---
layout: post
title:  How to Jekyll
permalink: how-to-jekyll
date:   2018-10-27 11:37:39 -0400
categories: 
  - jekyll
  - windows
  - blog
  - markdown
comments: true
---
```
> Learn more about 'Front Matter' on the [Jekyll website](https://jekyllrb.com/docs/front-matter/).

> Check out [this tutorial](https://poanchen.github.io/blog/2017/07/27/how-to-add-disqus-to-your-jekyll-site)
to find out more about adding [Disqus](https://disqus.com/) to your blog.

Then the rest of the file is your blog post content.

## Check your site

Before pushing your changes to GitHub, you should check your site locally. Run
the Jekyll site locally using the following command from the root of your repository.
```powershell
bundle exec jekyll serve
```

Then navigate to your site at `http://localhost:4000`.

## Deploy your site

Once the site looks good, then we can deploy our site by pushing
the latest changes to GitHub.
```powershell
git push origin master 
```

Then navigate to your custom domain name and check that it was
deployed successfully.

## Summary
In this post we covered:
- [Setting up our Jekyll environment on Windows](#environment-set-up)
- [Creating our site repository](#create-your-site-repository)
- [Hosting the site on GitHub pages](#host-your-site-on-github-pages)
- [Using a custom domain name with Cloudflare](#using-a-custom-domain-name)
- [Creating a post](#create-a-post)
- [Running a local server](#check-your-site)
- [Deploy site to GitHub](#deploy-your-site)

All the code for this site is located on [my GitHub repo](https://github.com/ameier38/andrewcmeier.com)
if you want to see more details. Let me know if you have any questions or find any issues in
the comments below. Thanks! :smile:

## Additional Resources
- [Jekyll Homepage](https://jekyllrb.com/)
- [Setting up GitHub Pages with Jekyll](http://www.stephaniehicks.com/githubPages_tutorial/pages/githubpages-jekyll.html)
- [Secure and fast GitHub Pages with CloudFlare](https://blog.cloudflare.com/secure-and-fast-github-pages-with-cloudflare/)
