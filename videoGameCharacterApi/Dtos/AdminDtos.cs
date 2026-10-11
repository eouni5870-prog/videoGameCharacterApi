using System.ComponentModel.DataAnnotations;

namespace videoGameCharacterApi.Dtos;

/// <summary>Numbers shown on the admin dashboard.</summary>
public class AdminStatsResponse
{
    public int UserCount { get; set; }
    public int CharacterCount { get; set; }
    public int GameCount { get; set; }
    public int ReviewCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public List<TopGameResponse> TopGames { get; set; } = [];
    public List<AdminOrderResponse> RecentOrders { get; set; } = [];
}

public class TopGameResponse
{
    public string GameName { get; set; } = string.Empty;
    public int Sold { get; set; }
    public decimal Revenue { get; set; }
}

public class AdminOrderResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
    public decimal Total { get; set; }
}

public class AdminUserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int OrderCount { get; set; }
    public decimal Spent { get; set; }
    public int ReviewCount { get; set; }
}

public class SetRoleRequest
{
    /// <summary>"Admin" or "User".</summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}
