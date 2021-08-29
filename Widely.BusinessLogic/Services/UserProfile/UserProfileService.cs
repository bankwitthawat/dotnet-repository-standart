using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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
    }
}
