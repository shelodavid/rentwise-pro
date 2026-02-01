using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Domain.Identity;
using RentWisePro.Web.Models.Account;
using RentWisePro.Web.Services;

namespace RentWisePro.Web.Controllers
{
    [AllowAnonymous]
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RentWiseProDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RentWiseProDbContext dbContext,
            IWebHostEnvironment environment,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("Register")]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await EnsureDefaultProfileAsync(user.Id);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token = encodedToken },
                protocol: Request.Scheme);

            if (!string.IsNullOrWhiteSpace(confirmationLink))
            {
                _logger.LogInformation("Email confirmation link for {Email}: {Link}", model.Email, confirmationLink);
            }

            var viewModel = new EmailConfirmationPendingViewModel
            {
                Email = model.Email,
                ConfirmationLink = _environment.IsDevelopment() ? confirmationLink : null
            };

            return View("EmailConfirmationPending", viewModel);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return View(new ConfirmEmailViewModel { Succeeded = false });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return View(new ConfirmEmailViewModel { Succeeded = false });
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            return View(new ConfirmEmailViewModel { Succeeded = result.Succeeded });
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account is locked due to multiple failed login attempts.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [Authorize]
        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task EnsureDefaultProfileAsync(string userId)
        {
            var hasProfile = await _dbContext.InvestmentProfiles
                .AnyAsync(profile => profile.UserId == userId);
            if (hasProfile)
            {
                return;
            }

            var defaultProfile = InvestmentProfileDefaults.CreateDefault();
            defaultProfile.UserId = userId;
            defaultProfile.IsDefault = true;

            _dbContext.InvestmentProfiles.Add(defaultProfile);
            await _dbContext.SaveChangesAsync();
        }
    }
}
