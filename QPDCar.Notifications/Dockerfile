# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY QPDCar.Notifications/QPDCar.Notifications.csproj QPDCar.Notifications/
COPY QPDCar.Infrastructure/QPDCar.Infrastructure.csproj QPDCar.Infrastructure/
COPY QPDCar.Models/QPDCar.Models.csproj QPDCar.Models/
COPY QPDCar.ServiceInterfaces/QPDCar.ServiceInterfaces.csproj QPDCar.ServiceInterfaces/
RUN dotnet restore QPDCar.Notifications/QPDCar.Notifications.csproj

COPY . .
WORKDIR /src/QPDCar.Notifications
RUN dotnet publish QPDCar.Notifications.csproj -c $BUILD_CONFIGURATION \
    -o /app/publish --no-restore --p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime  
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QPDCar.Notifications.dll"]
