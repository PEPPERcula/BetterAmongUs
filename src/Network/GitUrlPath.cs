namespace BetterAmongUs.Network;

internal struct GitUrlPath(string folder)
{
    private const string BASE_URL = "https://raw.githubusercontent.com/D1GQ/BetterAmongUs";
    internal const string BRANCH = "main";

    internal static readonly GitUrlPath Repository = new(BRANCH);
    internal static readonly GitUrlPath RepositoryApi = new($"{BRANCH}/api");
    internal static readonly GitUrlPath News = new($"{BRANCH}/api/news");
    private readonly string _folder = folder;

    internal readonly string Combine(params string[] paths)
    {
        return $"{BASE_URL}/{_folder}/{string.Join("/", paths)}";
    }

    public override readonly string ToString()
    {
        return $"{BASE_URL}/{_folder}";
    }
}
