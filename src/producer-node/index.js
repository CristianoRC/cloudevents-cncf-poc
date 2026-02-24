import express from "express";
import { CloudEvent, HTTP } from "cloudevents";
import { v4 as uuidv4 } from "uuid";

const app = express();
app.use(express.json());

const PORT = process.env.PORT || 3001;
const CONSUMER_DOTNET_URL = process.env.CONSUMER_DOTNET_URL || "http://localhost:5002";
const CONSUMER_NODE_URL = process.env.CONSUMER_NODE_URL || "http://localhost:3002";
const consumerUrls = [CONSUMER_DOTNET_URL, CONSUMER_NODE_URL];

async function sendToConsumers(cloudEvent) {
  const message = HTTP.structured(cloudEvent);
  const results = [];

  for (const url of consumerUrls) {
    try {
      const response = await fetch(`${url}/api/events`, {
        method: "POST",
        headers: message.headers,
        body: message.body,
      });

      results.push({
        consumer: url,
        status: response.status,
        success: response.ok,
      });

      console.log(
        `CloudEvent ${cloudEvent.id} (${cloudEvent.type}) sent to ${url} -> ${response.status}`
      );
    } catch (err) {
      results.push({
        consumer: url,
        status: 0,
        success: false,
        error: err.message,
      });
      console.warn(`Failed to send CloudEvent to ${url}: ${err.message}`);
    }
  }

  return results;
}

app.get("/health", (_req, res) => {
  res.json({ status: "healthy", service: "producer-node" });
});

app.post("/api/events/user-registered", async (_req, res) => {
  const userId = uuidv4();

  const cloudEvent = new CloudEvent({
    id: uuidv4(),
    type: "com.example.user.registered",
    source: "/producer-node/users",
    time: new Date().toISOString(),
    datacontenttype: "application/json",
    data: {
      userId,
      email: `user${Math.floor(Math.random() * 9999)}@example.com`,
      name: "João Silva",
      plan: "premium",
      registeredAt: new Date().toISOString(),
    },
    partitionkey: userId,
  });

  const results = await sendToConsumers(cloudEvent);
  res.json({ eventId: cloudEvent.id, type: cloudEvent.type, sentTo: results });
});

app.post("/api/events/user-updated", async (_req, res) => {
  const userId = uuidv4();

  const cloudEvent = new CloudEvent({
    id: uuidv4(),
    type: "com.example.user.updated",
    source: "/producer-node/users",
    time: new Date().toISOString(),
    datacontenttype: "application/json",
    data: {
      userId,
      changes: {
        email: `updated${Math.floor(Math.random() * 9999)}@example.com`,
        plan: "enterprise",
      },
      updatedAt: new Date().toISOString(),
    },
    partitionkey: userId,
  });

  const results = await sendToConsumers(cloudEvent);
  res.json({ eventId: cloudEvent.id, type: cloudEvent.type, sentTo: results });
});

app.post("/api/events/batch", async (_req, res) => {
  const events = [];

  for (let i = 0; i < 5; i++) {
    const userId = uuidv4();
    const cloudEvent = new CloudEvent({
      id: uuidv4(),
      type: "com.example.user.registered",
      source: "/producer-node/users",
      time: new Date().toISOString(),
      datacontenttype: "application/json",
      data: {
        userId,
        email: `batch-user${i}@example.com`,
        name: `Usuário ${i + 1}`,
        plan: i % 2 === 0 ? "premium" : "basic",
        registeredAt: new Date().toISOString(),
      },
      partitionkey: userId,
    });

    const results = await sendToConsumers(cloudEvent);
    events.push({ eventId: cloudEvent.id, type: cloudEvent.type, sentTo: results });
  }

  res.json({ batchSize: events.length, events });
});

app.listen(PORT, () => {
  console.log(`Producer Node.js running on port ${PORT}`);
  console.log(`Consumers: ${consumerUrls.join(", ")}`);
});
