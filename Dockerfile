# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/InvestmentPortfolio.API/InvestmentPortfolio.API.csproj", "src/InvestmentPortfolio.API/"]
COPY ["src/InvestmentPortfolio.Core/InvestmentPortfolio.Core.csproj", "src/InvestmentPortfolio.Core/"]
COPY ["src/InvestmentPortfolio.Infrastructure/InvestmentPortfolio.Infrastructure.csproj", "src/InvestmentPortfolio.Infrastructure/"]
COPY ["src/InvestmentPortfolio.Shared/InvestmentPortfolio.Shared.csproj", "src/InvestmentPortfolio.Shared/"]
RUN dotnet restore "src/InvestmentPortfolio.API/InvestmentPortfolio.API.csproj"

# Copy source and publish
COPY . .
WORKDIR "/src/src/InvestmentPortfolio.API"
RUN dotnet build "InvestmentPortfolio.API.csproj" -c Release -o /app/build
RUN dotnet publish "InvestmentPortfolio.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvestmentPortfolio.API.dll"]
