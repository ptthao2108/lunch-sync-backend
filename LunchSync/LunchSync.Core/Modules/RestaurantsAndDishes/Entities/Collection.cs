namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstactions;
public sealed class Collection: BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public int CoverageRadiusMeters { get; set; } = 800;
    public CollectionStatus Status { get; set; } = CollectionStatus.Active;
    
    //Navigation Properties
    public ICollection<RestaurantCollection> RestaurantCollections { get; set; } = new List<RestaurantCollection>();
}