using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceProviderValidationExtensions.Hosting;

namespace ServiceProviderValidationExtensions.Tests.Hosting;

public class HostFullReportingTests
{
    [Fact]
    public void DefaultBuilder()
    {
        var duplicateServiceReports = new List<string>();

        void ConfigureReporting(ReportConfigurer rb)
        {
            rb.OnDuplicateService(
                dsc => duplicateServiceReports.Add($"{dsc.ServiceType.DisplayName} is registered {dsc.ImplementationTypes.Count} times"),
                c => c.Except(typeof(IConfigureOptions<>))
                    .Except(typeof(IStartupValidator))
                    .Except(typeof(IOptionsChangeTokenSource<>))
                    .Except<ILoggerProvider>());
        }

        Host.CreateDefaultBuilder().UseServiceProviderExtendedValidation(ConfigureReporting).Build();

        duplicateServiceReports
            .Should()
            .ContainSingle().Which
            .Should()
            .Be("Microsoft.Extensions.Logging.Console.ConsoleFormatter is registered 3 times");
    }
}
