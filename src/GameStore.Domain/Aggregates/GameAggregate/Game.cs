using GameStore.Domain.Exceptions;
using GameStore.Domain.SeedWork.Entities;

namespace GameStore.Domain.Aggregates.GameAggregate;

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
        SetTitle(title);
        SetDescription(description);
        SetPrice(price);
        SetGenre(genre);
        ReleaseDate = releaseDate;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, decimal price, string genre, DateTime? releaseDate)
    {
        SetTitle(title);
        SetDescription(description);
        SetPrice(price);
        SetGenre(genre);
        ReleaseDate = releaseDate;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainRuleException("Title", "Título deve ser fornecido");
        }

        Title = title.Trim();
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainRuleException("Description", "Descrição deve ser fornecida");
        }

        Description = description.Trim();
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
        {
            throw new DomainRuleException("Price", "Preço deve ser maior que zero");
        }

        Price = price;
    }

    private void SetGenre(string genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
        {
            throw new DomainRuleException("Genre", "Gênero deve ser fornecido");
        }

        Genre = genre.Trim();
    }
}
