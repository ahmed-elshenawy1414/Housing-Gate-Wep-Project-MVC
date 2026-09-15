$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

# 1) Read current admin resx entries (before gen regenerates them)
function Read-Resx([string]$path) {
    $content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    $map = @{}
    foreach ($m in [regex]::Matches($content, '<data name="([^"]+)"[^>]*>\s*<value>([^<]*)</value>')) {
        $map[$m.Groups[1].Value] = $m.Groups[2].Value
    }
    return $map
}

$adminEnBefore = Read-Resx (Join-Path $root "Resources\Areas\AdminAreaResource.resx")
$adminArBefore = Read-Resx (Join-Path $root "Resources\Areas\AdminAreaResource.ar-EG.resx")
# Read the tsv to know which keys are "original" (came from tsv)
$tsvContent = [System.IO.File]::ReadAllText((Join-Path $root "Resources\strings.tsv"), [System.Text.Encoding]::UTF8)
$tsvAdminKeys = @{}
foreach ($line in $tsvContent.Split("`n")) {
    if ($line -match "^Areas/AdminAreaResource`t([^`t]+)`t([^`t]+)`t([^`t]+)$") {
        $tsvAdminKeys[$Matches[1]] = $true
    }
}

# Identify the 55 extra keys (not in tsv = my manual additions)
$extraKeys = @{}
foreach ($k in $adminEnBefore.Keys) {
    if (-not $tsvAdminKeys.ContainsKey($k)) {
        $extraKeys[$k] = @{ en = $adminEnBefore[$k]; ar = $adminArBefore[$k] }
    }
}
Write-Host "Found $($extraKeys.Count) extra admin keys to preserve"

# 2) Append new SharedResource rows to strings.tsv
$appendPath = Join-Path $root "tools\append-shared.tsv"
$appendContent = @"
Common.Search	Search	بحث
Common.SearchPlaceholder	Search...	بحث...
Audit.Action.User.Delete	Delete user	حذف المستخدم
Audit.Action.Property.Delete	Delete property	حذف العقار
"@
[System.IO.File]::WriteAllText($appendPath, $appendContent, $utf8NoBom)

$newLines = [System.IO.File]::ReadAllLines($appendPath, [System.Text.Encoding]::UTF8)
$tsvPath = Join-Path $root "Resources\strings.tsv"
$appended = @()
foreach ($line in $newLines) {
    $appended += ("SharedResource`t" + $line)
}
[System.IO.File]::AppendAllText($tsvPath, ([string]::Join("`n", $appended) + "`n"), $utf8NoBom)
Write-Host "Appended $($appended.Count) rows to strings.tsv"

# 3) Regenerate SharedResource resx (and all others) from tsv
& (Join-Path $root "tools\gen-resources.ps1") -InputFile $tsvPath -OutDir (Join-Path $root "Resources")

# 4) Re-merge extra admin keys back into the regenerated admin resx files
function Merge-Extra([string]$path, [hashtable]$extra) {
    $content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    $sb = New-Object System.Text.StringBuilder
    foreach ($key in ($extra.Keys | Sort-Object)) {
        $keyEsc = [System.Security.SecurityElement]::Escape($key)
        $valEsc = [System.Security.SecurityElement]::Escape($extra[$key])
        [void]$sb.AppendLine("  <data name=`"$keyEsc`" xml:space=`"preserve`">")
        [void]$sb.AppendLine("    <value>$valEsc</value>")
        [void]$sb.AppendLine("  </data>")
    }
    $content = $content.Replace("</root>", $sb.ToString() + "</root>")
    [System.IO.File]::WriteAllText($path, $content, $utf8NoBom)
    Write-Host "Re-merged $($extra.Count) extra keys into $path"
}

Merge-Extra (Join-Path $root "Resources\Areas\AdminAreaResource.resx") $extraKeys
$arExtra = @{}
foreach ($k in $extraKeys.Keys) { $arExtra[$k] = $extraKeys[$k].ar }
Merge-Extra (Join-Path $root "Resources\Areas\AdminAreaResource.ar-EG.resx") $arExtra

Remove-Item $appendPath -ErrorAction SilentlyContinue
Write-Host "Done"
