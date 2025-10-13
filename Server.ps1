#!/bin/pwsh
<#
	This file is part of NKK.

	NKK is free software: you can redistribute it and/or modify it under the
	terms of the GNU Affero General Public License as published by the Free
	Software Foundation, either version 3 of the License, or (at your option)
	any later version.

	NKK is distributed in the hope that it will be useful, but WITHOUT ANY
	WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
	FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for
	more details.

	You should have received a copy of the GNU Affero General Public License
	along with NKK. If not, see <http://www.gnu.org/licenses/>.
#>

Import-Module Pode
Import-Module Mizumiya

# its called we do a little bit of trolling
. (Get-Module Pode) {
	# TLDR: purposefully and painfully trample the safety of psd1 files
	# apparently, you can execute code in a module's scope, which allows us to
	# hot-patch away the little obstacle of not being able to use "dynamic
	# expressions" in the psd1 config file. the whole point of psd1 files is
	# that they are a *safe* way to load data in powershell format, but I don't
	# really care about that. personally, this feels akin to beating the runtime
	# over the head with a wrench
	# further reading: https://seeminglyscience.github.io/powershell/2017/09/30/invocation-operators-states-and-scopes
 	function Import-PowerShellDataFile {
		[CmdletBinding()]
		param (
			[String] $Path
		)
		
		return (Invoke-Expression -Command (Get-Content -LiteralPath $Path -Raw) -ErrorAction Stop)
	}
}

. ./functions.ps1

Start-PodeServer {
	# allow dynamic reload of routes without a full server restart (do Ctrl+R !)
	. ./Routes.ps1
}
