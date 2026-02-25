using System;
using System.Collections.Generic;
using System.Linq;

using CMS;

using EXLRT.Xperience.AIRA.Plugins.Contracts;
using EXLRT.Xperience.AIRA.Plugins.Services;

[assembly: RegisterImplementation(typeof(IAiraPluginRegistry), typeof(AiraPluginRegistry), Lifestyle = CMS.Core.Lifestyle.Singleton)]

namespace EXLRT.Xperience.AIRA.Plugins.Services;

/// <summary>
/// Default implementation of <see cref="IAiraPluginRegistry"/>.
/// Auto-registered as a singleton fallback via <c>[assembly: RegisterImplementation]</c>.
/// Explicit calls to <c>UseAiraPlugins()</c> replace this with a fully configured instance.
/// </summary>
internal sealed class AiraPluginRegistry : IAiraPluginRegistry
{
    private static readonly AiraPluginOptions DefaultOptions = new();

    public IReadOnlyList<IAiraPlugin> Plugins { get; }

    /// <summary>
    /// Internal access to the per-plugin options dictionary.
    /// Used by <see cref="Infrastructure.ChatCompletionServiceExtensionsBase"/> to create the enhancement filter.
    /// </summary>
    internal IReadOnlyDictionary<string, AiraPluginOptions> AllOptions { get; }

    /// <summary>
    /// Parameterless constructor used by the auto-registration fallback.
    /// </summary>
    public AiraPluginRegistry()
    {
        Plugins = Array.Empty<IAiraPlugin>();
        AllOptions = new Dictionary<string, AiraPluginOptions>();
    }

    private AiraPluginRegistry(
        IEnumerable<IAiraPlugin> plugins,
        Dictionary<string, AiraPluginOptions> optionsByPlugin)
    {
        Plugins = plugins.ToList().AsReadOnly();
        AllOptions = optionsByPlugin;
    }

    public AiraPluginOptions GetOptions(string pluginName)
    {
        if (string.IsNullOrEmpty(pluginName))
            return DefaultOptions;

        return AllOptions.TryGetValue(pluginName, out var options) ? options : DefaultOptions;
    }

    /// <summary>
    /// Factory for creating from DI.
    /// </summary>
    internal static AiraPluginRegistry Create(
        IEnumerable<IAiraPlugin> plugins,
        Dictionary<string, AiraPluginOptions> optionsByPlugin)
        => new(plugins, optionsByPlugin);
}
