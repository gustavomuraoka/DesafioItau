FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CompraAutomatizada.API/CompraAutomatizada.API.csproj", "src/CompraAutomatizada.API/"]
COPY ["src/CompraAutomatizada.Application/CompraAutomatizada.Application.csproj", "src/CompraAutomatizada.Application/"]
COPY ["src/CompraAutomatizada.Infrastructure/CompraAutomatizada.Infrastructure.csproj", "src/CompraAutomatizada.Infrastructure/"]
COPY ["src/CompraAutomatizada.Domain/CompraAutomatizada.Domain.csproj", "src/CompraAutomatizada.Domain/"]
RUN dotnet restore "src/CompraAutomatizada.API/CompraAutomatizada.API.csproj"
COPY . .
RUN dotnet publish "src/CompraAutomatizada.API/CompraAutomatizada.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CompraAutomatizada.API.dll"]
