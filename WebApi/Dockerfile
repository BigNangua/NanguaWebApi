# 使用 .NET 6 运行时镜像作为基础镜像
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 使用 .NET 6 SDK 镜像进行构建
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 复制项目文件并还原依赖项
COPY ["WebApi.csproj", "."]
RUN dotnet restore "./WebApi.csproj"

# 复制所有项目文件并进行构建和发布
COPY . .
RUN dotnet build "./WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "./WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 使用基础镜像运行应用程序
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
