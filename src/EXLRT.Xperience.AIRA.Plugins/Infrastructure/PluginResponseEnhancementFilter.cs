using System;
using System.Collections.Generic;
using System.Text.Json;
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

        var pluginName = context.Function.PluginName;
        if (string.IsNullOrEmpty(pluginName))
            return;

        if (!_optionsByPlugin.TryGetValue(pluginName, out var options)
            || string.IsNullOrEmpty(options.EnhancementPrompt))
            return;

        var result = context.Result;
        var resultText = ConvertResultToString(result);

        if (string.IsNullOrEmpty(resultText))
            return;

        var enhanced = $"[Enhancement instruction: {options.EnhancementPrompt}]\n\n{resultText}";
        context.Result = new FunctionResult(context.Function, enhanced);
    }

    private static string? ConvertResultToString(FunctionResult result)
    {
        var value = result.GetValue<object>();
        if (value == null)
            return null;

        if (value is string s)
            return s;

        try
        {
            return JsonSerializer.Serialize(value);
        }
        catch
        {
            return value.ToString();
        }
    }
}
