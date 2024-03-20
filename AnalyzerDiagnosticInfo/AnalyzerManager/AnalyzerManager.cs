using System.Reflection;
using AnalyzerDiagnosticInfo.CompilerCodesGenerator;
using AnalyzerDiagnosticInfo.Mappers;
using AnalyzerDiagnosticInfo.Output;
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
        var currentLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(currentLocation))
        {
            throw new Exception("Could not load assembly location");
        }
        
        return string.IsNullOrEmpty(customCompilerCodesFilename)
            ? Path.Combine(currentLocation, DefaultErrorCodesFilename)
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
                                Title = diagnostic.Title.ToString(),
                                Description = diagnostic.Description.ToString(),
                                EnabledByDefault = diagnostic.IsEnabledByDefault,
                                DefaultSeverity = diagnostic.DefaultSeverity,
                                EffectiveSeverity = diagnostic.GetEffectiveSeverity(project.CompilationOptions!),
                                FinalSeverity = 
                                    (Mapper.Map(diagnostic.GetEffectiveSeverity(project.CompilationOptions!))
                                     ?? Mapper.Map2(diagnostic.DefaultSeverity))!.Value,
                                Category = diagnostic.Category,
                                LinkUrl = string.IsNullOrEmpty(diagnostic.HelpLinkUri)
                                    ? null
                                    : diagnostic.HelpLinkUri,
                                AnalyzerInfo = new AnalyzerInfo
                                {
                                    ReferenceId = analyzerReference.Id.ToString() ?? string.Empty,
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
        var jsonContent = Converter.Deserialize<IEnumerable<CompilerCode>>(readJson);
        if (jsonContent != null)
        {
            return jsonContent.Select(Mapper.Map);
        }
        
        Console.WriteLine("Could not load any compiler analyzers");
        return Array.Empty<DiagnosticInfo>();
    }

    public static IEnumerable<PluginConfigAudit> RetrievePreviouslyGeneratedAnalyzers(string filePath)
    {
        var textJson = File.ReadAllText(filePath);
        var data = Converter.Deserialize<PluginConfig>(textJson);
        if (data != null)
        {
            return data.Audits;
        }
        
        Console.WriteLine("Could not load previously generated analyzers info");
        return Array.Empty<PluginConfigAudit>();
    }
}
