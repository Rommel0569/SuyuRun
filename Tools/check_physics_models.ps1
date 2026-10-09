$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
Add-Type -Path 'C:/Program Files/Unity/Hub/Editor/6000.3.17f1/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll'
Add-Type -Path (Join-Path $taskRoot 'Artifacts/VisualRefactor/CompileCheck/Assembly-CSharp.dll')
$taskReport=[System.Collections.Generic.List[string]]::new()
foreach($taskType in @([SuyuRun.SierraCaveWorld],[SuyuRun.SelvaMiniWorld])) {
    foreach($taskIndex in @(0,1)) {
        foreach($taskFps in @(30,60,120)) {
            $taskWorld=[Activator]::CreateInstance($taskType,@($taskIndex,5))
            for($taskStep=0;$taskStep -lt 120*$taskFps -and !$taskWorld.dead -and !$taskWorld.cleared;$taskStep++) {
                [single]$taskAxis=0
                [bool]$taskJump=$false
                [bool]$taskFire=$false
                $taskAim=[UnityEngine.Vector2]::zero
                $taskWorld.AutoControl([ref]$taskAxis,[ref]$taskJump,[ref]$taskFire,[ref]$taskAim)
                $taskWorld.Step(1.0/$taskFps,$taskAxis,$taskJump,$false,$taskFire,$taskAim)
            }
            $taskReport.Add(('{0} {1} {2}fps cleared={3} dead={4} time={5:F2} hp={6} boss={7} minions={8} x={9:F1} y={10:F1} trace={11}' -f $taskType.Name,$taskIndex,$taskFps,$taskWorld.cleared,$taskWorld.dead,$taskWorld.time,$taskWorld.health,$taskWorld.Boss.hp,$taskWorld.RemainingMinions,$taskWorld.pos.x,$taskWorld.pos.y,$taskWorld.damageTrace))
            if(!$taskWorld.cleared -or $taskWorld.dead){$taskReport | Set-Content -LiteralPath (Join-Path $taskRoot 'Artifacts/VisualRefactor/physics_models.txt');throw ('Model failed: '+$taskType.Name+' '+$taskIndex+' '+$taskFps+'fps')}
        }
    }
}
$taskReport.Add('Pure compiled combat-model simulation; not a Unity visual playthrough or music test.')
$taskReport | Set-Content -LiteralPath (Join-Path $taskRoot 'Artifacts/VisualRefactor/physics_models.txt')
$taskReport | Write-Output
