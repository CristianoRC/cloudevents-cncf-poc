import json
import logging
import os
from datetime import datetime, timezone

from cloudevents.http import from_http
from flask import Flask, jsonify, request

app = Flask(__name__)
logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
logger = logging.getLogger(__name__)

received_events: list[dict] = []

CLOUD_EVENT_CORE_ATTRS = {"id", "type", "source", "time", "datacontenttype", "specversion"}


@app.get("/health")
def health():
    return jsonify(status="healthy", service="consumer-python")


@app.post("/api/events")
def receive_event():
    cloud_event = from_http(request.headers, request.get_data())

    extensions = {
        key: cloud_event[key]
        for key in cloud_event.get_attributes().keys()
        if key not in CLOUD_EVENT_CORE_ATTRS
    }

    event_record = {
        "id": cloud_event["id"],
        "type": cloud_event["type"],
        "source": cloud_event["source"],
        "time": cloud_event["time"],
        "dataContentType": cloud_event["datacontenttype"],
        "data": cloud_event.data,
        "specVersion": cloud_event["specversion"],
        "receivedAt": datetime.now(timezone.utc).isoformat(),
        "extensions": extensions,
    }

    received_events.append(event_record)

    logger.info(
        "CloudEvent Received | ID: %s | Type: %s | Source: %s | Time: %s | SpecVersion: %s",
        event_record["id"],
        event_record["type"],
        event_record["source"],
        event_record["time"],
        event_record["specVersion"],
    )

    return jsonify(
        received=True,
        consumer="consumer-python",
        eventId=event_record["id"],
        eventType=event_record["type"],
    )


@app.get("/api/events")
def list_events():
    return jsonify(
        consumer="consumer-python",
        totalReceived=len(received_events),
        events=received_events,
    )


@app.delete("/api/events")
def clear_events():
    received_events.clear()
    return jsonify(cleared=True)


if __name__ == "__main__":
    port = int(os.environ.get("PORT", 8000))
    app.run(host="0.0.0.0", port=port)
