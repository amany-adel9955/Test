
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Test.WSAM.Identity
{
	public class CookieHandler : DelegatingHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
			request.Headers.Add(name: "X-Requested-With", "[XMLHttpRequest]");
			return base.SendAsync(request, cancellationToken);
		}
	}
}
