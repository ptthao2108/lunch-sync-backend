namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;

public class RestaurantCollection
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public Guid CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;
}