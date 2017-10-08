
@echo off
set H=R:\KSP_1.3.1_dev
echo %H%
cd

copy /Y "AugmentedReality\bin\Debug\AugmentedReality.dll" "GameData\AugmentedReality\Plugins"
copy /Y AugmentedReality.version GameData\AugmentedReality

cd GameData
xcopy /y /s /i AugmentedReality "%H%\GameData\AugmentedReality"

