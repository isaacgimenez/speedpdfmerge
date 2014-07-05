@echo off

rem sample usage: SpeedPdfMerge.exe -SOURCE "file1.pdf" "file2.pdf" -DEST "newfile.pdf"

"%CD%\..\bin\Debug\SpeedPdfMerge.exe" -SOURCE "%CD%\file1.pdf" "%CD%\file2.pdf" -DEST "%CD%\output.pdf"

pause