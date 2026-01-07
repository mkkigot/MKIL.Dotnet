using Microsoft.AspNetCore.Mvc;
using MKIL.DotnetTest.OrderService.Domain;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Messaging;

namespace MKIL.DotnetTest.OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserCacheService _userCacheService;
        // for testing failed msg
        private readonly IEventPublisher _eventPublisher;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly IConfiguration _configuration;
        public OrderController(IOrderService orderService, IUserCacheService userCacheService, ICorrelationIdService correlationIdService, IConfiguration configuration, IEventPublisher eventPublisher) 
        {
            _orderService = orderService;
            _userCacheService = userCacheService;
            _correlationIdService = correlationIdService;
            _configuration = configuration;
            _eventPublisher = eventPublisher;
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

        /// <summary>
        /// This is to test a permanent error and see how it would be handled in event consumer
        /// Expectation: Consumer shouldn't abort process. It should log properly and push the message to DeadLetter topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [HttpGet("try-fail/permanent-error")]
        public async Task<IActionResult> TryPermanentFailedMessage()
        {
            string correlationId = _correlationIdService.GetCorrelationId();
            await _eventPublisher.PublishAsync(TestTopic, "TEST PERMANENT ERROR", correlationId);
            await _eventPublisher.PublishAsync(TestTopic, "TEST error msg", correlationId); // will prompt exception
            return Ok();
        }

        /// <summary>
        /// This is to test retrying and see how it would be handled in event consumer
        /// Expectation: UserCreatedEventConsumer should retry to process the message. you should confirm in the log
        ///              Consumer shouldn't abort process. It should log properly and push the message to DeadLetter topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [HttpGet("try-fail/transient-error")]
        public async Task<IActionResult> TryTransientFailedMessage()
        {
            string correlationId = _correlationIdService.GetCorrelationId();
            await _eventPublisher.PublishAsync(TestTopic, "TEST TRANSIENT ERROR", correlationId);
            return Ok();
        }

        private string TestTopic
        {
            get
            {
                return _configuration["Kafka:Topic:NewOrder"];
            }
        }
    }
}
