using CommandLine;

namespace AnalyzerDiagnosticInfo.Cli;

public abstract class BaseOptions 
{
    [Option('p', "path", Required = true, HelpText = "Project path must be defined")]
    public string Path { get; set; }
    
    [Option('o', "output", Required = true, HelpText = "Output file must be defined")]
    public string OutputFilePath { get; set; }
}

[Verb("print-project-config", HelpText = "Prints project config")]
public class ProjectConfigOptions : BaseOptions
{
    [Option('c', "compilerOptionPath", Required = false, HelpText = "Custom compiler codes filepath")]
    public string? CustomCompilerOptionsFilepath { get; set; }
}


[Verb("print-solution-config", HelpText = "Prints solution config")]
public class SolutionOptions : BaseOptions
{
    [Option('c', "compilerOptionPath", Required = false, HelpText = "Custom compiler codes filepath")]
    public string? CustomCompilerOptionsFilepath { get; set; }
}

[Verb("lint-project", HelpText = "Analyze project")]
public class ProjectDiagnosticOptions : BaseOptions
{
}

[Verb("lint-solution", HelpText = "Prints solution config")]
public class SolutionDiagnosticOptions : BaseOptions
{
}

[Verb("generate-compiler-codes", HelpText = "Generates new compiler error codes")]
public class GenerateCompilerCodesFileOptions
{
    [Option('o', "output", Required = true, HelpText = "Output file must be defined")]
    public string OutputFilePath { get; set; }
}