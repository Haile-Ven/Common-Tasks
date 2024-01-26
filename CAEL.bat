@echo off

REM Stop Windows Error Reporting service
net stop WerSvc

REM Clear Event Logs
for /F "tokens=*" %%G in ('wevtutil.exe el') DO (
    call :do_clear "%%G"
)

REM Clear Problem Reports
del /f /S /Q /A "%ProgramData%\Microsoft\Windows\WER\ReportQueue\*"
del /f /S /Q /A "%ProgramData%\Microsoft\Windows\WER\ReportArchive\*"

REM Start Windows Error Reporting service
net start WerSvc

echo.
echo All Event Logs and Problem Reports have been cleared!
goto theEnd

:do_clear
echo Clearing %1
wevtutil.exe cl %1
goto :eof

:theEnd
Exit
