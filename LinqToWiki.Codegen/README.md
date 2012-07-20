The LinqToWiki.Codegen project
==============================

The LinqToWiki.Codegen project contains code that retrieves information about API modules in some wiki,
then uses that information to generate C# code to access those modules using Roslyn
and finally compiles the code into a library.

---

Roslyn was chosen, because it is superior when compared with common approaches for code generation in .Net,
namely Reflection.Emit and CodeDOM.

[Reflection.Emit](http://msdn.microsoft.com/en-us/library/8ffc3x75) is a set of types that allow code generation of code at runtime.
The generated code can then be directly executed or saved as an assembly (.dll or .exe) to disk.
The distinguishing feature is that it uses the low-level Common Intermediate Language (CIL),
which means writing any code beyond the simplest methods can be very tedious and error-prone.

[CodeDOM](http://msdn.microsoft.com/en-us/library/650ax5cx) can be used to generate code and compile it to an assembly.
It uses language-independent model, which can be converted to various .Net languages,
including C# and VB.NET.
This model is also the biggest disadvantage of CodeDOM, because it means it doesn't support all features of C#.
For example, even such basic feature as writing a `static` class is impossible in the CodeDOM model
without using “hacks”.

---

At this point, we have a library (LinqToWiki.Core) that can be used to access the MediaWiki API the way we want
from the final generated library.
We can also use the same library to get the information we need about the modules of the API from the `paraminfo` module.
And we have decided we want to use Roslyn to generate the final library.
What remains is to decide what code to generate, how exactly to map the modules, their parameters
and their results into the model of LinqToWiki.Core.

There are some decisions that were already made in LinqToWiki.Core
(the `sort` and `dir` parameters should map to `OrderBy()`;
the `prop` parameter maps to `Select()`),
but several other decisions still remain:

* How should the remaining parameters be mapped?
Should they all go into `Where()` or somewhere else? Where?
* How should the modules that don't return lists be mapped?
LINQ methods are not suitable for them, because they are meant to work with collections.
* How to name the generated types and members?
Specifically, how to represent names that can't be used (like those containing special characters)
and names that are undesirable (those that conflict with C# keywords).
Also, should the generated members follow .Net naming conventions?

Our answers to these questions are in the following couple of sections.

Naming of generated types and members
-------------------------------------

Let us start with the last question: Should the generated members follow .Net naming conventions?
[The .Net naming guidelines](http://msdn.microsoft.com/en-us/library/ms229002),
that are widely followed by various .Net libraries and the .Net framework itself,
state that names of types and public members should use PascalCase,
that is, each word of an identifier should start with a capital letter
and the identifier should not contain any delimiters (such as underscores).

We would prefer to follow these naming conventions, but, unfortunately, it is not possible.
That is because the names of modules, parameters, result properties and almost all enumerated types in the API
use names that are all lowercase, without delimiters between words.
That means there is no way to figure out which letters in an identifier should be capitalized
(apart from the first one).

As one of the more extreme examples,
one of the possible values of the `rights` parameter
of the `allusers` module on the English Wikipedia is `collectionsaveascommunitypage`.
A human can see that the proper name for that value using PascalCase would be `CollectionSaveAsCommunityPage`,
but a computer cannot.
(Actually, it is possible that the words could be reliably separated using natural language processing,
but doing that is outside the scope of this work.)

---

Because different .Net languages have different sets of reserved identifier names
(usually, those are the language keywords)
and because libraries written in one language should be usable from other languages,
.Net languages provide a way to use their keywords as identifiers.
In the case of C#, this is done by prefixing the identifier with an at sign.
So, for example, to use `new` as an identifier, one has to write `@new`.

Thanks to this, using keyword-named identifiers is still possible,
although slightly less convenient than with normal identifiers.
Also, the naming guidelines suggest avoiding keywords as identifiers.

In MediaWiki core API modules, there are four identifiers that are also C# keywords:
`namespace`, `new`, `true`, `false`.
Out of these, we decided to shorten `namespace` to `ns`,
which is a common abbreviation, so the meaning should not be lost.
The other three have to be written with `@` (`@new`, `@true` and `@false`) in C#,
because we did not find a reasonable alternative for them.

---

As for special characters, the delimiters hyphen (`-`), slash (`/`) and space
appear in some names in the API, but are not allowed in .Net identifiers,
so they are replaced by underscores (`_`).

Some names also start with the exclamation mark (`!`), to indicate negation.
Such names are translated by prefixing `not_`.
So for example, `!minor` (which means that an edit is not a minor edit)
is translated into `not_minor`.

One more special case is that some enumerated types allow an empty value.
Such value is then represented by the identifier `none`.

---

Another question is how to name the generated types.
There two kinds of generated types:
those that represent some enumerated type and those that represent parameters or results of some module.

For the latter kind, it is simple to come up with a convention like naming them by the module name,
suffixed by the specific kind of the type
(e.g. `blockResult` for the result of the `block` module
or `categorymembersWhere` for the type representing `Where()` parameters for the `categorymembers` module).

But for the former kind, the situation is more complicated.
Enumerated types do not have names by themselves, they are part of a parameter or property that has a name.
The problem is that different modules often have parameters and properties with the same name,
while their type sometimes is the same and sometimes it is not.

So, there are two options: either let the types that look the same actually be the same generated type,
or let each parameter and property have its own distinct type.
If we merge the types that look the same, we should not use the module name in their name,
because one type can be used with different modules.
But that means we need to distinguish different types in another way, like a number.
But names like `token5` are not very helpful for the user.

Because of that, we chose the other option, which means including the name of the module in the type name.
But doing it this way does not eliminate conflicts completely:
In the case when a module has a parameter and a property with the name,
their types still have to be distinguished.
An example of such type name is `recentchangestype2`.

Structure of generated code
---------------------------

At the start of each query is the `Wiki` type.
It contains methods for non-query modules as well as methods to create list-based `PagesSource`s.
It also contains the property `Query` that returns an object that contains methods for
`list` and `meta` query modules (`prop` query modules work differently).

---

With modules that don't return lists, the situation is mostly simple:
there are no parameters to sort or filter the result (because it's not a list)
and most of those modules also don't have parameters to choose the result properties.

Because of that, a method for each such module, that directly returns the result object is enough.
This method has parameters corresponding to the parameters of the module,
where required parameters of the module are mapped as normal method parameters
and parameters that are not required are mapped as optional parameters.
The code of this method builds `QueryParameters` from the method parameters
and then executes the query using `QueryProcessor`.

---

On the other hand, list modules can have several kinds of parameters:

* Those that affect order of the items in the list. They are naturally mapped as `OrderBy()`.
The parameters `sort` and `dir` belong here.
* Those that choose what properties appear in the result. They are naturally mapped as `Select()`.
Only the parameter `prop` belongs here.
* Those that filter what items appear in the result. They are naturally mapped as `Where()`.
For example, the parameters `namespace` and `startsortkey`
of the `categorymembers` module belong here.
* Various other parameters. They do not naturally map to any LINQ method.
For example, the parameter `title` (that decides which category to enumerate)
of the `categorymembers` module belongs here.

The first two kinds are not a problem, because it is clear which parameters belong to them.
The second two kinds are a problem, because there is no clear way to automatically distinguish between the two.
One exception is if a parameter is required (as indicated in its description),
then it means it belongs to the other parameters.

Required parameters are given as parameters of the module methods,
but we decided to treat all non-required parameters that do not belong to the first two kinds,
as if they were `Where()` parameters.
Unfortunately, this means that some queries do not logically make sense,
if we consider that the `Where()` method should only filter the results.

For example, consider this query:

```C#
wiki.Query.categorymembers()
    .Where(cm => cm.title == "Category:Query languages")
```

There, the `title` property does not actually represent filtering
by the title of the category member, it decides which category to enumerate.
And without it, the query would not even execute successfully
(the parameter `title` is not marked as required, because the parameter `pageid` can be used instead of it).

Proper solution to this problem would require human interaction when generating the code,
to choose which parameters belong to `Where()` and which do not.
As an alternative, the description of each parameter in the `paraminfo` module could contain its kind.

---

One more question is how to represent enumerated types.
The answer is seemingly simple: make them `enum`s
and for those parameters or properties that can have multiple values, use bit flags.
But the largest type that can be used as an underlying type for `enum`
is `ulong`, which has 64 bits.
That means this will work only if there is no enumerated type in the API,
that has more than 64 values and can have multiple values at the same time.
Unfortunately, the English Wikipedia has one:
the type of the `rights` parameter of the `allusers` module
has 106 values and the parameter can have multiple values at the same time.

Because of that, each enumerated type is represented by immutable class deriving from the common base class `StringValue`,
with inaccessible constructor and static field for each possible value.
Combination of values can be represented as a collection, like with other types.

`Wiki`
------

The top-level type that manages all code generation is `Wiki`
(not to be confused with the generated `Wiki` type).
It manages retrieving information about API modules and generating code for them.

When the code generation is complete, it saves the generated C# files to a temporary directory
and compiles them using CodeDOM.
CodeDOM is used for the compilation,
because its compiler is the full C# compiler and can handle all features of C# (unlike the CodeDOM object model).
The Roslyn compiler is not able to compile some useful expressions, such as collection initializers
(but the object model of Roslyn is complete).

`ModuleSource`
--------------

The `ModuleSource` class is used to retrieve information about modules of the API
and transform it from XML to objects, like `Module`, `Parameter` and `ParameterType`.
This information comes from the `paraminfo` module and is fetched using LinqToWiki.Core.

In fact, this code can be viewed as a sample on how to use LinqToWiki.Core
without code generated by LinqToWiki.Codegen.
Generated code cannot be used to work with the `paraminfo` module,
because it is one of the modules, whose response is complicated
and does not fit into the simple type system used by `paraminfo`.

Because the addition of result properties to `paraminfo` was made as a part of this work
and so is quite recent, there is also another option to get this information:
`ModuleSource` can accept a “props defaults” file, that contains the necessary information.
The file looks the same as `paraminfo` response (in XML format),
except it contains only the added information.
This file can be created from another wiki that can already provide this information,
or it can be written by hand.
It can be also useful to work with modules from extensions, that currently don't provide this information.

`ModuleGenerator`
-----------------

`ModuleGenerator` and related types are the ones that actually generate code for each module using Roslyn.
Each type generates code for a certain kind of module,
so for example `ModuleGenerator` works with non-query modules,
while `QueryModuleGenerator` works with most query modules.

Each generator creates all the code that is necessary for that module.
For example, for a `list` query module,
this includes generating `Where`, `Select` and possibly `OrderBy` classes,
method in the `QueryAction` class
(which is returned by the `Query` property of the `Wiki` class)
and types for all its enumerated types.

Each of the generated types and methods also has XML documentation comment attached,
based on description from `paraminfo`.
This means that a user of this library does not have to guess what each method or property means,
his IDE will show him description for it.

These descriptions sometimes contain references to details of the API that this library abstracts away.
For example, the description for the `unique` parameter of the `alllinks` module says:
“Only show unique links. Cannot be used with generator or alprop=ids.”
The reference to `alprop` makes sense to someone who uses the API directly,
but would be very confusing for a user of LinqToWiki.
Not only does LinqToWiki abstract away module prefixes (`al`),
it also doesn't expose the `prop` parameter directly
(the `Select()` method is used instead).

`SyntaxEx`
----------

Creating Roslyn syntax trees can be cumbersome.
The `SyntaxEx` class makes doing that easier by adding simpler alternatives
to the factory methods in Roslyn's `Syntax` class.
The `SyntaxEx` methods do not handle more complex cases,
so for those, using `Syntax` is still necessary.

For example this is how code can be written using `SyntaxEx`:

```C#
SyntaxEx.AutoPropertyDeclaration(
  new[]
  {
    SyntaxKind.PublicKeyword,
    SyntaxKind.AbstractKeyword
  },
  "CategoryInfoResult",
  "CategoryInfo",
  setModifier: SyntaxKind.PrivateKeyword,
  isAbstract: true)
```

---

Another improvement is that syntax nodes that represent declaration of property, field, parameter or variable
can be used to refer to them in later code, for example when assigning the value of a parameter to a property.
This is achieved by using implicit conversions and a helper type `NamedNode`.
In Roslyn without this extension, it is necessary to extract the name of the syntax node
and use that to create `IdentifierNameSyntax`.

As with `SyntaxEx`, this can make simple cases simpler, but cannot handle evything.
Because of that, complex cases still have to directly use Roslyn.
