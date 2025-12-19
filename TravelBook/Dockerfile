# ================================
# Stage 1 : Build & Publish
# ================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copier le projet et restaurer
COPY TravelBook/TravelBook.csproj TravelBook/
RUN dotnet restore TravelBook/TravelBook.csproj

# Copier le reste du code
COPY TravelBook/ TravelBook/
COPY TravelBook.Client/ TravelBook.Client/
COPY TravelBookDto/ TravelBookDto/

# Publier l'application
WORKDIR /src/TravelBook
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ================================
# Stage 2 : Runtime
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble
WORKDIR /app

# Copier l'application publiée
COPY --from=build /app/publish .

# Copier le script d'entrée et rendre exécutable
COPY --chmod=755 TravelBook/StartApplication.sh ./StartApplication.sh
COPY --chmod=755 /run/secrets/travelbook_azure_tenant_id ./travelbook_azure_tenant_id
COPY --chmod=755 /run/secrets/travelbook_azure_client_id ./travelbook_azure_client_id
COPY --chmod=755 /run/secrets/travelbook_azure_client_secret ./travelbook_azure_client_secret
COPY --chmod=755 /run/secrets/travelbook_azure_keyvault_uri ./travelbook_azure_keyvault_uri

# Créer un utilisateur non-root
RUN useradd -m appuser
USER appuser

# Exposer les ports
EXPOSE 80
EXPOSE 443

# Lancer le script ENTRYPOINT
ENTRYPOINT ["/bin/sh", "-c", "./StartApplication.sh"]
