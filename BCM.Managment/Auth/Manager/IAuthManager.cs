using BCM.Managment.Auth.DTOs;
using BCM.Managment.Base.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Auth.Manager
{
    public interface IAuthManager
    {

        Task<DefaultResponse<LoginResponse>> Login(AuthLoginRequest request);
    
    
    }
}
