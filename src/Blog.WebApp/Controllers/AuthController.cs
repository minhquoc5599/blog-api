using Blog.Core.Domain.Identity;
using Blog.Core.SeedWorks.Constants;
using Blog.WebApp.Helpers.Events.Login;
using Blog.WebApp.Helpers.Events.Register;
using Blog.WebApp.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApp.Controllers
{
	public class AuthController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly IMediator _mediator;
		public AuthController(UserManager<AppUser> userManager, 
			SignInManager<AppUser> signInManager, IMediator mediator)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_mediator = mediator;
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
				return Redirect(AppUrl.Profile);
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Login failed");
			}
			return View();
		}
	}
}
