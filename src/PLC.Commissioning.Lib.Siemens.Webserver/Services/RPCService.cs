using Serilog;
using Siemens.Simatic.S7.Webserver.API.Enums;
using Siemens.Simatic.S7.Webserver.API.Models;
using Siemens.Simatic.S7.Webserver.API.Services;
using Siemens.Simatic.S7.Webserver.API.Services.RequestHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using PLC.Commissioning.Lib.Siemens.Webserver.Abstractions;
using Siemens.Simatic.S7.Webserver.API.Models.Responses;

namespace PLC.Commissioning.Lib.Siemens.Webserver.Services
{
    /// <summary>
    /// Implementation of <see cref="IRPCService"/> for interacting with the Siemens PLC via the webserver API.
    /// </summary>
    public class RPCService : IRPCService
    {
        private IApiRequestHandler _requestHandler;
        private bool _isInitialized;

        /// <summary>
        /// Private constructor to enforce factory pattern usage.
        /// </summary>
        private RPCService() { }
        
        /// <inheritdoc/>
        public async Task InitializeAsync(string ipAddress, string username, string password)
        {
            if (_isInitialized)
            {
                Log.Warning("RPCService is already initialized.");
                return;
            }

            try
            {
                Log.Information("Initializing RPCService for IP: {IP}", ipAddress);

                // Set up certificate validation
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                var serviceFactory = new ApiStandardServiceFactory();
                _requestHandler = await serviceFactory.GetApiHttpClientRequestHandlerAsync(ipAddress, username, password);

                _isInitialized = true;
                Log.Information("RPCService initialized successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize RPCService.");
                throw;
            }
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<ApiClass>> BrowseMethodsAsync()
        {
            EnsureInitialized();
            try
            {
                var response = await _requestHandler.ApiBrowseAsync();
                Log.Information("Fetched {MethodCount} API methods.", response.Result.Count);
                return response.Result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to browse API methods.");
                return new List<ApiClass>();
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> HasRequiredMethodsAsync(string[] requiredMethods)
        {
            EnsureInitialized();
            var availableMethods = await BrowseMethodsAsync();
            return requiredMethods.All(method => availableMethods.Any(m => string.Equals(m.Name, method, StringComparison.OrdinalIgnoreCase)));
        }
        
        /// <inheritdoc/>
        public async Task<string> GetOperatingModeAsync()
        {
            EnsureInitialized();
            try
            {
                var response = await _requestHandler.PlcReadOperatingModeAsync();
                Log.Information("PLC operating mode is {Mode}.", response.Result);
                return response.Result.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read PLC operating mode.");
                return string.Empty;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> ChangeOperatingModeAsync(ApiPlcOperatingMode mode)
        {
            EnsureInitialized();
            try
            {
                var response = await _requestHandler.PlcRequestChangeOperatingModeAsync(mode);
                Log.Information("PLC mode change to {Mode} success: {Success}.", mode, response.Result);
                return response.Result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to change PLC operating mode to {Mode}.", mode);
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<T> ReadVariableAsync<T>(string variableName, ApiPlcProgramReadOrWriteMode mode = ApiPlcProgramReadOrWriteMode.Simple, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(variableName))
            {
                Log.Error("Invalid variable name provided for Read operation.");
                throw new ArgumentException("Variable name cannot be null or empty.", nameof(variableName));
            }

            try
            {
                Log.Information("Reading variable {Variable} with mode {Mode}.", variableName, mode);
                var response = await _requestHandler.PlcProgramReadAsync<T>(variableName, mode, cancellationToken);
                Log.Information("Read response for {Variable}: {Response}", variableName, response.Result);
                return response.Result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read variable {Variable}.", variableName);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> WriteVariableAsync(string variableName, object value, ApiPlcProgramReadOrWriteMode mode = ApiPlcProgramReadOrWriteMode.Simple, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(variableName))
            {
                Log.Error("Invalid variable name provided for Write operation.");
                return false;
            }
            if (value == null)
            {
                Log.Error("Null value provided for writing to variable {Variable}.", variableName);
                return false;
            }

            try
            {
                Log.Information("Writing to variable {Variable} with value {Value} and mode {Mode}.", variableName, value, mode);
                var response = await _requestHandler.PlcProgramWriteAsync(variableName, value, mode, cancellationToken);
                Log.Information("Write response for {Variable}: {Response}", variableName, response.Result);
                return response.Result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write to variable {Variable}.", variableName);
                return false;
            }
        }

        public async Task<ApiPlcProgramBrowseResponse> BrowseVariablesAsync(ApiPlcProgramBrowseMode mode, string variableName = null, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            try
            {
                Log.Information("Browsing variable {Variable} with mode {Mode}.", variableName ?? "root", mode);
                var response = await _requestHandler.PlcProgramBrowseAsync(mode, variableName, cancellationToken);
                Log.Information("Browse response for {Variable}: {Response}", variableName ?? "root", response.Result);
                return response;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to browse variable {Variable}.", variableName ?? "root");
                return new ApiPlcProgramBrowseResponse();
            }
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("RPCService is not initialized. Call InitializeAsync first.");
            }
        }

        /// <summary>
        /// Factory method to create and initialize an instance of RPCService.
        /// </summary>
        public static async Task<IRPCService> CreateAsync(string ipAddress, string username, string password)
        {
            var service = new RPCService();
            await service.InitializeAsync(ipAddress, username, password);
            return service;
        }
    }
}