using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceProviderValidationExtensions.Hosting;
using Xunit.Abstractions;

namespace Tests.Hosting;

public class HostFullReportingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public HostFullReportingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void DefaultBuilder()
    {
        var applicationBuilder = Host.CreateDefaultBuilder()
            .UseServiceProviderExtendedValidation(rb =>
                rb.OnDuplicateService(dsc => _testOutputHelper.WriteLine($"{dsc.ServiceType.DisplayName} is registered {dsc.ImplementationTypes.Count} times"))
                    .Except(typeof(IConfigureOptions<>))
                    .Except(typeof(IStartupValidator))
                    .Except(typeof(IOptionsChangeTokenSource<>))
                    .Except<ILoggerProvider>()
                );

        applicationBuilder.Build();
    }
}
