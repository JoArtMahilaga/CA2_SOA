namespace CA2SOA.DTOS;

public sealed record ReviewDto(
    int Id,
    int GameId,
    string GameTitle,
    int UserId,
    string UserName,
    int Rating,
    string Comment,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc
);

public sealed record CreateReviewRequest(int GameId, int Rating, string Comment);
public sealed record UpdateReviewRequest(int Rating, string Comment);