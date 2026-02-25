using System.Collections.Generic;

using EXLRT.Xperience.AIRA.Plugins.Infrastructure;
using EXLRT.Xperience.AIRA.Plugins.Contracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EXLRT.Xperience.AIRA.Plugins.Services;

/// <summary>
/// Pass-through wrapper that only injects plugins into the kernel
/// without replacing the underlying chat completion service.
/// </summary>
internal sealed class PluginInjectionChatService : ChatCompletionServiceExtensionsBase
{
    public PluginInjectionChatService(
        [FromKeyedServices(AiraPluginServiceKeys.OriginalChat)] IChatCompletionService kenticoService,
        IEnumerable<IAiraPlugin> plugins,
        IAiraPluginRegistry registry)
        : base(kenticoService, plugins, registry) { }
}
