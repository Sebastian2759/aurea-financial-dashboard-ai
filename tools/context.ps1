param([Parameter(ValueFromRemainingArguments=$true)][string[]]$ContextArguments)
$ErrorActionPreference='Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
if (-not (Test-Path '.venv/Scripts/python.exe')) {
    python -m venv .venv
    if ($LASTEXITCODE -ne 0) { throw 'Se necesita Python 3.12 para las herramientas de contexto.' }
    & '.venv/Scripts/python.exe' -m pip install -r tools/requirements.txt
    if ($LASTEXITCODE -ne 0) { throw 'No se pudieron instalar las herramientas de contexto.' }
}
$env:PYTHONHASHSEED='0'
$env:PYTHONIOENCODING='utf-8'
& '.venv/Scripts/python.exe' tools/context.py @ContextArguments
exit $LASTEXITCODE
