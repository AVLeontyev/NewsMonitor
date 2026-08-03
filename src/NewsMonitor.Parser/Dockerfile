# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем csproj файлы
COPY ["src/NewsMonitor.Parser/NewsMonitor.Parser.csproj", "src/NewsMonitor.Parser/"]
COPY ["src/NewsMonitor.Shared/NewsMonitor.Shared.csproj", "src/NewsMonitor.Shared/"]
COPY ["src/NewsMonitor.Parser.Core/NewsMonitor.Parser.Core.csproj", "src/NewsMonitor.Parser.Core/"]

# Восстанавливаем зависимости
RUN dotnet restore "src/NewsMonitor.Parser/NewsMonitor.Parser.csproj"

# Копируем весь код
COPY . .

# Собираем проект
WORKDIR /src/src/NewsMonitor.Parser
RUN dotnet build "NewsMonitor.Parser.csproj" -c Release -o /app/build

# Публикуем
FROM build AS publish
RUN dotnet publish "NewsMonitor.Parser.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Копируем опубликованные файлы
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NewsMonitor.Parser.dll"]