FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY LoggingDemo.sln ./
COPY src/ServiceA/ServiceA.csproj src/ServiceA/
RUN dotnet restore src/ServiceA/ServiceA.csproj

COPY . .
RUN dotnet publish src/ServiceA/ServiceA.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENTRYPOINT ["dotnet", "ServiceA.dll"]
