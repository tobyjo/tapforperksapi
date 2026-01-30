using Microsoft.AspNetCore.Mvc;
using SaveForPerksAPI.Models;
using SaveForPerksAPI.Services;

namespace SaveForPerksAPI.Controllers.User
{
    [Route("api/user")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
            : base(logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet("by-auth/{authProviderId}")]
        public async Task<ActionResult<UserDto>> GetUserByAuthProviderId(string authProviderId)
        {
            Logger.LogInformation(
                "GetUserByAuthProviderId called with AuthProviderId: {AuthProviderId}",
                authProviderId);

            return await ExecuteAsync(
                () => _userService.GetUserByAuthProviderIdAsync(authProviderId),
                nameof(GetUserByAuthProviderId));
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(UserForCreationDto userForCreationDto)
        {
            Logger.LogInformation(
                "CreateUser called with Email: {Email}, AuthProviderId: {AuthProviderId}, Name: {Name}",
                userForCreationDto.Email,
                userForCreationDto.AuthProviderId,
                userForCreationDto.Name);

            return await ExecuteAsync(
                () => _userService.CreateUserAsync(userForCreationDto),
                nameof(CreateUser));
        }
    }
}
