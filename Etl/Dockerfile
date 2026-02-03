FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Etl/RentWisePro.Etl.csproj Etl/
COPY Etl.Core/RentWisePro.Etl.Core.csproj Etl.Core/
COPY Etl.Persistence/RentWisePro.Etl.Persistence.csproj Etl.Persistence/
COPY Etl.Sources/RentWisePro.Etl.Sources.csproj Etl.Sources/

RUN dotnet restore "Etl/RentWisePro.Etl.csproj"

COPY Etl/ Etl/
COPY Etl.Core/ Etl.Core/
COPY Etl.Persistence/ Etl.Persistence/
COPY Etl.Sources/ Etl.Sources/

RUN dotnet publish "Etl/RentWisePro.Etl.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish/ ./
ENTRYPOINT ["dotnet", "RentWisePro.Etl.dll"]
