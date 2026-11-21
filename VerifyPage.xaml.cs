using System.Security.Cryptography;
namespace VersionHunterX;
public partial class VerifyPage : ContentPage
{
    string _hash = "";
    public VerifyPage() { InitializeComponent(); }
    async void OnPick(object? s, EventArgs e)
    {
        var f = await FilePicker.PickAsync();
        if (f == null) return;
        FileLbl.Text = f.FileName; Out.Text = "Вычисляю…";
        using var st = await f.OpenReadAsync();
        _hash = Convert.ToHexString(await SHA256.HashDataAsync(st)).ToLowerInvariant();
        Out.Text = _hash; Compare();
    }
    void OnCompare(object? s, TextChangedEventArgs e) => Compare();
    void Compare()
    {
        var exp = (Expected.Text ?? "").Trim().ToLowerInvariant().Replace(" ", "");
        if (_hash == "" || exp == "") { Res.Text = ""; return; }
        var ok = exp == _hash;
        Res.Text = ok ? "✅ Хэш совпадает — файл не изменён" : "❌ Хэш НЕ совпадает — не устанавливайте файл";
        Res.TextColor = ok ? Colors.LightGreen : Colors.IndianRed;
    }
}
