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

REM SharpShell.dll�����݂��Ȃ��ꍇ
:error1
echo SharpShell.dll�����݂��܂���
goto end

REM �ԍ������������͂���Ȃ������Ƃ�
:error2
echo ���������͂��Ă�������
goto end

REM RegAsm.exe�����݂��Ȃ��ꍇ
:error3
echo RegAsm.exe�����݂��܂���
goto end


:end
pause
