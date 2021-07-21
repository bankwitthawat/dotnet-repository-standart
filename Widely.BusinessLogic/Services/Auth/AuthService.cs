using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services.Base;
using Widely.BusinessLogic.Utilities;
using Widely.DataAccess.DataContext.Entities;
using Widely.DataAccess.Repositories.Auth;
using Widely.DataAccess.Repositories.UnitOfWork;
using Widely.DataModel.ViewModels.Auth.LogIn;
using Widely.DataModel.ViewModels.Auth.Register;
using Widely.DataModel.ViewModels.Auth.Token;
using Widely.DataModel.ViewModels.Common;

namespace Widely.BusinessLogic.Services.Auth
{
    public class AuthService : BaseService
    {
        private readonly IMapper _mapper;
        private readonly JwtManager _jwtManager;
        public readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IHttpContextAccessor httpContextAccessor
            , IUnitOfWork unitOfWork
            , IAuthRepository authRepository
            , JwtManager jwtManager
            , IMapper mapper
            , IConfiguration configuration
            ) : base(httpContextAccessor, unitOfWork)
        {
            var testHttpContext = httpContextAccessor;
            _configuration = configuration;
            _mapper = mapper;
            _jwtManager = jwtManager;
            _authRepository = authRepository;
        }

        public async Task<ServiceResponse<LogInResponse>> Login(LogInRequest request)
        {
            ServiceResponse<LogInResponse> response = new ServiceResponse<LogInResponse>();
            Int32.TryParse(_configuration.GetSection("JwtSetting:ExpireMins").Value, out int tokenExpire);
            try
            {
                var userRepository = _unitOfWork.AsyncRepository<Appusers>();
                var authTokenRepository = _unitOfWork.AsyncRepository<Authtokens>();

                var user = await userRepository.GetAsync(_ => _.Username.ToLower() == request.Username.ToLower());

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                }
                else if (user.IsActive == false)
                {
                    response.Success = false;
                    response.Message = "This account has been suspended, please contract adminstrator.";
                }
                else if (user.LoginAttemptCount >= 5)
                {
                    response.Success = false;
                    response.Message = "This account has been locked, please contract adminstrator.";
                }
                else if (!PasswordHashUtility.VerifyPasswordHash(request.Username, request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    response.Success = false;
                    response.Message = "Wrong password.";

                    user.LoginAttemptCount++;
                    await userRepository.UpdateAsync(user);
                }
                else
                {

                    //remove old refresh token
                    var oldRefreshToken = await authTokenRepository.ListAsync(_ => _.UserId == user.Id);
                    await authTokenRepository.RemoveRangeAsync(oldRefreshToken);


                    //save new refresh token
                    var refreshToken = _mapper.Map<Authtokens>(_jwtManager.GenerateRefreshToken(GetIpAddress(), GetUserAgnet(), GetMachineName()));
                    refreshToken.UserId = user.Id;
                    await authTokenRepository.AddAsync(refreshToken);

                    //update user
                    user.LastLogin = DateTime.Now;
                    user.LoginAttemptCount = 0;
                    await userRepository.UpdateAsync(user);


                    response.Data = this.GetLoginUserInfo(user);
                    response.Data.RefreshToken = refreshToken.Token;
                    response.Data.TokenExpire = DateTime.Now.AddMinutes(tokenExpire);
                    response.Data.TokenTimeoutMins = ((int)response.Data.TokenExpire.Subtract(DateTime.Now).TotalMinutes) - 1;
                    response.Success = true;
                    response.Message = "Login success. !!";
                }

                //save all changes to db
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
            

            return response;
        }

        private LogInResponse GetLoginUserInfo(Appusers user)
        {
            var userdto = _mapper.Map<LogInResponse>(user);



            userdto.Token = _jwtManager.CreateToken(userdto);
            return userdto;
        }

        public async Task<ServiceResponse<LogInResponse>> RefreshToken(string oldToken)
        {
            ServiceResponse<LogInResponse> response = new ServiceResponse<LogInResponse>();
            var tokenAge = Convert.ToInt32(_configuration.GetSection("AppSettings:ExpireMins").Value);
            var tokenExpire = DateTime.Now.AddMinutes(tokenAge);

            try
            {
                var username = GetUserID();
                if (string.IsNullOrEmpty(username))
                    return null;

                var userRepository = _unitOfWork.AsyncRepository<Appusers>();
                var authTokenRepository = _unitOfWork.AsyncRepository<Authtokens>();

                var user = await _authRepository.GetUserRelatedByToken(oldToken);
                if (user == null)
                    return null;

                var refreshToken = user.Authtokens.Single(x => x.Token == oldToken);
                if (refreshToken == null)
                    return null;

                var newRefreshToken = _mapper.Map<Authtokens>(_jwtManager.GenerateRefreshToken(GetIpAddress(), GetUserAgnet(), GetMachineName()));
                user.Authtokens.Add(newRefreshToken);
                await userRepository.UpdateAsync(user);
                await authTokenRepository.RemoveAsync(refreshToken);


                response.Data = this.GetLoginUserInfo(user);
                response.Data.RefreshToken = newRefreshToken.Token;
                response.Data.TokenExpire = tokenExpire;
                response.Data.TokenTimeoutMins = ((int)tokenExpire.Subtract(DateTime.Now).TotalMinutes) - 1;

            }
            catch (Exception e)
            {
                throw e;
            }

            await _unitOfWork.CommitAsync();

            return response;
        }

        public async Task<ServiceResponse<bool>> Register(RegisterRequest registerRequest)
        {
            ServiceResponse<bool> response = new ServiceResponse<bool>();

            try
            {
                var userRepository = _unitOfWork.AsyncRepository<Appusers>();

                var user = await userRepository.GetAsync(_ => _.Username.ToLower() == registerRequest.username.ToLower());

                if (user != null)
                {
                    response.Data = false;
                    response.Success = false;
                    response.Message = "มีชื่อผู้ใช้งานนี้แล้วในระบบ";
                }

                PasswordHashUtility.CreatePasswordHash(registerRequest.username, registerRequest.password, out byte[] passwordHash, out byte[] passwordSalt);

                var newUser = new Appusers()
                {
                    Username = registerRequest.username,
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                    Fname = registerRequest.fName,
                    Lname = registerRequest.lName,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "System"
                };

                await userRepository.AddAsync(newUser);
                await _unitOfWork.CommitAsync();

                response.Data = true;
                response.Success = true;
                response.Message = "ลงทะเบียนสำเร็จ !!";
            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }
    }
}
