# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MKIL.DotnetTest.OrderService/MKIL.DotnetTest.OrderService.Api/MKIL.DotnetTest.OrderService.Api.csproj", "src/MKIL.DotnetTest.OrderService/MKIL.DotnetTest.OrderService.Api/"]
COPY ["MKIL.DotnetTest.Shared.Lib/MKIL.DotnetTest.Shared.Lib.csproj", "src/MKIL.DotnetTest.Shared.Lib/"]
COPY ["MKIL.DotnetTest.UserService/MKIL.DotnetTest.UserService.Domain/MKIL.DotnetTest.UserService.Domain.csproj", "src/MKIL.DotnetTest.UserService/MKIL.DotnetTest.UserService.Domain/"]
COPY ["MKIL.DotnetTest.OrderService/MKIL.DotnetTest.OrderService.Domain/MKIL.DotnetTest.OrderService.Domain.csproj", "src/MKIL.DotnetTest.OrderService/MKIL.DotnetTest.OrderService.Domain/"]
COPY ["MKIL.DotnetTest.OrderService/MKIL.DotnetTest.OrderService.Infrastructure/MKIL.DotnetTest.OrderService.Infrastructure.csproj", "src/MKIL.DotnetTest.OrderService/MKIL.DotnetTest.OrderService.Infrastructure/"]
RUN dotnet restore "./src/MKIL.DotnetTest.OrderService/MKIL.DotnetTest.OrderService.Api/MKIL.DotnetTest.OrderService.Api.csproj"
COPY . .
WORKDIR "/src/MKIL.DotnetTest.OrderService/MKIL.DotnetTest.OrderService.Api"
RUN dotnet build "./MKIL.DotnetTest.OrderService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MKIL.DotnetTest.OrderService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MKIL.DotnetTest.OrderService.Api.dll"]