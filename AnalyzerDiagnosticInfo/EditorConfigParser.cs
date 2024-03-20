using AnalyzerDiagnosticInfo.Mappers;
using EditorConfig.Core;
using Microsoft.CodeAnalysis;

namespace AnalyzerDiagnosticInfo;

public static class EditorConfigManager
{
    public static IEnumerable<ConfigFileMetadata> GetAllConfigFiles(Project project)
    {
        var parser = new EditorConfigParser();
        var allProjectReferencedFiles = project.AnalyzerConfigDocuments;
        var allConfigsManually = GetAllConfigFiles2(parser, project.FilePath ?? string.Empty);

        return allProjectReferencedFiles
                // TODO: allows one level .editorconfig SLN + project
            // .Where(x => x.Folders.Count == 0)
            .GroupJoin(
                allConfigsManually,
                x => x.FilePath,
                x => Path.Combine(x.Directory, x.FileName),
                (document, files) => new ConfigFileMetadata
                {
                    AnalyzerConfigDocument = document,
                    EditorConfigFile = files.FirstOrDefault()
                })
            .Select(ParseFile);
    }

    public static IEnumerable<EditorConfigFile> GetAllConfigFiles2(EditorConfigParser parser, string projectPath)
    {
        return parser.GetConfigurationFilesTillRoot(projectPath);
    }

    public static ConfigFileMetadata ParseFile(ConfigFileMetadata metadata)
    {
        if (metadata.EditorConfigFile != null)
        {
            return metadata;
        }

        metadata.AnalyzerConfigDocumentParsed = EditorConfigFile.Parse(metadata.AnalyzerConfigDocument.FilePath);
        return metadata;
    }

    public static IEnumerable<DiagnosticInfo> ApplyConfigToAnalyzerInfo(IEnumerable<DiagnosticInfo> analyzers, IEnumerable<ConfigFileMetadata> configFiles)
    {
        var configFilesList = configFiles.ToList();
        var defaultFile = configFilesList.FirstOrDefault(x => x.IsDefault);
        var isGenerated = configFilesList.FirstOrDefault(x => x.IsGenerated);


        // update every severity, enable <-> disable  etc.
        var updateSeveritiesByDefaultConfig = HandleConfigFile(analyzers, defaultFile, false);
        var updateByGeneratedConfig = HandleConfigFile(updateSeveritiesByDefaultConfig, isGenerated, false);

        return configFilesList
            .Where(x => x is { IsDefault: false, IsGenerated: false })
            .OrderBy(x => x.NestingLevel)
            .Aggregate(
                updateByGeneratedConfig,
                // only enable changes for nested folder configs
                (acc, metadata) => HandleConfigFile(acc, metadata, !metadata.SolutionOrProjectLevel));
    }

    private static IEnumerable<DiagnosticInfo> HandleConfigFile(IEnumerable<DiagnosticInfo> analyzers, ConfigFileMetadata? configFile, bool onlyEnable)
    {
        if (configFile == null)
        {
            return analyzers;
        }

        return analyzers.Select(x =>
        {
            // TODO: how to handle if multiple sections contains one rule? enable vs disable?
            // in the same section the rules are overriden and the last one wins
            // for now take just the first one
            var severity = configFile.Sections
                .Select(y => RetrieveConfigLine(y, x.Id))
                .Where(y => y != null)
                .Select(y => Mapper.Map(y!.Value))
                .FirstOrDefault();

            
            if (severity == null) 
                return x;
            
            x.FinalSeverity = onlyEnable && severity == AnalyzerSeverity.None
                ? x.FinalSeverity
                : severity.Value;
            return x;
        });
    }

    private static EditorConfigSeverity? RetrieveConfigLine(ConfigSection section, string analyzerId)
    {
        var realKey = section.Keys.FirstOrDefault(x => x.Contains($"{analyzerId}.severity", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(realKey))
        {
            return null;
        }

        var value = section.GetValueOrDefault(realKey);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return Enum.TryParse<EditorConfigSeverity>(value, true, out var severity)
            ? severity
            : null;
    }
    
    
    
}

public class ConfigFileMetadata
{
    public AnalyzerConfigDocument AnalyzerConfigDocument { get; set; }
    public EditorConfigFile? EditorConfigFile { get; set; }
    
    public EditorConfigFile? AnalyzerConfigDocumentParsed { get; set; }

    public bool IsDefault => (AnalyzerConfigDocument.FilePath ?? string.Empty).EndsWith("globalconfig");

    public int NestingLevel => AnalyzerConfigDocument.Folders.Count;
    
    public bool IsGenerated => (AnalyzerConfigDocument.FilePath ?? string.Empty).Contains("GeneratedMSBuildEditorConfig");

    public IEnumerable<ConfigSection> Sections => EditorConfigFile != null
        ? EditorConfigFile.Sections
        : AnalyzerConfigDocumentParsed != null
            ? AnalyzerConfigDocumentParsed.Sections
            : Array.Empty<ConfigSection>();

    public bool SolutionOrProjectLevel => AnalyzerConfigDocument.Folders.Count == 0;
}
