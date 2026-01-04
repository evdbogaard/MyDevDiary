using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

using MyDevDiary.App.Services;

var serviceCollection = new ServiceCollection()
	.AddScoped<IDayAnalyzerService, DayAnalyzerService>();

serviceCollection
	.AddKernel()
	.AddOpenAIChatClient(
		modelId: "meta-llama-3.1-8b-instruct",
		endpoint: new Uri("http://host.docker.internal:1234/v1/"),
		apiKey: "secret"
	);

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