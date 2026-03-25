FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Gateway/TourConnect.Gateway/TourConnect.Gateway.csproj", "src/Gateway/TourConnect.Gateway/"]

RUN dotnet restore "src/Gateway/TourConnect.Gateway/TourConnect.Gateway.csproj"

COPY . .
WORKDIR "/src/src/Gateway/TourConnect.Gateway"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TourConnect.Gateway.dll"]
