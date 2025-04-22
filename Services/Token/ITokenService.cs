using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Token
{
    public interface ITokenService
    {
        string GenerateToken(Account account);
    }
}
