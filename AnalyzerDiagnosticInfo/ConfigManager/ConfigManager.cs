using Microsoft.CodeAnalysis;

namespace AnalyzerDiagnosticInfo.ConfigManager;

public static class ConfigManager
{
    public static ProjectConfig RetrieveProjectConfig(Project project, string? customCompilerCodesFilename)
    {
        var all = AnalyzerManager.AnalyzerManager.RetrieveAllAvailableAnalyzers(project,customCompilerCodesFilename);
        var configMetadata = EditorConfigManager.GetAllConfigFiles(project);
        return new ProjectConfig
        {
            Name = project.Name,
            Path = project.FilePath,
            Diagnostics = EditorConfigManager.ApplyConfigToAnalyzerInfo(all, configMetadata)
        };
    }

    public static IEnumerable<ProjectConfig> RetrieveSolutionConfig(Solution solution, string? customCompilerCodesFilename)
    {
        var compilerOptions = AnalyzerManager.AnalyzerManager.RetrieveAllCompilerAnalyzers(customCompilerCodesFilename);
        return solution.Projects.Select(x =>
        {
            var projectAnalyzers = AnalyzerManager.AnalyzerManager.RetrieveAllProjectAnalyzers(x);
            var configMetadata = EditorConfigManager.GetAllConfigFiles(x);
            return new ProjectConfig
            {
                Name = x.Name,
                Path = x.FilePath,
                Diagnostics =
                    EditorConfigManager.ApplyConfigToAnalyzerInfo(projectAnalyzers.Concat(compilerOptions),
                        configMetadata)
            };
        });
    }
}
