@echo off
setlocal
cd /d "%~dp0"

REM Lanzador. Toda la logica vive en make-tree-unity.ps1, en texto legible,
REM para poder editarla sin volver a codificar en Base64.
REM
REM   make-tree-unity.bat                 tree.txt + Repo_Tree_Index.md
REM   make-tree-unity.bat -IncludeAssets  anade .asset / .prefab / .unity
REM   make-tree-unity.bat -TreeOnly       solo tree.txt

if not exist "%~dp0make-tree-unity.ps1" (
  echo No se encuentra make-tree-unity.ps1 junto a este .bat.
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0make-tree-unity.ps1" %*
set "ERR=%ERRORLEVEL%"

if not "%ERR%"=="0" (
  echo Fallo al generar el arbol.
  pause
  exit /b %ERR%
)

echo.
echo Listo:
echo   "%~dp0tree.txt"              arbol completo, para leer
echo   "%~dp0Repo_Tree_Index.md"    indice curado, para el Project Knowledge
pause
