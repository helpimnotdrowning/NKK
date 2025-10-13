(function (Prism) {
	var pwsh = Prism.languages.pwsh = {
		'directive': [
			/^using.*/m,
			/^#Requires/m,
		],
		'comment': [
			{
				pattern: /(^|[^`])<#[\s\S]*?#>/,
				lookbehind: true
			},
			{
				pattern: /(^|[^`])#.*/,
				lookbehind: true
			}
		],
		'string': [
			{
				pattern: /@"[\s\S]*"@/i,
				greedy: true,
				inside: null // see below
			},
			{
				pattern: /@'[\s\S]*'@/i,
				greedy: true,
			},
			{
				pattern: /"(?:`[\s\S]|[^`"])*"/,
				// greedy: true,
				inside: null // see below
			},
			{
				pattern: /'(?:[^']|'')*'/,
				greedy: true
			}
		],
		// Matches name spaces as well as casts, attribute decorators. Force starting with letter to avoid matching array indices
		// Supports two levels of nested brackets (e.g. `[OutputType([System.Collections.Generic.List[int]])]`)
		'namespace': /\[[a-z](?:\[(?:\[[^\]]*\]|[^\[\]])*\]|[^\[\]])*\]/i,
		'boolean': /\$(?:false|true)\b/i,
		'number': {
			/*
			(^|[^a-zA-Z0-9_]) -- (lookbehind) word boundary since \b doesn't
			           quite work, see https://www.rexegg.com/regex-boundaries.php#real-word-boundary
			[-+]?   -- pos/neg
			(?:0x)? -- hex prefix
			\.?     -- decimal with no leading 0 (unfortunately this will also
			           accept a string like ".0.0", but that's probably fine)
			\d+     -- numbers
			(?:
				(?: -- decimal:
					\.       -- mid-number decimal separator
					(?:\d+)? -- numbers (optional here!)
				)?
				(?:e\+\d+)? -- exponent
				(?:d|l|uy|y|us|s|u|n)? -- see about_Numeric_Literals
			)?
			*/
			pattern: /(^|[^a-z0-9_])[-+]?(?:0x)?\.?\d+(?:(?:\.(?:\d+)?)?(?:e\+\d+)?(?:d|l|uy|y|us|s|u|n)?)?/i,
			lookbehind: true
		},
		'variable': [
			/\$[\w:]+\b/,
			// dont match escaped brace (`})
			/\${(.*(?!`}))?}/,
			{
				pattern: /(\$[\w:]+\b\.)[\w]+/,
				lookbehind: true
			}
		],
		// Cmdlets and aliases. Aliases should come last, otherwise "write" gets preferred over "write-host" for example
		'function': [
			// (Get-Verb | % Verb)+(Get-Command | ? { $_.ModuleName -match "Microsoft.PowerShell.(Util|Core|Management)" } | % { $_.Name.split('-')[0] }) | sort | uniq | Join-String -Separator '|'
			/\b(?:Add|Approve|Assert|Backup|Block|Build|Checkpoint|Clear|Close|Compare|Complete|Compress|Confirm|Connect|Convert|ConvertFrom|ConvertTo|Copy|Debug|Deny|Deploy|Disable|Disconnect|Dismount|Edit|Enable|Enter|Exit|Expand|Export|Find|ForEach|Format|Get|Grant|Group|Hide|Import|Initialize|Install|Invoke|Join|Limit|Lock|Measure|Merge|Mount|Move|New|Open|Optimize|Out|Ping|Pop|Protect|Publish|Push|Read|Receive|Redo|Register|Remove|Rename|Repair|Request|Reset|Resize|Resolve|Restart|Restore|Resume|Revoke|Save|Search|Select|Send|Set|Show|Skip|Sort|Split|Start|Step|Stop|Submit|Suspend|Switch|Sync|Tee|Test|Trace|Unblock|Undo|Uninstall|Unlock|Unprotect|Unpublish|Unregister|Update|Use|Wait|Watch|Where|Write)-[a-z]+\b/i,
			// (Get-Alias | ? { $_.ReferencedCommand.Module.Name -match "Microsoft.PowerShell.(Util|Core|Management)" } | % name)+$winalias | sort | uniq | Join-String -Separator '|'
			// where $winalias is
			// (on Windows) `Get-Alias | ?{ $_.ReferencedCommand.Module.Name -match "Microsoft.PowerShell.(Util|Core|Management)" } | % name`
			// (on Linux) `$winalias = @"<the above output pasted..." -split "`n"
			/\b(?:ac|cat|cd|chdir|clc|cli|clp|clv|compare|copy|cp|cpi|cpp|cvfj|cvpa|cvtj|dbp|del|diff|dir|ebp|echo|epal|epcsv|erase|fc|fhx|fl|ft|fw|gal|gbp|gc|gcb|gci|gcs|gdr|gerr|gi|gin|gl|gm|gp|gps|gpv|group|gsv|gtz|gu|gv|iex|ii|ipal|ipcsv|irm|iwr|kill|ls|measure|mi|move|mp|mv|nal|ndr|ni|nv|ogv|popd|ps|psmount|pushd|pwd|rbp|rd|rdr|ren|ri|rm|rmdir|rni|rnp|rp|rv|rvpa|sal|saps|sasv|sbp|scb|select|set|shcm|si|sl|sleep|sls|sort|sp|spps|spsv|start|stz|sv|tee|type|write)\b/i,
			/\b(?:\S+-\S+)\b/i,
			{
				pattern: /\b((?:function|filter|workflow) )\S+\b/i,
				lookbehind: true
			},
			{
				/*
				(\.) -- (lookbehind) "." for property access
				\w+  -- name
				(?<= -- make sure this is a method call!
					\(
				)
				*/
				pattern: /(\.)\w+(?<=\()/i,
				lookbehind: true
			},
			{
				/*
				(\. |& )?  -- (lookbehind) check if dot-sourcing or calling
				[\\/.\S]+? -- executable name including more periods
				\.         -- extension separator
				(?:com|exe|bat|cmd|vbs|vbe|js|jse|ps1) -- most of Windows' $env:PATHEXT
				*/
				pattern: /(\. |& )?[\\/.\S]+?\.(?:com|exe|bat|cmd|vbs|vbe|js|jse|ps1)/i,
				lookbehind: true
			}
		],
		'parameter': {
			pattern: /-{1,2}\S+/,
			alias: 'variable',
			lookbehind: true
		},
		'operator': {
			/*
			(^|\W) -- (lookbehind) check if at a valid start point
			(?:
				! -- alternate not operator
			|
				- -- operator "flag"
				(?:
					b? -- binary?
					(?:and|x?or|not)
				|
					[ci]? -- c(ase sensitive) and i(nsensitive)
					(?:gt|ge|lt|le|eq|ne)
					
				|
					[ci]?
					(?:not)?
					(?:contains|in|like|match)
				|
					as|is(?:not)?|join|replace|sh[lr]
				)
				\b
			|
				-- minus, decrement, and minus-equals
				-[-=]?
			|
				-- add, incremement, plus-equals
				\+[+=]?
			|
				-- mult/div/mod and their x-equals
				[*\/%]=?
			|
				-- file redirection: send (>) and append (>>)
				>>?
			|
				-- stream redirection: pwsh has 6 streams, which can either be
				   redirected (>>?) or send to stream 1 (the only valid stream-
				   to-stream redirection). see about_Redirection
				   CURRENTLY BROKEN, OVERRIDEN BY number
				[*1-6](?:>&1|>>?)
			)
			*/
			pattern: /(^|\W)(?:!|-(?:b?(?:and|x?or|not)|as|[ci]?(?:gt|ge|lt|le|eq|ne)|[ci]?(?:not)?(?:contains|in|like|match)|is(?:not)?|join|replace|sh[lr])\b|-[-=]?|\+[+=]?|[*\/%]=?|>>?|[*1-6](?:>&1|>>?))/i,
			lookbehind: true
		},
		// per http://technet.microsoft.com/en-us/library/hh847744.aspx
		'keyword': /\b(?:Begin|Break|Catch|Class|Continue|Data|Define|Do|DynamicParam|Else|ElseIf|End|Exit|Filter|Finally|For|ForEach|From|Function|If|InlineScript|Parallel|Param|Process|Return|Sequence|Switch|Throw|Trap|Try|Until|Using|Var|While|Workflow)\b/i,
		'punctuation': /[|{}[\];(),.]/
	};

	// Variable interpolation inside strings, and nested expressions
	var inner = {
		'function': {
			// Allow for one level of nesting
			/*
			(^|[^`]) -- (lookbehind) start of string but not at an escape character
			\$\( -- start subshell $(
			(?:
				\$\([^\r\n()]*\) -- more subshells
			|
				(?!
					\$\( -- not another subshell (see above)
				)
				[^\r\n)] -- greedy match to the end of the subshell
			)*
			\)
			*/
			// pattern: /(^|[^`])\$\((?:\$\([^\r\n()]*\)|(?!\$\()[^\r\n)])*\)/,
			// this seems to fix the string interpolation but will probably
			// break more things
			pattern: /(^|[^`])\$\(.*\)/s,
			lookbehind: true,
			greedy: true,
			inside: pwsh
		},
		'boolean': pwsh.boolean,
		'variable': pwsh.variable,
	};
	
	pwsh.string[0].inside = inner;
	pwsh.string[2].inside = inner;

}(Prism));
