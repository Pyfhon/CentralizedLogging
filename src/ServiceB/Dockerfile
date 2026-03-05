FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY LoggingDemo.sln ./
COPY src/ServiceB/ServiceB.csproj src/ServiceB/
RUN dotnet restore src/ServiceB/ServiceB.csproj

COPY . .
RUN dotnet publish src/ServiceB/ServiceB.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5002
ENTRYPOINT ["dotnet", "ServiceB.dll"]
