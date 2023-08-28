using MongoDB.Entities;

namespace SearchService;

public class AuctionServiceHttpClient
{
  private readonly HttpClient _httpClient;
  private readonly IConfiguration _configuration;

  public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
  {
    _httpClient = httpClient;
    _configuration = configuration;
  }

  public async Task<List<Item>> GetItemsForSearchDb()
  {
    var lastUpdated = await DB.Find<Item, string>()
      .Sort(e => e.Descending(x => x.UpdatedAt))
      .Project(e => e.UpdatedAt.ToString())
      .ExecuteFirstAsync();

    return await _httpClient
      .GetFromJsonAsync<List<Item>>(_configuration["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated);
  }
}
