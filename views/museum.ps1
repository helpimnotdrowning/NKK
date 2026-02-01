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

function _artifact_preview_component {
	param ($ArtifactFile)
	
	$ArtifactData = _parse_post_header $ArtifactFile -PostType Artifact
	$ArtifactData.Id = $ArtifactFile.Directory.GetFiles('01-*')[0].Directory.Name
	$ArtifactData.Image = $ArtifactFile.Directory.GetFiles('01-*')[0].Name
	
	div -Class 'artifact-component' {
		img -Src "/museum/$($ArtifactData.Id)/$($ArtifactData.Image)"
		
		h1 -Class 'artifact-title' {
			a -Href "/museum/$($ArtifactData.Id)" -InnerHTML $ArtifactData.Title
		}
	}
}

<# ### ### ### #>

doctype

html -Lang en {
	head {
		link -Rel stylesheet -Type text/css -Href /style.css
		_scripts
		
		title Artifacts
		meta -Name darkreader-lock
		meta -Name description -Content ""
		meta -Name viewport -Content "width=device-width, initial-scale=1"
	}
	
	body {
		_header
		
		div -Class 'n-box flex flex-col gap-2' {
			$Artifacts = gci $Data.MuseumRoot -Directory | ? { (gci $_).Count -ge 2 }
			
			div -Class 'artifact-holder' {
				for ($i=0; $i -lt $Artifacts.Count; $i++) {
					try {
						$ArtifactFile = gci $Artifacts[$i] artifact.md
						
						_artifact_preview_component $ArtifactFile
					} catch {
						# _warn $_.ScriptStackTrace
						throw $_
					}
				}
			}
		}
	}
}
