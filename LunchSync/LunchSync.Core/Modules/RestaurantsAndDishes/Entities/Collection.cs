namespace LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using LunchSync.Core.Common.Enums;
using LunchSync.Core.Common.Abstactions;
public sealed class Collection: BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double? Lat { get; set; }
    public double? lon { get; set; }
    public int CoverageRadiusMeters { get; set; } = 800;
    public RestaurantCollectionStatus Status { get; set; } = RestaurantCollectionStatus.Active;
    
    //Navigation Properties
    public ICollection<RestaurantCollection> RestaurantCollections { get; set; } = new List<RestaurantCollection>();
}