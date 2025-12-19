#!/bin/sh
set -e

# Vérification que les secrets existent
for i in $(seq 1 30); do
  [ -f /run/secrets/travelbook_azure_client_secret ] && break
  echo "Waiting for secret..."
  sleep 1
done

# Exporter les secrets en variables d'environnement
export AZURE_TENANT_ID=$(cat /run/secrets/travelbook_azure_tenant_id)
export AZURE_CLIENT_ID=$(cat /run/secrets/travelbook_azure_client_id)
export AZURE_CLIENT_SECRET=$(cat /run/secrets/travelbook_azure_client_secret)
export KEYVAULT_URI=$(cat /run/secrets/travelbook_keyvault_uri)

# Démarrer l'application Blazor
exec dotnet TravelBook.dll
