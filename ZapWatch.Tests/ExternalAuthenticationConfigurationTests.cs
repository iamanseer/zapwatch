using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ZapWatch.Tests;

// Regression coverage for the 2026-08-03 incident: registering an OAuth scheme with an empty
// ClientId crashed every single request in production, because AuthenticationMiddleware
// validates every registered scheme's options on every request, not just sign-in attempts.
// Program.cs's fix is to only call AddGoogle/AddMicrosoftAccount/AddGitHub when real credentials
// are configured. These tests pin down the underlying mechanism directly - cheap and fast, no DB
// or Hangfire required (unlike exercising Program.cs's actual startup).
public class ExternalAuthenticationConfigurationTests
{
    [Fact]
    public void Registering_an_OAuth_scheme_with_empty_ClientId_throws_on_options_resolution()
    {
        // Reproduces the exact crash: an unconditionally-registered scheme with an empty
        // ClientId throws the moment anything resolves its options - which AuthenticationMiddleware
        // does on every request for every registered scheme.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication().AddGoogle(options =>
        {
            options.ClientId = "";
            options.ClientSecret = "";
        });

        using var provider = services.BuildServiceProvider();
        var monitor = provider.GetRequiredService<IOptionsMonitor<GoogleOptions>>();

        Assert.Throws<ArgumentException>(() => monitor.Get(GoogleDefaults.AuthenticationScheme));
    }

    [Fact]
    public async Task Conditionally_skipping_registration_when_ClientId_is_empty_avoids_the_crash()
    {
        // Mirrors Program.cs's actual fix: only call AddGoogle when a real ClientId is present.
        var services = new ServiceCollection();
        services.AddLogging();

        var authBuilder = services.AddAuthentication();
        var clientId = ""; // simulates an unconfigured provider, same as production Microsoft/GitHub today
        var clientSecret = "";
        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
            });
        }

        using var provider = services.BuildServiceProvider();
        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();

        Assert.Null(await schemeProvider.GetSchemeAsync(GoogleDefaults.AuthenticationScheme));
    }

    [Fact]
    public async Task Registering_with_a_real_ClientId_makes_the_scheme_available()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var authBuilder = services.AddAuthentication();
        var clientId = "real-client-id";
        var clientSecret = "real-client-secret";
        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
            });
        }

        using var provider = services.BuildServiceProvider();
        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
        var monitor = provider.GetRequiredService<IOptionsMonitor<GoogleOptions>>();

        Assert.NotNull(await schemeProvider.GetSchemeAsync(GoogleDefaults.AuthenticationScheme));
        // Should not throw - a real ClientId/ClientSecret passes OAuthOptions.Validate().
        monitor.Get(GoogleDefaults.AuthenticationScheme);
    }
}
