using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunctionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FunctionController : ControllerBase
    {
        private static readonly List<FunctionModel> _functions = new List<FunctionModel>
        {
            new FunctionModel
            {
                Id = "1",
                Name = "Hello World",
                Description = "A simple hello world function",
                Runtime = "javascript",
                Code = "function main() { return 'Hello, World!'; }",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new FunctionModel
            {
                Id = "2",
                Name = "Calculate Sum",
                Description = "Calculates the sum of two numbers",
                Runtime = "javascript",
                Code = "function main(a, b) { return a + b; }",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new FunctionModel
            {
                Id = "3",
                Name = "Fetch Price",
                Description = "Fetches the price of a cryptocurrency",
                Runtime = "javascript",
                Code = "function main(symbol) { return { symbol: symbol, price: Math.random() * 10000 }; }",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        [HttpGet]
        public ActionResult<IEnumerable<FunctionModel>> GetAll()
        {
            return Ok(_functions);
        }

        [HttpGet("{id}")]
        public ActionResult<FunctionModel> GetById(string id)
        {
            var function = _functions.Find(f => f.Id == id);
            if (function == null)
            {
                return NotFound();
            }
            return Ok(function);
        }

        [HttpPost]
        public ActionResult<FunctionModel> Create(CreateFunctionRequest request)
        {
            var function = new FunctionModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                Runtime = request.Runtime,
                Code = request.Code,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _functions.Add(function);
            return CreatedAtAction(nameof(GetById), new { id = function.Id }, function);
        }

        [HttpPut("{id}")]
        public ActionResult<FunctionModel> Update(string id, UpdateFunctionRequest request)
        {
            var index = _functions.FindIndex(f => f.Id == id);
            if (index == -1)
            {
                return NotFound();
            }

            var function = _functions[index];
            function.Name = request.Name ?? function.Name;
            function.Description = request.Description ?? function.Description;
            function.Runtime = request.Runtime ?? function.Runtime;
            function.Code = request.Code ?? function.Code;
            function.UpdatedAt = DateTime.UtcNow;

            _functions[index] = function;
            return Ok(function);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            var index = _functions.FindIndex(f => f.Id == id);
            if (index == -1)
            {
                return NotFound();
            }

            _functions.RemoveAt(index);
            return NoContent();
        }

        [HttpPost("{id}/execute")]
        public ActionResult<ExecuteResult> Execute(string id, ExecuteRequest request)
        {
            var function = _functions.Find(f => f.Id == id);
            if (function == null)
            {
                return NotFound();
            }

            // Simulate function execution
            var result = new ExecuteResult
            {
                FunctionId = id,
                ExecutionId = Guid.NewGuid().ToString(),
                Status = "success",
                Result = $"Executed function '{function.Name}' with parameters: {string.Join(", ", request.Parameters ?? new string[0])}",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddSeconds(1),
                Duration = 1000 // milliseconds
            };

            return Ok(result);
        }
    }

    public class FunctionModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Runtime { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateFunctionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Runtime { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class UpdateFunctionRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Runtime { get; set; }
        public string? Code { get; set; }
    }

    public class ExecuteRequest
    {
        public string[]? Parameters { get; set; }
    }

    public class ExecuteResult
    {
        public string FunctionId { get; set; } = string.Empty;
        public string ExecutionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long Duration { get; set; }
    }
}
