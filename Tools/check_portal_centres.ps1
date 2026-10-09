$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
Add-Type -Path 'C:/Program Files/Unity/Hub/Editor/6000.3.17f1/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll'
Add-Type -Path (Join-Path $taskRoot 'Artifacts/VisualRefactor/CompileCheck/Assembly-CSharp.dll')
foreach($taskType in @([SuyuRun.SierraCaveWorld],[SuyuRun.SelvaMiniWorld])) {
    foreach($taskLongIndex in @(0,1)) {
        $taskLongWorld=[Activator]::CreateInstance($taskType,@($taskLongIndex,500))
        $taskLongWorld.time=61
        for($taskLongStep=0;$taskLongStep -lt 600;$taskLongStep++) {
            $taskLongWorld.Step(1.0/60,0,$false,$false,$false,[UnityEngine.Vector2]::zero)
        }
        if($taskLongWorld.dead){throw "Combat still expires after music duration: $($taskType.Name) $taskLongIndex"}
        Write-Output "PASS $($taskType.Name) ${taskLongIndex}: combat remains available beyond 65 seconds"
    }
    foreach($taskIndex in @(0,1)) {
        foreach($taskAxis in @(-1,1)) {
            $taskWorld=[Activator]::CreateInstance($taskType,@($taskIndex,5))
            foreach($taskFoe in $taskWorld.foes){$taskFoe.hp=0}
            $taskWorld.time=61
            $taskCentre=if($taskType -eq [SuyuRun.SierraCaveWorld]){[SuyuRun.SierraCaveWorld]::PortalX}else{[SuyuRun.SelvaMiniWorld]::PortalX+24}
            $taskFloor=if($taskType -eq [SuyuRun.SierraCaveWorld]){[SuyuRun.SierraCaveWorld]::PortalFloor}else{[SuyuRun.SelvaMiniWorld]::PortalFloor}
            $taskWorld.pos=[UnityEngine.Vector2]::new($taskCentre-20,$taskFloor)
            $taskWorld.grounded=$true
            for($taskStep=0;$taskStep -lt 190 -and !$taskWorld.cleared;$taskStep++) {
                $taskWorld.Step(1.0/60,$taskAxis,$true,$true,$true,[UnityEngine.Vector2]::zero)
            }
            if(!$taskWorld.cleared -or [Math]::Abs($taskWorld.pos.x-$taskCentre) -gt .01){throw "Portal centring failed: $($taskType.Name) $taskIndex axis=$taskAxis"}
            Write-Output "PASS $($taskType.Name) ${taskIndex}: centred despite held movement/jump/fire ($taskAxis)"
        }
    }
}
