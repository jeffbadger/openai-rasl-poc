using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Services.Loading;

public sealed class PlannerPackageLoader : IPlannerPackageLoader
{
    public async Task<PlannerPackage> LoadAsync(string rootPath, string? mockDataBasePath = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("Planner root path is required.", nameof(rootPath));
        if (!Directory.Exists(rootPath)) throw new DirectoryNotFoundException(rootPath);

        var skillPath = Path.Combine(rootPath, "SKILL.md");
        if (!File.Exists(skillPath)) throw new FileNotFoundException("Planner package must contain SKILL.md.", skillPath);

        var mockDataRoot = string.IsNullOrWhiteSpace(mockDataBasePath)
            ? Path.Combine(rootPath, "mock-data")
            : mockDataBasePath;

        var package = new PlannerPackage
        {
            RootPath = rootPath,
            MockDataRootPath = mockDataRoot,
            SkillContent = await File.ReadAllTextAsync(skillPath, cancellationToken),
            LoadedAt = DateTimeOffset.UtcNow
        };

        await LoadFolderAsync(Path.Combine(rootPath, "references"), package.ReferenceFiles, rootPath, ["*.md", "*.markdown"], cancellationToken);
        await LoadFolderAsync(Path.Combine(rootPath, "tests"), package.TestFiles, rootPath, ["*.md", "*.json"], cancellationToken);
        await LoadFolderAsync(mockDataRoot, package.MockDataFiles, mockDataRoot, ["*.json"], cancellationToken, "mock-data");
        return package;
    }

    private static async Task LoadFolderAsync(string folder, IDictionary<string, string> target, string rootPath, string[] patterns, CancellationToken cancellationToken, string? keyPrefix = null)
    {
        if (!Directory.Exists(folder)) return;
        foreach (var pattern in patterns)
        {
            foreach (var file in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relative = Path.GetRelativePath(rootPath, file).Replace(Path.DirectorySeparatorChar, '/');
                var key = string.IsNullOrWhiteSpace(keyPrefix) ? relative : $"{keyPrefix}/{relative}";
                target[key] = await File.ReadAllTextAsync(file, cancellationToken);
            }
        }
    }
}
