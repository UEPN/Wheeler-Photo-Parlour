using System;
using System.Collections.Generic;

namespace WheelerPhotoParlour.Services
{
    public enum AppLanguage
    {
        Chinese,
        English
    }

    /// <summary>双语文案中心。</summary>
    public static class LocalizationService
    {
        public static AppLanguage CurrentLanguage { get; private set; } = AppLanguage.Chinese;

        public static event Action? LanguageChanged;

        /// <summary>从配置恢复上次选择的语言。</summary>
        public static void RestoreFromConfig()
        {
            var saved = ConfigService.Config.Language;
            CurrentLanguage = saved == "en" ? AppLanguage.English : AppLanguage.Chinese;
        }

        public static void SetLanguage(AppLanguage lang)
        {
            if (CurrentLanguage == lang) return;
            CurrentLanguage = lang;

            ConfigService.Config.Language = lang == AppLanguage.English ? "en" : "zh";
            ConfigService.Save();

            LanguageChanged?.Invoke();
        }

        public static void ToggleLanguage()
        {
            SetLanguage(CurrentLanguage == AppLanguage.Chinese ? AppLanguage.English : AppLanguage.Chinese);
        }

        public static string T(string key)
        {
            var table = CurrentLanguage == AppLanguage.Chinese ? Chinese : English;
            return table.TryGetValue(key, out var value) ? value : key;
        }

        // ============================================================
        // 中文文案
        // ============================================================
        private static readonly Dictionary<string, string> Chinese = new()
        {
            // 框架 & 导航
            ["AppTitle"] = "惠勒照相馆",
            ["AboutBtn"] = "📜 本馆信誉",
            ["AboutBtnTip"] = "惠勒公司的百年信誉（关于项目）",

            // 操作按钮
            ["AutoScanBtn"] = "🔍 搜寻相机袋",
            ["AutoScanBtnTip"] = "搜寻马背上的相机袋（一键全自动扫描）",
            ["ManualPathBtn"] = "📁 翻看账本",
            ["ManualPathBtnTip"] = "翻看其他账本（手动选择存档路径）",
            ["ExportBtn"] = "📦 打包付梓",
            ["ExportBtnTip"] = "打包付梓，批量冲印（批量导出选中的照片）",
            ["ExportCheckedBtn"] = "📑 自选冲印",
            ["ExportCheckedBtnTip"] = "自选冲印（点击进入勾选模式，挑选想要的底片，再点一次完成挑选）",
            ["ExportCheckedBtnActive"] = "✓ 完成挑选",
            ["ExportSelectedBtn"] = "🖨️ 冲印此帧",
            ["ExportSelectedBtnTip"] = "冲印此帧（导出当前选中的这一张照片）",

            // 状态栏
            ["StatusInitializing"] = "⚙️ 正在为您擦拭镜头，请稍候...",
            ["StatusScanning"] = "🤠 正在翻箱倒柜搜寻您的二进制存档文件...",
            ["StatusExporting"] = "🧪 正在暗房中用银盐工艺冲洗照片，请勿关闭电源...",

            // 弹窗
            ["ScanSuccessTitle"] = "好极了！我们在西部荒野中找到了 {0} 张珍贵的记忆底片！",
            ["ScanSuccessSubtitle"] = "每一张都记录了您驰骋荒野的峥嵘岁月。",
            ["ExportSuccessTitle"] = "冲印完成！高精度纸质相片已安全送达您的桌面！",
            ["ExportSuccessSubtitle"] = "惠勒劳森公司祝您旅途愉快，愿您的西部牛仔梦永不褪色！",

            // 报错
            ["NoPhotosFoundTitle"] = "该死，您的相机袋空空如也，是忘记给高级相机装胶卷了吗？",
            ["CheckModeHint"] = "请点选您要的底片，挑好后再点一次「完成挑选」",
            ["NoPhotosFoundSubtitle"] = "未能自动找到您的照片存档。请确认拍摄记录确实存在，或者尝试【手动选择路径】。",
            ["InvalidExportPathTitle"] = "驿站马车迷路了！我们找不到您指定的导出文件夹。",
            ["InvalidExportPathSubtitle"] = "请重新选择一个安全的路径来存放您的精美影集。",
            ["FileCorruptedTitle"] = "噢，这张底片似乎在某次混战中被泥水泡烂了（文件损坏，无法解析）。",

            // 关于
            ["AboutCredit"] = "本工具旨在以最优雅、最高效的方式，将模糊陈旧的底片化为清晰相片。\n感谢您光临本馆，愿西部牛仔梦永不褪色。",
            ["AboutDisclaimer"] = "1. 本软件为基于《荒野大镖客：救赎 2》的同人衍生工具，源代码及编译成品完全免费公开，遵循 GNU General Public License v3.0（GPLv3）开源协议发布。\n\n2. 任何人均可依据 GPLv3 协议自由复制、修改、分发或引用本项目代码，亦可将其用于商业用途；但基于本项目代码的任何衍生作品在分发时，必须同样以 GPLv3 协议开源，保留原始版权声明并提供对应源代码。作者不对衍生版本的内容或后果承担任何责任。\n\n3. 本软件按\u201C现状\u201D提供，不包含任何明示或暗示的担保。作者不对因使用本工具所导致的任何直接或间接损失（包括但不限于游戏存档损坏、系统崩溃或数据丢失）承担任何责任。使用风险由使用者自行承担。详见随附的 GPLv3 协议全文。",

            // ---- 补充文案 ----
            ["LangBtn"] = "EN",
            ["PhotoListTitle"] = "马背上的相机袋",
            ["EmptyPreviewTitle"] = "请从左侧相机袋中，挑一张底片瞧瞧",
            ["NoPhotoSelected"] = "尚未挑选任何底片",
            ["StatusReady"] = "镜头已擦亮，恭候您的差遣",
            ["StatusSourceMissing"] = "驿站马车迷路了！未寻得存档账本，请翻看其他账本指认正确位置",
            ["StatusLoadFailedFormat"] = "翻箱倒柜未果：{0}",
            ["PhotoCountText"] = "相机袋里共有 {0} 张底片",
            ["SelectAllCheck"] = "全选",
            ["SelectedCountText"] = "已勾选 {0} 张",
            ["DeleteBtn"] = "🧹 焚毁此帧",
            ["DeleteBtnTip"] = "焚毁此帧（删除当前选中的这一张照片，移至回收站）",

            ["ExportModeTitle"] = "敢问这批底片，该如何打包付梓？",
            ["ExportAsZipOption"] = "装订成一册（ZIP 影集）",
            ["ExportToFolderOption"] = "逐帧冲印至文件夹",
            ["DialogOk"] = "就这么办",
            ["DialogCancel"] = "且容我三思",

            ["SaveZipTitle"] = "请定夺影集之去处",
            ["SaveZipFilter"] = "惠勒牌装订册|*.zip",
            ["SaveZipDefaultName"] = "惠勒照相馆底片集_{0}.zip",

            ["SelectFolderTitle"] = "请定夺冲印之去处",
            ["SaveSingleTitle"] = "请定夺此帧之去处",
            ["SaveSingleFilter"] = "惠勒牌相纸|*.jpg",

            ["OverwriteConfirmTitle"] = "账本中已有同名旧帧",
            ["OverwriteConfirmMessage"] = "发现 {0} 帧同名旧档，敢问是否一并覆去？",

            ["DeleteConfirmTitle"] = "确认焚毁？",
            ["DeleteConfirmMessage"] = "当真要将「{0}」付之一炬吗？\n（此举仅是请它暂避回收站，并非真的烧成灰烬——尚可寻回。）",

            ["ExportResultTitle"] = "冲印结果回执",
            ["ExportFailedMessage"] = "冲印不幸告败：{0}",
            ["ExportSingleFailed"] = "冲印告败：{0}",

            ["DeleteFailedMessage"] = "焚毁未遂：{0}",

            ["ErrorTitle"] = "本馆深表歉意",
            ["InfoTitle"] = "告示",
            ["WarningTitle"] = "请留意",

            ["RuntimeWarningTitle"] = "缺了点东西",
            ["RuntimeWarningMessage"] = "本馆探得贵机或许少了 .NET 8.0 这套底片显影药水（运行时），\n少了它，照相馆的器械恐难转动。\n\n是否即刻前去取一份回来？",

            ["SelectSourceFolderTitle"] = "请翻看账本，指认存档所在",

            ["UnknownLocationFolder"] = "无名荒野",
            ["GroupByLocationOption"] = "按拍摄地点分类整理",
            ["GroupByLocationOptionTip"] = "把同一地点拍的照片归到同一个文件夹里，文件名也会自动改成「地点_时间」",
            ["GameDateTimeLabel"] = "🗓️ 故事年代",

            // 关于窗口
            ["AboutWindowTitle"] = "惠勒公司的百年信誉",
            ["AboutOkBtn"] = "已知悉",

            // 时间戳
            ["TimestampSourceLabel"] = "时间戳来源",
            ["TimestampRealOption"] = "真实世界时间",
            ["TimestampRealOptionTip"] = "使用从文件中解析出的真实拍摄时间作为文件名中的时间戳",
            ["TimestampGameOption"] = "游戏内时间",
            ["TimestampGameOptionTip"] = "使用游戏故事年代的时间作为文件名中的时间戳",
            ["ExportModeTitleSingle"] = "敢问此帧底片，时间戳该用哪个？",
            ["UnknownTimestamp"] = "未知时间",
        };

        // ============================================================
        // English
        // ============================================================
        private static readonly Dictionary<string, string> English = new()
        {
            // Framework & nav
            ["AppTitle"] = "Wheeler Photo Parlour",
            ["AboutBtn"] = "📜 Credibility",
            ["AboutBtnTip"] = "Century-Old Credibility (About Us)",

            // Buttons
            ["AutoScanBtn"] = "🔍 Inspect Bag",
            ["AutoScanBtnTip"] = "Inspect the Saddlebag (Auto Scan)",
            ["ManualPathBtn"] = "📁 Alt. Ledger",
            ["ManualPathBtnTip"] = "Consult Alternative Ledger (Manual Path)",
            ["ExportBtn"] = "📦 Bulk Print",
            ["ExportBtnTip"] = "Bulk Printing & Publishing (Export Selected)",
            ["ExportCheckedBtn"] = "📑 Print Checked",
            ["ExportCheckedBtnTip"] = "Print Checked (click to enter check mode, tick the plates you want, then click again to finish)",
            ["ExportCheckedBtnActive"] = "✓ Done Checking",
            ["ExportSelectedBtn"] = "🖨️ Print This",
            ["ExportSelectedBtnTip"] = "Print This Plate (export the currently selected photo)",

            // Status
            ["StatusInitializing"] = "⚙️ Wiping the camera lens, please hold on...",
            ["StatusScanning"] = "🤠 Scouring through your binary save files...",
            ["StatusExporting"] = "🧪 Developing prints in the darkroom, do not turn off power...",

            // Dialogs
            ["ScanSuccessTitle"] = "Splendid! Found {0} precious negatives from the Wild West!",
            ["ScanSuccessSubtitle"] = "Each one documents your glorious days riding the open frontier.",
            ["ExportSuccessTitle"] = "Development Complete! High-quality prints delivered safely to your desktop!",
            ["ExportSuccessSubtitle"] = "Wheeler, Rawson & Co. wishes you a pleasant journey. May your cowboy dream never fade!",

            // Errors
            ["NoPhotosFoundTitle"] = "Dammit, your camera bag is empty. Forgot the film?",
            ["CheckModeHint"] = "Tick the plates you'd like, then click \"Done Checking\" when ready",
            ["NoPhotosFoundSubtitle"] = "No photo saves found automatically. Make sure your photos exist, or try [Manual Path].",
            ["InvalidExportPathTitle"] = "The stagecoach is lost! Cannot find your designated export folder.",
            ["InvalidExportPathSubtitle"] = "Please select a valid path to store your exquisite photo album.",
            ["FileCorruptedTitle"] = "Oh, this negative seems soaked in mud from some long-forgotten skirmish (File corrupted).",

            // About
            ["AboutCredit"] = "This tool aims to turn faded, blurry negatives into clear prints, in the most elegant and efficient way possible.\nThank you for stopping by — may your cowboy dream never fade.",
            ["AboutDisclaimer"] = "1. This software is a fan-made tool based on Red Dead Redemption 2. Its source code and compiled builds are entirely free and public, released under the GNU General Public License v3.0 (GPLv3).\n\n2. Anyone may freely copy, modify, distribute, or reference this project's code under the terms of the GPLv3, including for commercial purposes. However, any derivative work based on this project's code must also be distributed under the GPLv3, must retain the original copyright notice, and must make its corresponding source code available. The author bears no responsibility for the content or consequences of any derivative versions.\n\n3. This software is provided \"as is\", without warranty of any kind, express or implied. The author shall not be liable for any direct or indirect loss arising from the use of this tool, including but not limited to game save corruption, system crashes, or data loss. Use at your own risk. See the accompanying GPLv3 license text for full terms.",

            // ---- Supplementary ----
            ["LangBtn"] = "中",
            ["PhotoListTitle"] = "The Saddlebag of Negatives",
            ["EmptyPreviewTitle"] = "Pick a negative from the saddlebag, partner",
            ["NoPhotoSelected"] = "No negative chosen yet",
            ["StatusReady"] = "Lens polished, awaiting your orders",
            ["StatusSourceMissing"] = "The stagecoach is lost! No ledger found — try Manual Path to point us right",
            ["StatusLoadFailedFormat"] = "Rummaging failed: {0}",
            ["PhotoCountText"] = "{0} negatives in the saddlebag",
            ["SelectAllCheck"] = "Select All",
            ["SelectedCountText"] = "{0} checked",
            ["DeleteBtn"] = "🧹 Burn This One",
            ["DeleteBtnTip"] = "Burn This One (delete the currently selected photo, sent to Recycle Bin)",

            ["ExportModeTitle"] = "How shall we print and publish this batch, partner?",
            ["ExportAsZipOption"] = "Bind into a single album (ZIP)",
            ["ExportToFolderOption"] = "Print each plate to a folder",
            ["DialogOk"] = "So Be It",
            ["DialogCancel"] = "Let Me Reconsider",

            ["SaveZipTitle"] = "Name the Album's Destination",
            ["SaveZipFilter"] = "Wheeler Bound Album|*.zip",
            ["SaveZipDefaultName"] = "Wheeler_Photo_Collection_{0}.zip",

            ["SelectFolderTitle"] = "Name the Printing Destination",
            ["SaveSingleTitle"] = "Name This Plate's Destination",
            ["SaveSingleFilter"] = "Wheeler Fine Photo Paper|*.jpg",

            ["OverwriteConfirmTitle"] = "Old Negatives Already on File",
            ["OverwriteConfirmMessage"] = "Found {0} negative(s) already on file under the same name. Overwrite the lot?",

            ["DeleteConfirmTitle"] = "Confirm the Burning?",
            ["DeleteConfirmMessage"] = "Are you quite certain you wish to burn \"{0}\"?\n(Fear not — this merely banishes it to the Recycle Bin, not to actual ashes.)",

            ["ExportResultTitle"] = "Development Receipt",
            ["ExportFailedMessage"] = "The development has, regrettably, failed: {0}",
            ["ExportSingleFailed"] = "Development failed: {0}",

            ["DeleteFailedMessage"] = "The burning did not succeed: {0}",

            ["ErrorTitle"] = "Our Sincerest Apologies",
            ["InfoTitle"] = "Notice",
            ["WarningTitle"] = "A Word of Caution",

            ["RuntimeWarningTitle"] = "Something's Missing",
            ["RuntimeWarningMessage"] = "We've detected your machine may be lacking the .NET 8.0 developing solution (runtime),\nwithout which our equipment simply cannot turn.\n\nShall we fetch a bottle for you at once?",

            ["SelectSourceFolderTitle"] = "Consult the Ledger to Point Us to the Saves",

            ["UnknownLocationFolder"] = "Unnamed Wilds",
            ["GroupByLocationOption"] = "Group by Filming Location",
            ["GroupByLocationOptionTip"] = "Sort photos from the same location into one folder, and rename them as \"Location_Time\"",
            ["GameDateTimeLabel"] = "🗓️ Story Date",

            // About window
            ["AboutWindowTitle"] = "Century-Old Credibility",
            ["AboutOkBtn"] = "Noted",

            // Timestamp
            ["TimestampSourceLabel"] = "Timestamp Source",
            ["TimestampRealOption"] = "Real-World Time",
            ["TimestampRealOptionTip"] = "Use the real-world capture time parsed from the file as the timestamp in filenames",
            ["TimestampGameOption"] = "In-Game Time",
            ["TimestampGameOptionTip"] = "Use the in-game story era time as the timestamp in filenames",
            ["ExportModeTitleSingle"] = "Which timestamp shall we use for this plate, partner?",
            ["UnknownTimestamp"] = "Unknown Time",
        };
    }
}
