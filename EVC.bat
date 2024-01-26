@echo off

REM Display Event Log Count
setlocal enabledelayedexpansion

set count=0

for /F "tokens=*" %%G in ('wevtutil.exe el') DO (
    set /a count+=1
)

echo %count%

endlocal
goto theEnd

:theEnd
Exit
