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

function _post_component {
	param ($PostFile)
	
	$PostData = _parse_saying_header $PostFile
	
	div -Class 'post-component' {
		h1 -Class 'post-title' {
			a -Href $PostData.Path -InnerHTML $PostData.Title
		}
		p -Class 'flex gap-2' {
			span -Class 'text-current/50' { $PostData.Created }
			$PostData.Description
		}
	}
}

<# ### ### ### #>

doctype

html -Lang en {
	head {
		link -Rel stylesheet -Type text/css -Href /style.css
		_scripts
		
		title Posts
		meta -Name darkreader-lock
		meta -Name description -Content ""
		meta -Name viewport -Content "width=device-width, initial-scale=1"
	}
	
	body {
		_header
		
		div -Class 'n-box flex flex-col gap-2' {
			$Posts = gci $Data.SayingsRoot *.md
			
			for ($i=0; $i -lt $Posts.Count; $i++) {
				try {
					_post_component $Posts[$i]
					if ($i+1 -ne $Posts.Count) {
						# add divider after all except last post
						hr
					}
				} catch {
					_warn $_
				}
			}
		}
	}
}
