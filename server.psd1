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

Import-Module Mizumiya

@{
	NKK = @{
		EnableSplashText = $true
		# these are markdown!
		Splashes = @(
			"bau bau",
			"konrushi",
			"System.Object[]",
			"null",
			"missingno",
			"mooming",
			"2:23 AM",
			"Proudly sponsored by SPAM® Less Sodium: There’s no sacrifice with this meat treat!!1!",
			(HTMLEncode "hi :) `"</p><script id=`"jk`" src=https://xplo.it.ru/q.js></script>"),
			"TAke a look, y'all: data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAIAAABLbSncAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH6QIbFgkBvik3CQAAANNJREFUCNcByAA3/wF0z7UFAgIzFBkLDQQBBfzt8fnj7PJL+xIDHSo2ECMU4PHz6/X07fb57vj58///9gb9AhkqFw8QCh0HAvn+/vD4+ur1+jQPDuwNAQT/BgMd+wE1FBfWDfrz8vUKBgo5+Q7O8eEC/QcBFvH55bK9NPIKPwYUGdPj4LzFF/4CBP79/xUKDicrLwkMFAADAhkiBQAV9c776wL//gvd6u0JIR8A/f0A/P0FDArzAQDt+/0E9/IEoOju/PPeTjAlCAgH4+jvk9bm3/n8b9hiDUdDcYcAAAAASUVORK5CYII=",
			"hello world!",
			"%x%x%x%x",
			"[object Function]",
			"professional git commit history entangler",
			"chronic minecraft player",
			"[SYNTAX ERROR ON LINE (6408082)]",
			"nananan",
			"[恋をして!](https://jamies.page/)",
			"Not hosted on GitHub!",
			"OpenGL 2.1 (if supported)!",
			"Now With Multiplayer!",
			"Alpha version!",
			"As seen on TV!",
			"$(Get-ChildItem -Force -File -Recurse | ? { $_.Extension -notin '.woff2','.jpg','.png','.md','.ico' } | % { cat $_ } | wc -l) lines of code!",
			# this only runs once on (re)load so its always the same number until the server restarts, good enough?
			"The instruction at 0x$( "{0:x}" -f (get-random -min 0 -max 0x7fffffff) ) referenced memory at 0x00000000. The memory could not be read. Click [here](/garbage/bugs_when_you_lift_up_a_rock.jpg) to terminate the program.",
			"buhhhh",
			"こんるし〜"
		)
	}
	
	Server = @{
		AllowedActions = @{
			Suspend = $false
			Disable = $false
		}
	}
	
	Web = @{
		Compression = @{
			Enable = $true
		}
		
		Static = @{
			Cache = @{
				Enable = $true
				Include = @(
					'/font/*',
					'/prism/*'
				)
			}
		}
	}
}
