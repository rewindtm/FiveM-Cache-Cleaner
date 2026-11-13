# FiveM Cache Cleaner 🧹

> **New in v2.0:** a native Windows app (WPF, .NET 10) with a graphical interface. Download `FiveMCacheCleaner.exe` from the [Releases](https://github.com/rewindtm/FiveM-Cache-Cleaner/releases) page: no install, no .NET required. It auto-detects FiveM, previews the space each folder would free, warns you if FiveM is running and reports locked files. The original batch script is still available as `cleanup-fivem.bat`.
>
> **Novità v2.0:** app nativa per Windows (WPF, .NET 10) con interfaccia grafica. Scarica `FiveMCacheCleaner.exe` dalla pagina [Releases](https://github.com/rewindtm/FiveM-Cache-Cleaner/releases): nessuna installazione e nessun .NET richiesto. Trova FiveM da sola, mostra in anteprima lo spazio liberabile, avvisa se FiveM è aperto e segnala i file bloccati. Lo script batch originale resta disponibile come `cleanup-fivem.bat`.

Build from source / compilare dai sorgenti:
```sh
cd src
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

This project contains a **Windows Batch script** that automates the cleanup of **FiveM cache** and system temporary folders.  
Its purpose is to improve **FiveM client performance**, reduce crashes, and free up disk space.

---

## ✨ Features
- 🗑️ Automatically cleans:
  - FiveM logs, crashes, and cache  
  - Server-side cache (`server-cache`, `server-cache-priv`)  
  - Windows temporary folders (`Temp`, `Prefetch`, `User Temp`)  
- 🔢 Counts the **total number of deleted files**  
- 💾 Calculates **freed disk space** (in MB and GB, with two decimals)  
- 📊 Provides a **clear final summary**  

---

## 🚀 Usage
1. Download the script or clone the repository:
   ```sh
   git clone https://github.com/rewindtm/FiveM-Cache-Cleaner.git
   ```
2. Edit the configuration variable with the correct path to your FiveM installation:
   ```bat
   set "FIVEM_DIR=C:\Path\FiveM.app"
   ```
3. Run the script as **Administrator**:
   - Double-click the `.bat` file  
   - Or run from `cmd`  

---

## 📊 Example Output
```
[*] Cleaning FiveM in: "D:\Games\FiveM\FiveM.app"
[+] Cleaned: "...logs" (files: 34, size: 12.345 bytes)
[+] Cleaned: "...cache" (files: 120, size: 1.234.567 bytes)

============================================================
[*] Cleaning complete!
[*] Files deleted : 324
[*] Space freed   : 1.57 GB (~1609.32 MB)
============================================================
```

---

# 🇮🇹 FiveM Cache Cleaner 🧹

Questo progetto contiene uno script **Batch per Windows** che automatizza la pulizia della **cache di FiveM** e delle cartelle temporanee di sistema.  
Lo scopo è migliorare le **prestazioni del client FiveM**, ridurre i crash e liberare spazio su disco.

---

## ✨ Funzionalità
- 🗑️ Pulisce automaticamente:
  - Log, crash e cache di FiveM  
  - Cache lato server (`server-cache`, `server-cache-priv`)  
  - Cartelle temporanee di Windows (`Temp`, `Prefetch`, `User Temp`)  
- 🔢 Conta il **numero totale di file eliminati**  
- 💾 Calcola lo **spazio liberato** (in MB e GB, con due decimali)  
- 📊 Mostra un **riepilogo finale chiaro**  

---

## 🚀 Utilizzo
1. Scarica lo script o clona la repository:
   ```sh
   git clone https://github.com/rewindtm/FiveM-Cache-Cleaner.git
   ```
2. Modifica la variabile di configurazione con il percorso corretto della tua installazione di FiveM:
   ```bat
   set "FIVEM_DIR=C:\Percorso\FiveM.app"
   ```
3. Avvia lo script come **Amministratore**:
   - Doppio click sul file `.bat`  
   - Oppure da `cmd`  

---

## 📊 Esempio Output
```
[*] Cleaning FiveM in: "D:\Games\FiveM\FiveM.app"
[+] Cleaned: "...logs" (files: 34, size: 12.345 bytes)
[+] Cleaned: "...cache" (files: 120, size: 1.234.567 bytes)

============================================================
[*] Cleaning complete!
[*] Files deleted : 324
[*] Space freed   : 1.57 GB (~1609.32 MB)
============================================================
```

---

## 📜 License
MIT License – free to use and modify.
