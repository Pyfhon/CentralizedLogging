# CentralizedLogging (.NET 8 + NLog + Seq)

Минимальное демо централизованного логирования с корреляцией запросов:

- `Client.Console` → `ServiceA` → `ServiceB`
- логи во всех приложениях:
  - локально в файл (`TRACE+`)
  - централизованно в Seq (`INFO+`)

## Требования

- Docker Desktop (Linux containers) запущен, иначе `docker compose` не сможет подключиться к `//./pipe/dockerDesktopLinuxEngine`.
- .NET SDK 8.0+ для локального запуска без Docker.
## Запуск одной командой через Docker Compose

Поднять Seq + ServiceA + ServiceB:

Для `datalust/seq:latest` в демо отключена авторизация на первом запуске (`SEQ_FIRSTRUN_NOAUTHENTICATION=True`), чтобы контейнер стартовал без ручной инициализации.

```bash
docker compose up -d --build seq servicea serviceb
```

Выполнить клиент как one-shot (отдельный profile):

```bash
docker compose --profile client run --rm client
```

## Куда смотреть

- Seq UI: `http://localhost:8081`
- Локальные логи (bind mount): `./logs/...`
  - `./logs/ServiceA/{shortdate}.log`
  - `./logs/ServiceB/{shortdate}.log`
  - `./logs/Client/{shortdate}.log`

## Что проверять в Seq

После запуска one-shot клиента должна появиться единая цепочка логов:

- общий `traceId` по HTTP цепочке `Client -> ServiceA -> ServiceB`
- общий `jobId` (из заголовка `X-Job-Id`) во всех трёх приложениях

Примеры фильтров в Seq:

```text
traceId = '...'
```

```text
jobId = '...'
```

```text
service in ['Client', 'ServiceA', 'ServiceB']
```

## Локальный запуск без Docker (опционально)

```bash
dotnet restore LoggingDemo.sln
dotnet build LoggingDemo.sln
dotnet run --project src/ServiceB
dotnet run --project src/ServiceA
dotnet run --project src/Client.Console
```

По умолчанию локальные URL:

- Client → `http://localhost:5001`
- ServiceA → `http://localhost:5002`

В Docker URL подставляются из переменных окружения (`SERVICEA_URL`, `SERVICEB_URL`, `SEQ_URL`, `SERVICE_NAME`).




