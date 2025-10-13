var pz = Prism.languages['pwsh'];
pz.function.push(
	// Import-Module Mizumiya; Get-Module Mizumiya | % ExportedCommands | % Keys | Join-String -Sep '|'
	// WITH element,_new_tag
	/\b(?:element|_new_tag|a|abbr|address|area|article|aside|AttributeEncode|audio|b|base|bdi|bdo|blockquote|body|br|button|canvas|caption|cite|code|col|colgroup|comment|datalist|datatag|dd|del|details|dfn|dialog|div|dl|doctype|dt|em|embed|fencedframe|fieldset|figcaption|figure|footer|form|h1|h2|h3|h4|h5|h6|head|header|hgroup|hr|html|HTMLEncode|i|iframe|img|input|ins|kbd|label|legend|li|link|main|map|mark|marquee|menu|meta|meter|nav|New-HTMLElement|noscript|object|ol|optgroup|option|output|p|picture|pre|progress|q|rb|rp|rt|rtc|ruby|s|samp|script|search|section|selecttag|slot|small|source|span|strong|style|sub|summary|sup|table|tbody|td|template|textarea|tfoot|th|thead|time|title|tr|track|u|ul|vartag|video|wbr)\b/i
)
Prism.languages['pwsh-mizumiya'] = pz;
