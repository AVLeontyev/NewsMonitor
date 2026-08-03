# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем csproj файлы
COPY ["src/NewsMonitor.API/NewsMonitor.API.csproj", "src/NewsMonitor.API/"]
COPY ["src/NewsMonitor.Shared/NewsMonitor.Shared.csproj", "src/NewsMonitor.Shared/"]
COPY ["src/NewsMonitor.Parser.Core/NewsMonitor.Parser.Core.csproj", "src/NewsMonitor.Parser.Core/"]

# Восстанавливаем зависимости
RUN dotnet restore "src/NewsMonitor.API/NewsMonitor.API.csproj"

# Копируем весь код
COPY . .

# Собираем проект
WORKDIR /src/src/NewsMonitor.API
RUN dotnet build "NewsMonitor.API.csproj" -c Release -o /app/build

# Публикуем
FROM build AS publish
RUN dotnet publish "NewsMonitor.API.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Копируем опубликованные файлы
COPY --from=publish /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "NewsMonitor.API.dll"]