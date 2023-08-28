using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService;

public static class DbInitializer
{
  public static async Task InitDb(this WebApplication app)
  {

    await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

    await DB.Index<Item>()
    .Key(e => e.Make, KeyType.Text)
    .Key(e => e.Model, KeyType.Text)
    .Key(e => e.Color, KeyType.Text)
    .CreateAsync();

    var count = await DB.CountAsync<Item>();

    using var scope = app.Services.CreateScope();

    var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

    var items = await httpClient.GetItemsForSearchDb();

    System.Console.WriteLine(items.Count + " returned from the auction service");

    if(items.Count > 0)
    {
      await DB.SaveAsync(items);
    }
  }
}
