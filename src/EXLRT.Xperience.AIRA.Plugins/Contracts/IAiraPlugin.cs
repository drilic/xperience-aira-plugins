using System;
using System.Collections.Generic;

namespace EXLRT.Xperience.AIRA.Plugins.Contracts;

/// <summary>
/// Marker interface for Semantic Kernel plugins that should be auto-registered
/// with the AI kernel. Implement this interface on any class that has
/// <c>[KernelFunction]</c> methods, then register it with
/// <c>services.AddAiraPlugin&lt;T&gt;()</c>.
/// </summary>
public interface IAiraPlugin
{
    /// <summary>
    /// The name used when registering this plugin with the kernel.
    /// Defaults to the class name.
    /// </summary>
    string PluginName => GetType().Name;

    /// <summary>
    /// Provider types this plugin is restricted to.
    /// Null or empty = available to all providers.
    /// Specify types to restrict which providers receive this plugin.
    /// </summary>
    IReadOnlyList<Type>? TargetProviders => null;
}
