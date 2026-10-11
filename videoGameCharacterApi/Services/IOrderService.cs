using videoGameCharacterApi.Dtos;

namespace videoGameCharacterApi.Services;

public enum OrderStatus
{
    Success,
    UnknownGame,
    AlreadyOwned
}

public record OrderResult(OrderStatus Status, OrderResponse? Order = null, string? Message = null);

public interface IOrderService
{
    /// <summary>Simulated purchase: no payment, the games are simply added to the user's library.</summary>
    Task<OrderResult> CreateOrderAsync(int userId, CreateOrderRequest request);

    Task<List<OrderResponse>> GetMyOrdersAsync(int userId);

    /// <summary>Ids of the games the user already bought (their "library").</summary>
    Task<List<int>> GetOwnedGameIdsAsync(int userId);
}
