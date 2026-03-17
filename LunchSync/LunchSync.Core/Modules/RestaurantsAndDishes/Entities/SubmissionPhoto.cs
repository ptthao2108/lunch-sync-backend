namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;

using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstactions;

public sealed class SubmissionPhoto
{
    public Guid SubmissionId { get; set; } 
    public string PhotoUrl { get; set; } = null!;
    public int DisplayOrder { get; set; } = 0;

    //Navigation
    public Submission Submission { get; set; } = null!;
}