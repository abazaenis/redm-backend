namespace Redm_backend.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Redm_backend.Dtos.User;
    using Redm_backend.Models;
    using Redm_backend.Services.UserService;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPatch("UpdateUser")]
        public async Task<ActionResult<ServiceResponse<AccessToken>>> UpdateUser(UserDto user)
        {
            var response = await _userService.UpdateUser(user);

            if (response.StatusCode == 400)
            {
                return BadRequest(response);
            }
            else if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPatch("UpdatePassword")]
        public async Task<ActionResult<ServiceResponse<object?>>> UpdatePassword(UpdatePasswordDto user)
        {
            var response = await _userService.UpdatePassword(user);

            if (response.StatusCode == 400)
            {
                return BadRequest(response);
            }
            else if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPatch("UpdateCycleInfo")]
        public async Task<ActionResult<ServiceResponse<AccessToken>>> UpdateCycleInfo(CycleInfoDto cycleInfo)
        {
            var response = await _userService.UpdateCycleInfo(cycleInfo);

            if (response.StatusCode == 400)
            {
                return BadRequest(response);
            }
            else if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPatch("UpdateAvatarName")]
        public async Task<ActionResult<ServiceResponse<AccessToken>>> UpdateAvatarName(string avatarName)
        {
            var response = await _userService.UpdateAvatarName(avatarName);

            if (response.StatusCode == 400)
            {
                return BadRequest(response);
            }
            else if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<ServiceResponse<object?>>> DeleteUser()
        {
            var response = await _userService.DeleteUser();

            if (response.StatusCode == 404)
            {
                return NotFound(response);
            }

            return Ok(response);
        }
    }
}