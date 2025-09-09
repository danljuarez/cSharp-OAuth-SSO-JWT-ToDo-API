using System.Diagnostics;

namespace OauthSSOJwtTodoApiBackend.Helpers;

public static class SwaggerBrowserLauncher
{
    public static void Launch(IConfiguration config, IWebHostEnvironment env, IServiceProvider services, ILogger logger)
    {
        if (!env.IsDevelopment())
            return;

        var swaggerSection = config.GetSection("Swagger");

        bool autoLaunch = swaggerSection.GetValue<bool>("AutoLaunchBrowser");
        string? swaggerUrl = swaggerSection.GetValue<string>("Url");

        if (!autoLaunch || string.IsNullOrWhiteSpace(swaggerUrl))
            return;

        var browserProcess = new Process();

        try
        {
            if (OperatingSystem.IsWindows())
            {
                browserProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c start {swaggerUrl}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
            }
            else if (OperatingSystem.IsMacOS())
            {
                browserProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = swaggerUrl,
                    UseShellExecute = false
                };
            }
            else if (OperatingSystem.IsLinux())
            {
                browserProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = swaggerUrl,
                    UseShellExecute = false
                };
            }

            browserProcess.Start();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not open Swagger UI in browser.");
            return;
        }

        var lifetime = services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() =>
        {
            try
            {
                if (!browserProcess.HasExited)
                {
                    browserProcess.Kill(entireProcessTree: true);
                    browserProcess.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not close Swagger browser process.");
            }
        });
    }
}
