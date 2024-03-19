using System.Reflection;
using System.Text.Json;
using AnalyzerDiagnosticInfo.Mappers;
using Microsoft.CodeAnalysis;

namespace AnalyzerDiagnosticInfo.AnalyzerManager;

public static class AnalyzerManager
{
    private const string DefaultErrorCodesFilename = "DefaultCompilerCodes.json";
    public static IEnumerable<DiagnosticInfo> RetrieveAllAvailableAnalyzers(Project project, string? customCompilerCodesFilename)
    {
        var projectAnalyzers = RetrieveAllProjectAnalyzers(project);
        var compilerAnalyzers = RetrieveAllCompilerAnalyzers(CompilerCodesFilenamePath(customCompilerCodesFilename));
        return projectAnalyzers.Concat(compilerAnalyzers);
    }

    private static string CompilerCodesFilenamePath(string? customCompilerCodesFilename)
    {
        return string.IsNullOrEmpty(customCompilerCodesFilename)
            ? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DefaultErrorCodesFilename)
            : customCompilerCodesFilename;
    }
    
    public static IEnumerable<DiagnosticInfo> RetrieveAllProjectAnalyzers(Project project)
    {
        return project.AnalyzerReferences
            .SelectMany(analyzerReference => analyzerReference
                .GetAnalyzers(project.Language)
                .SelectMany(analyzer =>
                    {
                        return analyzer.SupportedDiagnostics
                            .GroupBy(x => x.Id)
                            .Select(x => x.First())
                            .Select(diagnostic => new DiagnosticInfo
                            {
                                Id = diagnostic.Id,
                                Title = diagnostic.Title,
                                Description = diagnostic.Description,
                                EnabledByDefault = diagnostic.IsEnabledByDefault,
                                DefaultSeverity = diagnostic.DefaultSeverity,
                                EffectiveSeverity = diagnostic.GetEffectiveSeverity(project.CompilationOptions!),
                                FinalSeverity = 
                                    (SeverityMapper.Map(diagnostic.GetEffectiveSeverity(project.CompilationOptions!))
                                     ?? SeverityMapper.Map(diagnostic.DefaultSeverity))!.Value,
                                Category = diagnostic.Category,
                                LinkUrl = string.IsNullOrEmpty(diagnostic.HelpLinkUri)
                                    ? null
                                    : diagnostic.HelpLinkUri,
                                AnalyzerInfo = new AnalyzerInfo
                                {
                                    ReferenceId = analyzerReference.Id.ToString(),
                                    ReferenceDisplay = analyzerReference.Display,
                                    AnalyzerName = analyzer.ToString()
                                }
                            });
                    }
                )
            );
    }

    public static IEnumerable<DiagnosticInfo> RetrieveAllCompilerAnalyzers(string? filePath)
    {
        var path = CompilerCodesFilenamePath(filePath);
        var readJson = File.ReadAllText(path);
        var jsonContent = JsonSerializer.Deserialize<IEnumerable<CompilerError>>(readJson);
        if (jsonContent != null)
        {
            Console.WriteLine(jsonContent.Count());
            return jsonContent.Select(DiagnosticInfoMapper.Map);
        }
        
        Console.WriteLine("Could not load any compiler analyzers");
        return Array.Empty<DiagnosticInfo>();
    }
}
