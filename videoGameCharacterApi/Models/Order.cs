namespace videoGameCharacterApi.Models;

/// <summary>A (simulated) purchase: one order contains one or more games.</summary>
public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}

/// <summary>
/// One game in an order. Name and price are copied at purchase time, so the order stays correct
/// even if the game's price changes or the game is deleted later.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int? GameId { get; set; }
    public Game? Game { get; set; }

    public string GameName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
