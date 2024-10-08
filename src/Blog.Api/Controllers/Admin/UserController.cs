﻿using AutoMapper;
using Blog.Api.Helps.Extensions;
using Blog.Api.Helps.Filters;
using Blog.Core.Domain.Identity;
using Blog.Core.Models.Base;
using Blog.Core.Models.System;
using Blog.Core.SeedWorks;
using Blog.Core.SeedWorks.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Controllers.Admin
{
    [Route("api/admin/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public UserController(IMapper mapper, IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        [Authorize(Permissions.Users.View)]
        public async Task<ActionResult<UserResponse>> GetUserByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(StatusMessage.NotFound.User);
            }
            var userResponse = _mapper.Map<AppUser, UserResponse>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userResponse.Roles = roles;
            return Ok(userResponse);
        }

        [HttpGet("paging")]
        [Authorize(Permissions.Users.View)]
        public async Task<ActionResult<PagingResponse<UserResponse>>> GetUsersPaging(string? keyword,
            int pageIndex = 1, int pageSize = 10)
        {
            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var query = _userManager.Users;
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                x.FirstName.Contains(keyword)
                || x.UserName.Contains(keyword)
                || x.Email.Contains(keyword)
                || x.PhoneNumber.Contains(keyword)
                );
            }
            var totalRow = await query.CountAsync();
            query = query.OrderByDescending(x => x.DateCreated)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);

            var pageResponse = new PagingResponse<UserResponse>()
            {
                Results = await _mapper.ProjectTo<UserResponse>(query).ToListAsync(),
                CurrentPage = pageIndex,
                RowCount = totalRow,
                PageSize = pageSize
            };
            return Ok(pageResponse);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Permissions.Users.Create)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (request == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if ((await _userManager.FindByNameAsync(request.UserName)) != null)
            {
                return Conflict(StatusMessage.Conflict.User);
            }

            if ((await _userManager.FindByNameAsync(request.Email)) != null)
            {
                return Conflict(StatusMessage.Conflict.User);
            }

            var user = _mapper.Map<CreateUserRequest, AppUser>(request);

            var result = await _userManager.CreateAsync(user, request.Password);
            return result.Succeeded ? Created()
                : StatusCode(500, string.Join("<br", result.Errors.Select(_x => _x.Description)));
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [Authorize(Permissions.Users.Edit)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            if (request == null || id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(StatusMessage.NotFound.User);
            }
            _mapper.Map(request, user);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? Ok()
                : StatusCode(500, string.Join("<br", result.Errors.Select(_x => _x.Description)));
        }

        [HttpDelete]
        [Authorize(Permissions.Users.Delete)]
        public async Task<IActionResult> DeleteUsers([FromQuery] Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (ids.Length == 1)
            {
                var user = await _userManager.FindByIdAsync(ids[0].ToString());
                if (user == null)
                {
                    return NotFound(StatusMessage.NotFound.User);
                }
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _unitOfWork.Users.RemoveRoles(user.Id, [.. currentRoles]);
                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded ? Ok()
                : StatusCode(500, string.Join("<br", result.Errors.Select(_x => _x.Description)));
            }
            else
            {
                foreach (var id in ids)
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    if (user == null)
                    {
                        continue;
                    }

                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _unitOfWork.Users.RemoveRoles(user.Id, [.. currentRoles]);
                    await _userManager.DeleteAsync(user);
                }
            }

            return Ok();
        }

        [HttpPut("change-password")]
        [ValidateModel]
        [Authorize(Permissions.Users.ChangePasswordCurrentUser)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null)
            {
                BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var user = await _userManager.FindByIdAsync(User.GetUserId().ToString());
            if (user == null)
            {
                return NotFound(StatusMessage.NotFound.User);
            }

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            return result.Succeeded ? Ok()
                : StatusCode(500, string.Join("<br", result.Errors.Select(_x => _x.Description)));
        }

        [HttpPut("set-password/{id}")]
        [ValidateModel]
        [Authorize(Permissions.Users.SetPassword)]
        public async Task<IActionResult> SetPassword(Guid id, [FromBody] SetPasswordRequest request)
        {
            if (request == null || id == Guid.Empty)
            {
                BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(StatusMessage.NotFound.User);
            }
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.NewPassword);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? Ok()
                : StatusCode(500, string.Join("<br", result.Errors.Select(_x => _x.Description)));
        }

        [HttpPut("change-email/{id}")]
        [ValidateModel]
        [Authorize(Permissions.Users.ChangeEmail)]
        public async Task<IActionResult> ChangeEmail(Guid id, [FromBody] ChangeEmailRequest request)
        {
            if (request == null || id == Guid.Empty)
            {
                BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(StatusMessage.NotFound.User);
            }
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.Email);

            var result = await _userManager.ChangeEmailAsync(user, request.Email, token);
            return result.Succeeded ? Ok()
                : StatusCode(500, string.Join("<br", result.Errors.Select(_x => _x.Description)));
        }

        [HttpPut("{id}/assign-users")]
        [ValidateModel]
        [Authorize(Permissions.Users.AssignRolesToUser)]
        public async Task<IActionResult> AssignRolesToUser(Guid id, [FromBody] string[] roles)
        {
            if (roles == null || roles.Length == 0 || id == Guid.Empty)
            {
                BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound(StatusMessage.NotFound.User);
            }
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _unitOfWork.Users.RemoveRoles(user.Id, [.. currentRoles]);
            var addedResult = await _userManager.AddToRolesAsync(user, roles);

            if (!addedResult.Succeeded)
            {
                List<IdentityError> addedErrorList = addedResult.Errors.ToList();
                var errorList = new List<IdentityError>();
                errorList.AddRange(addedErrorList);

                return StatusCode(500, string.Join("<br", errorList.Select(_x => _x.Description)));
            }
            return Ok();
        }
    }
}
