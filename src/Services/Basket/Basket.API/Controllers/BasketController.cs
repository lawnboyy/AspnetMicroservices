using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
  [ApiController]
  [Route("api/v1/[controller]")]
  public class BasketController : ControllerBase
  {
    private readonly IBasketRepository _repository;
    private readonly DiscountGrpcService _discountGrpcService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public BasketController(IBasketRepository repository, DiscountGrpcService discountGrpcService, IPublishEndpoint publishEndpoint, IMapper mapper)
    {
      _repository = repository ?? throw new ArgumentNullException(nameof(repository));
      _discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService));
      _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
      _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet("{userName}", Name = "GetBasket")]
    [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
    {
      var basket = await _repository.GetBasket(userName);
      return Ok(basket ?? new ShoppingCart(userName));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
    {
      foreach (var item in basket.Items)
      {
        var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
        item.Price -= coupon.Amount;
      }

      return Ok(await _repository.UpdateBasket(basket));
    }

    [HttpDelete("{userName}", Name = "DeleteBasket")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeleteBasket(string userName)
    {
      await _repository.DeleteBasket(userName);
      return Ok();
    }

    [Route("[action]")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
    {
      // Get the existing basket with total price...
      var basket = await _repository.GetBasket(basketCheckout.UserName);
      if (basket == null)
      {
        return BadRequest();
      }

      // Create basket checkout event...      
      var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            
      // Set the total price on the basket checkout event...
      // NOTE: This will already be set by the automapping above; but the checkout event is
      // coming from the UI so the total price source of truth is in the database which is why
      // we pull the price from the basket from the database (which includes any discounts that
      // were applied.
      eventMessage.TotalPrice = basket.TotalPrice;

      // Send checkout event to RabbitMQ
      await _publishEndpoint.Publish(eventMessage);

      // Remove the basket.
      await _repository.DeleteBasket(basket.UserName);

      return Accepted();
    }
  }
}
