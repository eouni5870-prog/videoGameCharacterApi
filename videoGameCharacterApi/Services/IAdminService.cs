using videoGameCharacterApi.Dtos;

namespace videoGameCharacterApi.Services;

public enum SetRoleStatus
{
    Success,
    UserNotFound,
    InvalidRole,
    CannotChangeOwnRole
}

public interface IAdminService
{
    Task<AdminStatsResponse> GetStatsAsync();

    Task<List<AdminUserResponse>> GetUsersAsync();

    /// <summary>
    /// Changes the role of a user. An admin cannot change their own role,
    /// so there is always at least one admin left.
    /// </summary>
    Task<SetRoleStatus> SetRoleAsync(int currentUserId, int userId, string role);
}
