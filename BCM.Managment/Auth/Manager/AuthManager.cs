using BCM.Managment.Auth.DTOs;
using BCM.Managment.Base;
using BCM.Managment.Base.DTOs;
using BCM.Managment.Token.Manager;
using BCM.Models.Data;
using BCM.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Auth.Manager
{
    public class AuthManager : BaseManager , IAuthManager
    {
        private readonly ITokenManager _token;


        public AuthManager(IConfiguration configuration, AppDbContext context, ITokenManager token) : base(configuration, context)
        {
            _token = token;
        }

        public async Task<DefaultResponse<LoginResponse>> Login(AuthLoginRequest request)
        {

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return DefaultResponse<LoginResponse>.FailureResponse(message_ar: "حدث خطأ في اسم المستخدم او رقم السري", message_en: "There is an error on email or password");


            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return DefaultResponse<LoginResponse>.FailureResponse(message_ar: "حدث خطأ في اسم المستخدم او رقم السري", message_en: "There is an error on email or password");


            var token = _token.GenerateToken(user);

            var response = new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToDisplayString(),
            };

            return DefaultResponse<LoginResponse>.SuccessResponse(response);
        }
    }
}
