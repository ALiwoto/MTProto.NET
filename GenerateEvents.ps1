
param (
    [Parameter(Mandatory=$false)]
    [string]$TargetFilePath = "./MTProto/Client/MTProtoEventsClient.cs",
    [Parameter(Mandatory=$false)]
    [string]$TypesListFilePath = "./MTProto/Client/Z_MTProtoEvents.cs"
)

# This function trims the given string (prefix and/or suffix flags have to be
# specified) and returns the results.
function Get-TrimmedStringValue {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [string]$TheValue,
        [Parameter(Mandatory = $false)]
        [string]$Prefix = $null,
        [Parameter(Mandatory = $false)]
        [string]$Suffix = $null
    )

    process {
        if (-not [string]::IsNullOrEmpty($Prefix)) {
            if ($TheValue.StartsWith($Prefix)) {
                $TheValue = $TheValue.SubString($Prefix.Length, $TheValue.Length - $Prefix.Length)
            }
        }

        if (-not [string]::IsNullOrEmpty($Suffix)) {
            if ($TheValue.EndsWith($Suffix)) {
                $TheValue = $TheValue.SubString(0, $TheValue.Length - $Suffix.Length)
            }
        }

        return $TheValue
    }
}

[System.Collections.Generic.List[string]]$AllUpdateTypes = @()

# These are more-general updates. We would want them to be
# at the end of the list.
[string[]]$generalUpdatesList = @(
    "TL.UpdateChannel",
    "TL.UpdateWebPage",
    "TL.UpdateDeleteMessages",
    "TL.UpdateNewMessage",
    "TL.UpdateShortSentMessage",
    "TL.UpdateShortChatMessage",
    "TL.UpdateShortMessage",
    "TL.UpdateShort"
)

$typesLines = Get-Content -Path $TypesListFilePath
foreach ($currentTypeLine in $typesLines) {
    $currentTypeLine = `
        $currentTypeLine.TrimStart("/", " ", "`t") | Get-TrimmedStringValue -Prefix "Derived classes: "
    if (!$currentTypeLine.StartsWith("TL")) {
        continue
    }

    $AllUpdateTypes.AddRange($currentTypeLine.Split([string[]]@(",", " "), [System.StringSplitOptions]::RemoveEmptyEntries))
}

$normalIndent = " " * 4
$theContent = [System.Text.StringBuilder]::new()
$theContent.AppendLine("// <auto-generated>")
$theContent.AppendLine("//     Generated by the GenerateEvents.ps1 script.  DO NOT EDIT!")
$theContent.AppendLine("//     source: GenerateEvents.ps1")
$theContent.AppendLine("// </auto-generated>")
$theContent.AppendLine("")

$theContent.AppendLine("using System;")
$theContent.AppendLine("using System.Collections.Generic;")
$theContent.AppendLine("using System.Linq;")
$theContent.AppendLine("using System.Text;")
$theContent.AppendLine("using System.Threading.Tasks;")
$theContent.AppendLine("using MTProto.Client.Events;")
$theContent.AppendLine("")
$theContent.AppendLine("namespace MTProto.Client")
$theContent.AppendLine("{")
$theContent.AppendLine("$($normalIndent)public class MTProtoEventsClient")
$theContent.AppendLine("$($normalIndent){")
$theContent.AppendLine("$($normalIndent*2)#region events region")

# adding event managers
foreach ($currentUpdateType in $AllUpdateTypes) {
    if ($currentUpdateType -in $generalUpdatesList) {
        continue
    }

    $trimmedValue = $currentUpdateType | Get-TrimmedStringValue -Prefix "TL.Update"
    $theContent.Append($normalIndent * 2) > $null
    $theContent.Append("public virtual EventManager<MTProtoClientBase, $currentUpdateType> ") > $null
    $theContent.Append("Event$trimmedValue { get; set; }") > $null
    $theContent.AppendLine("") > $null
}

# general event managers
foreach ($currentUpdateType in $generalUpdatesList) {
    $trimmedValue = $currentUpdateType | Get-TrimmedStringValue -Prefix "TL.Update"
    $theContent.Append($normalIndent * 2) > $null
    $theContent.Append("public virtual EventManager<MTProtoClientBase, $currentUpdateType> ") > $null
    $theContent.Append("Event$trimmedValue { get; set; }") > $null
    $theContent.AppendLine("") > $null
}

$theContent.AppendLine("")
$theContent.AppendLine("$($normalIndent*2)#endregion")
$theContent.AppendLine("")

$theContent.AppendLine("$($normalIndent*2)public virtual async Task HandleValidEvent(MTProtoClientBase client, TL.IObject arg)")
$theContent.AppendLine("$($normalIndent*2){")
$theContent.AppendLine("$($normalIndent*3)switch (arg)")
$theContent.AppendLine("$($normalIndent*3){")

# adding switch-cases
foreach ($currentUpdateType in $AllUpdateTypes) {
    if ($currentUpdateType -in $generalUpdatesList) {
        continue
    }

    $trimmedValue = $currentUpdateType | Get-TrimmedStringValue -Prefix "TL.Update"
    $theContent.Append($normalIndent*4) > $null
    $theContent.AppendLine("case $currentUpdateType update:") > $null
    $theContent.Append($normalIndent*5) > $null
    $theContent.AppendLine("await (Event$($trimmedValue)?.InvokeHandlers(client, update) ?? Task.CompletedTask);") > $null
    $theContent.Append($normalIndent*5) > $null
    $theContent.AppendLine("break;") > $null
}

# general switch-cases
foreach ($currentUpdateType in $generalUpdatesList) {
    $trimmedValue = $currentUpdateType | Get-TrimmedStringValue -Prefix "TL.Update"
    $theContent.Append($normalIndent*4) > $null
    $theContent.AppendLine("case $currentUpdateType update:") > $null
    $theContent.Append($normalIndent*5) > $null
    $theContent.AppendLine("await (Event$($trimmedValue)?.InvokeHandlers(client, update) ?? Task.CompletedTask);") > $null
    $theContent.Append($normalIndent*5) > $null
    $theContent.AppendLine("break;") > $null
}

$theContent.AppendLine("$($normalIndent*3)}")

# ---------------------------
# switch statement ends here.
# ---------------------------

$theContent.AppendLine("$($normalIndent*2)}")
$theContent.AppendLine("$($normalIndent*1)}")
$theContent.AppendLine("}")

# ------------
# End of file.
# ------------

Set-Content -Path $TargetFilePath -Value $theContent.ToString() > $null
