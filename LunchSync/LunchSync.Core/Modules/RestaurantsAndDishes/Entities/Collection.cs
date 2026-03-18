using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstractions;

namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
public class Collection : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double? LandmarkLat { get; set; }
    public double? LandmarkLon { get; set; }
    public int CoverageRadiusMeters { get; set; } = 250;
    public CollectionStatus Status { get; set; } = CollectionStatus.Active;

    //Navigation Properties
    public ICollection<RestaurantCollection> RestaurantCollections { get; set; } = new List<RestaurantCollection>();
}