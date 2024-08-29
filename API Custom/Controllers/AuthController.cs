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
        private readonly IMailService _mailService;

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
            IAuthService authService,
            IMailService mailService
        )
        {
            _userManager = userManager;
            _configuration = configuration;
            _userService = userService;
            _databaseContext = databaseContext;
            _msService = smsService;
            _utilsService = utilsService;
            _authService = authService;
            _mailService = mailService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                return BadRequest(new { Status = StatusCodes.Status400BadRequest, Errors = "User with this email already exists." });
            }

            User user = new User()
            {
                Email = request.Email,
                EmailConfirmed = false,
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
                return StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            try
            {
                await _mailService.SendConfirmationRegisterEmailAsync(request.Email);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = StatusCodes.Status400BadRequest, Errors = ex.Message });
            }

            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestError), StatusCodes.Status400BadRequest)]
        [Route("send-email-code")]
        public async Task<IActionResult> SendEmailCode([FromQuery] string email)
        {
            var userExists = await _userManager.FindByEmailAsync(email);
            if (userExists != null && userExists.IsGoogleAuth == true)
            {
                return BadRequest(new { Status = StatusCodes.Status400BadRequest, Errors = "User registered with Google Account cannot receive confirmation code." });
            }

            try
            {
                await _mailService.SendConfirmationRegisterEmailAsync(email);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = StatusCodes.Status400BadRequest, Errors = ex.Message });
            }

            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Route("validate-email-code")]
        public async Task<IActionResult> ValidateEmailCode([FromBody] ValidateEmailCodeRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null && user.EmailCode == request.Code)
            {
                user.EmailConfirmed = true;
                user.EmailCode = null;

                await _databaseContext.SaveChangesAsync();

                var token = await _authService.GenerateTokenAsync(user);

                return Ok(new TokenResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
            {
                var token = await _authService.GenerateTokenAsync(user);

                return Ok(new TokenResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized();
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

            user!.PhoneNumberConfirmed = true;
            user!.PhoneCode = null;

            await _databaseContext.SaveChangesAsync();

            return Ok(new TokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestError), StatusCodes.Status400BadRequest)]
        [Route("login-google-auth")]
        public async Task<IActionResult> LoginGoogleAuth([FromBody] LoginGoogleAuthRequest request)
        {
            try
            {
                var token = await _authService.GenerateTokenFromGoogleValidationAsync(request);

                return Ok(new TokenResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new BadRequestError
                {
                    Errors = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }
    }
}
