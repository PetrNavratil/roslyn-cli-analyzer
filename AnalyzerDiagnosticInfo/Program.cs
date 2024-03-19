using AnalyzerDiagnosticInfo;
using AnalyzerDiagnosticInfo.Cli;
using AnalyzerDiagnosticInfo.CompilerCodesGenerator;
using AnalyzerDiagnosticInfo.ConfigManager;
using AnalyzerDiagnosticInfo.SolutionManager;
using CommandLine;

var result = Parser.Default
    .ParseArguments<
        ProjectConfigOptions, 
        SolutionOptions, 
        ProjectDiagnosticOptions, 
        SolutionDiagnosticOptions,
        GenerateCompilerCodesFileOptions
        >(args)
    .WithParsed<ProjectConfigOptions>(x => ProjectConfig(x).Wait())
    .WithParsed<ProjectDiagnosticOptions>(x => ProjectDiagnostic(x).Wait())
    .WithParsed<SolutionOptions>(x => SolutionConfig(x).Wait())
    .WithParsed<SolutionDiagnosticOptions>(x => SolutionDiagnostic(x).Wait())
    .WithParsed<GenerateCompilerCodesFileOptions>(x => GenerateCompilerCodes(x).Wait());

if (result.Errors.Any())
{
    return;
}

return;


async Task ProjectConfig(ProjectConfigOptions projectOptions)
{
    var project = await SolutionManager.LoadProjectAsync(projectOptions.Path);
    var finalFormOfDiagnostic = ConfigManager.RetrieveProjectConfig(project, projectOptions.CustomCompilerOptionsFilepath);
    var json = Converter.ConvertWithStringEnums(finalFormOfDiagnostic);
    File.WriteAllText(projectOptions.OutputFilePath, json);
}

async Task ProjectDiagnostic(ProjectDiagnosticOptions projectDiagnosticOptions)
{
    var project = await SolutionManager.LoadProjectAsync(projectDiagnosticOptions.Path);
    var diagnostics = await DiagnosticGenerator.PerformDiagnostic(project, false);
    var diagnosticsJson = Converter.ConvertWithStringEnums(diagnostics);
    File.WriteAllText(projectDiagnosticOptions.OutputFilePath, diagnosticsJson);
}

async Task SolutionConfig(SolutionOptions solutionOptions)
{
    var solution = await SolutionManager.LoadSolutionAsync(solutionOptions.Path);
    var diagnostics = ConfigManager.RetrieveSolutionConfig(solution, solutionOptions.CustomCompilerOptionsFilepath);
    var diagnosticsJson = Converter.ConvertWithStringEnums(diagnostics);
    File.WriteAllText(solutionOptions.OutputFilePath, diagnosticsJson);
}

async Task SolutionDiagnostic(SolutionDiagnosticOptions projectOptions)
{
    var solution = await SolutionManager.LoadSolutionAsync(projectOptions.Path);
    var diagnostics = solution.Projects
        .Select(async x => await DiagnosticGenerator.PerformDiagnostic(x, false))
        .Select(x => x.Result);
    var diagnosticsJson = Converter.ConvertWithStringEnums(diagnostics);
    File.WriteAllText(projectOptions.OutputFilePath, diagnosticsJson);
}

async Task GenerateCompilerCodes(GenerateCompilerCodesFileOptions projectOptions)
{
    var codes = await CompilerCodesGenerator.GetErrorCodesAsync();
    var codesJson = Converter.ConvertWithNumericEnums(codes);
    File.WriteAllText(projectOptions.OutputFilePath, codesJson);
}
