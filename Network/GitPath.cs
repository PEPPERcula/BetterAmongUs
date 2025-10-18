namespace BetterAmongUs.Network;

internal struct GitPath(string folder)
{
    internal static readonly GitPath Repository = new("main");
    internal static readonly GitPath News = new("main/ModNews");

    private const string BaseUrl = "https://raw.githubusercontent.com/D1GQ/BetterAmongUs";
    private readonly string _folder = folder;

    internal readonly string Combine(params string[] paths)
    {
        return $"{BaseUrl}/{_folder}/{string.Join("/", paths)}";
    }

    public override readonly string ToString()
    {
        return $"{BaseUrl}/{_folder}";
    }
}
