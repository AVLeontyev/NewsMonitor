# News Monitor

Приложение для автоматического мониторинга новостей с real-time уведомлениями. Парсит RSS-ленты по заданным темам, сохраняет новости в БД, распределяет через очередь сообщений и мгновенно оповещает клиентов через WebSocket.

## Возможности

- **Автоматический парсинг** RSS-лент каждые 10 минут через Hangfire
- **Асинхронная обработка** через RabbitMQ + MassTransit (паттерн Producer/Consumer)
- **Real-time уведомления** клиентам через SignalR WebSockets
- **REST API** с документированием Swagger
- **Централизованное логирование** в Elasticsearch + визуализация в Kibana
- **Полная контейнеризация** всех сервисов через Docker Compose
- **Веб-интерфейс** на Vue.js 3 с SignalR-клиентом (опционально)

## Архитектура
```
Parser (Hangfire) → RabbitMQ (MassTransit) → Consumer → PostgreSQL
↕
Vue.js SPA ← SignalR Hub ← API (REST + Swagger) ←─────────────┘
↕
Elasticsearch + Kibana (логирование)
```

## Структура проекта
```
news monitor/
├── docker-compose.yml # Оркестрация 8 сервисов
├── docker/postgres/init.sql # Инициализация схемы БД
├── src/
│ ├── NewsMonitor.API/ # REST API + SignalR Hub
│ │ ├── Controllers/ # NewsController, TopicsController
│ │ ├── Hubs/NewsHub.cs # SignalR хаб
│ │ ├── Program.cs # DI, CORS, Middleware
│ │ └── Dockerfile
│ ├── NewsMonitor.Parser/ # Hangfire-воркер + MassTransit продюсер
│ │ ├── Program.cs
│ │ └── Dockerfile
│ ├── NewsMonitor.Parser.Core/ # Бизнес-логика парсинга RSS
│ │ └── Services/NewsParserService.cs
│ ├── NewsMonitor.Consumer/ # MassTransit консьюмер
│ │ ├── Consumers/NewsCreatedConsumer.cs
│ │ ├── Program.cs
│ │ └── Dockerfile
│ ├── NewsMonitor.Shared/ # EF Core контекст, модели
│ │ ├── Data/ApplicationDbContext.cs
│ │ └── Models/ # News, Topic
│ └── NewsMonitor.Shared.Messages/ # Контракты MassTransit
│ └── NewsCreatedMessage.cs
└── frontend/ # Vue.js 3 SPA (опционально)
├── src/
│ ├── stores/ # Pinia: signalr, news, topics
│ ├── views/ # HomeView, NewsView, TopicsView
│ └── api/ # Axios клиенты
└── package.json
```

## Технологический стек

| Слой | Технологии |
|---|---|
| Бекенд | ASP.NET Core 10, EF Core 10, SignalR 10 |
| БД | PostgreSQL 15 |
| Очередь | RabbitMQ 3.13 + MassTransit |
| Фоновые задачи | Hangfire 1.8 |
| Логирование | Serilog → Elasticsearch 8.11 + Kibana 8.11 |
| Контейнеризация | Docker + Docker Compose |
| Фронтенд | Vue.js 3, Pinia 2, Axios, SignalR Client |

## Требования

- Docker Desktop
- Git

Ни .NET SDK, ни Node.js, ни PostgreSQL на хост-машине не требуются.

## Быстрый старт

# 1. Клонировать репозиторий
git clone <URL>
cd "news monitor"

# 2. Запустить все сервисы
docker-compose up -d

# 3. Проверить состояние
docker ps
При первом запуске Docker автоматически скачает образы, соберёт .NET-приложения, создаст БД и таблицы.

##  Запуск фронтенда
```
powershell
cd frontend
npm install
npm run dev
```

##  Доступ к сервисам
```
Swagger	http://localhost:5269/swagger
Hangfire	http://localhost:5269/hangfire
RabbitMQ UI	http://localhost:15672
Kibana	http://localhost:5601
pgAdmin	http://localhost:5050
Vue Frontend	http://localhost:5173
```

##  Мониторинг и отладка
```
docker logs newsmonitor-api         # Логи API
docker logs newsmonitor-parser      # Логи парсера
docker logs newsmonitor-consumer    # Логи консьюмера
```

Логи также доступны в Kibana (индексы newsmonitor-api-*, newsmonitor-parser-*, newsmonitor-consumer-*).

## Примечания
```
Проект предназначен для демонстрации и локальной разработки
Для production необходимо вынести пароли в .env, включить HTTPS, настроить авторизацию
Парсер запускается при старте и далее каждые 10 минут через Hangfire
Таблицы создаются автоматически при первом запуске через init.sql
```
