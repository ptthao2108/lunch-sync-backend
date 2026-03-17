namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;

using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstactions;

public sealed class SubmissionDish
{
    public Guid SubmissionId { get; set; }
    public Guid DishId { get; set; } 

    //Navigation
    public Submission Submission { get; set; } = null!;
    public Dish Dish { get; set; } = null!;
}