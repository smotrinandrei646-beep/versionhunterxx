using System.Security.Cryptography;
using System.Text.Json;
namespace VersionHunterX;
public static class SearchService
{
    static readonly HttpClient Http = Make(TimeSpan.FromSeconds(20));
    static readonly HttpClient DlHttp = Make(Timeout.InfiniteTimeSpan);
    static HttpClient Make(TimeSpan t)
    {
        var h = new HttpClient { Timeout = t };
        h.DefaultRequestHeaders.UserAgent.ParseAdd("VersionHunterX/1.0");
        h.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        return h;
    }
    static string S(JsonElement e, string k) => e.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
    static int I(JsonElement e, string k) => e.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;
    static long L(JsonElement e, string k) => e.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt64() : 0;
    static async Task<JsonElement> Get(string url)
    {
        using var s = await Http.GetStreamAsync(url);
        using var d = await JsonDocument.ParseAsync(s);
        return d.RootElement.Clone();
    }
    static async Task<List<Repo>> Safe(Func<Task<List<Repo>>> f) { try { return await f(); } catch { return new(); } }

    public static async Task<List<Repo>> Search(string q, bool gh, bool gl, bool cb)
    {
        var e = Uri.EscapeDataString(q);
        var t = new List<Task<List<Repo>>>();
        if (gh) t.Add(Safe(async () => (await Get($"https://api.github.com/search/repositories?q={e}&per_page=15")).GetProperty("items").EnumerateArray()
            .Select(x => new Repo(S(x, "full_name"), S(x, "full_name"), S(x, "description"), S(x, "html_url"), "GitHub", I(x, "stargazers_count"))).ToList()));
        if (gl) t.Add(Safe(async () => (await Get($"https://gitlab.com/api/v4/projects?search={e}&per_page=15&order_by=star_count")).EnumerateArray()
            .Select(x => new Repo(I(x, "id").ToString(), S(x, "path_with_namespace"), S(x, "description"), S(x, "web_url"), "GitLab", I(x, "star_count"))).ToList()));
        if (cb) t.Add(Safe(async () => (await Get($"https://codeberg.org/api/v1/repos/search?q={e}&limit=15")).GetProperty("data").EnumerateArray()
            .Select(x => new Repo(S(x, "full_name"), S(x, "full_name"), S(x, "description"), S(x, "html_url"), "Codeberg", I(x, "stars_count"))).ToList()));
        return (await Task.WhenAll(t)).SelectMany(x => x).ToList();
    }

    public static async Task<List<Rel>> Releases(Repo r)
    {
        var list = new List<Rel>();
        if (r.Source == "GitLab")
        {
            var a = await Get($"https://gitlab.com/api/v4/projects/{r.Id}/releases?per_page=20");
            foreach (var x in a.EnumerateArray())
            {
                var assets = new List<Asset>();
                if (x.TryGetProperty("assets", out var ass))
                {
                    if (ass.TryGetProperty("links", out var ls))
                        foreach (var l in ls.EnumerateArray()) assets.Add(new(S(l, "name"), S(l, "direct_asset_url") is { Length: > 0 } d ? d : S(l, "url"), 0));
                    if (ass.TryGetProperty("sources", out var ss))
                        foreach (var l in ss.EnumerateArray()) if (S(l, "format") == "zip") assets.Add(new("Исходный код (zip)", S(l, "url"), 0));
                }
                list.Add(new(S(x, "tag_name"), Short(S(x, "released_at")), r.Url + "/-/releases", assets));
            }
        }
        else
        {
            var url = r.Source == "GitHub"
                ? $"https://api.github.com/repos/{r.Id}/releases?per_page=20"
                : $"https://codeberg.org/api/v1/repos/{r.Id}/releases?limit=20";
            var a = await Get(url);
            foreach (var x in a.EnumerateArray())
            {
                var assets = new List<Asset>();
                if (x.TryGetProperty("assets", out var ass))
                    foreach (var l in ass.EnumerateArray()) assets.Add(new(S(l, "name"), S(l, "browser_download_url"), L(l, "size")));
                var zip = S(x, "zipball_url");
                if (zip.Length > 0) assets.Add(new("Исходный код (zip)", zip, 0));
                list.Add(new(S(x, "tag_name"), Short(S(x, "published_at")), S(x, "html_url"), assets));
            }
        }
        return list;
    }
    static string Short(string d) => DateTime.TryParse(d, out var t) ? t.ToString("dd.MM.yyyy") : d;

    public static async Task<DownloadRec> Download(Asset a, IProgress<double> p)
    {
        var dir = Path.Combine(FileSystem.AppDataDirectory, "Downloads");
        Directory.CreateDirectory(dir);
        var name = string.Concat(a.Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var path = Path.Combine(dir, name);
        using var resp = await DlHttp.GetAsync(a.Url, HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();
        var total = resp.Content.Headers.ContentLength ?? a.Size;
        using (var src = await resp.Content.ReadAsStreamAsync())
        using (var dst = File.Create(path))
        {
            var buf = new byte[81920]; long n = 0; int r;
            while ((r = await src.ReadAsync(buf)) > 0)
            {
                await dst.WriteAsync(buf.AsMemory(0, r)); n += r;
                if (total > 0) p.Report((double)n / total);
            }
        }
        return new(a.Name, path, await Sha(path), DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
    }
    public static async Task<string> Sha(string path)
    {
        using var fs = File.OpenRead(path);
        return Convert.ToHexString(await SHA256.HashDataAsync(fs)).ToLowerInvariant();
    }
}
