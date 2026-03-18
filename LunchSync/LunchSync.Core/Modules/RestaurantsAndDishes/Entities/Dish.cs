namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstractions;
using LunchSync.Core.Common.ValueObjects;
public  class Dish : BaseEntity
{
    
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public DishProfile Profile { get; set; } = new();
    public int Version { get; set; } = 1;

    // Navigation Property
    public ICollection<RestaurantDish> RestaurantDishes { get; set; } = new List<RestaurantDish>();
    public ICollection<SubmissionDish> SubmissionDishes { get; set; } = new List<SubmissionDish>();
}