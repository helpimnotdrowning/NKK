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

Use-PodeScript -Path $ScriptBase/PreRoutes.ps1
Use-PodeScript -Path $ScriptBase/functions.ps1

Add-PodeRoute -Method GET -Path /random-splash -ScriptBlock {
	Write-PodeViewResponse -Path random_splash
}

Add-PodeRoute -Method GET -Path /sayings -ScriptBlock {
	Write-PodeViewResponse -Path sayings -Data @{ SayingsRoot = (Join-Path $ScriptBase 'sayings') }
}

Add-PodeRoute -Method GET -Path /sayings/img/* -ScriptBlock {
	$Path = $WebEvent.Path
	$JoinedPath = Join-Path $ScriptBase $Path
	
	if (-not (_check_joinedpath_is_based $JoinedPath)) {
		_warn "/sayings/img/*: query for $JoinedPath ($Path) refused!"
		Set-PodeResponseStatus -Code 404
		return
	}
	
	Write-PodeFileResponse -Path $JoinedPath
}

Add-PodeRoute -Method GET -Path /sayings/* -ScriptBlock {
	$Path = $WebEvent.Path + '.md'
	$JoinedPath = Join-Path $ScriptBase $Path
	
	if (-not (_check_joinedpath_is_based $JoinedPath)) {
		_warn "/sayings/*: query for $JoinedPath ($Path) refused!"
		Set-PodeResponseStatus -Code 404
		return
	}
	
	Write-PodeViewResponse -Path sayings_post -Data @{ Path=$JoinedPath }
}

Add-PodeRoute -Method GET -Path /museum -ScriptBlock {
	Write-PodeViewResponse -Path museum -Data @{ MuseumRoot = (Join-Path $ScriptBase 'museum') }
}

Add-PodeRoute -Method GET -Path /museum/:artifactId -ScriptBlock {
	$ArtifactId = $WebEvent.Parameters.ArtifactId
	$ArtifactFile = Join-Path $ScriptBase museum $ArtifactId artifact.md
	
	if (-not (_check_for_artifact $ArtifactId)) {
		return
	}
	
	Write-PodeViewResponse -Path museum_post -Data @{
		ArtifactId = $ArtifactId
		ArtifactFile = $ArtifactFile
		ArtifactDirectory = (Get-Item $ArtifactFile).Directory
	}
}

Add-PodeRoute -Method GET -Path /museum/:artifactId/:image -ScriptBlock {
	$ArtifactId = $WebEvent.Parameters.ArtifactId
	$ArtifactFile = Join-Path $ScriptBase museum $ArtifactId artifact.md
	
	if (-not (_check_for_artifact $ArtifactId)) { return }
}

Add-PodeRoute -Method GET -Path / -ScriptBlock {
	Write-PodeViewResponse -Path index
}
