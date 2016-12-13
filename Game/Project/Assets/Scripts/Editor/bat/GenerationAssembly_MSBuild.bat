set NET_FRAMEWORK_CSC=C:\Program Files (x86)\MSBuild\14.0\Bin\

set CS_FILE=F:\2018_Game\Game\Project\xk_Project.csproj

C:
cd %NET_FRAMEWORK_CSC%  
MSBuild.exe %CS_FILE% /t:Clean
MSBuild.exe %CS_FILE% /t:rebuild /p:Configuration=Release

set SourceFile=F:\2018_Game\Game\Project\Temp\UnityVS_bin\Release\xk_*.dll

XCOPY %SourceFile% F:\2018_Game\Game\Project\Assets\ResourceABs\scripts\test.bytes /r /h /c /e /y
::pause

