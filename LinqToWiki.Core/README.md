The LinqToWiki.Core project
===========================

The LinqToWiki.Core project contains shared code that can be used when querying any MediaWiki wiki
that has the API enabled.
It can be used together with code generated through LinqToWiki.Codegen,
but it can also be used without it.

In fact, LinqToWiki.Codegen internally uses LinqToWiki.Core to access the `paraminfo` module
using manually written code.

`QueryTypeProperties`
---------------------

The `QueryTypeProperties` class holds basic information about a “query type”,
which corresponds to an API module.
This information includes the prefix this module uses in its parameters,
what type of module it is or mapping of its result properties to values accepted by the `prop` parameter.
It is also able to parse XML elements this module returns.

`WikiQuery`
-----------

Probably the most often used and certainly the most interesting queries are those using `list` query modules.
Such queries are represented in LinqToWiki by a group of types whose names start with `WikiQuery`.

Specifically, there are four such types:
`WikiQuery`, `WikiQuerySortable`, `WikiQueryGenerator` and `WikiQuerySortableGenerator`.
If a module supports sorting, it is represented by a type with `Sortable` in its name
and if it supports being used as a generator for `prop` queries, it is represented by a type with `Generator` in its name.

There is also a fifth type: `WikiQueryResult`.
This type by itself represents a query that can't be modified anymore,
but can be used to execute it and get the results.
All of the four preceding types inherit from `WikiQueryResult`,
so it is possible  to execute the query using any one of them too.

The type governs what operations are available.
For example, if a type is one of the two `Sortable` types,
it will have an `OrderBy()` method, but no other type has this method.
Each method can also return a different type, as is necessary to form queries.

---

All of the `WikiQuery`-related types are generic
and their type parameters are used to decide what properties can be used in each operation.
For example, the type parameter `TOrderBy` of `WikiQuerySortable`
decides what properties can be used in the parameter of the `OrderBy` method.

The way this is achieved is that `TOrderBy` is a type that contains the properties that can be
used for sorting in the module `WikiQuerySortable` represents
and the `OrderBy` method accepts lambda expressions whose parameter is of this type.

For example, if some module supported sorting by `PageId` and `Title`,
then `TOrderBy` would be a type that contains two properties with those names.
Because of this, a query like `source.OrderBy(x => x.Title)` would compile and execute fine,
but `source.OrderBy(x => x.Name)` would fail to compile.

Because of the way lambda expressions work, queries like `source.OrderBy(x => x.Title.Substring(1))` or `source.OrderBy(x => random.Next())` would compile fine.
But because there is no way to efficiently execute such queries using the MediaWiki API,
they will fail with an exception at runtime.

---

The various methods available on the `WikiQuery` types are:

* `Where()` only sets some parameter or parameters of a query,
it always returns the same type.

 It is available on all four of the basic `WikiQuery` types
and uses the generic type parameter `TWhere`.

* `Select()` is used to choose how the elements in the resulting collection should look like
and what properties should they contain.
Because the result of the lambda passed into this method can be an arbitrary type,
it doesn't make sense to modify the query after calling this method.
Because of that, `Select()` returns `WikiQueryResult`.
This also follows query expression syntax, where `select` is the last clause of each query.

 It is available on all four of the `WikiQuery` types
and uses the type parameter `TSelect`.

* `ToEnumerable()` and `ToList()` are used to actually execute the query.
The distinction between the two methods is that `ToEnumerable()` returns an `IEnumerable`,
that lazily loads new pages of results on demand.
`ToList()`, on the other hand, returns a `List`,
that is immediately loaded with all of the results, possibly from many pages.

 These two methods are available on all of the `WikiQuery` types, including `WikiQueryResult`
and return the result based on the type parameter `TSource` for most of the types.
And exception is `WikiQueryResult`, which uses a separate `TResult` type parameter.

* `OrderBy()` (and `OrderByDescending()`) sets the ordering.
Because it doesn't make sense to sort the same query multiple times
and because no module supports sorting by multiple keys,
this method returns the type with `Sortable` removed.

 This method is available on the two `Sortable` types
and uses the type parameter `TOrderBy`.

* `Pages` is a property that returns a `PagesSource`
that can then be used in a `prop` query.

 This property is available on the two `Generator` types
and uses the type parameter `TPage`.

`PagesSource`
-------------

The `PagesSource` type represents a collection of pages that can be used in `prop` queries,
to get information about those pages.
This information can be for example a list of categories for each page in the collection.

There are two kinds of `PagesSource`s: generator-based and list-based.

---

List-based sources use a static list of pages, given as a collection of page titles, page IDs or revision IDs.

Because the number of pages given this way in a single API request is fairly limited (usually to 50),
large lists have to be queried multiple times.
`PagesSource` handles this transparently, so the user can input as many pages as he wants and doesn't have to worry about the limit.

One exception is if the limit is different than the default of 50 for the current user on the current wiki.
In that case, the user should change the limit by setting the static property `ListPagesCollection.MaxLimit`.
(In all other cases where limits are important in this library, they limit the output, not the input.
That is why simply setting `limit=max` works in those other cases, but doesn't work here.)

If the collection used to create a `PagesSource` is lazy, it is iterated in a lazy manner.
For example, it could be the result of another LinqToWiki query, with additional processing by LINQ to objects,
that is not possible using LinqToWiki alone.
Or it could the result of a query from another wiki.
In such cases, the original query will only make as many requests as necessary for the follow-up query.

---

Generator-based sources represent a dynamic list of pages that is the result of another API query,
like the list of all pages on a wiki from the `allpages` module.
This way, the list of pages doesn't have to be retrieved separately, only to be sent back.

Generator queries also have to handle paging,
including the exception for the `revisions` module.

---

Thanks to the fact that both kinds of page sources for `prop` queries are represented by the same
(abstract) type, the user of this library can use the same code to work with any source,
thus avoiding repetitive code.

---

To actually create a `prop` query for a page source, one uses the `Select()` method.
Its parameter is a lambda, whose parameter is the type parameter `TPage` of `PagesSource`.
This type is the same for all queries on the same wiki, but could be different for differnt wikis.

Inside the lambda, properties and methods of the `TPage` type can be accessed.
Each of them represents a `prop` module and all of the methods return one of the `WikiQuery` types,
which can then be queried as usual, with one condition:
the `WikiQuery` types can't “leak” outside of the query, so one has to use `ToEnumerable()` or `ToList()` inside the lambda.

There is a special case for the `revisions` module,
which can be also used with the `FirstOrDefault()` method,
which means only the most recent revision for each page is selected.

If a `prop` module has a single result (not a collection), it is represented as a property
that directly returns this result, no querying is possible.

---

The methods of these `prop` queries are inside a lambda expression,
so they are not actually executed unless the expression was compiled and the resulting delegate invoked.
Because of this, processing them is not as simple as with normal queries.

---

An example of `PagesSource` query:

```C#
pagesSource.Select(
    p =>
    new
    {
        p.Info,
        Categories =
            p.Categories()
            .Where(c => c.Show == Show.NotHidden)
            .Select(c => new { c.Title, c.SortKeyPrefix })
            .ToEnumerable()
            .Take(10)
	}
)
```

`QueryParameters`
-----------------

The `QueryParameters` type contains the parameters of a query:

* sort direction and parameter by which to sort,
* list of properties to select and a delegate that uses them to construct the result object,
* list of other parameters, as key-value pairs.

`QueryParameters` is an immutable type,
so that one beginning of a query can be safely used repeatedly, as is the case with LINQ to objects.
The list of other parameters is a functional-style immutable linked list.

---

The `PropQueryParameters` type derives from `QueryParameters`
and is used to store information about a single module in a `prop` query.
Apart from inherited members, it also contains the name of the module
and a special value indicating whether to retrieve only the first item,
which corresponds to the usage of the `FirstOrDefault()` method.

A related type is `PageQueryParameters`,
which represents a whole `prop` query.
That means it contains a list of `PropQueryParameters` objects
and also information about the source of the query.

`ExpressionParser`
------------------

The `ExpressionParser` static class is used to process expression trees from LINQ methods
and store the processed query parameters in `QueryParameters`.

Common for all expression tree processing is that closed-over local variables contained in the processed lambda,
which are represented as members of a compiler-generated closure class,
have to be first replaced by their actual value.
This is done using [`PartialEvaluator` written by Matt Warren](http://blogs.msdn.com/b/mattwar/archive/2007/08/01/linq-building-an-iqueryable-provider-part-iii.aspx).

Also, some property names have to be translated from their C# version to their API version.

---

Each of the methods requires different processing. Specifically:

* Expression trees from `Where()` are first split into one or more subexpressions
that are anded together (`x => subexpr1 && subexpr2 && ...`; or is not supported by the API)
and each of the subexpressions is then added as a key-value pair to the result.

 Each subexpression has to be in the form `x.Property == Value`,
where `Value` is a constant, possibly from an evaluated closed-over variable.
The reverse order (`Value == obj.Property`) is also allowed.
An alternative for boolean properties is accessing the property directly (`x.Property`)
or negated (`!x.Property`).

* Processing `OrderBy()` expression trees is simple:
they can either be identities (`x => x`), which means default sorting will be used
(which is the only possibility for some modules),
or they can be simple property accesses (`x => x.Property`),
which means the result will be sorted by that property.

 The order of sorting (ascending or descending) is decided by the method used:
whether it was `OrderBy()` or `OrderByDescending()`.

* Expression trees from `Select()` are processed in two steps.
First, the expression is scanned for usages of its parameter.
If any of its properties are used, it means those properties have to be retrieved from the API.
If the parameter is used directly, without accessing its properties,
it means all of the properties have to be retrieved, because it is impossible to say which of them will be used.

 For example, the expression `x => new { x.Property1, x.Property2 }`
means only `Property1` and `Property2` have to be retrieved.
On the other hand, `x => SomeMethod(x)` means all of the properties have to be retrieved.

 Second step is compiling the expression into a delegate,
which will then be executed for each item coming from the API.

 Put together, these two steps mean that `Select()` can be used with any expression
and only properties that are actually needed will be returned by the API.

`PageExpressionParser`
----------------------

The class `PageExpressionParser` is used to process
the `Select()` lambda in `PagesSource` queries.
The difficulty there is that the direct approach of building the query step-by-step,
used in normal queries, will not work.
That is because the expression has to be analyzed before there is any page object
that it expects as its parameter.

The result of this analysis is twofold:
the set of parameters needed for all of the `prop` queries,
as a collection of `PropQueryParameters`,
and a delegate that can be used to get the result object for each page in the API response.

---

Because the subquery for each `prop` module has to end with a call to `ToEnumerable()` or `ToList()`,
the parameters can be extracted by invoking the part of the subquery before that call.
At the beginning of each subquery is invoking a module-specific method on the page object.
But because there is no page object to use, that invocation is first replaced by an appropriate `WikiQuery` object.

For example, for the query in [Section `WikiQuery`](#wikiquery), the invoked code is (where `wikiQuery` is the appropriate `WikiQuery` object):

```C#
wikiQuery.Where(c => c.Show == Show.NotHidden)
	  .Select(c => new { c.Title, c.SortKeyPrefix })
```

---

To get the delegate, all calls to `Where()` and `OrderBy()` are removed,
because their only purpose is to modify the query parameters.
Then the single parameter of type `TPage` is replaced by a parameter of type `PageData`
and calls to module methods are replaced by calls to `GetData()`,
with a type parameter specifying the type of the result and a parameter specifying the name of the module.

The `GetData()` method returns a collection,
so for modules that return only a single item, like `info`,
a call to `SingleOrDefault()` is also added.

For example the expression in the query in [Section `WikiQuery`](#wikiquery) is transformed into:

```C#
pageData =>
new
{
    Info = pageData.GetData<InfoResult>("info")
    		     .SingleOrDefault(),
    Categories =
    	pageData.GetData<CategoriesSelect>("categories")
    		 .Select(c => new { c.Title, c.SortKeyPrefix })
    		 .Take(10)
}
```

Other types
-----------

The `QueryProcessor` type manages downloading the result and transforming it from XML to objects.
For queries whose result is a collection, it also handles returning the pages in a lazy manner
and downloading the follow-up pages when necessary.

The `QueryPageProcessor` type does the same for `PagesSource` queries.

---

The `Downloader` type takes care of forming the query string, executing the request and
returning the result as an `XDocument`.
`XDocument` is a part of LINQ to XML, a part of .Net framework for manipulating XML documents.

`Downloader` always uses POST and formats its requests as `application/x-www-form-urlencoded`.
This means that all modules work, including those that require POST.
On the other hand, uploads of files don't work, because they require `multipart/form-data`.

The decision to use `application/x-www-form-urlencoded` follows from the fact that
`multipart/form-data` is very inefficient when sending multiple parameters with short values,
which is common when making requests to the API.
