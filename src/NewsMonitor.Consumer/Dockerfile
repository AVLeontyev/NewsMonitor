# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем csproj файлы
COPY ["src/NewsMonitor.Consumer/NewsMonitor.Consumer.csproj", "src/NewsMonitor.Consumer/"]
COPY ["src/NewsMonitor.Shared/NewsMonitor.Shared.csproj", "src/NewsMonitor.Shared/"]

# Восстанавливаем зависимости
RUN dotnet restore "src/NewsMonitor.Consumer/NewsMonitor.Consumer.csproj"

# Копируем весь код
COPY . .

# Собираем проект
WORKDIR /src/src/NewsMonitor.Consumer
RUN dotnet build "NewsMonitor.Consumer.csproj" -c Release -o /app/build

# Публикуем
FROM build AS publish
RUN dotnet publish "NewsMonitor.Consumer.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "NewsMonitor.Consumer.dll"]