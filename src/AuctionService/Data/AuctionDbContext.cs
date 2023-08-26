using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
  public DbSet<Auction> Auctions { get; set; }

  public AuctionDbContext(DbContextOptions options) : base(options)
  {
  }
}
