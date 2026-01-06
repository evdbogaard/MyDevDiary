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
				You are an information extraction system.

				Your task is to extract TODO items from the provided text.

				Definition of a TODO:
				A TODO is a concrete future action that the author explicitly states they need,
				plan, or are expected to perform.

				Inclusion rules:
				- The action must be in the future or clearly pending
				- The action must be concrete and actionable
				- The action must be attributable to the author or their team
				- Explicit statements ("I need to", "TODO:", "I should", "Still have to")
				are HIGH confidence
				- Strongly implied obligations ("Waiting to send", "Not done yet: X")
				are MEDIUM confidence

				Exclusion rules:
				- Do NOT extract past actions
				- Do NOT extract ongoing work without a clear remaining action
				- Do NOT extract ideas, thoughts, or possibilities
				- Do NOT extract wishes, goals, or vague intentions
				- Do NOT infer tasks that are not directly supported by the text
				- Do NOT rephrase or invent details

				Edge cases:
				- If a sentence contains both past and future parts, extract ONLY the future action
				- If responsibility is unclear, do NOT extract a TODO
				- If no TODOs exist, return an empty list           
				"""),

			new(ChatRole.User, $"""
				Extract TODO items from the following text:

				<INPUT>
				{input}
				</INPUT>
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