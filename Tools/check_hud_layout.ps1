$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
Add-Type -Path 'C:/Program Files/Unity/Hub/Editor/6000.3.17f1/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll'
Add-Type -Path 'C:/Program Files/Unity/Hub/Editor/6000.3.17f1/Editor/Data/Managed/UnityEngine/UnityEngine.IMGUIModule.dll'
Add-Type -Path (Join-Path $taskRoot 'Artifacts/VisualRefactor/CompileCheck/Assembly-CSharp.dll')
$taskReport=[System.Collections.Generic.List[string]]::new()
foreach($taskRegion in @('costa','sierra','selva')) {
foreach($taskSize in @(@(640,360),@(960,540),@(1280,720),@(1920,1080),@(2560,1440),@(1366,768),@(3440,1440),@(1024,768))) {
    $taskViewport=[SuyuRun.RunnerHud]::Viewport($taskSize[0],$taskSize[1])
    $taskScale=$taskViewport.width/320
    $taskControls=[SuyuRun.RunnerHud]::ControlsFor($taskRegion)
    $taskFooter=[SuyuRun.RunnerHud]::get_Footer()
    $taskMessage=[SuyuRun.RunnerHud]::get_Message()
    if($taskControls.xMin -lt $taskFooter.xMin -or $taskControls.yMin -lt $taskFooter.yMin -or $taskControls.xMax -gt $taskFooter.xMax -or $taskControls.yMax -gt $taskFooter.yMax){throw 'Controls outside footer'}
    if($taskControls.yMin -lt $taskMessage.yMax){throw 'Controls overlap message'}
    if([Math]::Abs($taskViewport.width/$taskViewport.height-16.0/9)>0.001){throw 'Distorted aspect'}
    if($taskViewport.x -lt -0.01 -or $taskViewport.y -lt -0.01 -or $taskViewport.xMax -gt $taskSize[0]+0.01 -or $taskViewport.yMax -gt $taskSize[1]+0.01){throw 'Viewport outside screen'}
    for($taskIndex=0;$taskIndex -lt 4;$taskIndex++) {
        $taskSlot=[SuyuRun.RunnerHud]::PieceSlotFor($taskRegion,$taskIndex)
        $taskIcon=[UnityEngine.Rect]::new($taskSlot.center.x-5,$taskSlot.center.y-5,10,10)
        $taskMessage=[SuyuRun.RunnerHud]::get_Message()
        if($taskControls.xMin -lt $taskSlot.xMax -and $taskControls.xMax -gt $taskSlot.xMin -and $taskControls.yMin -lt $taskSlot.yMax -and $taskControls.yMax -gt $taskSlot.yMin){throw 'Controls overlap collectible'}
        if($taskSlot.xMin -lt $taskMessage.xMax -and $taskSlot.xMax -gt $taskMessage.xMin -and $taskSlot.yMin -lt $taskMessage.yMax -and $taskSlot.yMax -gt $taskMessage.yMin){throw 'Message overlaps collectible'}
        if($taskIcon.xMin -lt $taskSlot.xMin -or $taskIcon.yMin -lt $taskSlot.yMin -or $taskIcon.xMax -gt $taskSlot.xMax -or $taskIcon.yMax -gt $taskSlot.yMax){throw 'Icon outside frame'}
        if($taskSlot.yMax -gt 180 -or $taskSlot.xMax -gt 320){throw 'Slot outside gameplay'}
    }
    $taskReport.Add(('PASS {0}x{1}: viewport {2:F1},{3:F1} {4:F1}x{5:F1}; HUD inside gameplay; no slot/message overlap' -f $taskSize[0],$taskSize[1],$taskViewport.x,$taskViewport.y,$taskViewport.width,$taskViewport.height))
}
}
$taskReport.Add('Geometry test, not screenshot approval. Non-16:9 screens preserve aspect with bands.')
$taskReport | Set-Content -LiteralPath (Join-Path $taskRoot 'Artifacts/VisualRefactor/hud_geometry_models.txt')
$taskReport | Write-Output
