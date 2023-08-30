using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
      System.Console.WriteLine("--> Consuming auction deleted: " + context.Message.Id);

      var id = context.Message.Id;

      var result = await DB.DeleteAsync<Item>(id);

      if(!result.IsAcknowledged)
      {
        throw new MessageException(typeof(AuctionDeleted), "Problem deleting auction");
      }
    }
}
