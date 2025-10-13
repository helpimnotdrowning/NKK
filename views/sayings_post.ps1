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

# ASSUMING THE PATH HAS BEEN PRECHECKED...

$Path = $Data.Path
$PostData = _parse_saying_header ($Path)

<# ### ### ### #>

doctype

html -Lang en {
	head {
		link -Rel stylesheet -Type text/css -Href /style.css
		_scripts
		
		script -Src /prism.js
		script @"
Prism.plugins.autoloader.languages_path = '/prism/';
Prism.plugins.autoloader.use_minified = false;
"@
		
		title $PostData.Title
		meta -Name darkreader-lock
		meta -Name description -Content ""
		meta -Name viewport -Content "width=device-width, initial-scale=1"
	}
	
	body {
		_header
		
		div -Class "n-box xmin-h-[10em]! tx" -HxDisable {
			ConvertFrom-Markdown -InputObject (Get-Content $Path | Select-Object -skip 1 | Join-String -Sep "`n") | % Html
		}
	}
}
