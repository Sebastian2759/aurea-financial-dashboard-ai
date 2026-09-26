#!/usr/bin/env sh
set -eu
cd "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
command -v docker >/dev/null 2>&1 || { echo 'Instala Docker con Compose v2 y vuelve a ejecutar sh start.sh.' >&2; exit 1; }
docker info >/dev/null
docker compose version >/dev/null
if [ ! -f .env ]; then
    umask 077
    secret() { od -An -N32 -tx1 /dev/urandom | tr -d ' \n'; }
    {
      echo 'DASHBOARD_PORT=8080'
      printf 'SQL_PASSWORD=Aa1!%s\n' "$(secret)"
      printf 'JWT_SECRET=%s\n' "$(secret)"
      printf 'API_KEY=%s\n' "$(secret)"
      printf 'COINGECKO_API_KEY=%s\n' "${COINGECKO_API_KEY:-}"
    } > .env
    echo 'Configuración demo generada en .env (excluida de Git).'
fi
docker compose up --build --detach --wait --wait-timeout 300
port=$(sed -n 's/^DASHBOARD_PORT=//p' .env | head -n 1)
echo "Dashboard listo: http://localhost:${port:-8080}"
