using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Entities;
using Xunit;

namespace AuctionService.UnitTests
{
  //Method_Scenario_ExpectedResult()
  public class AuctionEntityTests
  {
    [Fact]
    public void HasReservePrice_ReservePriceGreaterThanZero_True()
    {
      //arrange
      Auction auction = new()
      {
        Id = Guid.NewGuid(),
        ReservePrice = 10,
      };

      //Act
      var result = auction.HasReservePrice();

      //Assert
      Assert.True(result);
    }

    [Fact]
    public void HasReservePrice_ReservePriceIsZero_False()
    {
      //arrange
      Auction auction = new()
      {
        Id = Guid.NewGuid(),
        ReservePrice = 0,
      };

      //Act
      var result = auction.HasReservePrice();

      //Assert
      Assert.False(result);
    }
  }
}