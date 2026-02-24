namespace ConsumerDotnet.Models;

public record OrderShippedData(
    string OrderId,
    string TrackingCode,
    string Carrier,
    DateTime EstimatedDelivery,
    DateTime ShippedAt
);
