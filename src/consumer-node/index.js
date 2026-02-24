import express from "express";
import { HTTP } from "cloudevents";

const app = express();
app.use(express.json());

const PORT = process.env.PORT || 3002;
const receivedEvents = [];

app.get("/health", (_req, res) => {
  res.json({ status: "healthy", service: "consumer-node" });
});

app.post("/api/events", (req, res) => {
  try {
    const cloudEvent = HTTP.toEvent({ headers: req.headers, body: req.body });

    const eventRecord = {
      id: cloudEvent.id,
      type: cloudEvent.type,
      source: cloudEvent.source,
      time: cloudEvent.time,
      datacontenttype: cloudEvent.datacontenttype,
      data: cloudEvent.data,
      specversion: cloudEvent.specversion,
      receivedAt: new Date().toISOString(),
      extensions: Object.fromEntries(
        Object.entries(cloudEvent)
          .filter(
            ([key]) =>
              ![
                "id", "type", "source", "time", "datacontenttype",
                "specversion", "data", "data_base64",
              ].includes(key)
          )
      ),
    };

    receivedEvents.push(eventRecord);

    console.log("╔══════════════════════════════════════════════════════════════╗");
    console.log("║  CloudEvent Received (consumer-node)                        ║");
    console.log("╠══════════════════════════════════════════════════════════════╣");
    console.log(`║  ID:          ${cloudEvent.id}`);
    console.log(`║  Type:        ${cloudEvent.type}`);
    console.log(`║  Source:      ${cloudEvent.source}`);
    console.log(`║  Time:        ${cloudEvent.time}`);
    console.log(`║  SpecVersion: ${cloudEvent.specversion}`);
    console.log("╚══════════════════════════════════════════════════════════════╝");
    console.log("Data:", JSON.stringify(cloudEvent.data, null, 2));

    res.json({
      received: true,
      consumer: "consumer-node",
      eventId: cloudEvent.id,
      eventType: cloudEvent.type,
    });
  } catch (err) {
    console.error("Error processing CloudEvent:", err.message);
    res.status(400).json({ error: "Invalid CloudEvent", details: err.message });
  }
});

app.get("/api/events", (_req, res) => {
  res.json({
    consumer: "consumer-node",
    totalReceived: receivedEvents.length,
    events: receivedEvents,
  });
});

app.delete("/api/events", (_req, res) => {
  receivedEvents.length = 0;
  res.json({ cleared: true });
});

app.listen(PORT, () => {
  console.log(`Consumer Node.js running on port ${PORT}`);
});
