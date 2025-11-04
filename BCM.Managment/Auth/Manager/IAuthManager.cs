using BCM.Managment.Auth.DTOs;
using BCM.Managment.Base.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Auth.Manager
{
    public interface IAuthManager
    {
        /// <summary>
        /// Authenticates a user with the provided credentials.
        /// </summary>
        /// <param name="request">The login request containing user credentials.</param>
        /// <returns>A response containing the login result with authentication token.</returns>
        Task<DefaultResponse<LoginResponse>> Login(AuthLoginRequest request);
    
    
    }
}
