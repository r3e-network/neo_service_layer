using System;
using System.Threading;

namespace NeoServiceLayer.Enclave
{
    public class SimpleProgram
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Neo Service Layer Enclave (Simple Version)...");
            Console.WriteLine("This is a simplified version for testing Docker setup.");
            
            // Keep the application running
            Console.WriteLine("Press Ctrl+C to exit.");
            
            // Simple infinite loop to keep the application running
            while (true)
            {
                Console.WriteLine("Enclave service is running...");
                Thread.Sleep(5000); // Sleep for 5 seconds
            }
        }
    }
}
