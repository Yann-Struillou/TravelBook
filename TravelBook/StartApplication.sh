#!/bin/sh
set -e

# Vérifier que les fichiers secrets existent
for secret_file in \
    "/run/secrets/travelbook_azure_client_id" \
    "/run/secrets/travelbook_azure_client_secret" \
    "/run/secrets/travelbook_azure_tenant_id" \
    "/run/secrets/travelbook_azure_keyvault_uri"; do
  [ -f "$secret_file" ] || { echo "Secret file $secret_file missing"; exit 1; }
done

# Exporter les valeurs en variables d'environnement
export AZURE_CLIENT_ID=$(cat "$AZURE_CLIENT_ID_FILE")
export AZURE_CLIENT_SECRET=$(cat "$AZURE_CLIENT_SECRET_FILE")
export AZURE_TENANT_ID=$(cat "$AZURE_TENANT_ID_FILE")
export AZURE_KEYVAULT_URI=$(cat "$AZURE_KEYVAULT_URI_FILE")

# Lancer l'application
exec dotnet TravelBook.dll
