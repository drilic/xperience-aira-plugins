using System.Collections.Generic;

using Anthropic.SDK;

using EXLRT.Xperience.AIRA.Plugins;
using EXLRT.Xperience.AIRA.Plugins.Contracts;
using EXLRT.Xperience.AIRA.Plugins.Infrastructure;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DancingGoat.AiraChatService;

/// <summary>
/// Semantic Kernel IChatCompletionService backed by Anthropic Claude API via Anthropic.SDK.
/// Sets <see cref="ChatCompletionServiceExtensionsBase.Inner"/> so all chat calls
/// route through Claude instead of Kentico's built-in Aira service.
/// Reads config from <c>AiraProviders:Anthropic</c> section — no dependency on the Providers library.
/// </summary>
public sealed class AnthropicChatCompletionService : ChatCompletionServiceExtensionsBase
{
    public AnthropicChatCompletionService(
        [FromKeyedServices(AiraPluginServiceKeys.OriginalChat)] IChatCompletionService kenticoService,
        IEnumerable<IAiraPlugin> plugins,
        IAiraPluginRegistry registry,
        IConfiguration configuration)
        : base(kenticoService, plugins, registry)
    {
        var section = configuration.GetSection("AiraProviders:Anthropic");
        var apiKey = section["ApiKey"] ?? string.Empty;
        var model = section["Model"] ?? "claude-haiku-4-5-20251001";

        var anthropicClient = new AnthropicClient(apiKey);

        IChatClient chatClient = new ChatClientBuilder(anthropicClient.Messages)
              .ConfigureOptions(o => o.ModelId ??= model)
              .UseFunctionInvocation(configure: AddEnhancementFunction)
              .Build();

        Inner = chatClient.AsChatCompletionService();
    }
}
