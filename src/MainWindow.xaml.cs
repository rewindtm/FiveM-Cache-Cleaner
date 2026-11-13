using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace FiveMCacheCleaner;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<CleanItem> _items = new();
    private bool _busy;

    public MainWindow()
    {
        InitializeComponent();
        List.ItemsSource = _items;

        var found = Cleaner.FindFiveMDir();
        if (found != null) PathBox.Text = found;
        else
        {
            BuildItems();
            AppendLog("FiveM non trovato automaticamente: usa «Sfoglia…» per scegliere la cartella FiveM.app.");
        }
    }

    private string FiveMDir => PathBox.Text.Trim().Trim('"');

    private void BuildItems()
    {
        _items.Clear();
        var d = FiveMDir;
        string P(params string[] parts) =>
            string.IsNullOrEmpty(d) ? "" : Path.Combine(new[] { d }.Concat(parts).ToArray());

        var win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        _items.Add(new("Log di FiveM", "logs", P("logs"), true, false));
        _items.Add(new("Crash dump", "crashes", P("crashes"), true, false));
        _items.Add(new("Cache client", @"data\cache", P("data", "cache"), true, false));
        _items.Add(new("Cache server", @"data\server-cache", P("data", "server-cache"), true, false));
        _items.Add(new("Cache server (privata)", @"data\server-cache-priv", P("data", "server-cache-priv"), true, false));
        _items.Add(new("Temp utente", "Cartella temporanea dell'utente corrente",
            Path.GetTempPath().TrimEnd('\\'), true, true));
        _items.Add(new("Temp di Windows", @"C:\Windows\Temp", Path.Combine(win, "Temp"), true, true));
        _items.Add(new("Prefetch di Windows",
            "Opzionale: svuotarla rallenta leggermente il primo avvio dei programmi.",
            Path.Combine(win, "Prefetch"), false, true));
    }

    private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        BuildItems();
        if (IsLoaded) _ = ScanAsync();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        _ = ScanAsync();
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Scegli la cartella FiveM.app" };
        if (dlg.ShowDialog(this) == true) PathBox.Text = dlg.FolderName;
    }

    private async void Scan_Click(object sender, RoutedEventArgs e) => await ScanAsync();

    private async Task ScanAsync()
    {
        if (_busy) return;
        SetBusy(true);
        Progress.IsIndeterminate = true;
        var items = _items.ToList();
        await Task.Run(() =>
        {
            foreach (var it in items)
            {
                var r = string.IsNullOrEmpty(it.Path) ? new ScanResult(0, 0, false) : Cleaner.Scan(it.Path);
                it.Files = r.Files; it.Bytes = r.Bytes;
                var text = r.Exists ? $"{r.Files} file · {Cleaner.FormatSize(r.Bytes)}" : "non trovata";
                Dispatcher.Invoke(() => it.Info = text);
            }
        });
        Progress.IsIndeterminate = false;
        var sel = items.Where(i => i.IsSelected).ToList();
        AppendLog($"Analisi completata. Selezionati: {sel.Sum(i => i.Files)} file, {Cleaner.FormatSize(sel.Sum(i => i.Bytes))}.");
        SetBusy(false);
    }

    private async void Clean_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        var chosen = _items.Where(i => i.IsSelected).ToList();
        if (chosen.Count == 0) { AppendLog("Nessuna voce selezionata."); return; }

        if (!await HandleRunningFiveM()) return;

        var ok = MessageBox.Show(this,
            "Verranno eliminati definitivamente i file delle cartelle selezionate. Continuare?",
            "Conferma pulizia", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (ok != MessageBoxResult.Yes) return;

        SetBusy(true);
        Progress.IsIndeterminate = false;
        Progress.Maximum = chosen.Count;
        Progress.Value = 0;
        long totalFiles = 0, totalBytes = 0, totalSkipped = 0;

        foreach (var it in chosen)
        {
            var r = await Task.Run(() => string.IsNullOrEmpty(it.Path)
                ? new CleanResult(0, 0, 0, false) : Cleaner.Clear(it.Path));
            if (!r.Exists) AppendLog($"[!] Non trovata: {it.Name}");
            else
            {
                totalFiles += r.Deleted; totalBytes += r.Bytes; totalSkipped += r.Skipped;
                var skip = r.Skipped > 0 ? $", {r.Skipped} in uso (saltati)" : "";
                AppendLog($"[+] {it.Name}: {r.Deleted} file, {Cleaner.FormatSize(r.Bytes)}{skip}");
                it.Info = "pulita";
            }
            Progress.Value++;
        }

        AppendLog("------------------------------------------------");
        AppendLog($"Pulizia completata. File eliminati: {totalFiles} · Spazio liberato: {Cleaner.FormatSize(totalBytes)}"
                  + (totalSkipped > 0 ? $" · File saltati: {totalSkipped}" : ""));
        SetBusy(false);
    }

    private async Task<bool> HandleRunningFiveM()
    {
        var procs = Process.GetProcesses()
            .Where(p => p.ProcessName.StartsWith("FiveM", StringComparison.OrdinalIgnoreCase)).ToList();
        if (procs.Count == 0) return true;

        var r = MessageBox.Show(this,
            "FiveM è in esecuzione. Per una pulizia completa conviene chiuderlo.\n\nSì = chiudi FiveM e continua\nNo = continua senza chiuderlo\nAnnulla = ferma",
            "FiveM aperto", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        if (r == MessageBoxResult.Cancel) return false;
        if (r == MessageBoxResult.Yes)
        {
            foreach (var p in procs) { try { p.Kill(true); } catch { } }
            await Task.Delay(1000);
            AppendLog("FiveM chiuso.");
        }
        return true;
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        ScanBtn.IsEnabled = CleanBtn.IsEnabled = !busy;
    }

    private void AppendLog(string line)
    {
        Log.AppendText(line + Environment.NewLine);
        Log.ScrollToEnd();
    }
}
