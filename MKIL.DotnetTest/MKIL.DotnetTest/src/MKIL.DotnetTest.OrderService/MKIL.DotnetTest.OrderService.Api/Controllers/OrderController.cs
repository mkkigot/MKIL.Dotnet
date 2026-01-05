using Microsoft.AspNetCore.Mvc;
using MKIL.DotnetTest.OrderService.Domain;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.Shared.Lib.DTO;

namespace MKIL.DotnetTest.OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserCacheService _userCacheService;
        public OrderController(IOrderService orderService, IUserCacheService userCacheService) 
        {
            _orderService = orderService;
            _userCacheService = userCacheService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto orderPayload)
        {
            try
            {
                Guid generatedOrderId = await _orderService.CreateOrder(orderPayload);

                return Ok(generatedOrderId);
            }
            catch (OrderServiceException us)
            {
                return StatusCode((int)us.ErrorCode, us.ErrorList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrder()
        {
            List<OrderDto> allOrderList = await _orderService.GetAllOrders();

            return Ok(allOrderList);
        }

        [HttpGet("check/usercaches")]
        public async Task<IActionResult> GetAllUserCaches()
        {
            var a = await _userCacheService.GetAllUserCache();
            return Ok(a);
        }
    }
}
