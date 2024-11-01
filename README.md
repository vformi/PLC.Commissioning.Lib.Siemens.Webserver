# dt.PLC.Commissioning.Lib.Siemens.Webserver
Webserver module that can be also used as a standalone library

# Example usage:
```csharp
using Serilog;
using System.Threading.Tasks;
using PLC.Commissioning.Lib.Siemens.Webserver.Controllers;
using Siemens.Simatic.S7.Webserver.API.Enums;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()               // Set the minimum log level to Debug
            .WriteTo.Console()
            .CreateLogger();

        // Initialize RPCController with IP address, username, and password
        var controller = await RPCController.InitializeAsync("192.168.60.1", "Everybody", "");

        // Use the initialized services
        await controller.ApiService.BrowseMethodsAsync();
        // Attempt to read the PLC operating mode
        var operatingMode = await controller.PlcController.GetOperatingModeAsync();
        if (operatingMode != null)
        {
            Log.Debug($"Current PLC Operating Mode: {operatingMode}");
        }
        else
        {
            Log.Debug("Failed to read PLC operating mode.");
        }
        // Attempt to change the PLC operating mode
        await controller.PlcController.ChangeOperatingModeAsync(ApiPlcOperatingMode.Stop);

        // Read and write variables using VariableService
        var readResponse = await controller.VariableService.ReadAsync<int>("\"Data\".Integer");
        await controller.VariableService.WriteAsync("\"Data\".Integer", 99);

        // Browse for variables
        await controller.VariableService.BrowseAsync(ApiPlcProgramBrowseMode.Children, "\"Data\"");
        await controller.VariableService.BrowseAsync(ApiPlcProgramBrowseMode.Var, "\"Data\".Integer");

        Log.Information("Operations completed.");
    }
}

```