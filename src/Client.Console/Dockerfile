FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY LoggingDemo.sln ./
COPY src/Client.Console/Client.Console.csproj src/Client.Console/
RUN dotnet restore src/Client.Console/Client.Console.csproj

COPY . .
RUN dotnet publish src/Client.Console/Client.Console.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Client.Console.dll"]
