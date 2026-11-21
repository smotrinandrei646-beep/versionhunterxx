namespace VersionHunterX;
public partial class ReleasesPage : ContentPage
{
    readonly Repo _repo;
    public ReleasesPage(Repo r)
    {
        InitializeComponent(); _repo = r;
        Title = "Версии"; Header.Text = r.Name; Sub.Text = r.Subtitle;
        UpdateFav(); Load();
    }
    void UpdateFav() => FavItem.Text = Store.Favorites.Any(x => x.Id == _repo.Id && x.Source == _repo.Source) ? "★ В избранном" : "☆ В избранное";
    void OnFav(object? s, EventArgs e) { Store.ToggleFav(_repo); UpdateFav(); }
    async void Load()
    {
        Busy.IsRunning = true;
        try
        {
            var l = await SearchService.Releases(_repo);
            List.ItemsSource = l;
            Status.Text = l.Count == 0 ? "Релизов нет — откройте страницу проекта." : $"Найдено версий: {l.Count}";
        }
        catch (Exception ex) { Status.Text = "Ошибка загрузки: " + ex.Message; }
        Busy.IsRunning = false;
    }
    async void OnDownload(object? s, EventArgs e)
    {
        if (((Button)s!).BindingContext is not Asset a) return;
        Prog.IsVisible = true; Prog.Progress = 0; Status.Text = "Скачивание: " + a.Name;
        try
        {
            var rec = await SearchService.Download(a, new Progress<double>(p => Prog.Progress = p));
            Store.Downloads.Insert(0, rec); Store.Save();
            Status.Text = "Готово. SHA-256: " + rec.Sha256;
        }
        catch (Exception ex) { Status.Text = "Ошибка: " + ex.Message; }
        Prog.IsVisible = false;
    }
    async void OnOpenLink(object? s, EventArgs e) { if (((Button)s!).BindingContext is Asset a) await Launcher.OpenAsync(a.Url); }
    async void OnRelease(object? s, EventArgs e) { if (((Button)s!).BindingContext is Rel r) await Launcher.OpenAsync(r.PageUrl); }
    async void OnProject(object? s, EventArgs e) => await Launcher.OpenAsync(_repo.Url);
}
