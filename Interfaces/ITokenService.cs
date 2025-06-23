using System.Threading.Tasks;
using simpliBuild.SWMS.Model;

namespace simpliBuild.Interfaces;

public interface ITokenService
{
    Task<SimpliAccessToken> GetTokenAsync();
}