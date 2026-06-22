# Wheeler Photo Parlour

[中文文档](README.md) | [![License](https://img.shields.io/badge/License-GPL%20v3.0-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

A vintage-styled photo browser and exporter, crafted for **Red Dead Redemption 2** players.

[Main Window Preview]<img width="2560" height="1440" alt="WheelerPhotoParlour2" src="https://github.com/user-attachments/assets/2efd6bdc-483a-4bbd-ad74-241da09e48a8" />




---

## Features

- **Auto Scan** — Automatically locate RDR2 photo save directory, no manual searching needed
- **Thumbnail Preview** — Smooth photo list browsing with async loading, no UI lag
- **High-Resolution Preview** — Click any thumbnail to view full photo, press F11 for fullscreen
- **Batch Export** — Pack all photos into a ZIP album, or export each frame to a folder
- **Selective Printing** — Check the negatives you want, export only your picks
- **Single Frame Export** — Export the currently selected photo
- **Safe Delete** — Unwanted negatives go to Recycle Bin, not permanently erased
- **Bilingual UI** — Toggle between Chinese and English with immersive cowboy-style copy
- **Vintage Theme** — Deep red and gold western UI, matching RDR2's visual style

---

## Requirements

- **OS**: Windows 10 / 11 (64-bit)
- **Runtime**: [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

> The app checks runtime on startup and guides you to download if missing.

---

## Installation

### Option 1: Download Release (Recommended)

1. Download `WheelerPhotoParlour.exe` from [Releases](https://github.com/UEPN/Wheeler-Photo-Parlour-/releases)
2. Ensure [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) is installed
3. Double-click to run

### Option 2: Build from Source

```bash
# Clone the repo
git clone https://github.com/UEPN/Wheeler-Photo-Parlour-.git

# Enter project directory
cd Wheeler-Photo-Parlour-/WheelerPhotoParlour-SourceOnly

# Build
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

Released under the **GNU General Public License v3.0 (GPLv3)**.

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

For questions or suggestions, please [open an Issue](https://github.com/UEPN/Wheeler-Photo-Parlour-/issues).

---

May your cowboy dream never fade.
