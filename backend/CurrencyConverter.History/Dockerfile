FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CurrencyConverter.History/CurrencyConverter.History.csproj", "CurrencyConverter.History/"]
RUN dotnet restore "CurrencyConverter.History/CurrencyConverter.History.csproj"
COPY . .
WORKDIR "/src/CurrencyConverter.History"
RUN dotnet build "./CurrencyConverter.History.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CurrencyConverter.History.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CurrencyConverter.History.dll"]
