namespace ConsumerDotnet.Models;

public record UserRegisteredData(
    string UserId,
    string Email,
    string Name,
    string Plan,
    string RegisteredAt
);
