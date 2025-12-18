# ============================================
# Stage 1 : Build & Publish
# ============================================
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

# ============================================
# Stage 2 : Runtime (image finale)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble
WORKDIR /app

# Préparer le lanceur de démarrage
COPY --chmod=755 TravelBook/StartApplication.sh ./StartApplication.sh

# Créer un utilisateur non-root
RUN useradd -m appuser
USER appuser

# Copier l'application publiée
COPY --from=build /app/publish .

# Ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["/bin/sh", "-c", "./StartApplication.sh"]
