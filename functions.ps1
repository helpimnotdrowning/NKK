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

function _log {
	param(
		[Parameter(ValueFromPipeline)]
		$Message,
		[ValidateSet('Request', 'Information', 'Warning', 'Fatal')]
		$Type
	)
	
	switch ($Type) {
		'Request' {
			$Style = $PSStyle.Foreground.BrightBlack
		}
		
		'Information' {
			$Style = ''
		}
		
		'Warning' {
			$Style = $PSStyle.Formatting.Warning
		}
		
		'Fatal' {
			$Style = $PSStyle.Formatting.Error
		}
	}
	
	Write-Host "$Style[NKK/$Type] $Message$($PSStyle.Reset)"
	if ($Type -eq 'Fatal') { throw $Message }
}

function _request {
	param(
		[Parameter(ValueFromPipeline)]
		$Message
	)

	_log -Type Request -Message $Message
}

function _info {
	param(
		[Parameter(ValueFromPipeline)]
		$Message
	)

	_log -Type Information -Message $Message
}

function _warn {
	param(
		[Parameter(ValueFromPipeline)]
		$Message
	)

	_log -Type Warning -Message $Message
}

function _fatal {
	param (
		[Parameter(ValueFromPipeline)]
		$Message
	)
	
	_log -Type Fatal $Message
}

function _get_splash {
	param ( $Index )
	
	if ($Index -and $Index -gt 0) {
		return (Get-PodeConfig).NKK.Splashes | Select-Object -Index $Index | ConvertFrom-Markdown | % Html
	}
	
	return (Get-PodeConfig).NKK.Splashes | Get-Random | ConvertFrom-Markdown | % Html
}

function _format_size ([uint64] $size) {
	switch ($Size) {
		{$size -gt 1tb} { return '{0:n2} TiB' -f ($size/1tb) }
		{$size -gt 1gb} { return '{0:n2} GiB' -f ($size/1gb) }
		{$size -gt 1mb} { return '{0:n2} MiB' -f ($size/1mb) }
		{$size -gt 1kb} { return '{0:n2} KiB' -f ($size/1kb) }
		default { return "$size B" }
	}
}

function _check_joinedpath_is_based ($Root, $JoinedPath) {
	$Resolved = Get-Item -LiteralPath $JoinedPath -ErrorAction Ignore
	
	return ($null -ne $Resolved -and $Resolved.FullName.StartsWith($Root))
}

function _scripts {
	script -Src /htmx.js
	# meta -Name htmx-config -Content (@{ scrollIntoViewOnBoost=$false; defaultHideShowStrategy='twDisplay' }|ConvertTo-Json -Compress)
	meta -Name htmx-config -Content (@{ scrollIntoViewOnBoost=$false  }|ConvertTo-Json -Compress)
}

function _header {
	header -Class "n-box clear-n-box flex flex-row! items-center mb-1!" {
		# img -Class "rounded-md pfp" -Src https://avatars.githubusercontent.com/helpimnotdrowning -Alt "Profile picture" -Width 70
		img -Class "rounded-md pfp" -Src /user.webp -Alt "Profile picture" -Width 70
		
		div -Class "ms-6" {
			span -Class "text-4xl" { 'helpimnotdrowning' }
			div -Id splash -HxGet /random-splash `
				-HxSwap innerText `
				-HxTrigger 'load,click' `
				{ p 'missingno' }
		}
	}
	
	nav -Class 'n-box mt-2! w-full' {
		div {
			div -Class 'float-left flex gap-4' {
				a -Href / { 'Home' }
				a -Href /sayings { 'Posts' }
			}
			div -Class 'float-right' {
				a `
					-Class 'text-blue-600 underline' `
					-Href 'https://git.helpimnotdrowning.net/helpimnotdrowning/NKK' `
					-Target _blank `
					{
						'running helpimnotdrowning/NKK'
					}
			}
		}
	}
}

function _parse_saying_header {
	param ([IO.FileInfo] $Path)
	$Data = Get-Content $Path | Select-Object -First 1 | ConvertFrom-Json -AsHashtable
	$Data.Path = (Join-Path /sayings $Path.BaseName)
	
	return $Data
}
