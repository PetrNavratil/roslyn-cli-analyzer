using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AnalyzerDiagnosticInfo;

public static class Converter
{
    public static string ConvertWithStringEnums(object value)
    {
        
        var jsonOptions = new StringEnumConverter();
        return JsonConvert.SerializeObject(value, Formatting.Indented, jsonOptions);
    }
    
    public static string ConvertWithNumericEnums(object value)
    {
        
        return JsonConvert.SerializeObject(value, Formatting.Indented);
    }
    
}
