namespace VersionHunterX;
public record Repo(string Id, string Name, string Description, string Url, string Source, int Stars)
{
    public string Subtitle => $"{Source}  ·  ★ {Stars}";
}
public record Asset(string Name, string Url, long Size)
{
    public string Label => Size > 0 ? $"{Name} ({Size / 1048576.0:0.0} МБ)" : Name;
}
public record Rel(string Tag, string Date, string PageUrl, List<Asset> Assets)
{
    public string Title => $"{Tag}  ·  {Date}";
}
public record DownloadRec(string Name, string Path, string Sha256, string Date)
{
    public string Subtitle => $"{Date}  ·  {Sha256}";
}
