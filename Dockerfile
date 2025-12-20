# ================================
# Stage 1 : Build & Publish
# ================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build

WORKDIR /app
WORKDIR /src

# Exposer les ports
EXPOSE 80
EXPOSE 443

# Copier le projet et restaurer
COPY ["TravelBook/TravelBook.csproj", "TravelBook/"]
COPY ["TravelBook.Client/TravelBook.Client.csproj", "TravelBook.Client/"]
RUN dotnet restore "TravelBook/TravelBook.csproj"
COPY ./ ./
WORKDIR "/src/TravelBook"
RUN dotnet build "TravelBook.csproj" -c Release -o /app/build

# Publier l'application
FROM build AS publish
RUN dotnet publish "TravelBook.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ================================
# Stage 2 : Runtime
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble as final
WORKDIR /app

# Installer Azure CLI
RUN apt-get update && \
    apt-get install -y curl && \
    curl -sL https://aka.ms/InstallAzureCLIDeb | bash && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copier l'application publiée
COPY --from=publish /app/publish .

# Copier le script d'entrée et rendre exécutable
COPY --chmod=755 TravelBook/StartApplication.sh ./StartApplication.sh

# Créer un utilisateur non-root
RUN useradd -m appuser
USER appuser

# Lancer le script ENTRYPOINT
ENTRYPOINT ["/bin/sh", "-c", "./StartApplication.sh"]