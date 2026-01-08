namespace MyDevDiary.App;

public sealed class HttpLoggingHandler : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		if (request.Content is not null)
		{
			var body = await request.Content.ReadAsStringAsync(cancellationToken);
			Console.WriteLine("=== REQUEST ===");
			Console.WriteLine(body);
		}

		var response = await base.SendAsync(request, cancellationToken);

		if (response.Content is not null)
		{
			var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
			Console.WriteLine("=== RESPONSE ===");
			Console.WriteLine(responseBody);
		}

		return response;
	}
}