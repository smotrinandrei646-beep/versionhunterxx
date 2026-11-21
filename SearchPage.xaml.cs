namespace VersionHunterX;
[QueryProperty(nameof(Query), "q")]
public partial class SearchPage : ContentPage
{
    string? _q;
    public string? Query
    {
        get => _q;
        set { _q = value; if (!string.IsNullOrWhiteSpace(value)) { Bar.Text = value; _ = RunSearch(); } }
    }
    public SearchPage() { InitializeComponent(); }
    async void OnSearch(object? s, EventArgs e) => await RunSearch();
    async Task RunSearch()
    {
        var q = Bar.Text?.Trim();
        if (string.IsNullOrEmpty(q)) return;
        Busy.IsRunning = true; List.ItemsSource = null; Status.Text = "Ищу…";
        Store.AddHistory(q);
        var r = (await SearchService.Search(q, Gh.IsChecked, Gl.IsChecked, Cb.IsChecked)).OrderByDescending(x => x.Stars).ToList();
        Busy.IsRunning = false;
        Status.Text = r.Count == 0 ? "Ничего не найдено. Проверьте запрос и интернет." : $"Найдено проектов: {r.Count}";
        List.ItemsSource = r; List.Opacity = 0; await List.FadeTo(1, 350);
    }
    async void OnSelected(object? s, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Repo r)
        {
            ((CollectionView)s!).SelectedItem = null;
            await Navigation.PushAsync(new ReleasesPage(r));
        }
    }
}
