@ECHO OFF
SETLOCAL

SET vswherePath=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
IF NOT EXIST "%vswherePath%" GOTO :ERROR

FOR /F "tokens=*" %%i IN (	'
	  "%vswherePath%" -latest -prerelease -products * ^
        -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 ^
        -property installationPath'
      ) DO SET vsBase=%%i

IF "%vsBase%"=="" GOTO :ERROR

CALL "%vsBase%\vc\Auxiliary\Build\vcvars64.bat" > NUL

for /f "delims=" %%i in ("%cd%") do set folder=%%~ni
msbuild %folder%.sln /p:Configuration=Debug /p:Platform="Any CPU" /p:GenerateDocumentationFile=true

EXIT /B 0

ENDLOCAL

:ERROR
    EXIT /B 1
