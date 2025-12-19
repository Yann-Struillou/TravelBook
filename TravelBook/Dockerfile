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

# Secrets
RUN echo "$AZURE_TENANT_ID" > ./travelbook_azure_tenant_id
RUN echo "$AZURE_CLIENT_ID" > ./travelbook_azure_client_id
RUN echo "$AZURE_CLIENT_SECRET" > ./travelbook_azure_client_secret
RUN echo "$AZURE_KEYVAULT_URI" > ./travelbook_azure_keyvault_uri

# Créer un utilisateur non-root
RUN useradd -m appuser
USER appuser

# Exposer les ports
EXPOSE 80
EXPOSE 443

# Lancer le script ENTRYPOINT
ENTRYPOINT ["/bin/sh", "-c", "./StartApplication.sh"]
