using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Test.WSAM.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Test.WSAM.Identity
{
	public class CookieAuthenticationStateProvider(IHttpClientFactory httpClientFactory) : AuthenticationStateProvider , IAccountManagement
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

		public async Task<AuthResult> LoginAsync(LoginModel _credentials)
		{
			try
			{
				var result = await _httpClient.PostAsJsonAsync("login?useCookies=true" , new
				{
					_credentials.Email,
					_credentials.Password
				});

				if (result.IsSuccessStatusCode)
				{
					NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
					return new AuthResult { Succeeded = true };
				}

				return new AuthResult { Succeeded = false, ErrorList = ["Invalid Email or Password"] };
			}
			catch
			{
				throw;
			}
		}
		public async Task<AuthResult> RegisterAsync(string email, string password)
		{
			string[] defaultErrors = ["An error occured while signing up!!"];
			try 
			{
				var result = await _httpClient.PostAsJsonAsync("register", new
				{
					email,
				    password
				});

				if (result.IsSuccessStatusCode)
				{
				
					return new AuthResult { Succeeded = true };
				}
				var details = await result.Content.ReadAsStringAsync();
				var problemDetails = JsonDocument.Parse(details);

				var errors = new List<string>();
				var errorList = problemDetails.RootElement.GetProperty(propertyName: "errors");

				foreach(var error in errorList.EnumerateObject())
				{
					if(error.Value.ValueKind == JsonValueKind.String)
					{
						errors.Add(error.Value.GetString()!);
					}
					else if(error.Value.ValueKind == JsonValueKind.Array)
					{
						var allErrors = error.Value.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(e => !string.IsNullOrEmpty(e));
						errors.AddRange(allErrors);
					}
				}

				return new AuthResult
				{
					Succeeded = false,
					ErrorList = [..errors]
				};

			}
			catch
			{

			}
			return new AuthResult
			{
				Succeeded = false,
				ErrorList = defaultErrors
			};
		}

		public	async Task LogoutAsync()
		{
			var emptyContent = new StringContent(content: "{}", Encoding.UTF8, mediaType: "application/json");
			await _httpClient.PostAsync(requestUri: "auth/logout", emptyContent);
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}
	}
}
