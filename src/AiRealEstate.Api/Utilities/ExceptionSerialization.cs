using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;

namespace AiRealEstate.Api.Utilities;

public static class ExceptionSerialization
{
    public static string ToSafeJson(this Exception ex, bool includeStack, int maxDepth = 3)
    {
        var obj = ToSafeObject(ex, includeStack, maxDepth, 0);
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
        return JsonSerializer.Serialize(obj, options);
    }

    public static object ToSafeObject(this Exception ex, bool includeStack, int maxDepth, int depth)
    {
        if (depth >= maxDepth || ex == null) return new { message = ex?.Message };

        if (ex is RequestFailedException rfe)
        {
            return new
            {
                type = ex.GetType().FullName,
                message = ex.Message,
                status = rfe.Status,
                errorCode = rfe.ErrorCode,
                data = GetData(ex),
                stackTrace = includeStack ? ex.StackTrace : null,
                inner = rfe.InnerException != null ? ToSafeObject(rfe.InnerException, includeStack, maxDepth, depth + 1) : null
            };
        }

        if (ex is AggregateException agg)
        {
            return new
            {
                type = ex.GetType().FullName,
                message = ex.Message,
                data = GetData(ex),
                stackTrace = includeStack ? ex.StackTrace : null,
                inners = agg.InnerExceptions?.Take(5).Select(e => ToSafeObject(e, includeStack, maxDepth, depth + 1)).ToArray()
            };
        }

        return new
        {
            type = ex.GetType().FullName,
            message = ex.Message,
            data = GetData(ex),
            stackTrace = includeStack ? ex.StackTrace : null,
            inner = ex.InnerException != null ? ToSafeObject(ex.InnerException, includeStack, maxDepth, depth + 1) : null
        };
    }

    private static IDictionary<string, object?>? GetData(Exception ex)
    {
        if (ex.Data == null || ex.Data.Count == 0) return null;
        var dict = new Dictionary<string, object?>();
        foreach (var key in ex.Data.Keys.Cast<object>())
        {
            var k = key?.ToString() ?? "?";
            if (!dict.ContainsKey(k) && key is not null)
                dict[k] = ex.Data[key];
        }
        return dict;
    }
}
