using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Test.WSAM.Models;

namespace Test.WSAM.Identity
{
	public class CookieAuthenticationStateProvider(IHttpClientFactory httpClientFactory) : AuthenticationStateProvider
	{
		private bool _authenticated;
		private readonly ClaimsPrincipal _unauthenticated = new(identity: new ClaimsIdentity());
		private readonly HttpClient _httpClient = httpClientFactory.CreateClient(name: "Auth");
		private readonly JsonSerializerOptions _jsonSerializerOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			_authenticated = false;

			var user = _unauthenticated;

			try
			{
				var userResponse = await _httpClient.GetAsync(requestUri: "manage/info");
				userResponse.EnsureSuccessStatusCode();

				var userjson = await userResponse.Content.ReadAsStringAsync();
				var userinfo = JsonSerializer.Deserialize<UserInfo>(userjson, _jsonSerializerOptions);

				if(userinfo is not null)
				{
					_authenticated = true;
					var claims = new List<Claim>
					{
						new (ClaimTypes.Name , userinfo.Email),
						new (ClaimTypes.Email , userinfo.Email),
					};
					var claimIdentity = new ClaimsIdentity(claims, nameof( CookieAuthenticationStateProvider));
					var newuser = new ClaimsPrincipal(claimIdentity);
				}
			}
			catch
			{

			}
			
			return new AuthenticationState(user);
		}
	}
}
