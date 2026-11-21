namespace VersionHunterX;
public partial class DownloadsPage : ContentPage
{
    public DownloadsPage() { InitializeComponent(); List.ItemsSource = Store.Downloads; }
    async void OnSelected(object? s, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not DownloadRec d) return;
        ((CollectionView)s!).SelectedItem = null;
        var act = await DisplayActionSheet(d.Name, "Отмена", null, "Копировать SHA-256", "Открыть / установить", "Удалить");
        switch (act)
        {
            case "Копировать SHA-256": await Clipboard.SetTextAsync(d.Sha256); break;
            case "Открыть / установить": await Launcher.OpenAsync(new OpenFileRequest(d.Name, new ReadOnlyFile(d.Path))); break;
            case "Удалить": try { File.Delete(d.Path); } catch { } Store.Downloads.Remove(d); Store.Save(); break;
        }
    }
}
