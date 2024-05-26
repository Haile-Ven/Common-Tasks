namespace Common_Tasks
{
    internal class BatContenet
    {
        public const string EventClearBatchContent = @"
@echo off

REM Function to check if a service is running
sc query WerSvc | findstr /i ""RUNNING""
if %errorlevel% equ 0 (
    set ""werSvcWasRunning=true""
    REM Stop Windows Error Reporting service
    net stop WerSvc
) else (
    set ""werSvcWasRunning=false""
)

REM Clear Event Logs
for /F ""tokens=*"" %%G in ('wevtutil.exe el') DO (
    call :do_clear ""%%G""
)

REM Clear Problem Reports
del /f /S /Q /A ""%ProgramData%\Microsoft\Windows\WER\ReportQueue\*""
del /f /S /Q /A ""%ProgramData%\Microsoft\Windows\WER\ReportArchive\*""

REM Start Windows Error Reporting service if it was running initially
if ""%werSvcWasRunning%""==""true"" (
    net start WerSvc
)

echo.
echo All Event Logs and Problem Reports Have Been Cleared!
goto theEnd

:do_clear
echo Clearing %1

REM Attempt to clear the log and catch errors
wevtutil.exe cl %1 2>> ""%TEMP%\clear_logs_errors.txt""
if %errorlevel% neq 0 (
    echo Failed to clear log %1: Access is denied or log does not exist. >> ""%TEMP%\clear_logs_errors.txt""
)
goto :eof

:theEnd
Exit

";

        public const string EventCountBatchContent = @"
@echo off

REM Display Event Log Count
setlocal enabledelayedexpansion

set count=0

for /F ""tokens=*"" %%G in ('wevtutil.exe el') DO (
    set /a count+=1
)

echo %count%

endlocal
goto theEnd

:theEnd
Exit
";
    }
}
