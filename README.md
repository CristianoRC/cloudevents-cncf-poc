# POC CloudEvents - Padronização de Eventos

## Objetivo

Demonstrar a viabilidade de adotar o padrão **CloudEvents** (CNCF) para padronizar a comunicação assíncrona entre serviços, independente da linguagem ou plataforma.


---

## O que é CloudEvents?

[CloudEvents](https://cloudevents.io/) é uma **especificação da CNCF** (Cloud Native Computing Foundation) que define um formato padrão para descrever eventos. É agnóstico a:

- **Linguagem** (C#, Node.js, Java, Go, Python, etc.)
- **Transporte** (HTTP, Kafka, AMQP, NATS, etc.)
- **Cloud Provider** (AWS, Azure, GCP, on-premises)

### Atributos obrigatórios de um CloudEvent

| Atributo        | Tipo      | Descrição                                         |
|-----------------|-----------|---------------------------------------------------|
| `specversion`   | String    | Versão da spec (atualmente `1.0`)                 |
| `id`            | String    | Identificador único do evento                     |
| `type`          | String    | Tipo do evento (ex: `com.example.order.created`)  |
| `source`        | URI-ref   | Origem do evento (quem produziu)                  |

### Atributos opcionais (mas recomendados)

| Atributo           | Tipo      | Descrição                              |
|--------------------|-----------|----------------------------------------|
| `time`             | Timestamp | Quando o evento ocorreu                |
| `datacontenttype`  | String    | MIME type do data (ex: `application/json`) |
| `data`             | Any       | Payload do evento                      |
| `subject`          | String    | Assunto do evento dentro do source     |

### Extension Attributes

CloudEvents permite **extensões customizadas** para necessidades específicas:

```json
{
  "partitionkey": "order-123",
  "traceparent": "00-abc123-def456-01"
}
```

---

## Arquitetura da POC

```
┌─────────────────────┐         ┌─────────────────────┐
│   Producer C#       │         │   Producer Node.js   │
│   (ASP.NET :5001)   │         │   (Express :3001)    │
│                     │         │                      │
│ order.created       │         │ user.registered      │
│ order.shipped       │         │ user.updated         │
└────────┬────────────┘         └────────┬─────────────┘
         │                               │
         │      CloudEvents HTTP         │
         │      (Structured Mode)        │
         ▼                               ▼
┌─────────────────────┐         ┌─────────────────────┐
│   Consumer C#       │         │   Consumer Node.js   │
│   (ASP.NET :5002)   │         │   (Express :3002)    │
│                     │         │                      │
│ Recebe QUALQUER     │         │ Recebe QUALQUER      │
│ CloudEvent          │         │ CloudEvent           │
└─────────────────────┘         └──────────────────────┘
```

**Ponto-chave**: Cada producer envia eventos para **ambos** os consumers, demonstrando que:
- Um evento produzido em **C#** é consumido corretamente por **Node.js** (e vice-versa)
- O formato é **auto-descritivo** - o consumer não precisa conhecer o producer previamente

---

## Exemplo de um CloudEvent (HTTP Structured Mode)

Quando o Producer C# envia um evento `order.created`, o HTTP request fica:

```http
POST /api/events HTTP/1.1
Content-Type: application/cloudevents+json

{
  "specversion": "1.0",
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "type": "com.example.order.created",
  "source": "/producer-dotnet/orders",
  "time": "2026-02-24T10:30:00Z",
  "datacontenttype": "application/json",
  "partitionkey": "order-xyz-123",
  "data": {
    "orderId": "order-xyz-123",
    "customerId": "customer-4521",
    "items": [
      { "productId": "PROD-001", "name": "Notebook Dell", "quantity": 1, "price": 4599.90 },
      { "productId": "PROD-002", "name": "Mouse Logitech", "quantity": 2, "price": 149.90 }
    ],
    "total": 4899.70,
    "currency": "BRL",
    "createdAt": "2026-02-24T10:30:00Z"
  }
}
```

---

## Como executar

### Pré-requisitos

- Docker e Docker Compose

### Subir os serviços

```bash
docker compose up --build
```

### Executar os testes

```bash
./test.sh
```

### Testar manualmente

```bash
# C# Producer - Criar pedido
curl -X POST http://localhost:5001/api/events/order-created | jq

# C# Producer - Enviar pedido
curl -X POST http://localhost:5001/api/events/order-shipped | jq

# Node.js Producer - Registrar usuário
curl -X POST http://localhost:3001/api/events/user-registered | jq

# Node.js Producer - Atualizar usuário
curl -X POST http://localhost:3001/api/events/user-updated | jq

# Envio em batch (5 eventos de uma vez)
curl -X POST http://localhost:5001/api/events/batch | jq
curl -X POST http://localhost:3001/api/events/batch | jq

# Consultar eventos recebidos
curl http://localhost:5002/api/events | jq
curl http://localhost:3002/api/events | jq
```

---

## Execução local (sem Docker)

```bash
# Terminal 1 - Consumer C#
cd src/consumer-dotnet
dotnet run --urls http://localhost:5002

# Terminal 2 - Consumer Node.js
cd src/consumer-node
npm start

# Terminal 3 - Producer C#
cd src/producer-dotnet
dotnet run --urls http://localhost:5001

# Terminal 4 - Producer Node.js
cd src/producer-node
npm start
```

---

## SDKs utilizados

| Linguagem | Pacote                              | Versão |
|-----------|-------------------------------------|--------|
| C#        | `CloudNative.CloudEvents`           | 2.8.0  |
| C#        | `CloudNative.CloudEvents.SystemTextJson` | 2.8.0 |
| C#        | `CloudNative.CloudEvents.AspNetCore`| 2.8.0  |
| Node.js   | `cloudevents`                       | 8.x    |

Ambos os SDKs são mantidos pela **CNCF** e seguem a spec CloudEvents v1.0.

---

## Convenção de tipos de evento proposta

```
com.<empresa>.<dominio>.<ação>
```

Exemplos:
- `com.example.order.created`
- `com.example.order.shipped`
- `com.example.user.registered`
- `com.example.user.updated`
- `com.example.payment.processed`
- `com.example.inventory.reserved`

---

