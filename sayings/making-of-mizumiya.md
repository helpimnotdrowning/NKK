{ "Title": "Making of Mizumiya", "Description": "The creation story of Mizumiya, an HTML DSL for PowerShell, its core functionality, and notes on a few quirks.", "Created": "2025-10-10" }
# Making of Mizumiya

<img src="img/mizumiya.jpg" align=middle class="mx-64" alt="Logo/banner of Mizumiya" />

<sup>alernatively: "what poor licensing does to a mf"</sup>

Mizumiya was made to exceed [PSHTML](https://github.com/Stephanevg/PSHTML) in
its main game: Being a usable HTML DSL.

And, it features built-in support for [HTMX](https://htmx.org/), the hottest new
development in the web development world! More on that later!

## Origins {#origins}
The PSHTML module is the *original* PowerShell DSL for HTML. That is, it is a
specialized mini-language within PowerShell that reads similarly to HTML, and
that itself generates HTML. See the following (abridged) example from their
README:
```pwsh-mizumiya
Import-Module PSHTML

html {
	head {
		title "woop title"
		link "css/normalize.css" "stylesheet"
	}
	
	body {
		h1 "This is h1 Title in header"
		div {
			p { "This is simply a paragraph in a div." }
			h1 "This is h1"
			h2 "This is h2"
```
... you get the idea.


To preface, I don't mean this in a rude way-- for one, I'm not particularly good
with words. But I have a lot of respect for what PSHTML has been able to do and
all of the capabilities it has with stuff like Charts.js.

However, I also found it lacking in a few areas.

Firstly, it's missing several HTML tags. As part of a now-discarded iteration of
Utatane (probably still unreleased), I needed to use the `<video>` and
`<source>` tags -- both of which are missing from PSHTML! These are among
a handful more of which I don't feel like retrieving again, but it is an
inconvenience. The workaround for this is to manually write the HTML like so:
```pwsh-mizumiya
body -Class '...' -Id '...' {
	div {
@"
		<video>
			<source src="$(somewhere else)" type="video/mp4"/>
		</video>
"@
	}
}
```
Not ideal!

Secondly, it's also missing most of the global attributes. This is things like
`autofocus` and `lang`, as well as the ARIA accessability attributes like
`aria-label`, `role`, and `aria-disabled`. These are not commonly used, but when
they are, they end up stuffed in the (IMO) ugly-looking `Attributes` parameter:
```pwsh-mizumiya
button -Class x -Id y -Attributes @{
	type = button
	{aria-label} = 'Reload'
} { '↻' }
```

Third-ly (and mainly), PSHTML is *not* easily extensible. All elements and their
attributes are defined in their own seperate files and functions, which means
that would-be extensions, like adding the global/ARIA attribute set to the
element functions, requires manually duplicating them across all functions. This
makes the other two issues a hassle to fix.

## Process {#process}
Rather than fork PSHTML, I decided to start over from scratch. The driving force
behind this, rather than forking it and bodge over the bad, was PSHTML's
license. I originally recognized it as MIT, which I would have been about fine
with despite being a GPL person. Upon closer inspection, commit
[531e680](https://github.com/Stephanevg/PSHTML/commit/531e680613cd16d77c9d929a530864cb394d1980)
actually removes the users' right to commercial use, which I was
[not okay with](https://www.gnu.org/licenses/gpl-faq.html#NoMilitary).

Another reason, though less-so, was features like the asset manager, the
aforementioned chart creator, and the color enum: these were features that I
didn't need and considered "bloat" -- of course this ignores all of the valid
use cases by others, but at the very least I was going to
[start](https://www.dreamsongs.com/WorseIsBetter.html)
[simple](https://www.dreamsongs.com/RiseOfWorseIsBetter.html).

So, I started over. A few weeks of on/off work and transcribing every supported
tags' attribute rules, to a self-imposed deadline of June 16th, and Mizumiya was
finished.

## Functionality {#functionality}
The root issue, extensability, is fixed by integrating another smaller
DSL to codegen Mizumiya's element module code.

Rather than initially defining each element as a function by itself, an element
can be quickly defined like so:
```pwsh-mizumiya
_new_tag form -Params @(
	@('Accept', 'String'),
	@('Autocomplete', 'String', @('on', 'off')),
	@('Name', 'String'),
	@('Rel', 'OptionalString', @('about', 'alternate', 'amphtml',
		'apple-touch-icon', 'apple-touch-icon-precompressed',
		'apple-touch-startup-image', '...')),
	@('Action', 'String'),
	@('EncType', 'String', @('application/x-www-form-urlencoded',
		'multipart/form-data', 'text/plain')),
	@('Method', 'String', @('post', 'get', 'dialog')),
	@('NoValidate', 'Switch'),
	@('Target', 'String', @('_self', '_blank', '_parent', '_top'))
)
```
`_new_tag` is an easy helper for `New-HTMLElementFunction` that takes a
specially-formed array and outputs a function definition (as a string). The
general form is:
```pwsh
@(
	@('Attribute Name', 'Type', 'Value Set' ),
	...
)
```
The `Attribute Name` is the HTML attribute name as it will be used in
PowerShell. In the example, you may notice that the names are pascal-cased (as
PowerShell is). The names are automatically fixed when output as HTML; most
names are just lowercased (even though this doesn't actually matter according to
the HTML spec), but some are specially transformed to appear in ways that either
can't be expressed in PowerShell or to better fit the pascal-casing of other
attributes. These special attributes are:
* `AriaDescribedBy` → `aria-describedby`
* `DownloadStr` → `download="..."`: This is due to a limitation in
PowerShell [^1]
* `HttpEquiv` → `http-equiv` This is the only standard non-ARIA attribute that
has a dash in it
* `HxSelect-Oob` → `hx-select-oob`: `hx` is the HTMX attribute prefix.

The `Type` is the "type" of the attribute -- not exactly the type of argument
the parameter will expect. There are only three valid types:
* `String`: For attributes that can accept a value. When the `Value Set` is also
passed, the value of this attribute *must* also belong to that set. HTML
attribute values, at least when passed from the server to the client, can only
be strings, so the distinction between strings, integers, floats, etc. isn't
needed.
* `OptionalString`: This is like `String`, but it requires passing the
`Value Set`. When using this type, PowerShell will suggest
values for it, just like `String`, but it won't require them. This is especially
useful in `<meta rel="...">`, where `rel` has
[a set of agreed-upon values](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Attributes/rel),
but also allows free-form input for
[HTML extensions and microdata](https://microformats.org/wiki/existing-rel-values).
* `Switch`: Attributes in HTML that only need their presence to be activated are
called "boolean" attributes (like `<a download>` or `<input checked>`).
PowerShell has a synonym for this: switch parameters! The usage is essentially
the same; a call to `a -Download` becomes `<a download></a>`.

So, a call like
```pwsh-mizumiya
_new_tag element -Void:$False -Params @(
	@('Autocomplete', 'String', @('on', 'off')),
	@('Name', 'String'),
	@('Rel', 'OptionalString', @('about', 'alternate', 'amphtml',
		'apple-touch-icon', '...')),
	@('NoValidate', 'Switch')
)
```
Would result in a function
```pwsh
function element {
	[CmdletBinding()]
	param (
		[Parameter(ValueFromPipeline)] $InnerHTML,
		[ValidateSet('on', 'off')] [String] ${Autocomplete},
		[String] ${Name},
		[ArgumentCompletions('about', 'alternate', 'amphtml', 'apple-touch-icon', '...')] [String] ${Rel},
		[Switch] ${NoValidate},
		[Hashtable] $Attributes
	)
	
	New-HTMLElement -Tag element -Attributes $PSBoundParameters -InnerHTML $InnerHTML
}
```
with the signature
```pwsh-functionsignature
element [[-InnerHTML] <Object>] [[-Autocomplete] {on | off}] [[-Name]
    <string>] [[-Rel] <string>] [[-Attributes] <hashtable>] [-NoValidate]
    [<CommonParameters>]
```

All generated functions will call into `New-HTMLElement`; it is the "meat", the
actual inner function that takes a tag name and hashtable (`$PSBoundParameters`
is a hashtable of all of a function's parameters) and emits a whole element.

You can now use `element` like just another PowerShell function. This example:
```pwsh-mizumiya
element -Autocomplete on -Name hello -Rel awawa -NoValidate -Attributes @{ 'data-first' = 'value' } {
	1..3 | % {
		element -NoValidate:$([bool]($_ % 2)) { $_ }
	}
}
```
will output... <sup>(formatted for clarity)</sup>
```html
<element data-first="value" autocomplete="on" novalidate rel="awawa" name="hello">
    <element novalidate>1</element>
    <element>2</element>
    <element novalidate>3</element>
</element>
```
You can find here, like in the PSHTML example, my favorite feature of Mizumiya:
the innerHTML (to use the DOM vocabulary) of an element can easily be defined
by adding a positional scriptblock to the end[^2] of the
function call. It feels almost like home, as if you were still writing HTML,
while also allowing you to execute whatever you want inside. You can also
explicitly ask for it with `-InnerHTML`

Small side note: the main `ElementGenerator.ps1` file defines a
`__GLOBAL_ATTR__` "pseudo" element
([not that one](https://developer.mozilla.org/en-US/docs/Web/CSS/Pseudo-elements));
when `_new_tag` gets an element named as such, it saves all of
its attributes and applies it to all future elements (until you clear
`$Script:GLOBAL_ATTR`).

This does, of course, only work with
[non-void](https://developer.mozilla.org/en-US/docs/Glossary/Void_element)
elements that do not accept innerHTML. This is represented in `_new_tag`,
`New-HTMLElementFunction`, and `New-HTMLElement`'s `-Void` parameter. Void
elements will not accept an innerHTML at all.

Attribute values are automatically escaped while the innerHTML is *not*, a
relation I hope is accurately portrayed by the parameter name, somewhat inspired
by the name of React's
[`dangerouslySetInnerHTML`](https://legacy.reactjs.org/docs/dom-elements.html#dangerouslysetinnerhtml).

### A Short Note on HTMX {#htmx}
Mizumiya has native support for the [HTMX](https://htmx.org)
[attribute set](https://htmx.org/reference)! This is only because I found it
useful for work on Utatane and NKK (this site!) -- if you want something else,
you can clone Mizumiya and poke the `__GLOBAL_ATTR__` pseudo-element to add your
own!

## A Note on Semantics {#semantics}
While Mizumiya generates syntactically valid HTML, it does not check the
*semantics*, at least in terms of element heirarchy and attribute values (these,
not beyond the value sets). An `<area>` element should only exist under a
`<map>` element -- they are meaningless otherwise; likewise, the ARIA attribute
`role=listbox` on a parent element is useless when its children list items don't
have `role=option`; Mizumiya won't check for this and will happily let you
generate the following semantically invalid HTML:
```pwsh-mizumiya
area {
	map {
		<# ... #>
	}
}

li -Role listbox {
	ul {
		li <# no -Role option! #> { <# ... #> }
	}
}
```
Mizumiya's functions are essentially just fancy string formatters. There's no
intermediate object representations (though this model *may* be worth
exploring), so this type of advanced checking is not possible.

## A Note on Performance {#performance}
There is unfortunately a small performance concern, though for most cases[^3]
you will probably not experience it. In Utatane, my file index meant to replace
Caddy's build-in file server, loading my `/bin` directory (which contains almost
4000 files) took just about 30 seconds to generate the table rows using
Mizymiya. After days and days spend optimizing the generator, including
experiments in C, C#, and a terrible experience wrangling C#'s FFI, the best
solution was to work around Mizumiya entirely and use string interpolation,
earning a 6x speedup from 30s to 5 seconds!
```pwsh
@"
<tr class="fsobject $( $IsFile ? "file" : "directory" )">
	<td>$( if ($IsFile) { @"
		<a
			download
			class="file-dl clickable"
			href="$FilePath"
			aria-label="Download file">⭳</a>
"@ })
"@
<# ... #>
```

## Pode and Mizumiya {#pode}
Mizumiya, being just PowerShell, is compatible with the Pode web framework.
It can easily be used as the preferred view engine, similarly to PSHTML! For
example, with the following file structure:
```treeview
server_root/
|-- Server.ps1
`-- views/
    |-- home.ps1
    |-- index.ps1
    `-- login.ps1
```

and the following engine definition...
```pwsh
Import-Module Mizumiya

Start-PodeServer {
	Set-PodeViewEngine -Type Mizumiya -Extension ps1 -ScriptBlock {
		param ($Path, $Data)
		
		[String] (. $Path $Data)
	}

	<# ... #>
}
```

Pode will run any `.ps1` file in a route and return its output as an HTML
document!

## And a Note on the Gallery... {#psgallery}
The module hasn't been submitted yet to the PowerShell Gallery, but it can be
manually installed by copying the `git:Mizumiya/Mizumiya` directory to a path
in your `$env:PSModulePath` as such:
```treeview
/.../share/powershell/Modules/
`-- Mizumiya <-- git:Mizumiya/Mizumiya
    |-- Mizumiya.psd1
    `-- Mizumiya.psm1
```

You can see the source code and download or clone it at my
[Forgejo instance](https://git.helpimnotdrowning.net/helpimnotdrowning/Mizumiya)
(or its [GitHub mirror](https://github.com/helpimnotdrowning/Mizumiya))!

[^1]: A PowerShell parameter can only be either a switch or not-a-switch. In
HTML, an `<a>` tag's `download` attribute can either have a value, which is the
requested file name upon download, or it can merely be present, where it will be
"assigned" a file name (which is usually just the file name on the server). This
optional-value behavior
[can't be done](https://github.com/PowerShell/PowerShell/issues/12104)
in PowerShell. The two workarounds I found are to either:
	1. Require the user to pass an empty string when they wanted the default
behavior; I found this to feel clunky:
		`a -Href https://somewhere/file.txt -Download ""`
		and
		`a -Href https://somewhere/file.txt -Download myname.txt`
	2. Use two different parameters for the different modes. This is the option
I went with:
	`a -Href https://somewhere/file.txt -Download` and
	`a -Href https://somewhere/file.txt -DownloadStr myname.txt`

[^2]: Technically anywhere, but for readability you will probably place it at
the end. When the functions are generated (and the element is
[non-void](https://developer.mozilla.org/en-US/docs/Glossary/Void_element)), the
`-InnerHTML` parameter is placed first *without a defined type*. This tells
PowerShell that the first argument to the function without a preceding name (a
positonal parameter) should be consumed by `-InnerHTML`. If this is a
scriptblock, it is executed. Otherwise it is just stringified and plainly
printed in the HTML.
[^3]: basically untested other than exclusively this case, but nevertheless,
