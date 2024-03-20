using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpYaml.Serialization;

namespace AnalyzerDiagnosticInfo.CompilerCodesGenerator;

public class CompilerCodesGenerator
{
    private const string ErrorCodesUrlFormat = "https://raw.githubusercontent.com/dotnet/roslyn/{0}/src/Compilers/CSharp/Portable/Errors/ErrorCode.cs";
    private const string ErrorResourcesUrl = "https://raw.githubusercontent.com/dotnet/roslyn/{0}/src/Compilers/CSharp/Portable/CSharpResources.resx";
    private const string DocBaseUrl = "https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/";
    private const string DocUrlTemplateFallback = "https://docs.microsoft.com/en-us/dotnet/csharp/misc/cs{0:D4}";
    private const string DocTableOfContentsUrl = "https://raw.githubusercontent.com/dotnet/docs/main/docs/csharp/language-reference/compiler-messages/toc.yml";
    private const string DefaultBranch = "main";
    
    public static async Task<IReadOnlyList<CompilerCodeOutput>> GetErrorCodesAsync()
    {
        using var client = new HttpClient();
        var enumMembers = await GetErrorCodeEnumMembersAsync(client, DefaultBranch);
        var messages = await GetResourceDictionaryAsync(client, DefaultBranch);
        var docRelativeUris = await GetDocRelativeUrisAsync(client);

        var docBaseUri = new Uri(DocBaseUrl);

        return enumMembers.Select(m => CompilerCodeOutput.Create(m, GetMessage, GetDocLink)).ToList();

        Uri GetDocLink(int value) => docRelativeUris.TryGetValue(value, out var relativeUrl)
            ? new KnownGoodUri(docBaseUri, relativeUrl)
            : new Uri(string.Format(DocUrlTemplateFallback, value));

        string GetMessage(string name) => messages.TryGetValue(name, out var msg) ? msg : "";
    }
    
    private static async Task<IReadOnlyList<EnumMemberDeclarationSyntax>> GetErrorCodeEnumMembersAsync(HttpClient client, string branchOrTag)
    {
        var url = string.Format(ErrorCodesUrlFormat, branchOrTag);
        var errorCodesFileContent = await client.GetStringAsync(url);
        var syntaxTree = CSharpSyntaxTree.ParseText(errorCodesFileContent);
        var root = await syntaxTree.GetRootAsync();
        var enumDeclaration =
            root.DescendantNodes()
                .OfType<EnumDeclarationSyntax>()
                .First(e => e.Identifier.ValueText == "ErrorCode");
        return enumDeclaration.Members;
    }

    private static async Task<IReadOnlyDictionary<string, string>> GetResourceDictionaryAsync(HttpClient client, string branchOrTag)
    {
        var url = string.Format(ErrorResourcesUrl, branchOrTag);
        var resourcesFileContent = await client.GetStringAsync(url);
        var doc = XDocument.Parse(resourcesFileContent);
        var dictionary =
            doc.Root!.Elements("data")
                .ToDictionary(
                    e => e.Attribute("name")!.Value,
                    e => e.Element("value")!.Value);
        return dictionary;
    }

    private static async Task<IReadOnlyDictionary<int, string>> GetDocRelativeUrisAsync(HttpClient client)
    {
        var tocContent = await client.GetStringAsync(DocTableOfContentsUrl);
        var serializer = new Serializer(new SerializerSettings
        {
            IgnoreUnmatchedProperties = true
        });
        
        var root = serializer.Deserialize<TocRoot>(tocContent);

        var codes = root!.Items
            // bad typing, it must be checked
            .Where(x => x.Items != null)
            .SelectMany(x => x.Items)
            .Aggregate(
                new Dictionary<int, string>(), 
                (acc, x) =>
                {
                    try
                    {

                        var code = int.Parse(Path.GetFileNameWithoutExtension(x.Name)[2..]);
                        var href = x.Href.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                            ? x.Href[..^3]
                            : x.Href;
                        acc.Add(code, href);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    return acc;
                });


        return root.Items
            .Where(x => !string.IsNullOrWhiteSpace(x.DisplayName))
            .Aggregate(codes, (acc, x) => Regex.Split(x.DisplayName, @"\s*,\s*")
                .Aggregate(acc, (acc2, y) =>
                {
                    try
                    {

                        var code = int.Parse(Path.GetFileNameWithoutExtension(y)![2..]);
                        var href = x.Href.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                            ? x.Href[..^3]
                            : x.Href;
                        acc2.Add(code, href);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    return acc;
                }));
    }
    
    private class TocRoot
    {
        [YamlMember("items")]
        public TocNode[] Items { get; set; }
    }

    private class TocNode
    {
        [YamlMember("name")]
        public string Name { get; set; }
        [YamlMember("displayName")]
        public string DisplayName { get; set; }
        [YamlMember("href")]
        public string Href { get; set; }
        [YamlMember("items")]
        public TocNode[] Items { get; set; }
    }

    private class KnownGoodUri : Uri
    {
        public KnownGoodUri(Uri baseUri, string relativeUri) : base(baseUri, relativeUri)
        {
        }
    }
}
