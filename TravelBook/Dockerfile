# ============================================
# Stage 1 : Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copier le projet et restaurer
COPY TravelBook/TravelBook.csproj TravelBook/
RUN dotnet restore TravelBook/TravelBook.csproj

# Copier le reste du code
COPY TravelBook/ TravelBook/

# Publier l'application
WORKDIR /src/TravelBook
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 2 : Runtime (IMAGE FINALE)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble
WORKDIR /app

# Ajouter un user non admin
RUN addgroup -S appusergroup && adduser -S appuser -G appusergroup
    
# Copier les fichiers publiés
COPY --from=publish /app/publish .

# Excécuter en tant que appuser (non admin, root)
USER appuser

# Exposer les ports
EXPOSE 8080

# Point d'entrée
ENTRYPOINT ["dotnet", "TravelBook.dll"]
