using AutomationPlanner.POC.Core.Interfaces;
using AutomationPlanner.POC.Models.Planner;

namespace AutomationPlanner.POC.Services.Loading;

public sealed class PlannerPackageLoader : IPlannerPackageLoader
{
    public async Task<PlannerPackage> LoadAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath)) throw new ArgumentException("Planner root path is required.", nameof(rootPath));
        if (!Directory.Exists(rootPath)) throw new DirectoryNotFoundException(rootPath);

        var skillPath = Path.Combine(rootPath, "SKILL.md");
        if (!File.Exists(skillPath)) throw new FileNotFoundException("Planner package must contain SKILL.md.", skillPath);

        var package = new PlannerPackage
        {
            RootPath = rootPath,
            SkillContent = await File.ReadAllTextAsync(skillPath, cancellationToken),
            LoadedAt = DateTimeOffset.UtcNow
        };

        await LoadFolderAsync(Path.Combine(rootPath, "references"), package.ReferenceFiles, rootPath, ["*.md", "*.markdown"], cancellationToken);
        await LoadFolderAsync(Path.Combine(rootPath, "tests"), package.TestFiles, rootPath, ["*.md", "*.json"], cancellationToken);
        await LoadFolderAsync(Path.Combine(rootPath, "mock-data"), package.MockDataFiles, rootPath, ["*.json"], cancellationToken);
        return package;
    }

    private static async Task LoadFolderAsync(string folder, IDictionary<string, string> target, string rootPath, string[] patterns, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(folder)) return;
        foreach (var pattern in patterns)
        {
            foreach (var file in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relative = Path.GetRelativePath(rootPath, file).Replace(Path.DirectorySeparatorChar, '/');
                target[relative] = await File.ReadAllTextAsync(file, cancellationToken);
            }
        }
    }
}
