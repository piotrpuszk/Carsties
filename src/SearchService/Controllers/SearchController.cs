using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace SearchService;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
  {
    var query = DB.PagedSearch<Item, Item>();

    query.Sort(e => e.Ascending(e => e.Make));

    if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
    {
      query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
    }

    query = searchParams.OrderBy switch
    {
      "make" => query.Sort(e => e.Ascending(x => x.Make)),
      "new" => query.Sort(e => e.Descending(x => x.CreatedAt)),
      _ => query.Sort(e => e.Ascending(x => x.AuctionEnd))
    };

    query = searchParams.FilterBy switch
    {
      "finished" => query.Match(e => e.AuctionEnd < DateTime.UtcNow),
      "endingSoon" => query.Match(e => e.AuctionEnd < DateTime.UtcNow.AddHours(6) && e.AuctionEnd > DateTime.UtcNow),
      _ => query.Match(e => e.AuctionEnd > DateTime.UtcNow)
    };

    if (!string.IsNullOrWhiteSpace(searchParams.Seller))
    {
      query.Match(e => e.Seller == searchParams.Seller);
    }

    if (!string.IsNullOrWhiteSpace(searchParams.Winner))
    {
      query.Match(e => e.Winner == searchParams.Winner);
    }

    query.PageNumber(searchParams.PageNumber);
    query.PageSize(searchParams.PageSize);

    var result = await query.ExecuteAsync();

    return Ok(new
    {
      result = result.Results,
      pageCount = result.PageCount,
      totalCount = result.TotalCount,
    });
  }
}
