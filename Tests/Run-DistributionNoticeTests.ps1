$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression
$project=Split-Path $PSScriptRoot
$work=Join-Path $project ('Backups/DistributionNotices-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work -Force|Out-Null
$required=@('LICENSE.md','THIRD-PARTY-NOTICES.md','Licenses/JipperResourcePack-BSD-3-Clause.txt','Licenses/JipperResourcePack-source.json','Assets/GmarketSans-OFL.txt','Assets/NotoSansKR-OFL.txt','Assets/NotoSansKR-source.json')
foreach($mode in @('valid','missing','changed','duplicate','dependency')){
    $path=Join-Path $work ($mode+'.zip');$file=[IO.File]::Create($path);$zip=[IO.Compression.ZipArchive]::new($file,[IO.Compression.ZipArchiveMode]::Create,$false)
    try{
        foreach($relative in $required){
            if($mode -eq 'missing' -and $relative -eq $required[2]){continue}
            $entry=$zip.CreateEntry('GhostifyOverlay/'+$relative);$stream=$entry.Open()
            try{
                $bytes=if($mode -eq 'changed' -and $relative -eq $required[2]){[Text.Encoding]::UTF8.GetBytes('short credit without the license conditions')}else{[IO.File]::ReadAllBytes((Join-Path $project $relative))}
                $stream.Write($bytes,0,$bytes.Length)
            }finally{$stream.Dispose()}
        }
        if($mode -eq 'duplicate'){$zip.CreateEntry('GhostifyOverlay/'+$required[2])|Out-Null}
        if($mode -eq 'dependency'){$zip.CreateEntry('GhostifyOverlay/SkyHook.Unity.dll')|Out-Null}
    }finally{$zip.Dispose();$file.Dispose()}
    $failed=$false
    try{& (Join-Path $project 'Installer/Verify-DistributionNotices.ps1') -Package $path -Root $project|Out-Null}catch{$failed=$true}
    if(($mode -eq 'valid') -eq $failed){throw ('FAIL distribution fixture '+$mode)}
    'PASS distribution notice fixture '+$mode
}
$info=Get-Content -LiteralPath (Join-Path $project 'Info.json') -Raw|ConvertFrom-Json
foreach($name in @(('GhostifyOverlay-'+$info.Version+'.zip'),('GhostifyOverlay-Setup-'+$info.Version+'.exe'))){
    & (Join-Path $project 'Installer/Verify-DistributionNotices.ps1') -Package (Join-Path $project ('dist/'+$name)) -Root $project
}
'PASS actual shipping ZIP and EXE retain complete license policy, notices and provenance'
