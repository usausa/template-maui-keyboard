namespace Template.MobileApp;

using Microsoft.Extensions.DependencyInjection;

using Smart.Mvvm.Resolver;

public sealed class ApplicationInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        // Setup provider
        ResolveProvider.Default.Provider = services;

#if DEBUG
        if (services is BunnyTail.DependencyInjection.GeneratedServiceProvider generatedProvider)
        {
            foreach (var line in BunnyTail.DependencyInjection.Diagnostics.ServiceFactoryReportExtensions.DescribeRuntimeFallbacks(generatedProvider).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                System.Diagnostics.Debug.WriteLine(line);
            }
        }
#endif
        // Setup navigator
        var navigator = services.GetRequiredService<INavigator>();
        navigator.Navigated += (_, args) =>
        {
            // for debug
            System.Diagnostics.Debug.WriteLine(
                $"Navigated: [{args.Context.FromId}]->[{args.Context.ToId}] : stacked=[{navigator.StackedCount}]");
        };
    }
}
