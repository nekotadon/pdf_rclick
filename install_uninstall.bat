@echo off
CD /D %~dp0

set file=pdf_rclick.dll

if not exist SharpShell.dll goto error1
if not exist C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe goto error3
echo 1:install
echo 2:uninstall

set num=
set /p num=input=

IF "%num%"=="1" goto num1
IF "%num%"=="2" goto num2
goto error2

REM install
:num1
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /codebase %file%
goto end

REM uninstall
:num2
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /u %file%
goto end

REM SharpShell.dllが存在しない場合
:error1
echo SharpShell.dllが存在しません
goto end

REM 番号が正しく入力されなかったとき
:error2
echo 正しく入力してください
goto end

REM RegAsm.exeが存在しない場合
:error3
echo RegAsm.exeが存在しません
goto end


:end
pause
