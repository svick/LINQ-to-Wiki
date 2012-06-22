LINQ to Wiki
============

LINQ to Wiki is a library for accessing sites running [MediaWiki](http://www.mediawiki.org/)
(including [Wikipedia](http://en.wikipedia.org/)) through the [MediaWiki API](https://www.mediawiki.org/wiki/API)
from .Net languages like C# and VB.NET.

It can be used to do almost anything that can be done from the web interface and more,
including things like editing articles, listing articles in categories, listing all kinds of links on a page
and much more.
Querying the various lists available can be done using LINQ queries,
which then get translated into efficient API reuqests.

Because the API can vary from wiki to wiki,
[it's necessary to configure the library thorough an automatically generated assembly](#generating-configuration-for-a-wiki).

- [Usage](#usage)
- [Generating configuration for a wiki](#generating-configuration-for-a-wiki)
- [Developer documentation](#developer-documentation)


Usage
-----

### Simple example

For example, to edit the [Sandbox](http://en.wikipedia.org/wiki/Wikipedia:Sandbox) on the English Wikipedia anonymously, you can use the following:

```C#
var wiki = new Wiki("en.wikipedia.org");

// get edit token, necessary to edit pages
var token = wiki.tokens(new[] { tokenstype.edit }).edittoken;

// create new section called "Hello" on the page "Wikipedia:Sandbox"
wiki.edit(
    token: token, title: "Wikipedia:Sandbox", section: "new", sectiontitle: "Hello", text: "Hello world!");
```

As you can see, in methods like this, you should use named parameters,
because the `edit()` method has lots of them, and you probably don't need them all.

The code looks more convoluted than necessary (can't the library get the token for me?),
but that's because it's all generated automatically.

### Queries

Where LINQ to Wiki really shines, though, are queries:
If you wanted to get the names of all pages in [Category:Mammals of Indonesia](http://en.wikipedia.org/wiki/Category:Mammals_of_Indonesia),
you can do:

```C#
var pages = (from cm in wiki.Query.categorymembers()
             where cm.title == "Category:Mammals of Indonesia"
             select cm.title)
            .ToEnumerable();
```

```
List of mammals of Indonesia
Mammals of Borneo
Agile gibbon
Andrew's Hill Rat
Anoa
…
```

The call to `ToEnumerable()` (or, alternatively, `ToList()`) is necessary,
so that LINQ to Wiki methods don't get mixed up with LINQ to Objects methods, but the result is now an ordinary `IEnumerable<string>`.

Well, actually, you want the list sorted backwards (maybe you want to know whether there are any Indonesiam mammals whose name starts with *Z*):

```C#
var pages = (from cm in wiki.Query.categorymembers()
              where cm.title == "Category:Mammals of Indonesia"
              orderby cm descending 
              select cm.title)
    .ToEnumerable();
```

```
Wild water buffalo
Wild boar
Whitish Dwarf Squirrel
Whitehead's Woolly Bat
White-thighed surili
…
```

Hmm, no luck with the *Z*. Okay, can I get the first section of those articles?
This is where things start to get more comlicated. If you were using the API directly,
you would have to use [generators](http://www.mediawiki.org/wiki/API:Query#Generators).
LINQ to Wiki can handle that for you, but since generators are quite powerful,
you have to do something like this:

```C#
var pages = (from cm in wiki.Query.categorymembers()
             where cm.title == "Category:Mammals of Indonesia"
             orderby cm descending
             select cm)
    .Pages
    .Select(
        page =>
        new
        {
            title = page.info.title,
            text = page.revisions()
                .Where(r => r.section == "0")
                .Select(r => r.value)
                .FirstOrDefault()
        })
    .ToEnumerable();
```

```
Wild water buffalo
{{About|the wild species|the domestic livestock varieties descended from it|water buffalo}}

{{Taxobox|…}}

The '''wild water buffalo''' (''Bubalus arnee''), also called '''Asian buffalo''' and '''Asiatic buffalo''',
is a large [[bovinae|bovine]] native to [[Southeast Asia]].  …
```

This deserves some explanation. When you use `Pages` to access more information about the pages in some list,
you then call `Select()` to choose what exactly do you want to know.
In that `Select()`, you can use `info` for basic information about the page, like its name, ID or whether you are watching it.
Then there are several lists, including `revisions()`.
You can again use LINQ methods to alter this part of the query.
For example,
I want only the first section (`Where(r => r.section == "0")`),
I want to select the text of the revision (here called “value”, `Select(r => r.value)`)
and only for the first (latest) revision (`FirstOrDefault()`).

For examples of almost all methods in LINQ to Wiki,
have a look at [the LinqToWiki.Samples project](https://github.com/svick/LINQ-to-Wiki/blob/master/LinqToWiki.Samples/Program.cs).

Generating configuration for a wiki
-----------------------------------

To generate a configuration assembly for a certain wiki, you can use the `linqtowiki-codegen` command-line application
(see [the LinqToWiki.Codegen.App project](https://github.com/svick/LINQ-to-Wiki/tree/master/LinqToWiki.Codegen.App)).
If you run it without parameters, it will show you basic usage, along with some examples:

```
Usage:    linqtowiki-codegen url-to-api [namespace [output-name]] [-d output-directory] [-p props-file-path]
Examples: linqtowiki-codegen en.wikipedia.org LinqToWiki.Enwiki linqtowiki-enwiki -d C:\Temp -p props-defaults-sample.xml
          linqtowiki-codegen https://en.wikipedia.org/w/api.php
```

The application retrieves information about the API from the API itself,
using the URL you gave as a first parameter.
This requires information about properties of results of the API,
that was not previously available from the API, and was added there because of this library.
This was done quite recently (on 12 June 2012),
so it's not available in the most recent public version of MediaWiki (1.19.1)
or the version currently in use on Wikipedia (1.20wmf5).
Hopefull, this will change soon.

If you don't have recent enough version of MediaWiki (which right now is more than likely),
you can use a workaround: get the necessary information from a file.
The file looks almost the same as an API response in XML format that would contain the information.
There is [a sample of the file](https://github.com/svick/LINQ-to-Wiki/blob/master/LinqToWiki.Codegen.App/props-defaults-sample.xml)
available, which will most likely work for you out of the box.

You don't have to generate a separate assembly for each wiki,
if the methods you want to use look the same on all of them.
In that case, don't forget to specify which wiki do you want to use
in the constructor of the `Wiki` class.

If you want to access multiple wikis with different configuration assemblies
from one program, you can, if you generate each of them into a different namespace
(the default namespace is `LinqToWiki.Generated`).

If you want to do something more complicated regarding generating the configuration assemblies
(for example, create a bunch of C# files that you can modify by hand and then compile into a configuration assembly),
you can use [the LinqToWiki.Codegen library](https://github.com/svick/LINQ-to-Wiki/tree/master/LinqToWiki.Codegen) directly from your own application.

Developer documentation
-----------------------

If you want to modify this code (patches are welcome) or just have a look at the implementation,
here is a short overview of the projects (more details are in the project directories):

* [LinqToWiki.Core](https://github.com/svick/LINQ-to-Wiki/tree/master/LinqToWiki.Core)
 – The core of the library. This project is referenced by all other projects and contains types necessary for acessing the API, processing LINQ expressions, etc.
* [LinqToWiki.Codegen](https://github.com/svick/LINQ-to-Wiki/tree/master/LinqToWiki.Codegen)
 – Handles generating code using Roslyn.
* [LinqToWiki.Codegen.App](https://github.com/svick/LINQ-to-Wiki/tree/master/LinqToWiki.Codegen.App)
 – The `linqtowiki-codegen` command-line application, see [above](#generating-configuration-for-a-wiki). 
* [LinqToWiki.Samples](https://github.com/svick/LINQ-to-Wiki/tree/master/LinqToWiki.Samples)
 – Samples of code that uses this library.
* [LinqToWiki.ManuallyGenerated](https://github.com/svick/LINQ-to-Wiki/tree/master/LinqToWiki.ManuallyGenerated)
 – A manually written configuration assembly. You could use this as a template for your own configuration assembly, but otherwise it's mostly useless.
