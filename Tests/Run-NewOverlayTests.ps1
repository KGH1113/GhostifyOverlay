param([string]$GameDir='C:\Program Files (x86)\Steam\steamapps\common\A Dance of Fire and Ice')
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot;$managed=Join-Path $GameDir 'A Dance of Fire and Ice_Data/Managed'
foreach($folder in @($managed,(Join-Path $managed 'UnityModManager'))){Get-ChildItem $folder -Filter *.dll|ForEach-Object{try{[Reflection.Assembly]::LoadFrom($_.FullName)|Out-Null}catch{}}}
$mod=[Reflection.Assembly]::LoadFrom((Join-Path $project 'bin/Release/GhostifyOverlay/GhostifyOverlay.dll'));$flags=[Reflection.BindingFlags]'Public,NonPublic,Static,Instance';$passed=0
function T($n){$mod.GetType('DonQuixoteOverlay.'+$n,$true)}
function Check($v,$m){if(!$v){throw ('FAIL '+$m)};$script:passed++;'PASS '+$m}
function SetField($t,$n,$v){$t.GetField($n,$flags).SetValue($null,$v)}
$settings=[Activator]::CreateInstance((T 'DonQuixoteSettings'));SetField (T 'Main') 'Settings' $settings
$settings.Overlay.BestProgress['level:test-a']=[float]0.52
$preserved=(T 'DonQuixoteSettings').GetMethod('CreateDefaultPreservingRecords').Invoke($null,@($settings))
Check ([Math]::Abs($preserved.Overlay.BestProgress['level:test-a'] - 0.52) -lt .0001) 'resetting appearance preserves best progress records'
$text=[Activator]::CreateInstance((T 'CustomOverlayText'));$text.Text='<size=999>literal</size>'; $text.Size=[float]::NaN;$text.X=[float]::PositiveInfinity;$settings.Overlay.CustomTexts.Add($text)
@((T 'SettingsNormalization').GetMethods($flags)|Where-Object {$_.Name -eq 'Normalize' -and $_.GetParameters()[0].ParameterType.Name -eq 'DonQuixoteSettings'})[0].Invoke($null,@($settings))|Out-Null
Check ($text.Size -eq 28 -and $text.X -eq 960 -and $text.Text -eq '<size=999>literal</size>') 'custom text bounds repair invalid sizes without interpreting markup'
$settings.Overlay.CustomTexts.Add([Activator]::CreateInstance((T 'CustomOverlayText')));$settings.Overlay.CustomTexts[1].Text='second';$settings.Overlay.CustomTexts[1].X=77
$work=Join-Path $project ('Tests/new-overlay-fixture-'+[Guid]::NewGuid().ToString('N'));New-Item -ItemType Directory $work|Out-Null
(T 'SettingsStore').GetMethod('Initialize').Invoke($null,[object[]]@($work.PSObject.BaseObject))|Out-Null;(T 'SettingsStore').GetMethod('Save').Invoke($null,@($settings))|Out-Null
$loaded=(T 'SettingsStore').GetMethod('Load').Invoke($null,@())
Check ($loaded.Overlay.CustomTexts.Count -eq 2 -and $loaded.Overlay.CustomTexts[1].X -eq 77 -and $loaded.Overlay.BestProgress['level:test-a'] -eq [float]0.52) 'custom entries and per-map best survive disk save and reload'
$defaults=Get-Content (Join-Path $PSScriptRoot 'Data/FactoryDefaults.json') -Raw -Encoding UTF8|ConvertFrom-Json
$layout=[Activator]::CreateInstance((T 'LayoutData'));$keys=[Activator]::CreateInstance(($mod.GetType('DonQuixoteOverlay.KeyViewerContents.KeyViewerSetting')))
Check ($layout.ComboY -eq $defaults.Layout.ComboY -and [Math]::Abs($layout.TopLeftScale-$defaults.Layout.TopLeftScale) -lt .00001) 'layout defaults match supplied ZIP'
Check ([int]$keys.KeyViewerStyle -eq 1 -and [int]$keys.FootKeyViewerStyle -eq 5 -and $keys.key16Text[5] -eq 'Num') 'default hand, foot and aliases match supplied ZIP'
Check ($settings.Overlay.FullAttempts.Count -eq 0 -and $defaults.Overlay.PSObject.Properties.Name -notcontains 'FullAttempts') 'private map paths and accumulated records are excluded from factory settings'
function CompareDefaults($actual,$expected,$path) {
    foreach($property in $expected.PSObject.Properties) {
        $name=$property.Name;if($name -eq 'SchemaVersion'){continue};$value=$actual.GetType().GetField($name).GetValue($actual);$saved=$property.Value
        if($saved -is [pscustomobject]){CompareDefaults $value $saved ($path+'.'+$name);continue}
        if($saved -is [array]){Check ($value.Length -eq $saved.Length) ('default array length '+$path+'.'+$name);for($i=0;$i -lt $saved.Length;$i++){if($null -eq $saved[$i]){Check ($null -eq $value[$i]) ('default empty alias '+$path+'.'+$name+'/'+$i)}elseif($saved[$i] -is [string]){Check ($value[$i] -ceq $saved[$i]) ('default alias '+$path+'.'+$name+'/'+$i)}else{Check ([int]$value[$i] -eq [int]$saved[$i]) ('default key '+$path+'.'+$name+'/'+$i)}};continue}
        if($saved -is [string] -and $saved.StartsWith('#')){$formatted=$mod.GetType('DonQuixoteOverlay.KeyViewerContents.KeyViewerColorConverter').GetMethod('Format',$flags).Invoke($null,@($value));Check ($formatted -ceq $saved) ('default color '+$path+'.'+$name)}
        elseif($saved -is [bool]){Check ($value -eq $saved) ('default option '+$path+'.'+$name)}
        elseif($saved -is [string]){Check ($value -ceq $saved) ('default text '+$path+'.'+$name)}
        else{Check ([Math]::Abs([double]$value-[double]$saved) -lt .00001) ('default numeric setting '+$path+'.'+$name)}
    }
}
CompareDefaults ([Activator]::CreateInstance((T 'OverlaySettings'))) $defaults.Overlay 'Overlay'
CompareDefaults $layout $defaults.Layout 'Layout'
CompareDefaults $keys $defaults.KeyViewer 'KeyViewer'
'New overlay assertions passed: '+$passed
