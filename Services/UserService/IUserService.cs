namespace Redm_backend.Services.UserService
{
    using Redm_backend.Dtos.User;
    using Redm_backend.Models;

    public interface IUserService
    {
        Task<ServiceResponse<AccessToken>> UpdateUser(UserDto user);

        Task<ServiceResponse<object?>> UpdatePassword(UpdatePasswordDto passwordDto);

        Task<ServiceResponse<AccessToken>> UpdateCycleInfo(CycleInfoDto cycleInfo);

        Task<ServiceResponse<AccessToken>> UpdateAvatarName(string avatarName);

        Task<ServiceResponse<object?>> DeleteUser();

        int GetUserId();

        Task<bool> UserExists(string email);
    }
}