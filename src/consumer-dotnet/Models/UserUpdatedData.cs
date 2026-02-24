namespace ConsumerDotnet.Models;

public record UserUpdatedData(
    string UserId,
    UserChanges Changes,
    string UpdatedAt
);

public record UserChanges(
    string? Email,
    string? Plan
);
