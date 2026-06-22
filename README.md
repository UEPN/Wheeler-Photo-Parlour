# 惠勒照相馆

[English](README_EN.md) | [![License](https://img.shields.io/badge/License-GPL%20v3.0-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

一款复古风格的照片浏览与导出工具，专为《荒野大镖客：救赎 2》玩家打造。

[主界面预览]<img width="2560" height="1440" alt="WheelerPhotoParlour1" src="https://github.com/user-attachments/assets/d22f44ee-3653-47f9-ad77-928157c93aed" />




---

## 功能特性

- **一键扫描** — 自动定位 RDR2 照片存档目录，无需手动翻找
- **缩略图预览** — 流畅的照片列表浏览，异步加载不卡顿
- **高清预览** — 点击缩略图即可查看完整照片，支持 F11 全屏欣赏
- **批量导出** — 打包为 ZIP 影集，或逐帧导出到文件夹
- **自选冲印** — 勾选想要的底片，只导出精选照片
- **单帧导出** — 导出当前选中的单张照片
- **安全删除** — 不想要的底片可移至回收站，而非永久删除
- **中英双语** — 一键切换界面语言，沉浸式西部牛仔风格文案
- **复古主题** — 深红暗金的西部复古 UI，契合 RDR2 视觉风格

---

## 系统要求

- **操作系统**: Windows 10 / 11 (64-bit)
- **运行环境**: [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

> 程序启动时会自动检测运行环境，若缺失会引导您前往下载页面。

---

## 安装与使用

### 方式一：下载发布版（推荐）

1. 从 [Releases](https://github.com/UEPN/Wheeler-Photo-Parlour-/releases) 页面下载最新版本的 `WheelerPhotoParlour.exe`
2. 确保已安装 [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
3. 双击运行即可

### 方式二：自行编译

```bash
# 克隆仓库
git clone https://github.com/UEPN/Wheeler-Photo-Parlour-.git

# 进入项目目录
cd Wheeler-Photo-Parlour-/WheelerPhotoParlour-SourceOnly

# 编译
dotnet build

# 或发布为单文件
dotnet publish -c Release
```

编译产物位于 `bin/Release/net8.0-windows/win-x64/publish/` 目录。

---

## 使用指南

1. **启动程序** — 双击 `WheelerPhotoParlour.exe`
2. **扫描照片** — 点击「搜寻相机袋」，程序会自动定位 RDR2 存档目录
3. **浏览照片** — 左侧「马背上的相机袋」显示所有照片缩略图，点击即可预览
4. **导出照片** — 选择导出方式：
   - 「打包付梓」— 批量导出所有照片为 ZIP 或文件夹
   - 「自选冲印」— 进入勾选模式，挑选想要的底片后导出
   - 「冲印此帧」— 导出当前选中的单张照片
5. **删除照片** — 不想要的底片可点击「焚毁此帧」移至回收站

> 若自动扫描未找到照片，可点击「翻看账本」手动选择存档路径。

---

## 技术细节

- **框架**: WPF (.NET 8.0 Windows)
- **照片格式**: RDR2 的 `.PRDR` 文件为自定义格式（前 300 字节为游戏头 + 标准 JPEG 数据）
- **转换原理**: 跳过前 300 字节即可提取 JPEG，无需第三方图片库
- **缓存机制**: 缩略图采用内存缓存 + 文件缓存，二次打开无需重新转换
- **异步加载**: 所有图片加载操作均在后台线程执行，UI 响应流畅

---

## 许可证

本项目采用 **GNU General Public License v3.0（GPLv3）** 许可证发布。

- ✅ 允许自由复制、修改、分发，**亦可用于商业用途**
- ✅ 衍生作品必须同样以 GPLv3 协议开源，并提供对应源代码
- 📝 分发时需保留原始版权声明及许可声明

详见 [LICENSE](LICENSE) 文件或 [GNU GPLv3 官方文本](https://www.gnu.org/licenses/gpl-3.0.html)。

---

## 免责声明

1. 本软件为基于《荒野大镖客：救赎 2》的同人衍生工具，源代码及编译成品完全免费公开。
2. 本软件按"现状"提供，不包含任何明示或暗示的担保。作者不对因使用本工具所导致的任何直接或间接损失承担责任。
3. 使用风险由使用者自行承担。

---

## 制作人员

- **项目发起与设计**: [Novilune](https://github.com/UEPN)
- **游戏**: Red Dead Redemption 2 by Rockstar Games
- **灵感来源**: Wheeler, Rawson and Co.

---

## 反馈与支持

如有问题或建议，请在 [GitHub Issues](https://github.com/UEPN/Wheeler-Photo-Parlour-/issues) 提交。

---

愿您的西部牛仔梦永不褪色。

