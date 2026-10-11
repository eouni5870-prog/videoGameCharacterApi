using System.ComponentModel.DataAnnotations;

namespace videoGameCharacterApi.Dtos;

public class CreateOrderRequest
{
    [Required, MinLength(1, ErrorMessage = "The cart is empty.")]
    public List<int> GameIds { get; set; } = [];
}

public class OrderResponse
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }
    public List<OrderItemResponse> Items { get; set; } = [];
}

public class OrderItemResponse
{
    public int? GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
