using Siemens.Simatic.S7.Webserver.API.Enums;
using Siemens.Simatic.S7.Webserver.API.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Siemens.Simatic.S7.Webserver.API.Models.Responses;

namespace PLC.Commissioning.Lib.Siemens.Webserver.Abstractions
{
    /// <summary>
    /// Interface for RPC operations with the Siemens PLC via the webserver API.
    /// </summary>
    public interface IRPCService
    {
        /// <summary>
        /// Initializes the RPC service with the specified PLC connection details.
        /// </summary>
        /// <param name="ipAddress">The IP address of the PLC.</param>
        /// <param name="username">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        /// <returns>A task representing the asynchronous initialization operation.</returns>
        Task InitializeAsync(string ipAddress, string username, string password);

        /// <summary>
        /// Browses available API methods on the PLC.
        /// </summary>
        /// <returns>A task returning a list of available API methods.</returns>
        Task<IEnumerable<ApiClass>> BrowseMethodsAsync();

        /// <summary>
        /// Checks if the specified API methods are available.
        /// </summary>
        /// <param name="requiredMethods">An array of method names to check.</param>
        /// <returns>True if all required methods are available; otherwise, false.</returns>
        Task<bool> HasRequiredMethodsAsync(string[] requiredMethods);

        /// <summary>
        /// Reads the current operating mode of the PLC.
        /// </summary>
        /// <returns>A task returning the operating mode as a string.</returns>
        Task<string> GetOperatingModeAsync();

        /// <summary>
        /// Changes the operating mode of the PLC.
        /// </summary>
        /// <param name="mode">The desired operating mode (e.g., Run, Stop).</param>
        /// <returns>A task returning true if successful; otherwise, false.</returns>
        Task<bool> ChangeOperatingModeAsync(ApiPlcOperatingMode mode);

        /// <summary>
        /// Reads a variable from the PLC.
        /// </summary>
        /// <typeparam name="T">The expected data type of the variable.</typeparam>
        /// <param name="variableName">The name of the variable to read.</param>
        /// <param name="mode">The read mode (e.g., Simple or Raw).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task returning the variable's value.</returns>
        Task<T> ReadVariableAsync<T>(string variableName, ApiPlcProgramReadOrWriteMode mode = ApiPlcProgramReadOrWriteMode.Simple, CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes a value to a variable in the PLC.
        /// </summary>
        /// <param name="variableName">The name of the variable to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="mode">The write mode (e.g., Simple or Raw).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task returning true if successful; otherwise, false.</returns>
        Task<bool> WriteVariableAsync(string variableName, object value, ApiPlcProgramReadOrWriteMode mode = ApiPlcProgramReadOrWriteMode.Simple, CancellationToken cancellationToken = default);

        /// <summary>
        /// Browses variables or child elements in the PLC.
        /// </summary>
        /// <param name="mode">The browsing mode (e.g., Var or Children).</param>
        /// <param name="variableName">The name of the variable or element to browse (optional).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task returning the browse response.</returns>
        Task<ApiPlcProgramBrowseResponse> BrowseVariablesAsync(ApiPlcProgramBrowseMode mode, string variableName = null, CancellationToken cancellationToken = default);
    }
}