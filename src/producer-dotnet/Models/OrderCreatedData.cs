namespace ProducerDotnet.Models;

public record OrderCreatedData(
    string OrderId,
    string CustomerId,
    List<OrderItem> Items,
    decimal Total,
    string Currency,
    DateTime CreatedAt
);

public record OrderItem(
    string ProductId,
    string Name,
    int Quantity,
    decimal Price
);
