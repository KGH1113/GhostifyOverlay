param([Parameter(Mandatory=$true)][string]$Package, [string]$Root=(Split-Path $PSScriptRoot))
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression
$packagePath=(Resolve-Path -LiteralPath $Package).Path
$required=@('LICENSE.md','THIRD-PARTY-NOTICES.md','Licenses/JipperResourcePack-BSD-3-Clause.txt','Licenses/JipperResourcePack-source.json','Assets/GmarketSans-OFL.txt','Assets/NotoSansKR-OFL.txt','Assets/NotoSansKR-source.json')
$memory=$null;$manifest=$null;$archive=$null;$stream=$null
try {
    if([IO.Path]::GetExtension($packagePath) -eq '.exe') {
        $assembly=[Reflection.Assembly]::LoadFrom($packagePath)
        $stream=$assembly.GetManifestResourceStream('Payload.zip')
        if(!$stream){throw 'Installer payload is missing'}
        $memory=[IO.MemoryStream]::new();$stream.CopyTo($memory);$stream.Dispose();$stream=$null;$memory.Position=0
        $stream=$assembly.GetManifestResourceStream('Payload.json')
        if(!$stream){throw 'Installer manifest is missing'}
        $reader=[IO.StreamReader]::new($stream)
        try{$manifest=$reader.ReadToEnd()|ConvertFrom-Json}finally{$reader.Dispose();$stream=$null}
    } elseif([IO.Path]::GetExtension($packagePath) -eq '.zip') {
        $memory=[IO.File]::OpenRead($packagePath)
    } else {throw 'Expected an installer EXE or mod ZIP'}
    $archive=[IO.Compression.ZipArchive]::new($memory,[IO.Compression.ZipArchiveMode]::Read,$true)
    if(@($archive.Entries|Where-Object {$_.FullName -match '\.dll$' -and $_.FullName -ne 'GhostifyOverlay/GhostifyOverlay.dll'}).Count){throw 'Unexpected redistributed dependency DLL'}
    foreach($relative in $required){
        $entries=@($archive.Entries|Where-Object {$_.FullName -ceq ('GhostifyOverlay/'+$relative)})
        if($entries.Count -ne 1){throw ('Required distribution notice is missing or duplicated: '+$relative)}
        $sha=[Security.Cryptography.SHA256]::Create();$stream=$entries[0].Open()
        try{$actual=[BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','')}finally{$stream.Dispose();$stream=$null;$sha.Dispose()}
        $expected=(Get-FileHash -LiteralPath (Join-Path $Root $relative) -Algorithm SHA256).Hash
        if($actual -ne $expected){throw ('Distribution notice differs from source: '+$relative)}
        if($manifest){
            $declared=@($manifest.Files|Where-Object {$_.Path -ceq $relative})
            if($declared.Count -ne 1 -or $declared[0].Sha256 -ne $actual){throw ('Installer notice manifest mismatch: '+$relative)}
        }
        'PASS distribution notice '+$relative
    }
} finally {
    if($stream){$stream.Dispose()}
    if($archive){$archive.Dispose()}
    if($memory){$memory.Dispose()}
}
