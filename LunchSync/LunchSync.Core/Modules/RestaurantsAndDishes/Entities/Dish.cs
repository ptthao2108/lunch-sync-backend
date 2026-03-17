namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstactions;
public sealed class Dish
{
    
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public DishProfileVector Profile { get; set; } = new();
    public int Version { get; set; } = 1;
    public string? UpdatedBy { get; set;}

    // Navigation Property
    public ICollection<RestaurantDish> RestaurantDishes { get; set; } = new List<RestaurantDish>();
}