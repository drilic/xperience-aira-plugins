# xperience-aira-plugins

The xperience-aira-plugins project is a plugin framework for extending Kentico's Aira AI assistant with custom Semantic Kernel plugins. It enables registration and injection of custom AI functions into the Aira chat completion pipeline without replacing Kentico's underlying LLM provider.

![Aira Plugins](img/aira-plugins.png)

> This library is designed as an extensibility layer on top of Kentico's built-in Aira integration. It uses the decorator pattern to wrap the existing `IChatCompletionService` and inject registered plugins into the Semantic Kernel before each chat call. Plugins are auto-discovered via marker interfaces and registered through dependency injection.

## Requirements

- **Kentico.Xperience.Admin 31.1.2** or newer version to use latest Xperience by Kentico
- **net10.0** as a long-term support (LTS) release
- **Microsoft.SemanticKernel** packages for plugin development

## Download & Installation

1. Download source code
2. Include downloaded library into your project
    * Copy folder _EXLRT.Xperience.AIRA.Plugins_ into your project
    * Add as existing project in Visual Studio/VS Code
    * Add project reference to main project
    * Rebuild solution

## Setup

### Create a Plugin

Create a class that implements `IAiraPlugin` and decorate methods with `[KernelFunction]` and `[Description]` attributes:

```csharp
using EXLRT.Xperience.AIRA.Plugins.Abstractions;
using Microsoft.SemanticKernel;
using System.ComponentModel;

[Description("Provides real-time weather data from the Open-Meteo API.")]
public class WeatherPlugin : IAiraPlugin
{
    // unique name of the plugin, used for identification in Aira and admin UI
    public string PluginName => "Weather";

    [KernelFunction("get_weather")]
    [Description("Gets the current weather for a given city")]
    public string GetWeather(string city)
    {
        // Do a custom logic here, call API, etc.
        return $"The weather in {city} is sunny.";
    }
}
```

### Register Plugins

Method definitions:
```csharp
// Register individual plugin (with optional per-plugin enhancement)
IServiceCollection AddAiraPlugin<TPlugin>(this IServiceCollection services, Action<AiraPluginOptions>? configure = null)
    where TPlugin : class, IAiraPlugin;

// Initialize plugin support and wrap chat completion service
IServiceCollection UseAiraPlugins(this IServiceCollection services);
```

Example of configuration (Program.cs):

```csharp
using EXLRT.Xperience.AIRA.Plugins;

// Initialize plugin support
builder.Services.UseAiraPlugins();

// Register plugins (before UseAiraPlugins)
builder.Services.AddAiraPlugin<WeatherPlugin>(options =>
{
    options.EnhancementPrompt = "Summarize weather naturally.";
});
builder.Services.AddAiraPlugin<ConfluencePlugin>(); // no enhancement
```

### Per-Plugin Response Enhancement (Optional)

Each plugin can have its own enhancement prompt configured at registration time. When set, the prompt is prepended to the plugin's function result via a Semantic Kernel `IFunctionInvocationFilter`, allowing the LLM to naturally incorporate the instruction when processing the tool result.

```csharp
builder.Services.AddAiraPlugin<WeatherPlugin>(options =>
{
    options.EnhancementPrompt = "Summarize weather data naturally in a conversational tone.";
});
```

- If `EnhancementPrompt` is set, enhancement is active for that plugin.
- If `EnhancementPrompt` is null or empty, the plugin's results are returned unchanged.
- Enhancement works with streaming since it modifies the tool result, not the final response.

> **Note:** The `IFunctionInvocationFilter`-based enhancement works when using Kentico's built-in Aira service (which uses the SK kernel natively). Custom providers using `Microsoft.Extensions.AI`'s `UseFunctionInvocation()` may not invoke SK's `kernel.FunctionInvocationFilters`.

### Admin UI

Once plugins are registered, an **Aira Plugins** page is automatically available in the Xperience admin under the Aira application. The page displays:

- A card for each registered plugin showing its name, description, available functions, target provider restrictions, and enhancement status
- A "No Plugins Registered" message if no plugins exist

## Disclaimer

This project is a **showcase and proof of concept** demonstrating how Kentico's Aira AI assistant can be extended and customized with plugins. It is intended to illustrate the possibilities of the Aira plugin architecture — how you can inject custom Semantic Kernel functions, configure per-plugin behavior, and integrate third-party AI providers alongside Kentico's built-in service.

This is **not an official Kentico product** and is not intended for production use without further review and hardening. Use it as a reference, learning resource, and starting point for building your own Aira customizations.

## Contributions and Support

Feel free to fork and submit pull requests or report issues to contribute. Either this way or another one, we will look into them as soon as possible.
