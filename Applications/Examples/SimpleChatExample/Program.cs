//#define Streaming
using BpeChatAI.Chat;
using BpeChatAI.OpenAI;
using BpeChatAI.OpenAI.ChatCompletions;

using BpeTokenizer;

using System.Text.RegularExpressions;
namespace SimpleChatExample;
// The main impetus behind this sample is to demonstrate cost tracking,
// and tracking the moderation state of input and output texts.
static partial class SimpleChat
{
    private static async Task Main()
    {
        // Sample assumes 1 prompt output for simplicity.
        var openAIClientSettings = new ApiClientSettings { ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") };
        var apiClient = new ApiClient(openAIClientSettings);
        var chatManager = new ChatManager(apiClient, new ChatManagerOptions(KnownChatCompletionModel.GPT3PointFiveTurbo, 0.35, isModerated: true));

        Console.Title = $"Session cost: $0.00 [{chatManager.Parameters.Model}]";
        WriteInformationalPrompt
            ("Please enter a prompt to get started."
            , "Type `done`, `exit`, or `quit` to end your session."
            , string.Empty);
#if Streaming
        await StreamChat(chatManager);
#else
        await SynchronousChat(chatManager);
#endif
        Console.WriteLine($"Session cost: ${chatManager.TotalCost} [{chatManager.Parameters.Model}]");
    }

    private static async Task StreamChat(ChatManager chatManager)
    {
        chatManager.StreamTokenReceived += ChatManager_StreamTokenReceived;
        WriteUserPrefixText();
        while (Console.ReadLine() is { } currentPromptFromUser)
        {
            if (string.IsNullOrWhiteSpace(currentPromptFromUser))
            {
                WriteInformationalPrompt("Please enter a prompt.");
                continue;
            }
            if (DoneRegex.IsMatch(currentPromptFromUser))
                break;
            WriteAssistantPrefixText();
            var results = chatManager.PostStreamingUserMessageAsync(currentPromptFromUser);
            await foreach (var streamResult in results)
            {
                if (!streamResult.IsSuccess)
                {
                    var message = streamResult.ErrorMessage ?? streamResult.Exception?.Message;
                    Console.Clear();
                    WriteAssistantPrefixText();
                    if (!string.IsNullOrWhiteSpace(message))
                        Console.Write(message);
                }
                else if (chatManager.Parameters.Messages.LastOrDefault() is Message m
                         && m.ModerationResponse!.IsFlagged)
                {
                    Console.Clear();
                    WriteAssistantPrefixText();
                    Console.Write("Message flagged for moderation. Please try again.");
                }
            }
            Console.WriteLine();
            WriteUserPrefixText();
        }
        static void ChatManager_StreamTokenReceived(object? sender, StreamTokenReceivedEventArgs e)
        {
            if (e.TokenText != null)
                Console.Write(e.TokenText);
            if (sender is ChatManager chatManager)
                Console.Title = $"Session cost: ${chatManager.TotalCost} [{chatManager.Parameters.Model}]";
        }
    }

    private static async Task SynchronousChat(ChatManager chatManager)
    {
        var encoder = await Models.EncodingForModelAsync(chatManager.Parameters.Model);
        WriteUserPrefixText();
        while (Console.ReadLine() is { } currentPromptFromUser)
        {
            if (string.IsNullOrWhiteSpace(currentPromptFromUser))
            {
                WriteInformationalPrompt("Please enter a prompt.");
                WriteUserPrefixText();
                continue;
            }
            if (DoneRegex.IsMatch(currentPromptFromUser))
                break;
            var results = await chatManager.PostUserMessageAsync(currentPromptFromUser);
            if (results.IsSuccess
                && results.Choices?.FirstOrDefault()?.Message is Message message)
            {

                if (message.ModerationResponse != null
                    && message.ModerationResponse.IsFlagged)
                {
                    WriteInformationalPrompt("Message flagged for moderation. Please try again.");
                    WriteUserPrefixText();
                    continue;
                }
                else
                {
                    WriteAssistantPrefixText();
                    Console.WriteLine(message.Content);
                    if (results.Usage is Usage u)
                    {
                        Console.Title = $"Session cost: ${chatManager.TotalCost} [{chatManager.Parameters.Model}]";
                        var outputTokenCount = u.CompletionTokens;
                        var computedTokenCount = encoder.CountTokens(message.Content!);
                        if (outputTokenCount != computedTokenCount)
                            Console.WriteLine("Token output count mismatch.");
                    }
                }
            }
            else if (results.IsModeratedAndFlagged)
            {
                var errorMessage = results.ErrorMessage ?? results.Exception?.Message;
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    Console.Clear();
                    WriteInformationalPrompt(errorMessage);
                }
            }

            Console.WriteLine();
            WriteUserPrefixText();
        }
    }

    private static void WriteInformationalPrompt(params string[] promptTexts)
    {
        if (promptTexts is null)
            return;
        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var promptText in promptTexts)
            Console.WriteLine(promptText);
    }

    private static void WriteUserPrefixText()
        => WritePrefix("User: ", ConsoleColor.Green, ConsoleColor.White);

    private static void WriteAssistantPrefixText()
        => WritePrefix("Assistant: ", ConsoleColor.DarkCyan, ConsoleColor.White);

    private static void WritePrefix(string text, ConsoleColor prefixColor, ConsoleColor textColor)
    {
        Console.ForegroundColor = prefixColor;
        Console.Write(text);
        Console.ForegroundColor = textColor;
    }

    const string DoneRegexText = @"^(?:done|quit|exit)(?:(?: *?\. *?)*|(?: *?! *?)?) *?$";

    [ GeneratedRegex(DoneRegexText, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex GetDoneRegex();
    private static readonly Regex DoneRegex = GetDoneRegex();
}
