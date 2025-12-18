#!/bin/sh
set -e
export AZURE_TENANT_ID=$(cat /run/secrets/travelbook_azure_tenant_id)
export AZURE_CLIENT_ID=$(cat /run/secrets/travelbook_azure_client_id)
export AZURE_CLIENT_SECRET=$(cat /run/secrets/travelbook_azure_client_secret)
export KEYVAULT_URI=$(cat /run/secrets/travelbook_keyvault_uri)
exec dotnet TravelBook.dll
