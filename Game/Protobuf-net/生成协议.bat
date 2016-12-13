set CURRENT_DIR=%cd%
set CURRENT_DISK=%~d0 
set NET_FRAMEWORK_CSC="%windir%\Microsoft.NET\Framework\v3.5\
set PROTO_FILE=%CURRENT_DIR%\Proto\
set CS_FILE=%CURRENT_DIR%\Proto_out
set PROTOBUF_DLL=%CURRENT_DIR%\protobuf-net\ProtoGen\protobuf-net.dll
set PROTOBUF_OUT_DLL=%CURRENT_DIR%\Proto_out\protobuf-net.dll
set CS_DLL=%CURRENT_DIR%\Proto_out\GameProtocolsData.dll
set SERIALIZER_DLL=%CURRENT_DIR%\Proto_out\GameProtocols.dll
set GEN_TOOL=%CURRENT_DIR%\protobuf-net\ProtoGen\protogen.exe
set PRECOMPIE_TOOL=%CURRENT_DIR%\protobuf-net\Precompile\precompile.exe 

for /r %PROTO_FILE% %%i in (*.proto) do (
%GEN_TOOL% -i:%%i -o:%CS_FILE%/%%~ni.cs -ns:game.protobuf.data

)

%SystemDrive%
cd %NET_FRAMEWORK_CSC%  
csc.exe /t:library /r:%PROTOBUF_DLL%  /out:%CS_DLL% %CS_FILE%\*.cs 

%CURRENT_DISK% 
XCOPY  %PROTOBUF_DLL% %PROTOBUF_OUT_DLL%  /r /h /c /e /y
::生成序列化和反序列化文件
%PRECOMPIE_TOOL%   %CS_DLL% -o:%SERIALIZER_DLL% -t:game.protobuf.data.GameProtocols
pause