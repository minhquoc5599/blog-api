using Blog.Core.Domain.Identity;
using Blog.Core.SeedWorks;
using Blog.Core.SeedWorks.Constants;
using Blog.WebApp.Helpers.Constants;
using Blog.WebApp.Helpers.Extensions;
using Blog.WebApp.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.WebApp.Controllers
{
	[Authorize]
	public class UserController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		public UserController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[Route("profile")]
		public async Task<IActionResult> Profile()
		{
			var userId = User.GetUserId();
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			var viewModel = new ProfileViewModel
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				UserName = user.UserName,
				Email = user.Email
			};
			return View(viewModel);
		}

		[HttpGet]
		[Route("profile/edit")]
		public async Task<IActionResult> EditProfile()
		{
			var userId = User.GetUserId();
			var user = await _userManager.FindByIdAsync(userId.ToString());
			var viewModel = new EditProfileViewModel()
			{
				FirstName = user.FirstName,
				LastName = user.LastName
			};
			return View(viewModel);
		}

		[HttpPost]
		[Route("profile/edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditProfile([FromForm] EditProfileViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var userId = User.GetUserId();
			var user = await _userManager.FindByIdAsync(userId.ToString());
			user.FirstName = model.FirstName;
			user.LastName = model.LastName;
			var result = await _userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				TempData[AppConstant.SuccessFormMessage] = "Update profile successfully.";
				return Redirect(AppUrl.Profile);
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Update profile failed");
			}
			return View(model);
		}

		[HttpGet]
		[Route("profile/change-password")]
		public IActionResult ChangePassword()
		{
			return View();
		}

		[HttpPost]
		[Route("profile/change-password")]
		[ValidateAntiForgeryToken]
		public async  Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError(string.Empty, "Invalid");
				return View(model);
			}

			var userId = User.GetUserId();
			var user = await _userManager.FindByIdAsync(userId.ToString());

			var checkOldPassword = await _userManager.CheckPasswordAsync(user, model.OldPassword);
			if (!checkOldPassword)
			{
				ModelState.AddModelError(string.Empty, "Old password is not correct");
				return View(model);
			}

			var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.ConfirmPassword);
			if (result.Succeeded)
			{
				await _signInManager.RefreshSignInAsync(user);
				TempData[AppConstant.SuccessFormMessage] = "Change password successfully";
				return Redirect(AppUrl.Profile);
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			await HttpContext.SignOutAsync();
			return Redirect(AppUrl.Home);
		}
	}
}
