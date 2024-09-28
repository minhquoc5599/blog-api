using AutoMapper;
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
    [Route("api/admin/role")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<AppRole> _roleManager;

        public RoleController(IMapper mapper, IUnitOfWork unitOfWork, RoleManager<AppRole> roleManager)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Permissions.Roles.Create)]
        public async Task<IActionResult> CreateRole([FromBody] CreateUpdateRoleRequest request)
        {
            if (request == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var result = await _roleManager.CreateAsync(new AppRole
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                DisplayName = request.DisplayName,
            });

            return result.Succeeded ? Created() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [Authorize(Permissions.Roles.Edit)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] CreateUpdateRoleRequest request)
        {
            if (request == null || id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound(StatusMessage.NotFound.Role);
            }
            role.Name = request.Name;
            role.DisplayName = request.DisplayName;

            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
        }

        [HttpDelete]
        [Authorize(Permissions.Roles.Delete)]
        public async Task<IActionResult> DeleteRoles([FromQuery] Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            if (ids.Length == 1)
            {
                var role = await _roleManager.FindByIdAsync(ids[0].ToString());
                if (role == null)
                {
                    return NotFound(StatusMessage.NotFound.Role);
                }
                if (await _unitOfWork.Roles.CheckExistUser(ids[0]))
                {
                    return Conflict($"The {role.Name} role contains users and cannot be deleted");
                }
                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
                var result = await _roleManager.DeleteAsync(role);
                return result.Succeeded ? Ok() : StatusCode(500, StatusMessage.InternalServerError);
            }
            else
            {
                foreach (var id in ids)
                {
                    var role = await _roleManager.FindByIdAsync(id.ToString());
                    if (role == null)
                    {
                        continue;
                    }
                    if (await _unitOfWork.Roles.CheckExistUser(id))
                    {
                        continue;
                    }
                    var claims = await _roleManager.GetClaimsAsync(role);
                    foreach (var claim in claims)
                    {
                        await _roleManager.RemoveClaimAsync(role, claim);
                    }
                    await _roleManager.DeleteAsync(role);
                }
            }

            return Ok();
        }

        [HttpGet("{id}")]
        [Authorize(Permissions.Roles.View)]
        public async Task<ActionResult<RoleResponse>> GetRoleById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound(StatusMessage.NotFound.Role);
            }

            return Ok(_mapper.Map<AppRole, RoleResponse>(role));
        }

        [HttpGet]
        [Route("paging")]
        [Authorize(Permissions.Roles.View)]
        public async Task<ActionResult<PagingResponse<RoleResponse>>> GetRolesPaging(string? keyword,
            int pageIndex = 1, int pageSize = 10)
        {
            if (pageIndex <= 0 || pageSize <= 0)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var query = _roleManager.Roles;
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword) || x.DisplayName.Contains(keyword));
            }
            var totalRow = query.Count();
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var data = await _mapper.ProjectTo<RoleResponse>(query).ToListAsync();
            return Ok(new PagingResponse<RoleResponse>
            {
                Results = data,
                CurrentPage = pageIndex,
                RowCount = totalRow,
                PageSize = pageSize
            });
        }

        [HttpGet("all")]
        [Authorize(Permissions.Roles.View)]
        public async Task<ActionResult<List<RoleResponse>>> GetAllRoles()
        {
            var model = await _mapper.ProjectTo<RoleResponse>(_roleManager.Roles).ToArrayAsync();
            return Ok(model);
        }

        [HttpGet("{roleId}/permissions")]
        [Authorize(Permissions.Roles.ViewRolePermissions)]
        public async Task<ActionResult<PermissionModel>> GetAllRolePermissions(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var model = new PermissionModel();
            var allPermissions = new List<RoleClaimsDto>();
            var types = typeof(Permissions).GetNestedTypes();
            foreach (var type in types)
            {
                allPermissions.GetPermissions(type);
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(StatusMessage.NotFound.Role);
            model.RoleId = roleId;
            var claims = await _roleManager.GetClaimsAsync(role);
            var allClaimValues = allPermissions.Select(a => a.Value).ToList();
            var roleClaimValues = claims.Select(a => a.Value).ToList();
            var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
            foreach (var permission in allPermissions)
            {
                if (authorizedClaims.Any(a => a == permission.Value))
                {
                    permission.Selected = true;
                }
            }
            model.RoleClaims = allPermissions;
            return Ok(model);
        }

        [HttpPut("permissions")]
        [Authorize(Permissions.Roles.EditRolePermissions)]
        public async Task<IActionResult> SavePermission([FromBody] PermissionModel model)
        {
            if (model == null)
            {
                return BadRequest(StatusMessage.BadRequest.InvalidRequest);
            }

            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
                return NotFound(StatusMessage.NotFound.Role);

            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
            var selectedClaims = model.RoleClaims.Where(a => a.Selected).ToList();
            foreach (var claim in selectedClaims)
            {
                await _roleManager.AddPermissionClaim(role, claim.Value);
            }
            return Ok();
        }
    }
}
