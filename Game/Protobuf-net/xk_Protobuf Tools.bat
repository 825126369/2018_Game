set PROTO_FILE=F:\2018_Config\Config\xk_protobuf
set CS_FILE=F:\2018_Game\Game\Protobuf-net\xk_protobuf_out
set CS_DLL=F:\2018_Game\Game\Protobuf-net\xk_protobuf_out\xk_protobuf_data.dll
set PROTOBUF_DLL=F:\2018_Game\Game\Protobuf-net\xk_protobuf\xk_Protobuf.dll
set NET_FRAMEWORK_CSC="%windir%\Microsoft.NET\Framework\v3.5\
set protoc=F:\2018_Game\Game\Protobuf-net\xk_protobuf\protoc.exe


cd %PROTO_FILE%
for /r %PROTO_FILE% %%i in (*.proto) do (
%protoc% --csharp_out=%CS_FILE% --proto_path=%PROTO_FILE% %%i 	

)

%SystemDrive%
cd %NET_FRAMEWORK_CSC%  
csc.exe /t:library /r:%PROTOBUF_DLL% /out:%CS_DLL% %CS_FILE%\*.cs 

pause