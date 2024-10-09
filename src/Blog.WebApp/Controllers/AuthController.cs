using Azure.Core;
using Blog.Core.Domain.Identity;
using Blog.Core.SeedWorks.Constants;
using Blog.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApp.Controllers
{
	public class AuthController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
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
	}
}
