using System.Collections.Generic;
using System.Text.Json;

namespace EXLRT.Xperience.AIRA.Plugins.Infrastructure;

/// <summary>
/// Shared enhancement logic used by both the SK <see cref="PluginResponseEnhancementFilter"/>
/// and the M.E.AI <c>FunctionInvoker</c> delegate in <see cref="ChatCompletionServiceExtensionsBase"/>.
/// </summary>
internal static class PluginResponseEnhancer
{
    /// <summary>
    /// If the given <paramref name="pluginName"/> has an <see cref="AiraPluginOptions.EnhancementPrompt"/>
    /// configured, converts <paramref name="resultValue"/> to a string and prepends the enhancement
    /// instruction. Returns <c>null</c> when no enhancement applies (caller should keep the original result).
    /// </summary>
    public static string? TryEnhance(
        string? pluginName,
        object? resultValue,
        IReadOnlyDictionary<string, AiraPluginOptions> optionsByPlugin)
    {
        if (string.IsNullOrEmpty(pluginName))
            return null;

        if (!optionsByPlugin.TryGetValue(pluginName, out var options)
            || string.IsNullOrEmpty(options.EnhancementPrompt))
            return null;

        var resultText = ConvertToString(resultValue);
        if (string.IsNullOrEmpty(resultText))
            return null;

        return $"[Enhancement instruction: {options.EnhancementPrompt}]\n\n{resultText}";
    }

    private static string? ConvertToString(object? value)
    {
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
