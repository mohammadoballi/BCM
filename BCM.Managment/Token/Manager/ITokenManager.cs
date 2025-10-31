using BCM.Models.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Token.Manager
{
    public interface ITokenManager
    {
        string GenerateToken(User user);
    }
}
