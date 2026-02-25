using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.SemanticKernel;

namespace EXLRT.Xperience.AIRA.Plugins.Infrastructure;

/// <summary>
/// Semantic Kernel function invocation filter that prepends the per-plugin
/// <see cref="AiraPluginOptions.EnhancementPrompt"/> to the function result.
/// This allows the LLM to naturally incorporate the enhancement instruction
/// when processing the tool result.
/// </summary>
internal sealed class PluginResponseEnhancementFilter : IFunctionInvocationFilter
{
    private readonly IReadOnlyDictionary<string, AiraPluginOptions> _optionsByPlugin;

    public PluginResponseEnhancementFilter(IReadOnlyDictionary<string, AiraPluginOptions> optionsByPlugin)
    {
        _optionsByPlugin = optionsByPlugin ?? throw new ArgumentNullException(nameof(optionsByPlugin));
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        await next(context);

        var enhanced = PluginResponseEnhancer.TryEnhance(
            context.Function.PluginName,
            context.Result.GetValue<object>(),
            _optionsByPlugin);

        if (enhanced != null)
        {
            context.Result = new FunctionResult(context.Function, enhanced);
        }
    }
}
