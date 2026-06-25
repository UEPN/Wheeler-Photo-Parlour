<div align="center">

<img src="https://github.com/user-attachments/assets/2d2c990a-ab4a-4e2d-81b5-f90451a507a6" width="96" />

# 惠勒照相馆

一款复古风格的照片浏览与导出工具，专为《荒野大镖客：救赎 2》玩家打造

[![License](https://img.shields.io/badge/License-GPL%20v3.0-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

[English](README_EN.md) · [下载](https://github.com/UEPN/Wheeler-Photo-Parlour/releases) · [反馈问题](https://github.com/UEPN/Wheeler-Photo-Parlour/issues)

</div>

---

<div align="center">

<img src="https://github.com/user-attachments/assets/7a88c146-92cb-4be9-bd49-4b81c1b27fcb" width="600" />

主界面预览

</div>

---

## 功能特性

- **一键扫描** — 自动定位 RDR2 照片存档目录
- **缩略图预览** — 流畅浏览，异步加载不卡顿
- **高清预览** — 点击缩略图查看完整照片，F11 全屏显示
- **地点解析** — 自动识别照片拍摄地点
- **批量导出** — 打包为 ZIP 影集或导出到文件夹，支持按地点分类
- **智能命名** — 导出文件自动命名为"地点_拍摄时间.jpg"
- **时间戳选择** — 可选择真实世界时间或游戏内时间作为文件名时间戳
- **自选冲印** — 勾选想要的底片，只导出精选
- **单帧导出** — 导出当前选中的单张照片
- **安全删除** — 移至回收站，不永久删除
- **中英双语** — 一键切换界面语言
- **复古主题** — 深红暗金西部风格 UI

---

## 系统要求

- **操作系统**: Windows 10 / 11 (64-bit)
- **运行环境**: [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

> 程序启动时会自动检测运行环境，若缺失会引导您前往下载页面。

---

## 安装与使用

### 方式一：下载发布版（推荐）

1. 从 [Releases](https://github.com/UEPN/Wheeler-Photo-Parlour/releases) 页面下载最新版本
2. 确保已安装 [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
3. 双击运行即可

### 方式二：自行编译

```bash
git clone https://github.com/UEPN/Wheeler-Photo-Parlour.git
cd Wheeler-Photo-Parlour/WheelerPhotoParlour
dotnet build

# 或发布为单文件
dotnet publish -c Release
```

编译产物位于 `bin/Release/net8.0-windows/win-x64/publish/` 目录。

---

## 使用指南

1. **启动程序** — 双击 `WheelerPhotoParlour.exe`
2. **扫描照片** — 点击「搜寻相机袋」，程序会自动定位 RDR2 存档目录
3. **浏览照片** — 左侧列表显示所有照片缩略图，点击即可预览
4. **导出照片** — 选择导出方式：
   - 「打包付梓」— 批量导出所有照片，弹窗可选 ZIP 或文件夹、是否按地点分目录、时间戳用哪个
   - 「自选冲印」— 进入勾选模式，挑完再点一次导出
   - 「冲印此帧」— 导出当前选中的单张照片
5. **删除照片** — 不想要的底片可点击「焚毁此帧」移至回收站

> 若自动扫描未找到照片，可点击「翻看账本」手动选择存档路径。

---

## 技术细节

- **框架**: WPF (.NET 8.0 Windows)
- **照片格式**: RDR2 的 `.PRDR` 文件为自定义格式（前 300 字节为游戏头 + 标准 JPEG 数据）
- **转换原理**: 跳过前 300 字节即可提取 JPEG，无需第三方图片库
- **元数据解析**: 从 PRDR 文件中解析拍摄地点（TITL）、游戏内时间（JSON）和真实拍摄时间
- **缓存机制**: 缩略图采用内存缓存 + 文件缓存，二次打开无需重新转换
- **异步加载**: 所有图片加载操作均在后台线程执行，UI 响应流畅

---

## 更新日志

### v1.1.0

- 新增地点解析
- 新增导出时间戳来源选择（真实时间 / 游戏内时间）
- 新增按拍摄地点分类导出
- 导出文件命名改为"地点_时间.jpg"
- 修复窗口最大化时全屏切换后还原尺寸异常

### v1.0.0

- 初始版本

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

如有问题或建议，请在 [GitHub Issues](https://github.com/UEPN/Wheeler-Photo-Parlour/issues) 提交。

---

<div align="center">

愿您的西部牛仔梦永不褪色

</div>
