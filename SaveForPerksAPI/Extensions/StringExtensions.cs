namespace SaveForPerksAPI.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) 
            ? "_" + x.ToString() 
            : x.ToString())).ToLower();
    }

    public static string ToPascalCase(this string str)
    {
        return string.Join("", str.Split('_')
            .Select(word => char.ToUpper(word[0]) + word.Substring(1)));
    }
}