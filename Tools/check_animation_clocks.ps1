$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
Add-Type -Path 'C:/Program Files/Unity/Hub/Editor/6000.3.17f1/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll'
Add-Type -Path (Join-Path $taskRoot 'Artifacts/VisualRefactor/CompileCheck/Assembly-CSharp.dll')
$taskLines=[System.Collections.Generic.List[string]]::new()
foreach($taskFps in @(30,60,120)) {
    $taskPrevious=0
    $taskVisited=[System.Collections.Generic.HashSet[int]]::new()
    for($taskFrame=0;$taskFrame -lt $taskFps;$taskFrame++) {
        $taskSeconds=$taskFrame/[float]$taskFps
        $taskAge=[SuyuRun.GameScale]::GaitClock($taskSeconds*7*32)
        $taskIndex=[int][Math]::Floor($taskAge*24)
        if($taskIndex-$taskPrevious -gt 1){throw 'Run animation skips poses'}
        [void]$taskVisited.Add($taskIndex%12)
        $taskPrevious=$taskIndex
    }
    if($taskVisited.Count -ne 12){throw 'Run cycle omits poses'}
    $taskLines.Add("PASS ${taskFps}fps: all 12 poses, no skipped gait pose at Costa speed")
}
$taskClock=[SuyuRun.RunnerPoseClock]::new()
if($taskClock.Age('jump',10) -ne 0){throw 'First pose did not start'}
if([Math]::Abs($taskClock.Age('jump',10.2)-.2) -gt .0001){throw 'Pose age unstable'}
[void]$taskClock.Age('run',11)
if($taskClock.Age('jump',12) -ne 0){throw 'Repeated jump reused old clock'}
if($taskClock.Age('jump',1) -ne 0){throw 'Checkpoint rewind reused future clock'}
$taskLines.Add('PASS state re-entry and checkpoint rewind restart one-shot poses')
$taskAir=[SuyuRun.RunnerAirClock]::new()
[void]$taskAir.Age($true,0,0,0)
if($taskAir.Age($false,1,-200,.1) -ne 0){throw 'Takeoff clock did not start'}
if([Math]::Abs($taskAir.Age($false,1,-100,.3)-.2) -gt .0001){throw 'Air clock restarted during ascent'}
if($taskAir.Age($false,2,-200,.35) -ne 0){throw 'Double jump did not restart impulse'}
if($taskAir.Age($false,2,1,.7) -ne 0){throw 'Descent clock did not start'}
if([Math]::Abs($taskAir.Age($false,2,100,.9)-.2) -gt .0001){throw 'Descent clock stalled'}
$taskLines.Add('PASS shared flight clock: takeoff, continuous ascent, double impulse and full descent')
$taskLines.Add('Timing/selection test, not artistic approval or full Unity playthrough.')
$taskLines | Set-Content -LiteralPath (Join-Path $taskRoot 'Artifacts/VisualRefactor/animation_clock_audit.txt')
$taskLines | Write-Output
