using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace MyDevDiary.App.Services;

public interface IDayAnalyzerService
{
	Task<string> ExtractSummary(string input);
	Task<string> ExtractTodos(string input);
}

public class DayAnalyzerService(Kernel kernel) : IDayAnalyzerService
{
	private readonly Kernel _kernel = kernel;

	public async Task<string> ExtractSummary(string input)
	{
		var chatClient = _kernel.GetRequiredService<IChatClient>();

		var history = new List<ChatMessage>()
		{
			new(ChatRole.System, $"""
                You are an expert in summerizer given input as a text.
                The text is the work a developer did on a specific day.
                Summerize the work in a concise manner in 2 sentences.
                The output must be a string only containing the summerization.

                Input:
                {input}
                """)
		};

		var response = await chatClient.GetResponseAsync(history, options: new ChatOptions()
		{
			TopP = 0.0f,
			TopK = 0,
			Temperature = 0.0f,
			ResponseFormat = ChatResponseFormat.ForJsonSchema<string>()
		});

		return response.Text;
	}

	public async Task<string> ExtractTodos(string input)
	{
		var chatClient = _kernel.GetRequiredService<IChatClient>();

		var history = new List<ChatMessage>()
		{
			new(ChatRole.System, $"""
                You task is to scan a piece of text and retrieve any possible tasks that still need to be done.
                The output must be a JSON array of strings, each string being an task.
                Don't give items of tasks that are already done on that day.

                Input:
                {input}
                """)
		};

		var response = await chatClient.GetResponseAsync(history, options: new ChatOptions()
		{
			TopP = 0.0f,
			TopK = 0,
			Temperature = 0.0f,
			ResponseFormat = ChatResponseFormat.ForJsonSchema<string[]>()
		});

		return response.Text;
	}
}