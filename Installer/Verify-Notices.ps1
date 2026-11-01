param([string]$Root=(Split-Path $PSScriptRoot))
$ErrorActionPreference='Stop'

# Reviewed license texts. Normalize line endings so a Windows/Linux checkout
# preserves the same check while retaining all copyright, conditions and disclaimers.
# Jipper: ac6d6410571ee7d585aedce7f5085de34f615af2/LICENSE.
# Noto: google/fonts 4efc2774c63917927efe769ca845def6bd6debae/ofl/notosanskr/OFL.txt.
# Gmarket: the vendor license retained with the original Light/Medium fonts.
$notices=@{
    'Licenses/JipperResourcePack-BSD-3-Clause.txt'='B85C066B532EA07471B8575D0495DB987C811186805DE2266062529CF0A81226'
    'Assets/GmarketSans-OFL.txt'='730DB2B2DD4C86E67F8EB48E293979448976A6E4709AA6A0FB4608B004CA8C39'
    'Assets/NotoSansKR-OFL.txt'='1C05C68C34F9708415AADA51F17E1B0092D2CEA709BF4A94CD38114F9E73D7D9'
}
foreach($entry in $notices.GetEnumerator()){
    $path=Join-Path $Root $entry.Key
    if(!(Test-Path -LiteralPath $path -PathType Leaf)){throw ('Required third-party notice is missing: '+$entry.Key)}
    $text=[IO.File]::ReadAllText($path).Replace("`r",'').TrimEnd([char]10)+"`n"
    $sha=[Security.Cryptography.SHA256]::Create()
    try{$hash=[BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($text))).Replace('-','')}finally{$sha.Dispose()}
    if($hash -ne $entry.Value){throw ('Third-party notice differs from its reviewed full text: '+$entry.Key)}
    'PASS complete third-party notice '+$entry.Key
}

foreach($relative in @('LICENSE.md','THIRD-PARTY-NOTICES.md','Licenses/JipperResourcePack-source.json','Assets/NotoSansKR-source.json')) {
    if(!(Test-Path -LiteralPath (Join-Path $Root $relative) -PathType Leaf)){throw ('Missing license policy or provenance: '+$relative)}
}
$provenance=Get-Content -LiteralPath (Join-Path $Root 'Licenses/JipperResourcePack-source.json') -Raw -Encoding UTF8|ConvertFrom-Json
if($provenance.commit -ne 'ac6d6410571ee7d585aedce7f5085de34f615af2' -or !$provenance.derivedFiles.Count){throw 'Missing upstream commit or derived source inventory'}
if(Test-Path -LiteralPath (Join-Path $Root 'src/Mod')) {
    foreach($relative in $provenance.derivedFiles){
        if($relative -notmatch '^src/Mod/[A-Za-z0-9_/.-]+\.cs$' -or $relative.Contains('..')){throw 'Invalid provenance source path'}
        $source=Join-Path $Root $relative
        if(!(Test-Path -LiteralPath $source -PathType Leaf)){throw ('Missing attributed source: '+$relative)}
        $text=[IO.File]::ReadAllText($source)
        if($text -notmatch '\A// Derived from JipperResourcePack by Jongyeol\. Copyright \(c\) 2024, Jongyeol\.' -or $text -notmatch 'Licenses/JipperResourcePack-BSD-3-Clause.txt'){throw ('Missing upstream copyright or license reference: '+$relative)}
    }
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $Root 'src/Mod') -Filter *.cs -Recurse){
        if([IO.File]::ReadAllText($file.FullName) -match '\A// (Derived from|Adapted from) JipperResourcePack'){
            $relative=$file.FullName.Substring([IO.Path]::GetFullPath($Root).TrimEnd('\','/').Length+1).Replace('\','/')
            if($provenance.derivedFiles -cnotcontains $relative){throw ('Attributed source missing from provenance inventory: '+$relative)}
        }
    }
    'PASS upstream copyright headers and source inventory'
}
