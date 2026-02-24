# Getting Started

## Como executar

### Pré-requisitos

- Docker e Docker Compose

### Subir os serviços

```bash
docker compose up --build
```

### Testar manualmente

```bash
# Criar pedido
curl -X POST http://localhost:5001/api/events/order-created | jq

# Enviar pedido
curl -X POST http://localhost:5001/api/events/order-shipped | jq

# Consultar eventos recebidos em cada consumer
curl http://localhost:5002/api/events | jq   # Consumer C#
curl http://localhost:3002/api/events | jq   # Consumer Node.js
curl http://localhost:8000/api/events | jq   # Consumer Python

# Limpar eventos
curl -X DELETE http://localhost:5002/api/events
curl -X DELETE http://localhost:3002/api/events
curl -X DELETE http://localhost:8000/api/events
```

### Swagger UI

- Producer C#: http://localhost:5001/swagger
- Consumer C#: http://localhost:5002/swagger

---

## Execução local (sem Docker)

```bash
# Terminal 1 - Consumer C#
cd src/consumer-dotnet
dotnet run --urls http://localhost:5002

# Terminal 2 - Consumer Node.js
cd src/consumer-node
npm start

# Terminal 3 - Consumer Python
cd src/consumer-python
pip install -r requirements.txt
python app.py

# Terminal 4 - Producer C#
cd src/producer-dotnet
dotnet run --urls http://localhost:5001
```

---

## SDKs utilizados

| Linguagem | Pacote                                     | Versão |
|-----------|--------------------------------------------|--------|
| C#        | `CloudNative.CloudEvents`                  | 2.8.0  |
| C#        | `CloudNative.CloudEvents.SystemTextJson`   | 2.8.0  |
| C#        | `CloudNative.CloudEvents.AspNetCore`       | 2.8.0  |
| C#        | `Swashbuckle.AspNetCore`                   | 10.1.4 |
| Node.js   | `cloudevents`                              | 8.x    |
| Python    | `cloudevents`                              | 1.11.0 |
| Python    | `flask`                                    | 3.1.0  |

Todos os SDKs de CloudEvents são mantidos pela **CNCF** e seguem a spec CloudEvents v1.0.

---

## Estrutura do projeto

```
poc-cloudevents/
├── docker-compose.yml
├── test.sh
├── README.md
├── docs/
│   └── GETTING-STARTED.md
└── src/
    ├── producer-dotnet/          # ASP.NET - Produz CloudEvents
    │   ├── Controllers/
    │   ├── Models/
    │   └── Services/
    ├── consumer-dotnet/          # ASP.NET - Consome CloudEvents
    │   ├── Controllers/
    │   ├── Models/
    │   └── Services/
    ├── consumer-node/            # Express - Consome CloudEvents
    └── consumer-python/          # Flask - Consome CloudEvents
```
