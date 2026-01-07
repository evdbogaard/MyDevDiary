using System.ClientModel.Primitives;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

using MyDevDiary.App;
using MyDevDiary.App.Services;

using OpenAI;

var serviceCollection = new ServiceCollection()
	.AddSingleton(sp =>
	{
		var httpClient = sp.GetRequiredService<IHttpClientFactory>()
			.CreateClient("OpenAI");

		var openAIClient = new OpenAIClient(
			new System.ClientModel.ApiKeyCredential("secret"),
			new OpenAIClientOptions()
			{
				Endpoint = new Uri("http://host.docker.internal:1234/v1/"),
				Transport = new HttpClientPipelineTransport(httpClient)
			}
		);

		return openAIClient
			.GetChatClient("meta-llama-3.1-8b-instruct")
			.AsIChatClient();
	})
	.AddSingleton<HttpLoggingHandler>()
	.AddScoped<IDayAnalyzerService, DayAnalyzerService>();

serviceCollection
	.AddHttpClient("OpenAI")
	.AddHttpMessageHandler<HttpLoggingHandler>();

var serviceProvider = serviceCollection
	.BuildServiceProvider();

var dayAnalyzer = serviceProvider.GetRequiredService<IDayAnalyzerService>();

var text = await File.ReadAllTextAsync("diary.txt");

var summary = await dayAnalyzer.ExtractSummary(text);
Console.WriteLine("Summary:");
Console.WriteLine(summary);
Console.WriteLine();

var todos = await dayAnalyzer.ExtractTodos(text);
Console.WriteLine("Todos:");
Console.WriteLine(todos);