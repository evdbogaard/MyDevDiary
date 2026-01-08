using System.ClientModel.Primitives;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MyDevDiary.App;
using MyDevDiary.App.Services;

using OpenAI;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
var devcontainerAppsettingsFile = Environment.GetEnvironmentVariable("DEV_CONTAINER_APPSETTINGS_FILE");

var configuration = new ConfigurationBuilder()
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddEnvironmentVariables()
	.AddJsonFile("appsettings.json", optional: false)
	.AddJsonFile($"appsettings.{environment}.json", optional: true)
	.AddJsonFile($"appsettings.{devcontainerAppsettingsFile}.json", optional: true)
	.Build();

var serviceCollection = new ServiceCollection()
	.AddSingleton<IConfiguration>(configuration)
	.AddSingleton(sp =>
	{
		var httpClient = sp.GetRequiredService<IHttpClientFactory>()
			.CreateClient("OpenAI");

		var configuration = sp.GetRequiredService<IConfiguration>();
		var endpoint = configuration["OpenAI:Endpoint"] ?? throw new NullReferenceException("OpenAI:Endpoint configuration is missing.");

		var openAIClient = new OpenAIClient(
			new System.ClientModel.ApiKeyCredential("secret"),
			new OpenAIClientOptions()
			{
				Endpoint = new Uri(endpoint),
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