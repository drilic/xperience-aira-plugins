using System;
using System.Collections.Generic;
using System.Linq;

using EXLRT.Xperience.AIRA.Plugins.Contracts;
using EXLRT.Xperience.AIRA.Plugins.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EXLRT.Xperience.AIRA.Plugins;

public static class ServiceCollectionExtensions
{
    // Options accumulated during AddAiraPlugin<T> calls, keyed by Type.
    private static readonly Dictionary<Type, AiraPluginOptions> PluginOptionsByType = new();

    /// <summary>
    /// Registers an <see cref="IAiraPlugin"/> so it is automatically added to the
    /// Semantic Kernel when the chat completion service is invoked.
    /// Optionally configure per-plugin options such as <see cref="AiraPluginOptions.EnhancementPrompt"/>.
    /// </summary>
    public static IServiceCollection AddAiraPlugin<TPlugin>(
        this IServiceCollection services,
        Action<AiraPluginOptions>? configure = null)
        where TPlugin : class, IAiraPlugin
    {
        services.AddSingleton<IAiraPlugin, TPlugin>();

        var options = new AiraPluginOptions();
        configure?.Invoke(options);
        PluginOptionsByType[typeof(TPlugin)] = options;

        return services;
    }

    /// <summary>
    /// Wraps Kentico's chat service to inject registered <see cref="IAiraPlugin"/> instances
    /// into the kernel — without replacing the underlying LLM.
    /// Call after <c>AddKentico()</c>. Use <c>AddAiraPlugin&lt;T&gt;()</c> to register plugins.
    /// </summary>
    public static IServiceCollection UseAiraPlugins(this IServiceCollection services)
    {
        CaptureKenticoChatService(services);

        services.AddKeyedSingleton<IChatCompletionService>("Aira", (serviceProvider, _) =>
            ActivatorUtilities.CreateInstance<PluginInjectionChatService>(serviceProvider));

        services.AddSingleton<IAiraPluginRegistry>(serviceProvider =>
        {
            var plugins = serviceProvider.GetServices<IAiraPlugin>().ToList();

            // Re-key options by actual PluginName (resolved at runtime from DI instances).
            var optionsByName = new Dictionary<string, AiraPluginOptions>();
            foreach (var plugin in plugins)
            {
                if (PluginOptionsByType.TryGetValue(plugin.GetType(), out var opts))
                {
                    optionsByName[plugin.PluginName] = opts;
                }
            }

            return AiraPluginRegistry.Create(plugins, optionsByName);
        });

        return services;
    }

    private static void CaptureKenticoChatService(IServiceCollection services)
    {
        var descriptor = services.LastOrDefault(
            registration => registration.IsKeyedService
              && registration.ServiceType == typeof(IChatCompletionService)
              && string.Equals(registration.ServiceKey as string, "Aira", StringComparison.Ordinal));

        if (descriptor != null)
        {
            services.AddKeyedSingleton<IChatCompletionService>("Kentico", (serviceProvider, _) =>
            {
                if (descriptor.ImplementationFactory != null)
                    return (IChatCompletionService)descriptor.ImplementationFactory(serviceProvider);
                if (descriptor.KeyedImplementationFactory != null)
                    return (IChatCompletionService)descriptor.KeyedImplementationFactory(serviceProvider, "Aira");
                if (descriptor.ImplementationInstance != null)
                    return (IChatCompletionService)descriptor.ImplementationInstance;
                if (descriptor.KeyedImplementationInstance != null)
                    return (IChatCompletionService)descriptor.KeyedImplementationInstance;
                return (IChatCompletionService)ActivatorUtilities.CreateInstance(
                    serviceProvider, descriptor.ImplementationType ?? descriptor.KeyedImplementationType!);
            });
        }
    }
}
