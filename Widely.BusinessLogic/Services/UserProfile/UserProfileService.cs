using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services.Base;
using Widely.BusinessLogic.Utilities;
using Widely.DataAccess.DataContext.Entities;
using Widely.DataAccess.Repositories.UnitOfWork;
using Widely.DataAccess.Repositories.UserProfile;
using Widely.DataModel.ViewModels.Common;
using Widely.DataModel.ViewModels.UserProfile;
using Widely.Infrastructure.Exceptions;

namespace Widely.BusinessLogic.Services.UserProfile
{
    public class UserProfileService : BaseService
    {
        private readonly IMapper _mapper;
        private readonly IUserProfileRepository _userProfileRepository;

        public UserProfileService(
             IHttpContextAccessor httpContextAccessor
             , IUnitOfWork unitOfWork
             , IMapper mapper
             , IUserProfileRepository userProfileRepository
            ) : base(httpContextAccessor, unitOfWork)
        {
            _mapper = mapper;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<ServiceResponse<bool>> ForceChangePassword(UserProfileForceChangePasswordRequest request)
        {
            var transactionDate = DateTime.Now;
            ServiceResponse<bool> response = new ServiceResponse<bool>();

            var userRequest = this.GetUserID();

            if (string.IsNullOrEmpty(userRequest))
            {
                throw new AppException("User not found.");
            }

            int.TryParse(userRequest, out int userId);

            //init DbSet
            var userRepository = _unitOfWork.AsyncRepository<Appusers>();
            var user = await userRepository.GetAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new AppException("User not found.");
            }

            if (request.password != request.passwordConfirm)
            {
                throw new AppException("Password does match.");
            }

            PasswordHashUtility.CreatePasswordHash(user.Username, request.password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.IsForceChangePwd = false;
            user.LastChangePwd = transactionDate;
            user.ModifiedBy = GetUserName();
            user.ModifiedDate = transactionDate;

            await userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            response.Data = true;
            response.Success = true;
            response.Message = "Successfully.";

            return response;
        }
        
        public async Task<ServiceResponse<bool>> ChangePassword(UserProfileChangePasswordRequest request)
        {
            var transactionDate = DateTime.Now;
            ServiceResponse<bool> response = new ServiceResponse<bool>();

            var userRequest = this.GetUserID();

            if (string.IsNullOrEmpty(userRequest))
            {
                throw new AppException("User not found.");
            }

            int.TryParse(userRequest, out int userId);

            //init DbSet
            var userRepository = _unitOfWork.AsyncRepository<Appusers>();
            var user = await userRepository.GetAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new AppException("User not found.");
            }

            if (request.password != request.passwordConfirm)
            {
                throw new AppException("Password does match.");
            }

            if (!PasswordHashUtility.VerifyPasswordHash(user.Username, request.currentPassword, user.PasswordHash, user.PasswordSalt))
            {
                throw new AppException("Wrong password.");
            }

            PasswordHashUtility.CreatePasswordHash(user.Username, request.password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.IsForceChangePwd = false;
            user.LastChangePwd = transactionDate;
            user.ModifiedBy = GetUserName();
            user.ModifiedDate = transactionDate;

            await userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            response.Data = true;
            response.Success = true;
            response.Message = "Successfully.";

            return response;
        }

        public async Task<ServiceResponse<bool>> UpdateUserProfile(UserProfileUpdateRequest request)
        {
            var transactionDate = DateTime.Now;
            ServiceResponse<bool> response = new ServiceResponse<bool>();

            var userRequest = this.GetUserID();
            if (string.IsNullOrEmpty(userRequest))
            {
                throw new AppException("User not found.");
            }

            int.TryParse(userRequest, out int userId);

            //init DbSet
            var userRepository = _unitOfWork.AsyncRepository<Appusers>();
            var user = await userRepository.GetAsync(x => x.Id == userId);

            if (user == null)
            {
                throw new AppException("User not found.");
            }

            user.Fname = !string.IsNullOrEmpty(request.fName) ? request.fName : null;
            user.Lname = !string.IsNullOrEmpty(request.lName) ? request.lName : null;
            user.Email = !string.IsNullOrEmpty(request.email) ? request.email : null;
            user.MobilePhone = !string.IsNullOrEmpty(request.mobilePhone) ? request.mobilePhone : null;
            user.BirthDate = !string.IsNullOrEmpty(request.birthDate) ? DateTime.ParseExact(request.birthDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
            user.ModifiedBy = GetUserName();
            user.ModifiedDate = transactionDate;

            await userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            response.Data = true;
            response.Success = true;
            response.Message = "Successfully.";

            return response;
        }

        public async Task<ServiceResponse<UserProfileResponse>> GetUserProfile()
        {
            ServiceResponse<UserProfileResponse> response = new ServiceResponse<UserProfileResponse>();

            var userRequest = this.GetUserID();

            if (string.IsNullOrEmpty(userRequest))
            {
                throw new AppException("User not found.");
            }

            int.TryParse(userRequest, out int userId);

            //init DbSet
            var userRepository = _unitOfWork.AsyncRepository<Appusers>();
            var user = await userRepository.GetAsync(x => x.Id == userId, i => i.Role);

            if (user == null)
            {
                throw new AppException("User not found.");
            }

            response.Data = new UserProfileResponse
            {
                id = user.Id,
                username = user.Username,
                role = user.Role.Name,
                email = user.Email,
                fName = user.Fname,
                lName = user.Lname,
                mobilePhone = user.MobilePhone,
                birthDate = user.BirthDate != null ? user.BirthDate.Value : null
            };

            response.Success = true;
            response.Message = "OK";

            return response;
        }
    }
}
