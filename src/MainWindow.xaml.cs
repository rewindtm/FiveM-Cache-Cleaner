using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;

namespace FiveMCacheCleaner;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<CleanItem> _items = new();
    private bool _busy;
    private bool _showingResult;

    public MainWindow()
    {
        InitializeComponent();

        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(_items);
        view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(CleanItem.Group)));
        List.ItemsSource = view;

        var args = Environment.GetCommandLineArgs();
        var found = args.Length > 1 && Directory.Exists(args[1]) ? args[1] : Cleaner.FindFiveMDir();
        if (found != null) PathBox.Text = found;
        else
        {
            BuildItems();
            AppendLog("FiveM non trovato automaticamente: usa «Sfoglia…» per scegliere la cartella FiveM.app.");
        }
    }

    // Barra del titolo scura, coerente col tema dell'app
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        int dark = 1, caption = 0x120D0B /* #0B0D12 come COLORREF */, text = 0xF1F0EE;
        DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(int));    // immersive dark mode
        DwmSetWindowAttribute(hwnd, 35, ref caption, sizeof(int)); // colore barra del titolo
        DwmSetWindowAttribute(hwnd, 36, ref text, sizeof(int));    // colore testo titolo
    }

    private string FiveMDir => PathBox.Text.Trim().Trim('"');

    private void BuildItems()
    {
        foreach (var old in _items) old.PropertyChanged -= Item_Changed;
        _items.Clear();
        var d = FiveMDir;
        string P(params string[] parts) =>
            string.IsNullOrEmpty(d) ? "" : Path.Combine(new[] { d }.Concat(parts).ToArray());

        var win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        _items.Add(new("FIVEM", "\uE8A5", "Log di FiveM", @"Registri delle sessioni (logs)", P("logs"), true));
        _items.Add(new("FIVEM", "\uE7BA", "Crash dump", @"Report degli arresti anomali (crashes)", P("crashes"), true));
        _items.Add(new("FIVEM", "\uE896", "Cache client", @"File scaricati dai server (data\cache)", P("data", "cache"), true));
        _items.Add(new("FIVEM", "\uE753", "Cache server", @"data\server-cache", P("data", "server-cache"), true));
        _items.Add(new("FIVEM", "\uE72E", "Cache server privata", @"data\server-cache-priv", P("data", "server-cache-priv"), true));
        _items.Add(new("WINDOWS", "\uE77B", "Temp utente", "Cartella temporanea dell'utente corrente",
            Path.GetTempPath().TrimEnd('\\'), true));
        _items.Add(new("WINDOWS", "\uE713", "Temp di Windows", @"C:\Windows\Temp", Path.Combine(win, "Temp"), true));
        _items.Add(new("WINDOWS", "\uE7C1", "Prefetch di Windows",
            "Facoltativo: rallenta un po' il primo avvio dei programmi", Path.Combine(win, "Prefetch"), false));

        foreach (var it in _items) it.PropertyChanged += Item_Changed;
    }

    private void Item_Changed(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CleanItem.IsSelected)) return;
        _showingResult = false;
        UpdateHero();
    }

    private void UpdateHero()
    {
        if (_showingResult) return;
        var sel = _items.Where(i => i.IsSelected).ToList();
        HeroLabel.Text = "SPAZIO RECUPERABILE";
        HeroValue.Text = Cleaner.FormatSize(sel.Sum(i => i.Bytes));
        HeroSub.Text = $"{sel.Sum(i => i.Files):N0} file in {sel.Count} cartelle";
    }

    private void Row_Click(object sender, MouseButtonEventArgs e)
    {
        if (_busy) return;
        if (sender is FrameworkElement { DataContext: CleanItem it }) it.IsSelected = !it.IsSelected;
    }

    private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        BuildItems();
        UpdateHero();
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

    private void LogToggle_Click(object sender, RoutedEventArgs e)
    {
        var show = Log.Visibility != Visibility.Visible;
        Log.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        LogToggle.Content = show ? "Nascondi dettagli" : "Mostra dettagli";
    }

    private async void Scan_Click(object sender, RoutedEventArgs e) => await ScanAsync();

    private async Task ScanAsync()
    {
        if (_busy) return;
        SetBusy(true);
        _showingResult = false;
        Progress.IsIndeterminate = true;
        var items = _items.ToList();
        await Task.Run(() =>
        {
            foreach (var it in items)
            {
                var r = string.IsNullOrEmpty(it.Path) ? new ScanResult(0, 0, false) : Cleaner.Scan(it.Path);
                it.Files = r.Files; it.Bytes = r.Bytes;
                var text = r.Exists ? $"{r.Files:N0} file · {Cleaner.FormatSize(r.Bytes)}" : "non trovata";
                Dispatcher.Invoke(() => it.Info = text);
            }
        });
        Progress.IsIndeterminate = false;
        UpdateHero();
        AppendLog("Analisi completata.");
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

        _showingResult = true;
        HeroLabel.Text = "PULIZIA COMPLETATA";
        HeroValue.Text = Cleaner.FormatSize(totalBytes);
        HeroSub.Text = $"{totalFiles:N0} file eliminati" + (totalSkipped > 0 ? $" · {totalSkipped:N0} in uso, saltati" : "");
        AppendLog($"Pulizia completata: {totalFiles:N0} file, {Cleaner.FormatSize(totalBytes)} liberati"
                  + (totalSkipped > 0 ? $", {totalSkipped:N0} saltati." : "."));
        SetBusy(false);
    }

    private async Task<bool> HandleRunningFiveM()
    {
        // Processi del gioco: FiveM, FiveM_GTAProcess, FiveM_ChromeBrowser, FiveM_b3258_GTAProcess...
        // Escluso questo stesso programma (FiveMCacheCleaner).
        var self = Environment.ProcessId;
        var procs = Process.GetProcesses()
            .Where(p => p.Id != self &&
                        (p.ProcessName.Equals("FiveM", StringComparison.OrdinalIgnoreCase) ||
                         p.ProcessName.StartsWith("FiveM_", StringComparison.OrdinalIgnoreCase)))
            .ToList();
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
