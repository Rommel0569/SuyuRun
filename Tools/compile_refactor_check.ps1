$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$taskOutput=Join-Path $taskRoot 'Artifacts/VisualRefactor/CompileCheck'
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskCompiler='C:/Program Files/Unity/Hub/Editor/6000.3.17f1/Editor/Data/DotNetSdkRoslyn/csc.dll'
$taskRuntime='C:/Program Files/Unity/Hub/Editor/6000.3.17f1/Editor/Data/NetCoreRuntime/dotnet.exe'
Push-Location $taskRoot
try {
    foreach($taskName in @('Assembly-CSharp','Assembly-CSharp-Editor')) {
        $taskResponse=Get-Content ('Library/Bee/artifacts/1900b0aE.dag/'+$taskName+'.rsp')
        $taskResponse=@($taskResponse | Where-Object {$_ -notmatch '^-(out|refout):'})
        if($taskName -eq 'Assembly-CSharp-Editor') {
            $taskResponse=@($taskResponse | ForEach-Object {$_ -replace 'Library/Bee/artifacts/1900b0aE.dag/Assembly-CSharp.ref.dll','Artifacts/VisualRefactor/CompileCheck/Assembly-CSharp.dll'})
        }
        $taskFolder=if($taskName -eq 'Assembly-CSharp'){'Scripts'}else{'Editor'}
        foreach($taskFile in Get-ChildItem ('Assets/SuyuRun/'+$taskFolder) -Filter '*.cs') {
            $taskRelative='"Assets/SuyuRun/'+$taskFolder+'/'+$taskFile.Name+'"'
            if($taskResponse -notcontains $taskRelative){$taskResponse+=$taskRelative}
        }
        $taskResponse+='-out:"Artifacts/VisualRefactor/CompileCheck/'+$taskName+'.dll"'
        $taskRsp=Join-Path $taskOutput ($taskName+'.rsp')
        $taskResponse | Set-Content -LiteralPath $taskRsp
        $taskResult=& $taskRuntime $taskCompiler ('@'+$taskRsp) 2>&1
        $taskResult | Set-Content -LiteralPath (Join-Path $taskOutput ($taskName+'.log'))
        $taskResult | Write-Output
        if($LASTEXITCODE -ne 0){throw ('Compilation failed: '+$taskName)}
    }
    'PASS runtime and Editor C# compile with Unity references; not a Player build.' | Set-Content -LiteralPath (Join-Path $taskOutput 'result.txt')
} finally {Pop-Location}
