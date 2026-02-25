using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CMS.Membership;

using EXLRT.Xperience.AIRA.Plugins.Contracts;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages.Internal;

using Microsoft.SemanticKernel;

[assembly: UIPage(
    parentType: typeof(AiraApplication),
    slug: "aira-plugins",
    uiPageType: typeof(EXLRT.Xperience.AIRA.Plugins.UIPages.AiraPluginsPage),
    name: "Plugins",
    templateName: TemplateNames.OVERVIEW,
    order: 501)]

namespace EXLRT.Xperience.AIRA.Plugins.UIPages
{
    [UIEvaluatePermission(SystemPermissions.VIEW)]
    public class AiraPluginsPage : OverviewPageBase
    {
        private readonly IAiraPluginRegistry registry;

        public AiraPluginsPage(IAiraPluginRegistry registry)
        {
            this.registry = registry;
        }

        public override Task ConfigurePage()
        {
            PageConfiguration.Caption = "Plugins";

            // ── Plugins section (one card per plugin) ──
            if (registry.Plugins.Count > 0)
            {
                foreach (var plugin in registry.Plugins)
                {
                    var group = PageConfiguration.CardGroups.AddCardGroup();
                    group.AddCard(BuildPluginCard(plugin));
                }
            }
            else
            {
                var emptyGroup = PageConfiguration.CardGroups.AddCardGroup();
                emptyGroup.AddCard(new OverviewCard
                {
                    Headline = "No Plugins Registered",
                    Components = new List<IOverviewCardComponent>
                    {
                        new StringContentCardComponent
                        {
                            Content = "<p>Use <code>services.AddAiraPlugin&lt;T&gt;()</code> to register plugins.</p>",
                            ContentAsHtml = true
                        }
                    }
                });
            }

            return base.ConfigurePage();
        }

        private OverviewCard BuildPluginCard(IAiraPlugin plugin)
        {
            var type = plugin.GetType();
            var description = type.GetCustomAttribute<DescriptionAttribute>()?.Description;

            var targetNames = ResolveTargetProviderNames(plugin);
            var pluginOptions = registry.GetOptions(plugin.PluginName);

            var functions = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.GetCustomAttribute<KernelFunctionAttribute>() != null)
                .Select(method =>
                {
                    var kernelFunctionAttr = method.GetCustomAttribute<KernelFunctionAttribute>()!;
                    var functionName = kernelFunctionAttr.Name ?? method.Name;
                    var functionDescription = method.GetCustomAttribute<DescriptionAttribute>()?.Description;

                    return string.IsNullOrEmpty(functionDescription) ? functionName : $"{functionName}: {functionDescription}";
                })
                .ToList();

            var components = new List<IOverviewCardComponent>();
            if (!string.IsNullOrEmpty(description))
            {
                components.Add(new StringContentCardComponent { Content = description, ContentAsHtml = true });
            }

            if (functions.Count > 0)
            {
                components.Add(new UnorderedListCardComponent
                {
                    Items = functions.Select(function => new UnorderedListItem { Text = function }).ToList()
                });
            }

            var extraInfo = $"<strong>Target providers:</strong> {System.Net.WebUtility.HtmlEncode(targetNames)}";

            // Per-plugin enhancement status
            if (!string.IsNullOrEmpty(pluginOptions.EnhancementPrompt))
            {
                extraInfo += $"<br /><strong>Enhancement:</strong> {System.Net.WebUtility.HtmlEncode(pluginOptions.EnhancementPrompt)}";
            }

            components.Add(new StringContentCardComponent { Content = $"<p>{extraInfo}</p>", ContentAsHtml = true });

            return new OverviewCard
            {
                Headline = plugin.PluginName,
                Components = components
            };
        }

        private static string ResolveTargetProviderNames(IAiraPlugin plugin)
        {
            if (plugin.TargetProviders == null || plugin.TargetProviders.Count == 0)
                return "All providers";

            var names = plugin.TargetProviders.Select(type => type.Name).ToList();
            return string.Join(", ", names);
        }
    }
}
