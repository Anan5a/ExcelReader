# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Must be commented for latest SQL Server
# Fixes TLS handshake issue because the DB server does not support modern TLS >= 1.2
COPY ["ExcelReader/docker-replace/openssl.cnf", "/etc/ssl/"]

# This stage is used to build the .NET service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExcelReader/ExcelReader.csproj", "ExcelReader/"]
COPY ["BLL/BLL.csproj", "BLL/"]
COPY ["DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Utility/Utility.csproj", "Utility/"]

# Restore .NET dependencies
RUN dotnet restore "./ExcelReader/ExcelReader.csproj"
COPY . .

# Build .NET project
WORKDIR "/src/ExcelReader"
RUN dotnet build "./ExcelReader.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:IsDockerBuild=true

# -------------------
# Add new stage to build Angular (UI)
# -------------------
FROM node:18 AS ui-build
WORKDIR /app
COPY ["ExcelReader/WebUI/ExcelReader/package.json", "ExcelReader/WebUI/ExcelReader/package-lock.json", "ExcelReader/WebUI/"]
# Install dependencies for Angular
WORKDIR /app/ExcelReader/WebUI
RUN npm install
# Build the Angular application
COPY ["ExcelReader/WebUI/ExcelReader", "."]
RUN npm run build:dotnet
RUN ls 

# -------------------
# Publish .NET service project
# -------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExcelReader.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:IsDockerBuild=true

# -------------------
# Copy the Angular build output to the final image
# -------------------
FROM base AS final
WORKDIR /app
# Copy the .NET published output
RUN mkdir -p /app/wwwroot/frontend
COPY --from=publish /app/publish .
# Copy Angular build output into wwwroot (or other desired directory)
COPY --from=ui-build /app/ExcelReader/WebUI/dist/dotnet/** /app/wwwroot/frontend

ENTRYPOINT ["dotnet", "ExcelReader.dll"]
