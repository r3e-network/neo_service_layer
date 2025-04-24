using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Service for managing function dependencies
    /// </summary>
    public class FunctionDependencyService : IFunctionDependencyService
    {
        private readonly ILogger<FunctionDependencyService> _logger;
        private readonly IFunctionDependencyRepository _dependencyRepository;
        private readonly IFunctionRepository _functionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionDependencyService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="dependencyRepository">Dependency repository</param>
        /// <param name="functionRepository">Function repository</param>
        public FunctionDependencyService(
            ILogger<FunctionDependencyService> logger,
            IFunctionDependencyRepository dependencyRepository,
            IFunctionRepository functionRepository)
        {
            _logger = logger;
            _dependencyRepository = dependencyRepository;
            _functionRepository = functionRepository;
        }

        /// <inheritdoc/>
        public async Task<FunctionDependency> AddDependencyAsync(Guid functionId, string name, string version, string type)
        {
            _logger.LogInformation("Adding dependency {Name}@{Version} of type {Type} to function {FunctionId}", name, version, type, functionId);

            try
            {
                // Check if the function exists
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Check if the dependency already exists
                var existingDependencies = await _dependencyRepository.GetByFunctionIdAsync(functionId);
                var existingDependency = existingDependencies.FirstOrDefault(d => d.Name == name && d.Type == type);
                if (existingDependency != null)
                {
                    throw new Exception($"Dependency already exists: {name}@{existingDependency.Version}");
                }

                // Create the dependency
                var dependency = new FunctionDependency
                {
                    Id = Guid.NewGuid(),
                    FunctionId = functionId,
                    Name = name,
                    Version = version,
                    Type = type,
                    Source = GetSourceFromType(type),
                    IsRequired = true,
                    IsDevelopmentDependency = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                return await _dependencyRepository.CreateAsync(dependency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding dependency {Name}@{Version} of type {Type} to function {FunctionId}", name, version, type, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionDependency> UpdateDependencyAsync(Guid id, string version)
        {
            _logger.LogInformation("Updating dependency {DependencyId} to version {Version}", id, version);

            try
            {
                // Get the dependency
                var dependency = await _dependencyRepository.GetByIdAsync(id);
                if (dependency == null)
                {
                    throw new Exception($"Dependency not found: {id}");
                }

                // Update the version
                dependency.Version = version;
                dependency.UpdatedAt = DateTime.UtcNow;

                return await _dependencyRepository.UpdateAsync(dependency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dependency {DependencyId} to version {Version}", id, version);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveDependencyAsync(Guid id)
        {
            _logger.LogInformation("Removing dependency {DependencyId}", id);

            try
            {
                // Get the dependency
                var dependency = await _dependencyRepository.GetByIdAsync(id);
                if (dependency == null)
                {
                    throw new Exception($"Dependency not found: {id}");
                }

                // Delete the dependency
                return await _dependencyRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing dependency {DependencyId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionDependency>> GetDependenciesAsync(Guid functionId)
        {
            _logger.LogInformation("Getting dependencies for function {FunctionId}", functionId);

            try
            {
                return await _dependencyRepository.GetByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dependencies for function {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionDependency> GetDependencyAsync(Guid id)
        {
            _logger.LogInformation("Getting dependency {DependencyId}", id);

            try
            {
                return await _dependencyRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dependency {DependencyId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> InstallDependenciesAsync(Guid functionId)
        {
            _logger.LogInformation("Installing dependencies for function {FunctionId}", functionId);

            try
            {
                // Get the function
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the dependencies
                var dependencies = await _dependencyRepository.GetByFunctionIdAsync(functionId);
                if (!dependencies.Any())
                {
                    _logger.LogInformation("No dependencies to install for function {FunctionId}", functionId);
                    return true;
                }

                // Group dependencies by type
                var dependenciesByType = dependencies.GroupBy(d => d.Type);

                // Install dependencies for each type
                foreach (var group in dependenciesByType)
                {
                    var type = group.Key;
                    var deps = group.ToList();

                    _logger.LogInformation("Installing {Count} dependencies of type {Type} for function {FunctionId}", deps.Count, type, functionId);

                    // Generate package file
                    var packageFileContent = await GeneratePackageFileForTypeAsync(functionId, deps, type);
                    var packageFileName = GetPackageFileNameForType(type);

                    // TODO: Implement actual installation logic based on the function runtime and dependency type
                    // For now, we'll just log the package file content
                    _logger.LogInformation("Generated package file {FileName} for function {FunctionId}:\n{Content}", packageFileName, functionId, packageFileContent);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing dependencies for function {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionDependencyUpdate>> CheckForUpdatesAsync(Guid functionId)
        {
            _logger.LogInformation("Checking for dependency updates for function {FunctionId}", functionId);

            try
            {
                // Get the function
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the dependencies
                var dependencies = await _dependencyRepository.GetByFunctionIdAsync(functionId);
                if (!dependencies.Any())
                {
                    _logger.LogInformation("No dependencies to check for updates for function {FunctionId}", functionId);
                    return new List<FunctionDependencyUpdate>();
                }

                var updates = new List<FunctionDependencyUpdate>();

                // Check for updates for each dependency
                foreach (var dependency in dependencies)
                {
                    // TODO: Implement actual update checking logic based on the dependency type and source
                    // For now, we'll just create a mock update
                    var currentVersion = dependency.Version;
                    var latestVersion = IncrementVersion(currentVersion);

                    var update = new FunctionDependencyUpdate
                    {
                        DependencyId = dependency.Id,
                        FunctionId = functionId,
                        Name = dependency.Name,
                        CurrentVersion = currentVersion,
                        LatestVersion = latestVersion,
                        LatestMajorVersion = IncrementMajorVersion(currentVersion),
                        LatestMinorVersion = IncrementMinorVersion(currentVersion),
                        LatestPatchVersion = IncrementPatchVersion(currentVersion),
                        IsMajorUpdate = false,
                        IsMinorUpdate = false,
                        IsPatchUpdate = true,
                        ReleaseDate = DateTime.UtcNow,
                        IsRecommended = true,
                        IsSecurityUpdate = false,
                        Severity = "low"
                    };

                    updates.Add(update);
                }

                return updates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for dependency updates for function {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionDependency>> UpdateAllDependenciesAsync(Guid functionId)
        {
            _logger.LogInformation("Updating all dependencies for function {FunctionId}", functionId);

            try
            {
                // Get the function
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the dependencies
                var dependencies = await _dependencyRepository.GetByFunctionIdAsync(functionId);
                if (!dependencies.Any())
                {
                    _logger.LogInformation("No dependencies to update for function {FunctionId}", functionId);
                    return new List<FunctionDependency>();
                }

                var updatedDependencies = new List<FunctionDependency>();

                // Update each dependency
                foreach (var dependency in dependencies)
                {
                    // TODO: Implement actual update logic based on the dependency type and source
                    // For now, we'll just increment the version
                    var currentVersion = dependency.Version;
                    var newVersion = IncrementVersion(currentVersion);

                    dependency.Version = newVersion;
                    dependency.UpdatedAt = DateTime.UtcNow;

                    var updatedDependency = await _dependencyRepository.UpdateAsync(dependency);
                    updatedDependencies.Add(updatedDependency);
                }

                return updatedDependencies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating all dependencies for function {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionDependency>> ParseDependenciesAsync(Guid functionId, string packageFileContent, string packageFileType)
        {
            _logger.LogInformation("Parsing dependencies from {PackageFileType} for function {FunctionId}", packageFileType, functionId);

            try
            {
                // Get the function
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                var dependencies = new List<FunctionDependency>();

                // Parse dependencies based on the package file type
                switch (packageFileType.ToLower())
                {
                    case "package.json":
                        dependencies = ParseNpmDependencies(functionId, packageFileContent);
                        break;
                    case "requirements.txt":
                        dependencies = ParsePipDependencies(functionId, packageFileContent);
                        break;
                    case "csproj":
                        dependencies = ParseNuGetDependencies(functionId, packageFileContent);
                        break;
                    default:
                        throw new Exception($"Unsupported package file type: {packageFileType}");
                }

                // Save the dependencies
                foreach (var dependency in dependencies)
                {
                    await _dependencyRepository.CreateAsync(dependency);
                }

                return dependencies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing dependencies from {PackageFileType} for function {FunctionId}", packageFileType, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GeneratePackageFileAsync(Guid functionId, string packageFileType)
        {
            _logger.LogInformation("Generating {PackageFileType} for function {FunctionId}", packageFileType, functionId);

            try
            {
                // Get the function
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the dependencies
                var dependencies = await _dependencyRepository.GetByFunctionIdAsync(functionId);
                if (!dependencies.Any())
                {
                    _logger.LogInformation("No dependencies to generate package file for function {FunctionId}", functionId);
                    return string.Empty;
                }

                // Get the dependencies of the specified type
                string type = GetTypeFromPackageFileType(packageFileType);
                var typedDependencies = dependencies.Where(d => d.Type == type).ToList();

                // Generate the package file
                return await GeneratePackageFileForTypeAsync(functionId, typedDependencies, type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating {PackageFileType} for function {FunctionId}", packageFileType, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionDependencyValidationResult>> ValidateDependenciesAsync(Guid functionId)
        {
            _logger.LogInformation("Validating dependencies for function {FunctionId}", functionId);

            try
            {
                // Get the function
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the dependencies
                var dependencies = await _dependencyRepository.GetByFunctionIdAsync(functionId);
                if (!dependencies.Any())
                {
                    _logger.LogInformation("No dependencies to validate for function {FunctionId}", functionId);
                    return new List<FunctionDependencyValidationResult>();
                }

                var validationResults = new List<FunctionDependencyValidationResult>();

                // Validate each dependency
                foreach (var dependency in dependencies)
                {
                    // TODO: Implement actual validation logic based on the dependency type and source
                    // For now, we'll just create a mock validation result
                    var validationResult = new FunctionDependencyValidationResult
                    {
                        DependencyId = dependency.Id,
                        FunctionId = functionId,
                        Name = dependency.Name,
                        Version = dependency.Version,
                        IsValid = true,
                        Message = "Dependency is valid",
                        Level = "info",
                        Code = "VALID",
                        HasVulnerabilities = false,
                        VulnerabilityCount = 0,
                        HighestVulnerabilitySeverity = "none",
                        IsDeprecated = false,
                        IsCompatible = true
                    };

                    validationResults.Add(validationResult);
                }

                return validationResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating dependencies for function {FunctionId}", functionId);
                throw;
            }
        }

        private string GetSourceFromType(string type)
        {
            return type.ToLower() switch
            {
                "npm" => "npm",
                "nuget" => "nuget",
                "pip" => "pip",
                _ => "custom"
            };
        }

        private string GetTypeFromPackageFileType(string packageFileType)
        {
            return packageFileType.ToLower() switch
            {
                "package.json" => "npm",
                "requirements.txt" => "pip",
                "csproj" => "nuget",
                _ => "custom"
            };
        }

        private string GetPackageFileNameForType(string type)
        {
            return type.ToLower() switch
            {
                "npm" => "package.json",
                "nuget" => "project.csproj",
                "pip" => "requirements.txt",
                _ => "dependencies.txt"
            };
        }

        private async Task<string> GeneratePackageFileForTypeAsync(Guid functionId, List<FunctionDependency> dependencies, string type)
        {
            switch (type.ToLower())
            {
                case "npm":
                    return GenerateNpmPackageFile(functionId, dependencies);
                case "pip":
                    return GeneratePipRequirementsFile(functionId, dependencies);
                case "nuget":
                    return GenerateNuGetProjectFile(functionId, dependencies);
                default:
                    return GenerateGenericDependenciesFile(functionId, dependencies);
            }
        }

        private string GenerateNpmPackageFile(Guid functionId, List<FunctionDependency> dependencies)
        {
            var packageJson = new
            {
                name = $"function-{functionId}",
                version = "1.0.0",
                description = "Neo Service Layer Function",
                main = "index.js",
                scripts = new
                {
                    test = "echo \"Error: no test specified\" && exit 1"
                },
                author = "",
                license = "ISC",
                dependencies = dependencies
                    .Where(d => !d.IsDevelopmentDependency)
                    .ToDictionary(d => d.Name, d => d.Version),
                devDependencies = dependencies
                    .Where(d => d.IsDevelopmentDependency)
                    .ToDictionary(d => d.Name, d => d.Version)
            };

            return JsonSerializer.Serialize(packageJson, new JsonSerializerOptions { WriteIndented = true });
        }

        private string GeneratePipRequirementsFile(Guid functionId, List<FunctionDependency> dependencies)
        {
            return string.Join("\n", dependencies.Select(d => $"{d.Name}=={d.Version}"));
        }

        private string GenerateNuGetProjectFile(Guid functionId, List<FunctionDependency> dependencies)
        {
            var packageReferences = string.Join("\n    ", dependencies.Select(d => $"<PackageReference Include=\"{d.Name}\" Version=\"{d.Version}\" />"));

            return $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    {packageReferences}
  </ItemGroup>
</Project>";
        }

        private string GenerateGenericDependenciesFile(Guid functionId, List<FunctionDependency> dependencies)
        {
            return string.Join("\n", dependencies.Select(d => $"{d.Name}@{d.Version}"));
        }

        private List<FunctionDependency> ParseNpmDependencies(Guid functionId, string packageFileContent)
        {
            var dependencies = new List<FunctionDependency>();

            try
            {
                var packageJson = JsonSerializer.Deserialize<JsonElement>(packageFileContent);

                // Parse regular dependencies
                if (packageJson.TryGetProperty("dependencies", out var deps))
                {
                    foreach (var dep in deps.EnumerateObject())
                    {
                        var name = dep.Name;
                        var version = dep.Value.GetString();

                        dependencies.Add(new FunctionDependency
                        {
                            Id = Guid.NewGuid(),
                            FunctionId = functionId,
                            Name = name,
                            Version = version.TrimStart('^', '~'),
                            Type = "npm",
                            Source = "npm",
                            IsRequired = true,
                            IsDevelopmentDependency = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Parse dev dependencies
                if (packageJson.TryGetProperty("devDependencies", out var devDeps))
                {
                    foreach (var dep in devDeps.EnumerateObject())
                    {
                        var name = dep.Name;
                        var version = dep.Value.GetString();

                        dependencies.Add(new FunctionDependency
                        {
                            Id = Guid.NewGuid(),
                            FunctionId = functionId,
                            Name = name,
                            Version = version.TrimStart('^', '~'),
                            Type = "npm",
                            Source = "npm",
                            IsRequired = false,
                            IsDevelopmentDependency = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing NPM dependencies for function {FunctionId}", functionId);
                throw;
            }

            return dependencies;
        }

        private List<FunctionDependency> ParsePipDependencies(Guid functionId, string packageFileContent)
        {
            var dependencies = new List<FunctionDependency>();

            try
            {
                var lines = packageFileContent.Split('\n');

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    {
                        continue;
                    }

                    var match = Regex.Match(trimmedLine, @"^([a-zA-Z0-9_\-\.]+)(?:==|>=|<=|~=|!=|>|<)?([\d\.\*]+)?");
                    if (match.Success)
                    {
                        var name = match.Groups[1].Value;
                        var version = match.Groups[2].Success ? match.Groups[2].Value : "latest";

                        dependencies.Add(new FunctionDependency
                        {
                            Id = Guid.NewGuid(),
                            FunctionId = functionId,
                            Name = name,
                            Version = version,
                            Type = "pip",
                            Source = "pip",
                            IsRequired = true,
                            IsDevelopmentDependency = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing PIP dependencies for function {FunctionId}", functionId);
                throw;
            }

            return dependencies;
        }

        private List<FunctionDependency> ParseNuGetDependencies(Guid functionId, string packageFileContent)
        {
            var dependencies = new List<FunctionDependency>();

            try
            {
                var matches = Regex.Matches(packageFileContent, @"<PackageReference\s+Include=""([^""]+)""\s+Version=""([^""]+)""\s*/>");

                foreach (Match match in matches)
                {
                    var name = match.Groups[1].Value;
                    var version = match.Groups[2].Value;

                    dependencies.Add(new FunctionDependency
                    {
                        Id = Guid.NewGuid(),
                        FunctionId = functionId,
                        Name = name,
                        Version = version,
                        Type = "nuget",
                        Source = "nuget",
                        IsRequired = true,
                        IsDevelopmentDependency = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing NuGet dependencies for function {FunctionId}", functionId);
                throw;
            }

            return dependencies;
        }

        private string IncrementVersion(string version)
        {
            try
            {
                var parts = version.Split('.');
                if (parts.Length < 3)
                {
                    return version;
                }

                if (int.TryParse(parts[2], out var patch))
                {
                    parts[2] = (patch + 1).ToString();
                }

                return string.Join(".", parts);
            }
            catch
            {
                return version;
            }
        }

        private string IncrementMajorVersion(string version)
        {
            try
            {
                var parts = version.Split('.');
                if (parts.Length < 3)
                {
                    return version;
                }

                if (int.TryParse(parts[0], out var major))
                {
                    parts[0] = (major + 1).ToString();
                    parts[1] = "0";
                    parts[2] = "0";
                }

                return string.Join(".", parts);
            }
            catch
            {
                return version;
            }
        }

        private string IncrementMinorVersion(string version)
        {
            try
            {
                var parts = version.Split('.');
                if (parts.Length < 3)
                {
                    return version;
                }

                if (int.TryParse(parts[1], out var minor))
                {
                    parts[1] = (minor + 1).ToString();
                    parts[2] = "0";
                }

                return string.Join(".", parts);
            }
            catch
            {
                return version;
            }
        }

        private string IncrementPatchVersion(string version)
        {
            try
            {
                var parts = version.Split('.');
                if (parts.Length < 3)
                {
                    return version;
                }

                if (int.TryParse(parts[2], out var patch))
                {
                    parts[2] = (patch + 1).ToString();
                }

                return string.Join(".", parts);
            }
            catch
            {
                return version;
            }
        }
    }
}
