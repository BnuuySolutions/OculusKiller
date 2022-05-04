:: By KptnKMan
:: This script will install or remove OculusKiller, simply run to install and run to remove.
:: Place this script in the SAME FOLDER with the modified OculusDash.exe file, and run script.
:: Any dir will work, I put mine in a folder in My Documents. This script will create a backup of OculusDash.exe.
:: ALWAYS run this script "AS ADMINISTRATOR", or you will get "Access Denied" error.
:: You should not need to modify anything, if you installed Oculus Home to a different folder, then update OCULUSDIR.

@echo off
:: Modify this "OCULUSDIR" if you installed to a different folder.
set OCULUSDIR=C:\Program Files\Oculus\Support\oculus-dash\dash\bin

echo.
echo *********************************************
echo *** Checking if OculusKiller is installed ***
echo *********************************************
echo.
if exist "%OCULUSDIR%\OculusDash.exe.bak" goto ALREADYON

net stop OVRService

echo F|xcopy "%OCULUSDIR%\OculusDash.exe" "%~dp0\OculusDash.exe.bak" /f /i /y
echo F|xcopy "%OCULUSDIR%\OculusDash.exe" "%OCULUSDIR%\OculusDash.exe.bak" /f /i /y
del "%OCULUSDIR%\OculusDash.exe" /f
echo F|xcopy "%~dp0\OculusDash.exe" "%OCULUSDIR%\OculusDash.exe" /h /f /i /y

echo.
echo *****************************************
echo *** OculusKiller mod is now INSTALLED ***
echo *****************************************
echo.
echo NOTE: A copy of OculusDash.exe is left in OculusKiller dir, as backup.
goto EXIT

:ALREADYON
echo.
echo *************************************************************
echo *** OculusKiller is already setup - REMOVING OculusKiller ***
echo *************************************************************
echo.

del "%OCULUSDIR%\OculusDash.exe" /f
echo F|xcopy "%OCULUSDIR%\OculusDash.exe.bak" "%OCULUSDIR%\OculusDash.exe" /f /i /y
del "%OCULUSDIR%\OculusDash.exe.bak" /f

net start OVRService

echo.
echo ***************************************
echo *** OculusKiller mod is now REMOVED ***
echo ***************************************
echo.
echo NOTE: In case of error, a backup copy of OculusDash.exe is in the OculusKiller dir.
goto EXIT

:EXIT
set OCULUSDIR=
pause

exit
