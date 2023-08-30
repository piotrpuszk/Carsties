using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
  private readonly IMapper _mapper;

  public AuctionUpdatedConsumer(IMapper mapper)
  {
    _mapper = mapper;
  }

  public async Task Consume(ConsumeContext<AuctionUpdated> context)
  {
    System.Console.WriteLine("--> Consuming auction updated: " + context.Message.Id);

    var item = _mapper.Map<Item>(context.Message);

    var result = await DB.Update<Item>()
    .MatchID(item.ID)
    .ModifyOnly(e => new {e.Make, e.Model, e.Color, e.Mileage, e.Year}, item)
    .ExecuteAsync();

    if(!result.IsAcknowledged)
    {
      throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");
    }
  }
}
