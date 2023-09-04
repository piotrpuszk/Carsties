using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services
{
  public class CheckAuctionFinished : BackgroundService
  {
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
    {
      _logger = logger;
      _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Starting check for finished auctions");

      stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

      while (!stoppingToken.IsCancellationRequested)
      {
        await checkAuctions(stoppingToken);

        await Task.Delay(5000, stoppingToken);
      }
    }

    private async Task checkAuctions(CancellationToken stoppingToken)
    {
      var finishedAuctions = await DB.Find<Auction>()
        .Match(e => e.AuctionEnd <= DateTime.UtcNow)
        .Match(e => !e.Finished)
        .ExecuteAsync(stoppingToken);

      if (finishedAuctions.Count == 0)
      {
        return;
      }

      _logger.LogInformation($"==> Found {finishedAuctions.Count} auctions that have completed");
      using var scope = _services.CreateScope();
      var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

      foreach (var auction in finishedAuctions)
      {
        auction.Finished = true;
        await auction.SaveAsync(null, stoppingToken);

        var winningBid = await DB.Find<Bid>()
          .Match(e => e.AuctionId == auction.ID)
          .Match(e => e.BidStatus == BidStatus.Accepted)
          .Sort(e => e.Descending(x => x.Amount))
          .ExecuteFirstAsync(stoppingToken);

        await endpoint.Publish(new AuctionFinished
        {
          AuctionId = auction.ID,
          ItemSold = winningBid is not null,
          Winner = winningBid?.Bidder,
          Amount = winningBid?.Amount,
          Seller = auction.Seller,
        }, stoppingToken);
      }
    }
  }
}