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
Import-Module PSParseHTML -Function Optimize-HTML

if (-not (Get-Command tailwindcss -ErrorAction SilentlyContinue)) {
	_warn "The tailwindcss binary is missing. Source styles will not be synced with the public style.css."
	_warn "This is only needed during development, don't worry about it in production"
} else {
	Add-PodeFileWatcher -Path $PSScriptRoot -Exclude "$PSScriptRoot/public" -ScriptBlock {
		tailwindcss --input $PSScriptRoot/tailwind/style.tw.css --output $PSScriptRoot/public/style.css
	}
}

# Use-PodeScript -Path functions.ps1 # https://github.com/Badgerati/Pode/issues/1582
Use-PodeScript -Path $PSScriptRoot/functions.ps1

New-PodeLoggingMethod -Custom -ScriptBlock {
	param ($Item)
	
	$Date = $Item.UtcDate | Get-Date -Format s
	$Query = $Item.Request.Query -eq '-' ? '' : '?' + $Item.Request.Query
	
	$PrevCol = $PSStyle.Reset + $PSStyle.Foreground.BrightBlack
	switch ($Item.Response.StatusCode) {
		{$_ -ge 400} { $Col = $PSStyle.Foreground.BrightRed }
		{$_ -ge 500} { $Col = $PSStyle.Foreground.White + $PSStyle.Background.Red }
		default      { $Col = '' }
	}
	
	$RequestLine = "$($Item.Host) $($Item.Request.Method) $($PSStyle.Foreground.White)$($Item.Request.Resource)$Query$PrevCol $($Item.Request.Protocol) by ""$($Item.Request.Agent)"""
	$ResponseLine = "$Col$($Item.Response.StatusCode) $($Item.Response.StatusDescription)$PrevCol $(_format_size $Item.Response.Size)"
	
	_request "$Date | $RequestLine >>> $ResponseLine"
} | Enable-PodeRequestLogging -Raw

New-PodeLoggingMethod -Custom -ScriptBlock {
	param ($Item)
	
	_warn "$($Item.Date | Get-Date -Format s) | $($Item.Level) ($($Item.Server):$($Item.ThreadId)) | $($Item.Category): $($Item.Message)"
	$Item.StackTrace -split "`n" | % { _warn $_ }
} | Enable-PodeErrorLogging -Raw -Levels Error, Warning, Informational

Add-PodeEndpoint -Address * -Port 8081 -Protocol HTTP

Set-PodeViewEngine -Type Mizumiya -Extension ps1 -ScriptBlock {
	param ($Path, $Data)
	. ./functions.ps1
	
	([String] (. $Path $Data)) | Optimize-HTML
}

Add-PodeRoute -Method GET -Path /random-splash -ScriptBlock {
	Write-PodeViewResponse -Path random_splash
}

Add-PodeRoute -Method GET -Path /sayings -ScriptBlock {
	Write-PodeViewResponse -Path sayings -Data @{ SayingsRoot = (Join-Path $PSScriptRoot 'sayings') }
}

Add-PodeRoute -Method GET -Path /sayings/img/* -ScriptBlock {
	# terminate with / to avoid something like .../NKK/... <-> .../NKKsomewhere/...
	$Root = $PSScriptRoot + '/'
	$Path = $WebEvent.Path
	$JoinedPath = Join-Path $Root $Path
	
	if (-not (_check_joinedpath_is_based $Root $JoinedPath)) {
		_warn "/sayings/img/*: query for $JoinedPath ($Path) refused!"
		Set-PodeResponseStatus -Code 404
		return
	}
	
	Write-PodeFileResponse -Path $JoinedPath
}

Add-PodeRoute -Method GET -Path /sayings/* -ScriptBlock {
	# terminate with / to avoid something like .../NKK/... <-> .../NKKsomewhere/...
	$Root = $PSScriptRoot + '/'
	$Path = $WebEvent.Path + '.md'
	$JoinedPath = Join-Path $Root $Path
	
	if (-not (_check_joinedpath_is_based $Root $JoinedPath)) {
		_warn "/sayings/*: query for $JoinedPath ($Path) refused!"
		Set-PodeResponseStatus -Code 404
		return
	}
	
	Write-PodeViewResponse -Path sayings_post -Data @{ Path=$JoinedPath }
}

Add-PodeRoute -Method GET -Path / -ScriptBlock {
	Write-PodeViewResponse -Path index
}
