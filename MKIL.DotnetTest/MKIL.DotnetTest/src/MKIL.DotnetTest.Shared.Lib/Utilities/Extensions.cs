using System.Text.Json;

namespace MKIL.DotnetTest.Shared.Lib.Utilities
{
    public static class Extensions
    {
        public static string ToJson(this object? json)
        {
            var a = JsonSerializer.Serialize(json); 
            if (string.IsNullOrWhiteSpace(a))
                return "{}";
                
            return a;
        }
    }
}
