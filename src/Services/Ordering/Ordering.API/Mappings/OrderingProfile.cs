using EventBus.Messages.Events;
using Ordering.Application.Features.Orders.Commands.CheckoutOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Ordering.API.Mappings
{
  public class OrderingProfile : Profile
  {
    public OrderingProfile()
    {
      CreateMap<CheckoutOrderCommand, BasketCheckoutEvent>().ReverseMap();
    }
  }
}
