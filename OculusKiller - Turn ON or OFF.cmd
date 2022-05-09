:: By KptnKMan
:: This script will install or remove OculusKiller, simply run to install and run to remove.
:: Place this script in the SAME FOLDER with the modified OculusDash.exe file, and run script.
:: Any dir will work, I put mine in a folder in My Documents. This script will create a backup of OculusDash.exe.
:: ALWAYS run this script "AS ADMINISTRATOR", or you will get "Access Denied" error.
:: You should not need to modify anything, if you installed Oculus Home to a different folder, then update OCULUSDIR.

@echo off
:: Modify this "CUSTOMOCULUSBASE" if you installed to a different folder, and have not set OCULUSBASE.
set CUSTOMOCULUSBASE=C:\Program Files\Oculus

echo *********************************************************************
echo ******** NOTICE: This script requires a OCULUSBASE Variable *********
echo *** If OCULUSBASE System Variable is not set, one can be set here ***
echo *********************************************************************

:: Check if OCULUSBASE Var is defined
if "%OCULUSBASE%" NEQ "" (
    echo OCULUSBASE is set to %OCULUSBASE%, continuing...
    set OCULUSBASE="%OCULUSBASE%"
    goto START
) else (
    goto OCFIX
)

:OCFIX
echo *********************************
echo *** OCULUSBASE is NOT defined ***
echo *********************************
echo You should really set an OCULUSBASE Variable.

:: Choice if OCULUSBASE Var is NOT defined
set /P FIXRESULT=Set your OCULUSBASE Variable now to %CUSTOMOCULUSBASE%?(Y/N)?
if /i "%FIXRESULT%"=="y" goto FIXYES
if /i "%FIXRESULT%"=="n" goto SETVAR
echo Please enter only Y/y or N/n!
goto EXIT

:SETVAR
:: Choice to set a custom variable
set /P FIXRESULT2=Would you like to set a custom OCULUSBASE Variable now?(Y/N)?
if /i "%FIXRESULT2%"=="y" goto SETVARYES
if /i "%FIXRESULT2%"=="n" goto FIXNO

:: Set a custom OCULUSBASE path right here
:SETVARYES
set /P CUSTOMOCULUSBASE=Please enter your OCULUSBASE Path:?
echo You entered '%CUSTOMOCULUSBASE%' (Press any Key to continue or CTRL-C to exit)

pause
goto FIXYES

:: If not to fix, warn and exit
:FIXNO
echo If you have a custom path to your Oculus dir, you can set it in this script and rerun.
echo You can also open this script and set the CUSTOMOCULUSBASE Variable at the top to whatever you use.
echo EXITING...
goto EXIT

:: If to fix, set OCULUSBASE
:FIXYES
set OCULUSBASE="%CUSTOMOCULUSBASE%"

set /P FIXRESULT3=Would you like to set '%CUSTOMOCULUSBASE%' as a System Variable?(Y/N)?
if /i "%FIXRESULT3%"=="y" setx /M OCULUSBASE "%CUSTOMOCULUSBASE%"
if /i "%FIXRESULT3%"=="n" echo Not setting System Variable, continuing...

:START
set OCULUSDIR="%OCULUSBASE%\Support\oculus-dash\dash\bin"

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
