using CA2SOA.Entities;

namespace CA2SOA.DTOS;

public sealed record LibraryEntryDto(
    int Id,
    int GameId,
    string GameTitle,
    string Platform,
    LibraryStatus Status,
    DateTime CreatedUtc
);

public sealed record CreateLibraryEntryRequest(int GameId, LibraryStatus Status);
public sealed record UpdateLibraryEntryRequest(LibraryStatus Status);