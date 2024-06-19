using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Test.API.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class AuthController(SignInManager<IdentityUser> signIn) : ControllerBase
	{
		private readonly SignInManager<IdentityUser> _signIn = signIn;

		[Authorize]
		[HttpPost(template: "logout")]
		public async Task<IActionResult> Logout([FromBody] object empty)
		{
			if(empty is not null)
			{
				await _signIn.SignOutAsync();
				return Ok();
			}
			return Unauthorized();
		}
	}
}
