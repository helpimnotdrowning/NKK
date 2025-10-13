// assumes a direct copy-paste from a cmdlet's SYNTAX help section, like
/*
element [[-InnerHTML] <Object>] [[-Autocomplete] {on | off}] [[-Name] 
    <string>] [[-Rel] <string>] [[-Attributes] <hashtable>] [-NoValidate] 
    [<CommonParameters>]
*/

Prism.languages['pwsh-functionsignature'] = {
	'function': {
		// the function name will always be the first word!
		pattern: /^(?:\s+)?\w+ /i,
	},
	'parameter': {
		pattern: /-\w+/,
		alias: 'variable'
	},
	'punctuation': /[\[\]\(\)\{\}\<\>]/
}
