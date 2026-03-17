using LunchSync.Core.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using LunchSync.Core.Modules.Auth.Entities;
using LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using LunchSync.Core.Modules.Sessions.Entities;
using LunchSync.Infrastructure.Persistence.Configurations;

namespace LunchSync.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Participant> Participants => Set<Participant>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Dish> Dishes => Set<Dish>();
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<RestaurantCollection> RestaurantCollections => Set<RestaurantCollection>();
        public DbSet<RestaurantDish> RestaurantDishes => Set<RestaurantDish>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply tất cả configurations từ assembly này
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
