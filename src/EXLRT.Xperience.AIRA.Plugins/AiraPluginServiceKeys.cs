namespace EXLRT.Xperience.AIRA.Plugins;

/// <summary>
/// DI keyed-service keys used by the AIRA plugins library.
/// </summary>
public static class AiraPluginServiceKeys
{
    /// <summary>
    /// Key for the <c>IChatCompletionService</c> captured before
    /// the plugins library wraps it with plugin injection.
    /// </summary>
    public const string OriginalChat = "CustomAiraPlugins.OriginalKenticoAiraChat";
}
