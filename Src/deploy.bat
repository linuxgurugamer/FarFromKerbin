

set H=R:\KSP_1.2.2_dev
echo %H%
copy bin\Debug\FarFromKerbin.dll ..\GameData\FarFromKerbin\Plugins

xcopy /E /Y ..\GameData\FarFromKerbin %H%\GameData\FarFromKerbin
