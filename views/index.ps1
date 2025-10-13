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

function _link {
	param ($Title, $Href)
	
	return li {
		a -Href $Href { $Title }
	}
}

<# ### ### ### #>

doctype

html -Lang en {
	head {
		link -Rel stylesheet -Type text/css -Href /style.css
		_scripts
		
		meta -Name darkreader-lock
	}
	
	body {
		_header
		
		# tailwind wont pick up the class if I leave it as ".no_underline!",
		# since the preceding "." in the class directive prevents it from
		# recognizing the class name
		# https://github.com/tailwindlabs/tailwindcss/pull/18967#issuecomment-3393700620
		$no_underline = 'no-underline!'
		$tb = 'target="_blank"'
		# can you tell I like markdown?
		div -Class "n-box tx p-[1rem]!" {
@"
i would have a portfolio thing here, but for now you can...

* read my [posts](/sayings)!
* click the <div class="you-know-you-want-to inline-block p-[0.5ch]!">splash</div> ↑↑↑
* consider consulting my archives: [files.helpimnotdrowning.net](https://files.helpimnotdrowning.net){$tb}
	* (currently using Caddy's fileserver but will soon use my own!)
* collect my pages[:](/garbage/my_pages.png){.$no_underline $tb}
	* [My Forgejo instance](https://git.helpimnotdrowning.net/explore/repos){$tb}
	* [Github](https://github.com/helpimnotdrowning){$tb}
"@ | ConvertFrom-Markdown | % html
		}
	}
}
