using AutomationPlanner.POC.Services.Loading;
using Xunit;

namespace AutomationPlanner.POC.Tests;

public sealed class PlannerPackageLoaderTests
{
    [Fact]
    public async Task LoadAsync_LoadsSkillReferencesAndMockData()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "package");
        Directory.CreateDirectory(Path.Combine(root, "references", "nested"));
        Directory.CreateDirectory(Path.Combine(root, "mock-data"));
        await File.WriteAllTextAsync(Path.Combine(root, "SKILL.md"), "skill");
        await File.WriteAllTextAsync(Path.Combine(root, "references", "nested", "ref.md"), "reference");
        await File.WriteAllTextAsync(Path.Combine(root, "mock-data", "scenario.json"), "{}");

        var package = await new PlannerPackageLoader().LoadAsync(root);

        Assert.Equal("skill", package.SkillContent);
        Assert.Contains("references/nested/ref.md", package.ReferenceFiles.Keys);
        Assert.Contains("mock-data/scenario.json", package.MockDataFiles.Keys);
    }
    [Fact]
    public async Task LoadAsync_UsesExternalMockDataBasePathWhenProvided()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "package-external");
        var mockRoot = Path.Combine(AppContext.BaseDirectory, "external-mocks");
        Directory.CreateDirectory(Path.Combine(root, "references"));
        Directory.CreateDirectory(mockRoot);
        await File.WriteAllTextAsync(Path.Combine(root, "SKILL.md"), "skill");
        await File.WriteAllTextAsync(Path.Combine(mockRoot, "external-scenario.json"), "{}");

        var package = await new PlannerPackageLoader().LoadAsync(root, mockRoot);

        Assert.Equal(mockRoot, package.MockDataRootPath);
        Assert.Contains("mock-data/external-scenario.json", package.MockDataFiles.Keys);
    }
}
