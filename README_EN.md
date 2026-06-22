<div align="center">

<img src="https://github.com/user-attachments/assets/2d2c990a-ab4a-4e2d-81b5-f90451a507a6" width="96" />

# Wheeler Photo Parlour

A vintage-styled photo browser and exporter for Red Dead Redemption 2 players

[![License](https://img.shields.io/badge/License-GPL%20v3.0-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

[中文文档](README.md) · [Download](https://github.com/UEPN/Wheeler-Photo-Parlour/releases) · [Report Issue](https://github.com/UEPN/Wheeler-Photo-Parlour/issues)

</div>

---

<div align="center">

<img src="https://github.com/user-attachments/assets/2efd6bdc-483a-4bbd-ad74-241da09e48a8" width="600" />

Main Window Preview

</div>

---

## Features

- **Auto Scan** — Automatically locate RDR2 photo save directory
- **Thumbnail Preview** — Smooth browsing with async loading
- **High-Resolution Preview** — Click thumbnail to view full photo, F11 for fullscreen
- **Batch Export** — Pack into ZIP album or export to folder
- **Selective Printing** — Check negatives you want, export only picks
- **Single Frame Export** — Export the currently selected photo
- **Safe Delete** — Move to Recycle Bin, not permanent erase
- **Bilingual UI** — Toggle Chinese/English
- **Vintage Theme** — Deep red and gold western UI

---

## Requirements

- **OS**: Windows 10 / 11 (64-bit)
- **Runtime**: [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

> The app checks runtime on startup and guides you to download if missing.

---

## Installation

### Download Release (Recommended)

1. Download from [Releases](https://github.com/UEPN/Wheeler-Photo-Parlour/releases)
2. Install [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
3. Double-click to run

### Build from Source

```bash
git clone https://github.com/UEPN/Wheeler-Photo-Parlour.git
cd Wheeler-Photo-Parlour/WheelerPhotoParlour
dotnet build

# Or publish as single-file exe
dotnet publish -c Release
```

Output will be in `bin/Release/net8.0-windows/win-x64/publish/`.

---

## Quick Guide

1. **Launch** — Double-click `WheelerPhotoParlour.exe`
2. **Scan Photos** — Click "Inspect Bag" to auto-locate RDR2 save directory
3. **Browse** — Left panel "The Saddlebag of Negatives" shows all photo thumbnails, click to preview
4. **Export** — Choose your export method:
   - "Bulk Print" — Export all photos as ZIP or to a folder
   - "Print Checked" — Enter check mode, pick desired negatives then export
   - "Print This" — Export the currently selected single photo
5. **Delete** — Click "Burn This One" to send unwanted negatives to Recycle Bin

> If auto-scan fails, click "Alt. Ledger" to manually select the save path.

---

## Technical Notes

- **Framework**: WPF (.NET 8.0 Windows)
- **Photo Format**: RDR2's `.PRDR` files are custom format (300-byte game header + standard JPEG data)
- **Conversion**: Skip first 300 bytes to extract JPEG — no third-party image libraries needed
- **Caching**: Thumbnails use memory + file cache, no re-conversion on second launch
- **Async Loading**: All image operations run on background threads, UI stays responsive

---

## License

Released under **GNU General Public License v3.0 (GPLv3)**.

- ✅ Free to copy, modify, distribute, **including for commercial use**
- ✅ Derivative works must also be released under GPLv3, with corresponding source code made available
- 📝 Original copyright and license notices must be retained when redistributing

See [LICENSE](LICENSE) or the [official GNU GPLv3 text](https://www.gnu.org/licenses/gpl-3.0.html).

---

## Disclaimer

1. This software is a fan-made tool based on Red Dead Redemption 2. Source code and compiled builds are entirely free and public.
2. This software is provided "as is", without warranty of any kind. The author is not liable for any direct or indirect loss from using this tool.
3. Use at your own risk.

---

## Credits

- **Concept & Design**: [Novilune](https://github.com/UEPN)
- **Game**: Red Dead Redemption 2 by Rockstar Games
- **Inspiration**: Wheeler, Rawson and Co.

---

## Feedback & Support

For questions or suggestions, please [open an Issue](https://github.com/UEPN/Wheeler-Photo-Parlour/issues).

---

<div align="center">

May your cowboy dream never fade

</div>
