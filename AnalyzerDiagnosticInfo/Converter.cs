using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AnalyzerDiagnosticInfo;

public static class Converter
{
    public static string ConvertWithStringEnums(object value)
    {
        
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            }
        };
        
        return JsonConvert.SerializeObject(value, Formatting.Indented, jsonSettings);
    }
    
    public static string ConvertWithNumericEnums(object value)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
        return JsonConvert.SerializeObject(value, Formatting.Indented, jsonSettings);
    }

    public static T? Deserialize<T>(string jsonContent)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
        
        return JsonConvert.DeserializeObject<T>(jsonContent, jsonSettings);

    }
    
}
