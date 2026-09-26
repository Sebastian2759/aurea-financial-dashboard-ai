#!/usr/bin/env sh
set -eu
cd "$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
if [ ! -f .venv/bin/python ]; then
    python3 -m venv .venv
    .venv/bin/python -m pip install -r tools/requirements.txt
fi
export PYTHONHASHSEED=0 PYTHONIOENCODING=utf-8
exec .venv/bin/python tools/context.py "$@"
