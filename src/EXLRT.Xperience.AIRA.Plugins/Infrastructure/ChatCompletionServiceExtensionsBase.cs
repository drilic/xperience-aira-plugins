using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EXLRT.Xperience.AIRA.Plugins.Contracts;
using EXLRT.Xperience.AIRA.Plugins.Services;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EXLRT.Xperience.AIRA.Plugins.Infrastructure;

/// <summary>
/// Abstract base <see cref="IChatCompletionService"/> that delegates every call to the
/// original Kentico Aira chat service by default. Override only the methods you want
/// to customize — everything else passes through to Kentico automatically.
/// </summary>
public abstract class ChatCompletionServiceExtensionsBase : IChatCompletionService
{
    /// <summary>
    /// The original Kentico Aira chat completion service.
    /// </summary>
    protected IChatCompletionService Kentico { get; }

    private readonly IReadOnlyList<IAiraPlugin> _registeredPlugins;
    private readonly PluginResponseEnhancementFilter? _enhancementFilter;
    private readonly IReadOnlyDictionary<string, AiraPluginOptions>? _pluginOptionsByName;

    /// <summary>
    /// The provider type associated with this chat service,
    /// used to filter plugins via <see cref="IAiraPlugin.TargetProviders"/>.
    /// <c>null</c> means "Kentico built-in" — only plugins with no target restriction apply.
    /// </summary>
    protected virtual Type? AssociatedProviderType => null;

    protected ChatCompletionServiceExtensionsBase(
        [FromKeyedServices(AiraPluginServiceKeys.OriginalChat)] IChatCompletionService kenticoService,
        IEnumerable<IAiraPlugin> plugins,
        IAiraPluginRegistry registry)
    {
        Kentico = kenticoService ?? throw new ArgumentNullException(nameof(kenticoService));
        _registeredPlugins = plugins?.ToList() ?? [];

        // Create enhancement filter from the internal options dictionary.
        if (registry is AiraPluginRegistry concreteRegistry && concreteRegistry.AllOptions.Count > 0)
        {
            _pluginOptionsByName = concreteRegistry.AllOptions;
            _enhancementFilter = new PluginResponseEnhancementFilter(_pluginOptionsByName);
        }
    }

    /// <summary>
    /// Configures a <see cref="FunctionInvokingChatClient"/> to apply the same
    /// per-plugin <see cref="AiraPluginOptions.EnhancementPrompt"/> logic that
    /// <see cref="PluginResponseEnhancementFilter"/> provides for SK-based pipelines.
    /// Pass this method to <c>UseFunctionInvocation(configure:)</c> in M.E.AI pipelines.
    /// </summary>
    protected void AddEnhancementFunction(FunctionInvokingChatClient client)
    {
        if (_pluginOptionsByName == null || _pluginOptionsByName.Count == 0)
            return;

        var options = _pluginOptionsByName;

        client.FunctionInvoker = async (context, cancellationToken) =>
        {
            var result = await context.Function.InvokeAsync(context.Arguments, cancellationToken);

            // Extract plugin name from the M.E.AI naming convention "PluginName_FunctionName".
            var fullName = context.Function.Name;
            var separatorIndex = fullName.IndexOf('_');
            var pluginName = separatorIndex > 0 ? fullName.Substring(0, separatorIndex) : null;

            return PluginResponseEnhancer.TryEnhance(pluginName, result, options)
                ?? result;
        };
    }

    /// <summary>
    /// Registers all <see cref="IAiraPlugin"/> instances with the given kernel
    /// and adds the <see cref="PluginResponseEnhancementFilter"/> if configured.
    /// Called automatically before every chat call.
    /// </summary>
    protected void RegisterPlugins(Kernel? kernel)
    {
        if (kernel == null) return;

        var providerType = AssociatedProviderType;
        foreach (var plugin in _registeredPlugins)
        {
            bool shouldRegister = plugin.TargetProviders == null
                || plugin.TargetProviders.Count == 0
                || (providerType != null && plugin.TargetProviders.Contains(providerType));

            if (shouldRegister)
            {
                if (!kernel.Plugins.Contains(plugin.PluginName))
                {
                    kernel.Plugins.AddFromObject(plugin, plugin.PluginName);
                }
            }
            else
            {
                var existing = kernel.Plugins.FirstOrDefault(kernelPlugin => kernelPlugin.Name == plugin.PluginName);
                if (existing != null)
                {
                    kernel.Plugins.Remove(existing);
                }
            }
        }

        if (_enhancementFilter != null
            && !kernel.FunctionInvocationFilters.Contains(_enhancementFilter))
        {
            kernel.FunctionInvocationFilters.Add(_enhancementFilter);
        }
    }

    /// <inheritdoc />
    public virtual IReadOnlyDictionary<string, object?> Attributes => Kentico.Attributes;

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        RegisterPlugins(kernel);
        return await Kentico.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        RegisterPlugins(kernel);
        return Kentico.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
    }
}
