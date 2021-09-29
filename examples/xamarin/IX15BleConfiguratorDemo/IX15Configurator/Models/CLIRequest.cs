using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IX15Configurator.Models
{
    class CLIRequest
    {
        // Constants
        private static string DEFAULT_TYPE = "xbee-bt-cli";

        // Properties
        [JsonPropertyName("type")]
        public string Type { get; } = DEFAULT_TYPE;

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }

        [JsonPropertyName("cli-data")]
        public string Data { get; set; }

        [JsonPropertyName("idx")]
        public int Index { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        /// <summary>
        /// Class constructor. Instantiates a new <c>CLIRequest</c> with the given
        /// parameters.
        /// </summary>
        /// <param name="id">Command ID to execute.</param>
        /// <param name="timeout">Command execution timeout.</param>
        /// <param name="data">Command data in BASE64.</param>
        /// <param name="index">Command chunk index.</param>
        /// <param name="total">Total number of command chunks.</param>
        public CLIRequest(int id, int timeout, string data, int index, int total)
        {
            Id = id;
            Timeout = timeout;
            Data = data;
            Index = index;
            Total = total;
        }

        /// <summary>
        /// Composes the CLI command request to execute in JSON format.
        /// </summary>
        /// <param name="commandId">The CLI command ID.</param>
        /// <param name="command">The CLI command to execute.</param>
        /// <param name="timeout">The CLI command timeout.</param>
        /// <returns>The composed CLI command as a JSON string.</returns>
        public static string ComposeCLICommandRequest(int commandId, string command, int timeout)
        {
            byte[] commandBytes = Encoding.ASCII.GetBytes(command);
            string commandEncoded = Convert.ToBase64String(commandBytes);
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            CLIRequest cliRequest = new CLIRequest(commandId, timeout, commandEncoded, 1, 1);
            string jsonString = JsonSerializer.Serialize(cliRequest, serializeOptions);

            return jsonString;
        }
    }
}
