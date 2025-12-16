# ============================================
# Stage 1 : Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copier le fichier projet et restaurer les dépendances
COPY ["TravelBook.csproj", "./"]
RUN dotnet restore "TravelBook.csproj"

# Copier tout le code source et builder
COPY . .
RUN dotnet build "TravelBook.csproj" -c Release -o /app/build

# ============================================
# Stage 2 : Publish
# ============================================
FROM build AS publish
RUN dotnet publish "TravelBook.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 3 : Runtime (IMAGE FINALE LÉGÈRE)
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS final
WORKDIR /app

# Installer les certificats SSL pour HTTPS
RUN apk add --no-cache icu-libs

# Ajouter un user non admin
RUN addgroup -S appusergroup \
    && adduser -S appuser -G appusergroup
    
# Copier les fichiers publiés
COPY --from=publish /app/publish .

# Excécuter en tant que appuser (non admin, root)
USER appuser

# Configurer les variables d'environnement
ENV ASPNETCORE_URLS=https://+:8080;http://+:8081
ENV ASPNETCORE_HTTPS_PORTS=8080
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/cert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=VotreMotDePasseCert

# Exposer les ports
EXPOSE 8080 8081

# Point d'entrée
ENTRYPOINT ["dotnet", "TravelBook.dll"]
