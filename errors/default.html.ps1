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

$Root = $Data.Root
$Path = $Data.Path

doctype

html -class 'from-red-300! to-red-500!' {
	head {
		link -Rel stylesheet -Href '/style.css'
		
		_scripts
		meta -Name darkreader-lock
	}
	
	body {
		_header
		
		div -Class "n-box text-center" {
			h1 {
				span -Class 'text-6xl' { $Data.Status.Code }
				br
				span -Class 'text-4xl' { $Data.Status.Description }
			}
			
			hr
			
			"OOPSIE WOOPSIE!! Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!"
			
			hr
			
			a -Href '/' -Class 'clickable p-4' { 'go home......' }
		}
	}
}
