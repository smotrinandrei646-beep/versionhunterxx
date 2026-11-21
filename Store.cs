using System.Collections.ObjectModel;
using System.Text.Json;
namespace VersionHunterX;
public static class Store
{
    public static ObservableCollection<Repo> Favorites = Load<Repo>("fav");
    public static ObservableCollection<string> History = Load<string>("hist");
    public static ObservableCollection<DownloadRec> Downloads = Load<DownloadRec>("dl");

    static ObservableCollection<T> Load<T>(string key)
    {
        try { return new(JsonSerializer.Deserialize<List<T>>(Preferences.Get(key, "[]")) ?? new()); }
        catch { return new(); }
    }
    public static void Save()
    {
        Preferences.Set("fav", JsonSerializer.Serialize(Favorites.ToList()));
        Preferences.Set("hist", JsonSerializer.Serialize(History.ToList()));
        Preferences.Set("dl", JsonSerializer.Serialize(Downloads.ToList()));
    }
    public static void ToggleFav(Repo r)
    {
        var ex = Favorites.FirstOrDefault(x => x.Id == r.Id && x.Source == r.Source);
        if (ex != null) Favorites.Remove(ex); else Favorites.Insert(0, r);
        Save();
    }
    public static void AddHistory(string q)
    {
        History.Remove(q); History.Insert(0, q);
        while (History.Count > 50) History.RemoveAt(50);
        Save();
    }
}
