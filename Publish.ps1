[CmdletBinding(SupportsShouldProcess=$true)]
param (
	[parameter(Mandatory=$true,
		HelpMessage="Create token using https://dev.azure.com/github-editor-tools/_usersSettings/tokens?action=edit with Organization: All accessible organizations, Selected scopes: Marketplace (publish)")]
	[string] $personalAccessToken
	,
	[parameter(
		HelpMessage="Publisher name which must match the one in the manifest")]
	[string] $publisherName="JamieCansdale"
	,
	[parameter(
		HelpMessage="Path to the .vsix file for publishing")]
	[string] $payload="$PSScriptRoot\TemporaryProjects.Vsix\bin\Release\TemporaryProjects.vsix"
	,
	[parameter(
		HelpMessage="Path to the publish manifest .json file")]
	[string] $publishManifest = "$PSScriptRoot\publishManifest.json"
)

& $PSScriptRoot\Publish-Vsix.ps1 -WhatIf:$WhatIf -Confirm:$Confirm -publisherName:$publisherName -personalAccessToken:$personalAccessToken -payload:$payload -publishManifest:$publishManifest
