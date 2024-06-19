using Test.WSAM.Models;

namespace Test.WSAM.Identity
{
	public interface IAccountManagement
	{
		Task<AuthResult> LoginAsync(LoginModel _credentials);
		Task<AuthResult> RegisterAsync(string email, string pssword);
		Task LogoutAsync();
	}
}
