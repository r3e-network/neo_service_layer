using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;

namespace NeoServiceLayer.Enclave.Execution.DotNet
{
    /// <summary>
    /// .NET runtime for executing C# functions
    /// </summary>
    public class Runtime
    {
        /// <summary>
        /// Executes a C# function
        /// </summary>
        /// <param name="functionCode">Function code</param>
        /// <param name="entryPoint">Entry point method</param>
        /// <param name="parameters">Function parameters</param>
        /// <param name="context">Execution context</param>
        /// <returns>Function result</returns>
        public async Task<object> ExecuteFunctionAsync(string functionCode, string entryPoint, Dictionary<string, object> parameters, Dictionary<string, object> context)
        {
            try
            {
                // Compile the function
                var assembly = CompileFunction(functionCode);
                
                // Find the entry point method
                var entryPointParts = entryPoint.Split('.');
                var className = entryPointParts.Length > 1 ? entryPointParts[0] : "UserFunction";
                var methodName = entryPointParts.Length > 1 ? entryPointParts[1] : entryPoint;
                
                var type = assembly.GetType(className);
                if (type == null)
                {
                    throw new Exception($"Class {className} not found");
                }
                
                var method = type.GetMethod(methodName);
                if (method == null)
                {
                    throw new Exception($"Method {methodName} not found in class {className}");
                }
                
                // Create an instance of the class
                var instance = Activator.CreateInstance(type);
                
                // Set context properties
                var contextProperty = type.GetProperty("Context");
                if (contextProperty != null)
                {
                    contextProperty.SetValue(instance, context);
                }
                
                // Call the method
                var result = method.Invoke(instance, new object[] { parameters });
                
                // Handle async methods
                if (result is Task task)
                {
                    await task;
                    var resultProperty = task.GetType().GetProperty("Result");
                    if (resultProperty != null)
                    {
                        result = resultProperty.GetValue(task);
                    }
                    else
                    {
                        result = null;
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error executing function: {ex.Message}");
                throw;
            }
        }
        
        private Assembly CompileFunction(string functionCode)
        {
            // Parse the function code
            var syntaxTree = CSharpSyntaxTree.ParseText(functionCode);
            
            // Create references to necessary assemblies
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)
            };
            
            // Create compilation
            var compilation = CSharpCompilation.Create(
                "UserFunction",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            
            // Emit the assembly
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                
                if (!result.Success)
                {
                    var errors = new List<string>();
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        if (diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            errors.Add(diagnostic.ToString());
                        }
                    }
                    
                    throw new Exception($"Compilation failed: {string.Join(Environment.NewLine, errors)}");
                }
                
                ms.Seek(0, SeekOrigin.Begin);
                
                // Load the assembly
                var assemblyLoadContext = new AssemblyLoadContext("UserFunction", true);
                return assemblyLoadContext.LoadFromStream(ms);
            }
        }
    }
}
