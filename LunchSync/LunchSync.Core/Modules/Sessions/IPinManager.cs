namespace LunchSync.Core.Modules.Sessions;

public interface IPinManager
{
    // Lấy PIN từ Pool hoặc Gen mới nếu Pool trống
    Task<string> GetUnusedPinAsync();

    // Đưa PIN trở lại Pool khi Session kết thúc
    Task<bool> ReleasePinAsync(string pin);
}
