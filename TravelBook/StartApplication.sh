#!/bin/sh
set -e

# Vérifier que les fichiers secrets existent
for secret_file in \
    "$AZURE_CLIENT_ID_FILE" \
    "$AZURE_CLIENT_SECRET_FILE" \
    "$AZURE_TENANT_ID_FILE" \
    "$KEYVAULT_URI_FILE"; do
  [ -f "$secret_file" ] || { echo "Secret file $secret_file missing"; exit 1; }
done

# Exporter les valeurs en variables d'environnement
export AZURE_CLIENT_ID=$(cat "$AZURE_CLIENT_ID_FILE")
export AZURE_CLIENT_SECRET=$(cat "$AZURE_CLIENT_SECRET_FILE")
export AZURE_TENANT_ID=$(cat "$AZURE_TENANT_ID_FILE")
export KEYVAULT_URI=$(cat "$KEYVAULT_URI_FILE")

# Lancer l'application
exec dotnet TravelBook.dll
