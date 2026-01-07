using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.Shared.Lib.Logging;
using MKIL.DotnetTest.Shared.Lib.Messaging;
using MKIL.DotnetTest.UserService.Domain.Entities;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using static MKIL.DotnetTest.Shared.Lib.Utilities.Constants;

namespace MKIL.DotnetTest.UserService.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IValidator<UserDto> _userValidator;
        private readonly IValidator<UserOrder> _userOrderValidator;
        private readonly IEventPublisher _eventPublisher;
        private readonly IConfiguration _configuration;
        private readonly ICorrelationIdService _correlationIdService;

        public UserService(IUserRepository repository, IValidator<UserDto> userValidator, IEventPublisher eventPublisher, IConfiguration configuration, ICorrelationIdService correlationIdService, IValidator<UserOrder> userOrderValidator) 
        {
            _repository = repository;
            _userValidator = userValidator;
            _eventPublisher = eventPublisher;
            _configuration = configuration;
            _userOrderValidator = userOrderValidator;
            _correlationIdService = correlationIdService;
        }


        public async Task<Guid> CreateUser(UserDto userDto)
        {
            // validation checking
            ValidationResult? validationResult = await _userValidator.ValidateAsync(userDto);

            if (!validationResult.IsValid)
                throw new UserServiceException(StatusCode.ValidationError, validationResult.ToErrorDtoList());

            // Save the user to the database
            User user = userDto.ToUserEntity();
            user.Id = Guid.Empty;
            user.CreatedDate = DateTime.Now;

            await _repository.CreateUser(user);

            // for debugging and tracing
            string correlationId = _correlationIdService.GetCorrelationId();

            // notify new msg to OrderService
            await _eventPublisher.PublishAsync(CreateUserTopic, user.ToDto(), correlationId);

            return user.Id;
        }

        public async Task<UserDto?> GetUserById(Guid userId)
        {
            User? a = await _repository.GetUserById(userId);

            if (a != null)
                return a.ToDto();

            return null;
        }

        public async Task<List<UserDto>> GetAllUsers()
        {
            var userEntityList = await _repository.GetAllUsers();
            
            if(userEntityList.Any())
            {
                var dtoList = userEntityList.Select(p => p.ToDto()).ToList();
                return dtoList;
            }

            return new List<UserDto>(); // return empty
        }

        public async Task<List<UserOrder>> GetAllOrdersOfUser(Guid userId)
        {
            List<UserOrder> userOrderLIst = await _repository.GetAllOrdersOfUser(userId);
            return userOrderLIst;
        }

        public async Task<int> InsertOrUpdateUserOrder(OrderDto orderDto)
        {
            // validation checking
            UserOrder userOrderToValidate = orderDto.ToUserOrderEntity();
            ValidationResult? validationResult = await _userOrderValidator.ValidateAsync(userOrderToValidate);

            if (!validationResult.IsValid)
                throw new UserServiceException(StatusCode.ValidationError, validationResult.ToErrorDtoList());

            // save user order to database
            UserOrder? userOrder = await _repository.GetUserOrderByOrderId(orderDto.Id.Value);

            if (userOrder == null)
            {
                userOrder = new UserOrder();
                userOrder.OrderId = orderDto.Id.Value;
                userOrder.UserId = orderDto.UserId; 
            }

            userOrder.SyncedAt = DateTime.Now;
            await _repository.InsertOrUpdateUserOrder(userOrder);

            return userOrder.UId;
        }

        private string CreateUserTopic
        {
            get
            {
                return _configuration["Kafka:Topic:NewUser"] ?? throw new InvalidOperationException("Kafka:Topic:NewUser configuration is missing");
            }
        }

    }
}
