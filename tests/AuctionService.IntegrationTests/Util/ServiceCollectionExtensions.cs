using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util
{
  public static class ServiceCollectionExtensions
  {
    public static void RemoveDBContext<T>(this IServiceCollection services)
    {
      var descriptor = services.SingleOrDefault(e => e.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

      if (descriptor is not null)
      {
        services.Remove(descriptor);
      }
    }

    public static void EnsureCreated<T>(this IServiceCollection services)
    {
      var sp = services.BuildServiceProvider();

      using var scope = sp.CreateScope();
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<AuctionDbContext>();
      db.Database.Migrate();
      DbHelper.InitDbForTests(db);
    }
  }
}