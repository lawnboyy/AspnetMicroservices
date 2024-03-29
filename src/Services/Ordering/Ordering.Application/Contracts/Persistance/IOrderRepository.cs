﻿using Ordering.Application.Contracts.Persistence;
using Ordering.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ordering.Application.Contracts.Persistance
{
  public interface IOrderRepository : IAsyncRepository<Order>
  {
    Task<IEnumerable<Order>> GetOrderByUserName(string userName);
  }
}
