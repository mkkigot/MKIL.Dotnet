using Microsoft.AspNetCore.Mvc;
using MKIL.DotnetTest.OrderService.Domain;
using MKIL.DotnetTest.OrderService.Domain.DTO;
using MKIL.DotnetTest.OrderService.Domain.Interface;

namespace MKIL.DotnetTest.OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService) 
        {
            _orderService = orderService;
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
    }
}
