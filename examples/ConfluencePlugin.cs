using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using EXLRT.Xperience.AIRA.Plugins.Contracts;
using Microsoft.SemanticKernel;

namespace DancingGoat.Plugins;

/// <summary>
/// Semantic Kernel plugin that simulates a Confluence-like knowledge base.
/// Returns hardcoded project documentation: team roster, architecture overview, and project status.
/// Demonstrates: multiple kernel functions, hardcoded data sources, TargetProviders filtering.
/// </summary>
[Description("Provides project knowledge base data: team info, architecture, and project status.")]
public class ConfluencePlugin : IAiraPlugin
{
   [KernelFunction("get_project_team")]
   [Description("Gets the project team members with their roles, responsibilities, and contact emails, in case they can help you with your issues.")]
   public Task<string> GetProjectTeamAsync(CancellationToken ct = default)
   {
      return Task.FromResult(@"# Project Team — Dancing Goat Platform

1. Michael Turner (michael.turner@drilic.dev)
   Role: Solution Architect
   Defines overall system architecture, validates integration patterns, ensures platform scalability.

2. Sofia Ramirez (sofia.ramirez@drilic.dev)
   Role: Project Manager
   Coordinates timelines, manages stakeholders, tracks risks, ensures delivery milestones are met.

3. Daniel Novak (daniel.novak@drilic.dev)
   Role: Backend Lead
   Designs core APIs, reviews backend code, ensures performance, security, and maintainability.

4. Emma Johansson (emma.johansson@drilic.dev)
   Role: Frontend Lead
   Oversees SPA architecture, UI consistency, accessibility, and integration with backend services.

5. Liam O'Connor (liam.oconnor@drilic.dev)
   Role: DevOps Engineer
   Manages CI/CD pipelines, Docker environments, infrastructure provisioning, and deployment automation.

6. Ava Chen (ava.chen@drilic.dev)
   Role: QA Lead
   Defines test strategies, automation coverage, regression plans, and quality metrics.

7. Benjamin Clarke (ben.clarke@drilic.dev)
   Role: Security Engineer
   Performs security reviews, penetration testing coordination, and ensures compliance with security standards.

8. Isabella Rossi (isabella.rossi@drilic.dev)
   Role: UX Designer
   Designs user flows, wireframes, and ensures the product delivers intuitive user experiences.

9. Noah Patel (noah.patel@drilic.dev)
   Role: Database Architect
   Models data structures, optimizes queries, and manages database performance and migrations.

10. Olivia Schmidt (olivia.schmidt@drilic.dev)
    Role: Business Analyst
    Gathers requirements, translates business needs into technical specifications, and maintains documentation.

11. Lucas Ferreira (lucas.ferreira@drilic.dev)
    Role: Integration Specialist
    Manages third-party integrations, API contracts, and data synchronization workflows.

12. Mia Andersson (mia.andersson@drilic.dev)
    Role: Automation Engineer
    Builds automated test suites and supports CI integration of quality gates.

13. Ethan Williams (ethan.williams@drilic.dev)
    Role: Cloud Architect
    Defines cloud topology, networking, scaling strategies, and cost optimization practices.

14. Charlotte Dubois (charlotte.dubois@drilic.dev)
    Role: Scrum Master
    Facilitates sprint ceremonies, removes blockers, and ensures agile best practices are followed.

15. Alexander Petrov (alex.petrov@drilic.dev)
    Role: AI Engineer
    Integrates AI services, manages prompt engineering workflows, and optimizes inference performance.

16. Grace Kim (grace.kim@drilic.dev)
    Role: Technical Writer
    Prepares system documentation, API documentation, and end-user guides.

17. Mateo Silva (mateo.silva@drilic.dev)
    Role: Performance Engineer
    Conducts load testing, performance benchmarking, and identifies bottlenecks.

18. Hannah Müller (hannah.muller@drilic.dev)
    Role: Data Analyst
    Defines reporting dashboards, analytics tracking, and interprets business data insights.

19. Jack Thompson (jack.thompson@drilic.dev)
    Role: Support Engineer
    Handles production incidents, monitors logs, and ensures SLA adherence.

20. Elena Popescu (elena.popescu@drilic.dev)
    Role: Release Manager
    Coordinates release cycles, manages versioning, and ensures smooth production rollouts.");
   }

   [KernelFunction("get_architecture_overview")]
   [Description("Gets the high-level architecture overview of the Dancing Goat platform including tech stack and integrations.")]
   public Task<string> GetArchitectureOverviewAsync(CancellationToken ct = default)
   {
      return Task.FromResult(@"# Architecture Overview — Dancing Goat Platform

## Tech Stack
- Runtime: ASP.NET Core 10.0 on .NET 10
- CMS: Xperience by Kentico v31.1.2
- Database: SQL Server (dancinggoat_31.1.2)
- Frontend: Razor Views + LESS + Grunt pipeline
- AI: Kentico Aira + Semantic Kernel plugins + custom LLM providers

## Key Components
- Page Builder: composable widgets, sections, and inline editors for live-site content
- Email Builder: MJML-based email templates with drag-and-drop components
- Commerce Module: cart, checkout, pricing, promotions, and order management
- Admin UI: custom Xperience admin pages for AI provider and plugin management

## Integrations
- Anthropic Claude API (custom chat completion via EXLRT.Xperience.AIRA.Providers)
- OpenAI API (alternative provider support)
- Open-Meteo API (weather plugin for AI assistant)
- ASP.NET Identity (user authentication and authorization)

## Deployment
- Development: Kestrel on port 50760
- Separated Admin Mode supported for live-site-only deployments
- CI/CD: GitHub Actions pipeline with staging and production environments");
   }

   [KernelFunction("get_project_status")]
   [Description("Gets the current project status including sprint progress, blockers, and upcoming milestones.")]
   public Task<string> GetProjectStatusAsync(CancellationToken ct = default)
   {
      return Task.FromResult(@"# Project Status — Sprint 14 (Feb 17–28, 2026)

## Sprint Goal
Complete AI plugin framework and custom provider integration for Xperience by Kentico.

## Completed
- AIRA Plugin library (EXLRT.Xperience.AIRA.Plugins) — plugin registry, DI, admin UI
- AIRA Providers library (EXLRT.Xperience.AIRA.Providers) — multi-provider routing, admin page
- Weather plugin with live Open-Meteo API integration
- Confluence plugin with hardcoded knowledge base
- Anthropic Claude provider (chat completion + IAiraClient)
- Admin pages for plugin and provider introspection

## In Progress
- OpenAI provider refinement (emma.johansson@drilic.dev)
- Plugin documentation and usage guide (grace.kim@drilic.dev)

## Blockers
- None currently

## Upcoming Milestones
- Sprint 15 (Mar 3–14): E-commerce AI assistant plugin, content analysis tools
- Sprint 16 (Mar 17–28): Production deployment, performance testing, load testing");
   }
}
