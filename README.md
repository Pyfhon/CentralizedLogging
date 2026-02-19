# CentralizedLogging (.NET 8 + NLog + Seq)

Минимальное демо централизованного логирования с корреляцией:

- `Client.Console` → вызывает `ServiceA`
- `ServiceA` → вызывает `ServiceB`
- Все 3 приложения пишут:
  - локально в rolling file (`TRACE+`)
  - в Seq (`INFO+`) через `NLog.Targets.Seq` + `BufferingWrapper`

## Структура

- `docker-compose.yml` — локальный Seq
- `LoggingDemo.sln`
- `src/Client.Console`
- `src/ServiceA`
- `src/ServiceB`

## Корреляция

### Техническая (trace/span)

Используется `System.Diagnostics.Activity` (W3C).

В layout NLog присутствуют:

- `${activity:TraceId}`
- `${activity:SpanId}`

Для этого подключён пакет `NLog.DiagnosticSource`.

### Бизнес-корреляция (jobId)

- Client генерирует `jobId` (`Guid`) и отправляет в заголовке `X-Job-Id`
- ServiceA и ServiceB читают `X-Job-Id` и кладут в MDLC на время обработки:
  - `MappedDiagnosticsLogicalContext.SetScoped("jobId", jobId)`
- В логах поле выводится как `${mdlc:item=jobId}`

## Быстрый старт

> Требуется .NET SDK 8.0+ и Docker.

### 1) Запустить Seq

```bash
docker compose up -d
```

- UI: `http://localhost:8081`
- Ingestion: `http://localhost:5341`

### 2) Восстановить и собрать

```bash
dotnet restore LoggingDemo.sln
dotnet build LoggingDemo.sln
```

### 3) Запустить ServiceB

```bash
dotnet run --project src/ServiceB
```

Слушает: `http://localhost:5002`

### 4) Запустить ServiceA (в другом терминале)

```bash
dotnet run --project src/ServiceA
```

Слушает: `http://localhost:5001`

### 5) Запустить Client (в третьем терминале)

```bash
dotnet run --project src/Client.Console
```

## Что смотреть в Seq

Откройте `http://localhost:8081` и проверьте цепочку логов от трёх сервисов.

Полезные запросы (пример):

- По бизнес-корреляции:
  ```
  jobId = 'ВАШ_GUID'
  ```
- По трассировке:
  ```
  traceId = 'TRACE_ID_ИЗ_ЛОГА'
  ```
- Только демо-сервисы:
  ```
  service in ['Client.Console', 'ServiceA', 'ServiceB']
  ```

## Ожидаемый сценарий

Один запуск `Client.Console` создаёт цепочку логов в трёх приложениях:

- одинаковый `traceId` по HTTP-цепочке `Client -> ServiceA -> ServiceB`
- одинаковый `jobId` как бизнес-корреляция через `X-Job-Id`

## Логи на диске

Каждое приложение пишет в свой файл:

- `logs/Client.Console/{shortdate}.log`
- `logs/ServiceA/{shortdate}.log`
- `logs/ServiceB/{shortdate}.log`

Формат строки лога:

```text
${longdate}|${uppercase:${level}}|svc=${var:service}|trace=${activity:TraceId}|span=${activity:SpanId}|job=${mdlc:item=jobId}|${logger}|${message} ${exception:format=tostring}
```
