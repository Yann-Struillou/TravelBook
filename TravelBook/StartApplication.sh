#!/bin/sh

set -e

# Connexion avec Service Principal
#echo "Connexion � Azure avec Service Principal..."
#az login --service-principal \
#  -u ${AZURE_CLIENT_ID} \
# -p ${AZURE_CLIENT_SECRET} \
#  --tenant ${AZURE_TENANT_ID} > /dev/null 2>&1

# R�cup�rer le secret
#echo "R�cup�ration du secret depuis Key Vault..."
#export ENTRA_CLIENT_SECRET=$(az keyvault secret show \
#  --vault-name ${KEY_VAULT_NAME} \
#  --name ${KEY_VAULT_SECRET_NAME} \
#  --query value -o tsv)

#if [ -z "$ENTRA_CLIENT_SECRET" ]; then
#	echo "ERREUR: Impossible de r�cup�rer le secret depuis Key Vault"
#	exit 1
#fi

# Lancer l'application
#echo "Secret r�cup�r� avec succ�s, d�marrage de l application TravelBook ..."
exec dotnet TravelBook.dll