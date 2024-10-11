using Blog.Core.Configs;
using Blog.Core.Domain.Identity;
using Blog.Core.SeedWorks.Constants;
using Blog.WebApp.Helpers.Constants;
using Blog.WebApp.Helpers.Events.Login;
using Blog.WebApp.Helpers.Events.Register;
using Blog.WebApp.Helpers.Extensions;
using Blog.WebApp.Models.Auth;
using Blog.WebApp.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Blog.WebApp.Controllers
{
    public class AuthController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IMediator _mediator;
		private readonly IEmailService _emailService;
		private readonly AppSettings _appSettings;
		public AuthController(UserManager<AppUser> userManager, 
			SignInManager<AppUser> signInManager, IMediator mediator, 
			IEmailService emailService, IOptions<AppSettings> appSettings)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_mediator = mediator;
			_emailService = emailService;
			_appSettings = appSettings.Value;
		}

		[HttpGet]
		[Route("register")]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		[Route("register")]
		public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			if ((await _userManager.FindByNameAsync(model.UserName)) != null)
			{
				ModelState.AddModelError(string.Empty, "UserName already exists");
				return View();
			}

			if ((await _userManager.FindByNameAsync(model.Email)) != null)
			{
				ModelState.AddModelError(string.Empty, "Email already exists");
				return View();
			}

			var result = await _userManager.CreateAsync(new AppUser
			{
				FirstName = model.FirstName,
				LastName = model.LastName,
				UserName = model.UserName,
				Email = model.Email,
			}, model.Password);
			if (result.Succeeded)
			{
				var user = await _userManager.FindByNameAsync(model.UserName);
				await _signInManager.SignInAsync(user, true);
				await _mediator.Publish(new RegisterEvent(user.UserName));
				return Redirect(AppUrl.Profile);
			}
			else
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return View();
		}

		[HttpGet]
		[Route("login")]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		[Route("login")]
		public async Task<IActionResult> Login([FromForm] LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}
			var user = await _userManager.FindByNameAsync(model.UserName);
			if (user == null)
			{
				ModelState.AddModelError(string.Empty, "User has not registered");
				return View();
			}

			var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);
			if (result.Succeeded)
			{
				await _signInManager.SignInAsync(user, false);
				await _mediator.Publish(new LoginEvent(user.UserName));
				return Redirect(AppUrl.Home);
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Login failed");
			}
			return View();
		}

		[HttpGet]
		[Route("forgot-password")]
		[AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		[Route("forgot-password")]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (!ModelState.IsValid) return View(model);
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				ModelState.AddModelError(string.Empty, "User does not exist");
			}

			// For more information on how to enable account confirmation and password reset please
			var code = await _userManager.GeneratePasswordResetTokenAsync(user);

			var callbackUrl = Url.ResetPasswordCallbackLink(user.Id.ToString(), code, Request.Scheme);

			//var emailData = new EmailModel
			//{
			//	ToEmail = user.Email ?? string.Empty,
			//	Subject = $"{_appSettings.Name} - Reset passowrd",
			//	Content = $"Hi {user.FirstName}. You must send request to reset password at " +
			//	$"{_appSettings.Name}. Click: <a href='{callbackUrl}'>at here</a> to reset password. " +
			//	$"Best regards."
			//};
			//await _emailService.SendEmail(emailData);


			TempData[AppConstant.SuccessFormMessage] = "You need to check mail to reset password";
			return Redirect(AppUrl.Login);
		}

		[HttpGet]
		[Route("reset-password")]
		[AllowAnonymous]
		public IActionResult ResetPassword(string? code = null)
		{
			if (code == null)
			{
				throw new ApplicationException("Code is required");
			}
			return View(new ResetPasswordViewModel { Code = code });
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("reset-password")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				// Don't reveal that the user does not exist
				ModelState.AddModelError(string.Empty, "Email does not existed");
				return View();
			}

			var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
			if (result.Succeeded)
			{
				TempData[AppConstant.SuccessFormMessage] = "Reset password successfully";
				return Redirect(AppUrl.Login);
			}
			return View();
		}
	}
}
