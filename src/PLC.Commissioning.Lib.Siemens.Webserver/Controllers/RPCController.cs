using Serilog;
using System.Threading.Tasks;
using PLC.Commissioning.Lib.Siemens.Webserver.Services;
using Siemens.Simatic.S7.Webserver.API.Services;
using Siemens.Simatic.S7.Webserver.API.Services.RequestHandling;
using System.Net;
using System;
using System.Linq;

namespace PLC.Commissioning.Lib.Siemens.Webserver.Controllers
{
    /// <summary>
    /// The RPCController class handles initialization of services, including certificate validation and service creation.
    /// </summary>
    public class RPCController
    {
        private readonly IApiRequestHandler _requestHandler;
        public ApiService ApiService { get; }
        public ControllerService PlcController { get; }
        public VariableService VariableService { get; }

        /// <summary>
        /// Constructor for RPCController.
        /// </summary>
        /// <param name="requestHandler">The API request handler for making API calls.</param>
        public RPCController(IApiRequestHandler requestHandler)
        {
            _requestHandler = requestHandler;

            // Initialize the services
            ApiService = new ApiService(_requestHandler);
            PlcController = new ControllerService(_requestHandler);
            VariableService = new VariableService(_requestHandler);
        }

        /// <summary>
        /// Initializes the RPCController with the required IP address, username, and password to set up the services.
        /// </summary>
        /// <param name="ipAddress">The IP address of the PLC to connect to.</param>
        /// <param name="username">Username for PLC authentication.</param>
        /// <param name="password">Password for PLC authentication.</param>
        /// <returns>An instance of the initialized RPCController.</returns>
        public static async Task<RPCController> InitializeAsync(string ipAddress, string username, string password)
        {
            Log.Information("Initializing services...");

            // Set the certificate validation callback
            SetupCertificateValidation();

            // Create the service factory and request handler
            var serviceFactory = new ApiStandardServiceFactory();
            var requestHandler = await serviceFactory.GetApiHttpClientRequestHandlerAsync(ipAddress, username, password);

            return new RPCController(requestHandler);
        }

        /// <summary>
        /// Sets up certificate validation. This can be edited to handle certificate requirements.
        /// </summary>
        private static void SetupCertificateValidation()
        {
            Log.Information("Setting up global certificate validation callback...");
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        }
        
        /// <summary>
        /// Checks if the RPCController has the specified required API methods.
        /// </summary>
        /// <param name="requiredMethods">An array of method names that are required.</param>
        /// <returns>True if all required methods are available; otherwise, false.</returns>
        public bool HasRequiredMethods(string[] requiredMethods)
        {
            var availableMethods = ApiService.BrowseMethodsAsync().GetAwaiter().GetResult();
            bool allMethodsAvailable = true;

            foreach (var method in requiredMethods)
            {
                // Check if the required method is present in the available methods, case-insensitively
                if (!availableMethods.Any(m => string.Equals(m.Name, method, StringComparison.OrdinalIgnoreCase)))
                {
                    Log.Warning($"Method '{method}' is missing in the available API methods.");
                    allMethodsAvailable = false;
                }
            }
            return allMethodsAvailable;
        }
    }
}
