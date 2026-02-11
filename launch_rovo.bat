@echo off
:: Force UTF-8 Code Page
chcp 65001 > nul
set PYTHONUTF8=1
set PYTHONIOENCODING=utf-8:surrogateescape

echo Starting Rovo Dev with UTF-8 Multi-Guard...
.\acli.exe rovodev run
pause
