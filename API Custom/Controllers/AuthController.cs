using API_Custom.Models;
using API_Custom.Models.DTOs.Auth;
using API_Custom.Models.DTOs.ErrorHandling;
using API_Custom.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace API_Custom.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISmsService _msService;
        private readonly IUtilsService _utilsService;
        private readonly IAuthService _authService;

        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _databaseContext;

        public AuthController(
            UserManager<User> userManager,
            IConfiguration configuration,
            IUserService userService,
            DatabaseContext databaseContext,
            ISmsService smsService,
            IUtilsService utilsService,
            IAuthService authService
        )
        {
            _userManager = userManager;
            _configuration = configuration;
            _userService = userService;
            _databaseContext = databaseContext;
            _msService = smsService;
            _utilsService = utilsService;
            _authService = authService;
        }

        [HttpPost]
        [Route("register-classic")]
        public async Task<IActionResult> RegisterClassic([FromBody] RegisterClassicRequest request)
        {
            var userExists = await _userManager.FindByNameAsync(request.Username);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            User user = new User()
            {
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = request.Username
            };

            if (request.PhoneNumber != null)
            {
                user.PhoneNumber = request.PhoneNumber;
            }

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestError), StatusCodes.Status400BadRequest)]
        [Route("register-phone")]
        public async Task<IActionResult> RegisterPhone([FromBody] RegisterPhoneRequest request)
        {
            var userExists = await _userService.FindByPhone(request.PhoneNumber);
            if (userExists != null)
            {
                return BadRequest(new { Status = StatusCodes.Status400BadRequest, Errors = "User with provided phone number already exists." });
            }

            User user = new User()
            {
                PhoneNumber = request.PhoneNumber,
                PhoneNumberConfirmed = false,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            await _databaseContext.Users.AddAsync(user);
            await _databaseContext.SaveChangesAsync();


            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BadRequestError), StatusCodes.Status400BadRequest)]
        [Route("phone-code-request")]
        public async Task<IActionResult> PhoneCodeRequest([FromBody] PhoneCodeRequest request)
        {
            if (request.PhoneNumber == null)
            {
                return BadRequest(new { Status = StatusCodes.Status400BadRequest, Errors = "You must provide a phone number." });
            }

            var phoneCode = _utilsService.GenerateCode();
            var foundUser = await _userService.FindByPhone(request.PhoneNumber);

            if (foundUser == null)
            {
                return NotFound(new NotFoundError { Status = StatusCodes.Status404NotFound, Title = "User not found." });
            }

            foundUser.PhoneCode = phoneCode;

            await _databaseContext.SaveChangesAsync();


            _msService.SendSms(request.PhoneNumber, phoneCode.ToString());

            return Ok();
        }


        [HttpPost]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BadRequestError), StatusCodes.Status400BadRequest)]
        [Route("login-phone")]
        public async Task<IActionResult> LoginPhone([FromBody] LoginPhoneRequest request)
        {
            var user = await _userService.FindByPhone(request.PhoneNumber);

            if (user == null)
            {
                return NotFound(new NotFoundError { Status = StatusCodes.Status404NotFound, Title = "User not found." });
            }

            if (user != null && user.PhoneCode?.ToString() != request.Code) 
            {
                return BadRequest(new { Status = StatusCodes.Status400BadRequest, Errors = "Provided code is incorrect." });
            }

            var token = await _authService.GenerateTokenAsync(user!);

            return Ok(new TokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await _authService.GenerateTokenAsync(user);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }
    }
}
