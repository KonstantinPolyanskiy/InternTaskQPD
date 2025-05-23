# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# copy csproj + restore
COPY QPDCar.Api/QPDCar.Api.csproj QPDCar.Api/
COPY QPDCar.Infrastructure/QPDCar.Infrastructure.csproj QPDCar.Infrastructure/
COPY QPDCar.Models/QPDCar.Models.csproj QPDCar.Models/
COPY QPDCar.ServiceInterfaces/QPDCar.ServiceInterfaces.csproj QPDCar.ServiceInterfaces/
COPY QPDCar.Jobs/QPDCar.Jobs.csproj QPDCar.Jobs/
COPY QPDCar.Repositories/QPDCar.Repositories.csproj QPDCar.Repositories/
COPY QPDCar.Services/QPDCar.Services.csproj QPDCar.Services/
COPY QPDCar.UseCases/QPDCar.UseCases.csproj QPDCar.UseCases/
RUN dotnet restore QPDCar.Api/QPDCar.Api.csproj

# copy sources + publish
COPY . .
WORKDIR /src/QPDCar.Api
RUN dotnet publish QPDCar.Api.csproj -c $BUILD_CONFIGURATION \
    -o /app/publish --no-restore --p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QPDCar.Api.dll"]
