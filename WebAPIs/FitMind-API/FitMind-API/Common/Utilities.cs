using System.Text.Json;

namespace FitMind_API.Common
{
    public static class Utilities
    {
        public static TTarget Map<TSource, TTarget>(TSource source)
        {
            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<TTarget>(json) ?? throw new InvalidOperationException("Mapping failed.");

        }

        public static TTarget Mapper<TSource, TTarget>(this TSource source)
        {
            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<TTarget>(json) ?? throw new InvalidOperationException("Mapping failed.");
        }

       
    }
}
