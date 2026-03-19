FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:${PORT:-80}
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Używamy wildcarda (*), żeby Docker sam znalazł plik .csproj 
# niezależnie od tego, czy szuka w root, czy w podfolderze.
COPY **/RecipesAPI.csproj ./
RUN dotnet restore *.csproj

# Kopiujemy wszystko z folderu RecipesAPI do bieżącej lokalizacji
COPY RecipesAPI/ .
RUN dotnet build "RecipesAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RecipesAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecipesAPI.dll"]
