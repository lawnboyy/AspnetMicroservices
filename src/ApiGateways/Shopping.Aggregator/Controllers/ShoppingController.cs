using Microsoft.AspNetCore.Mvc;
using Shopping.Aggregator.Models;
using Shopping.Aggregator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Shopping.Aggregator.Controllers
{
  [ApiController]
  [Route("api/v1/[controller]")]
  public class ShoppingController : ControllerBase
  {
    private readonly ICatalogService _catalogService;
    private readonly IBasketService _basketService;
    private readonly IOrderService _orderService;

    public ShoppingController(ICatalogService catalogService, IBasketService basketService, IOrderService orderService)
    {
      _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
      _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
      _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    [HttpGet("{userName}", Name = "GetShopping")]
    [ProducesResponseType(typeof(ShoppingModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingModel>> GetShopping(string userName)
    {
      // Get the basket by username
      var basket = await _basketService.GetBasket(userName);
      // Iterate over basket items and consume products
      foreach (var item in basket.Items)
      {
        var product = await _catalogService.GetCatalog(item.ProductId);
        // Map product members into basket item DTO with extended columns
        item.ProductName = product.Name;
        item.Category = product.Category;
        item.Summary = product.Summary;
        item.Description = product.Description;
        item.ImageFile = product.ImageFile;
      }

      // Consume ordering microservices in order to retrieve order list
      var orders = await _orderService.GetOrdersByUserName(userName);

      var shoppingModel = new ShoppingModel
      {
        UserName = userName,
        BasketWithProducts = basket,
        Orders = orders
      };

      // Return the root shopping model
      return Ok(shoppingModel);
    }
  }
}
