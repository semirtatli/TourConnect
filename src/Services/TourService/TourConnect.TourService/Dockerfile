FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/TourService/TourConnect.TourService/TourConnect.TourService.csproj", "src/Services/TourService/TourConnect.TourService/"]
COPY ["src/Services/TourService/TourConnect.TourService.Domain/TourConnect.TourService.Domain.csproj", "src/Services/TourService/TourConnect.TourService.Domain/"]
COPY ["src/Services/TourService/TourConnect.TourService.Application/TourConnect.TourService.Application.csproj", "src/Services/TourService/TourConnect.TourService.Application/"]
COPY ["src/Services/TourService/TourConnect.TourService.Infrastructure/TourConnect.TourService.Infrastructure.csproj", "src/Services/TourService/TourConnect.TourService.Infrastructure/"]
COPY ["src/BuildingBlocks/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus.Messages/"]

RUN dotnet restore "src/Services/TourService/TourConnect.TourService/TourConnect.TourService.csproj"

COPY . .
WORKDIR "/src/src/Services/TourService/TourConnect.TourService"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TourConnect.TourService.dll"]
