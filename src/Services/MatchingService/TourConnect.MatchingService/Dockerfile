FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/MatchingService/TourConnect.MatchingService/TourConnect.MatchingService.csproj", "src/Services/MatchingService/TourConnect.MatchingService/"]
COPY ["src/Services/MatchingService/TourConnect.MatchingService.Domain/TourConnect.MatchingService.Domain.csproj", "src/Services/MatchingService/TourConnect.MatchingService.Domain/"]
COPY ["src/Services/MatchingService/TourConnect.MatchingService.Application/TourConnect.MatchingService.Application.csproj", "src/Services/MatchingService/TourConnect.MatchingService.Application/"]
COPY ["src/Services/MatchingService/TourConnect.MatchingService.Infrastructure/TourConnect.MatchingService.Infrastructure.csproj", "src/Services/MatchingService/TourConnect.MatchingService.Infrastructure/"]
COPY ["src/BuildingBlocks/EventBus.Messages/EventBus.Messages.csproj", "src/BuildingBlocks/EventBus.Messages/"]

RUN dotnet restore "src/Services/MatchingService/TourConnect.MatchingService/TourConnect.MatchingService.csproj"

COPY . .
WORKDIR "/src/src/Services/MatchingService/TourConnect.MatchingService"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TourConnect.MatchingService.dll"]
