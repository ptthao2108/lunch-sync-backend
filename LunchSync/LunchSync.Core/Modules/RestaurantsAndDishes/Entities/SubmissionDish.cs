namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;

using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstractions;

public class SubmissionDish
{
    public Guid SubmissionId { get; set; }
    public Guid DishId { get; set; } 

    //Navigation
    public Submission Submission { get; set; } = null!;
    public Dish Dish { get; set; } = null!;
}