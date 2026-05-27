#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$ROOT_DIR/SmartPark"
MYSQL_HOST="${SMARTPARK_MYSQL_HOST:-127.0.0.1}"
MYSQL_PORT="${SMARTPARK_MYSQL_PORT:-3307}"
MYSQL_DB="${SMARTPARK_MYSQL_DB:-smartpark}"
MYSQL_USER="${SMARTPARK_MYSQL_USER:-smartpark}"
MYSQL_PASSWORD="${SMARTPARK_MYSQL_PASSWORD:-smartpark}"
MYSQL_INIT_SQL="$PROJECT_DIR/Sql/init-mysql-schema.sql"
MYSQL_SEED_SQL="$PROJECT_DIR/Sql/e2e-ui-flow-seed.mysql.sql"
MYSQL_VERIFY_SQL="$PROJECT_DIR/Sql/e2e-ui-flow-verify.mysql.sql"

printf '\n[1/4] Checking prerequisites...\n'
command -v mysql >/dev/null
[[ -f "$MYSQL_INIT_SQL" ]] && [[ -f "$MYSQL_SEED_SQL" ]] && [[ -f "$MYSQL_VERIFY_SQL" ]]

printf '[2/4] Using MySQL %s:%s, DB=%s ...\n' "$MYSQL_HOST" "$MYSQL_PORT" "$MYSQL_DB"

printf '[3/4] Creating schema if needed...\n'
MYSQL_PWD="$MYSQL_PASSWORD" mysql -h "$MYSQL_HOST" -P "$MYSQL_PORT" -u "$MYSQL_USER" "$MYSQL_DB" < "$MYSQL_INIT_SQL"

printf '[4/5] Seeding deterministic E2E data...\n'
MYSQL_PWD="$MYSQL_PASSWORD" mysql -h "$MYSQL_HOST" -P "$MYSQL_PORT" -u "$MYSQL_USER" "$MYSQL_DB" < "$MYSQL_SEED_SQL"

printf '[5/5] Running verification queries...\n\n'
MYSQL_PWD="$MYSQL_PASSWORD" mysql -t -h "$MYSQL_HOST" -P "$MYSQL_PORT" -u "$MYSQL_USER" "$MYSQL_DB" < "$MYSQL_VERIFY_SQL"

cat <<'MSG'

Desktop UI flow to complete now:
1. Open the SmartPark desktop app and go to Vehicle Entry.
2. Register vehicle number E2E-UI-ENTRY with owner UI Test Driver.
3. Go to Active Parking and record exit for E2E-UI-ENTRY.
4. Re-run this command to verify DB state and use the checks below.

DB checks after UI actions:
- Confirm entry exists:
  MYSQL_PWD="${SMARTPARK_MYSQL_PASSWORD:-smartpark}" mysql -t -h "${SMARTPARK_MYSQL_HOST:-127.0.0.1}" -P "${SMARTPARK_MYSQL_PORT:-3307}" -u "${SMARTPARK_MYSQL_USER:-smartpark}" "${SMARTPARK_MYSQL_DB:-smartpark}" \
    -e "SELECT VehicleNumber, OwnerName, EntryTime, ExitTime, IsCompleted FROM ParkingRecords WHERE VehicleNumber='E2E-UI-ENTRY' ORDER BY Id DESC LIMIT 1;"
- Confirm slot is released after exit:
  MYSQL_PWD="${SMARTPARK_MYSQL_PASSWORD:-smartpark}" mysql -t -h "${SMARTPARK_MYSQL_HOST:-127.0.0.1}" -P "${SMARTPARK_MYSQL_PORT:-3307}" -u "${SMARTPARK_MYSQL_USER:-smartpark}" "${SMARTPARK_MYSQL_DB:-smartpark}" \
    -e "SELECT s.SlotNumber, s.IsOccupied FROM ParkingSlots s JOIN ParkingRecords r ON r.ParkingSlotId=s.Id WHERE r.VehicleNumber='E2E-UI-ENTRY' ORDER BY r.Id DESC LIMIT 1;"

MSG
