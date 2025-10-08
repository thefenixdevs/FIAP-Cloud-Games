namespace GameStore.Domain.Entities;

public class Game:BaseEntity
{
 
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string Genre { get; private set; }
    public DateTime? ReleaseDate { get; private set; }


    private Game()
    {
        Title = string.Empty;
        Description = string.Empty;
        Genre = string.Empty;
    }

    public Game(string title, string description, decimal price, string genre, DateTime? releaseDate)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Price = price;
        Genre = genre;
        ReleaseDate = releaseDate;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, decimal price, string genre, DateTime? releaseDate)
    {
        Title = title;
        Description = description;
        Price = price;
        Genre = genre;
        ReleaseDate = releaseDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
