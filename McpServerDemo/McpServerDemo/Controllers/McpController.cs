using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace McpServerDemo.Controllers
{
    [Route("mcp")]
    [ApiController]
    public class McpController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("MCP çalışıyor");
        }

        [HttpGet("containers")]
        public IActionResult GetContainers()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "ps --format \"{{.Names}}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var containers = output
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            return Ok(containers);
        }

        [HttpGet("logs/{containerName}")]
        public IActionResult GetLogs(string containerName)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"logs {containerName} --tail 50",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return Ok(output);
        }

        [HttpPost("query")]
        public async Task<IActionResult> RunQuery([FromBody] string sql)
        {
            using var conn = new Npgsql.NpgsqlConnection("Host=;Port=;Database=;Username=;Password=");
            await conn.OpenAsync();

            using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var results = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                results.Add(row);
            }

            return Ok(results);
        }



    }
}
