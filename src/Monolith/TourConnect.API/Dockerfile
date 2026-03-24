FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Monolith/TourConnect.API/TourConnect.API.csproj", "src/Monolith/TourConnect.API/"]
COPY ["src/Monolith/TourConnect.Application/TourConnect.Application.csproj", "src/Monolith/TourConnect.Application/"]
COPY ["src/Monolith/TourConnect.Infrastructure/TourConnect.Infrastructure.csproj", "src/Monolith/TourConnect.Infrastructure/"]
COPY ["src/Monolith/TourConnect.Domain/TourConnect.Domain.csproj", "src/Monolith/TourConnect.Domain/"]

RUN dotnet restore "src/Monolith/TourConnect.API/TourConnect.API.csproj"

COPY . .
WORKDIR "/src/src/Monolith/TourConnect.API"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TourConnect.API.dll"]
