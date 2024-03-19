using System.Collections;

namespace AnalyzerDiagnosticInfo.Extensions;

public static class WhereIfExtensionMethod
{
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, bool condition)
    {
        return condition
            ? enumerable.Where(predicate)
            : enumerable;
    }
}
