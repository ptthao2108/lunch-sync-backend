using LunchSync.Core.Common.Abstractions;
using LunchSync.Core.Common.Enums;
namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
namespace LunchSync.Core.Modules.Auth.Entities;

public sealed class User: BaseEntity
{
    public string CognitoSub { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; } 
    public UserRole Role { get; set; } = UserRole.Host;
    public Boolean IsActive { get; set; } = true;

    //Navigation
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}