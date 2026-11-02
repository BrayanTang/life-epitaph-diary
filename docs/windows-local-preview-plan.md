# Windows 本地预览方案（M1）

## 目标

在开发者 Windows 笔记本上 **5～15 分钟内** 看到可交互的「人生墓碑日记」WinUI 窗口，并可选连接本机 API stub，用于产品走查与截图，而不是等 iOS/Android 模拟器。

## 范围

| In | Out |
|----|-----|
| `net8.0-windows` unpackaged 运行 | 商店 MSIX 签名打包 |
| 本机 `LifeEpitaph.Api`（5188） | 真实 LLM / 生产部署 |
| 五 Tab 壳走查 + 截图 | 首次建墓引导打磨、献花限频 |
| 文档化 SDK / workload / 常见报错 | Linux CI 上编 WinUI |

## 架构（本机）

```
[LifeEpitaph.App WinUI]
        |  HTTP  http://127.0.0.1:5188/
        v
[LifeEpitaph.Api Minimal API]
        |
   Shared DTOs + Mock AI
```

- App 默认 `api_base_url` → `http://127.0.0.1:5188/`
- API 挂了：匿名会话与今日提问仍可走本地 SQLite / 本地题库；广场完整数据依赖 API

## 环境要求

1. **.NET 8 SDK**（仓库 `global.json` 钉版本；避免本机默认 SDK 10 还原 net8 MAUI 时 `MauiVersion` 为空）
2. **maui-windows** workload（或 VS 2022+ 的 .NET MAUI 工作负载）
3. 已安装的 Windows App Runtime（本机常见已有 1.5+；unpackaged + `WindowsPackageType=None` 即可引导）

## 操作步骤（摘要）

详见 Cursor skill：`.cursor/skills/maui-windows-preview/SKILL.md`。

```powershell
dotnet build src/LifeEpitaph.Api -c Debug
dotnet run --project src/LifeEpitaph.Api --launch-profile http
dotnet build src/LifeEpitaph.App -c Debug -f net8.0-windows10.0.19041.0
.\src\LifeEpitaph.App\bin\Debug\net8.0-windows10.0.19041.0\win10-x64\LifeEpitaph.App.exe
```

## 工程改动说明（为可跑）

1. **`global.json`**：锁定 SDK 8，保证 `$(MauiVersion)` 与 net8 TFM 一致。
2. **`WindowsPackageType=None`**：允许 `dotnet run` / 直接 exe，避免仅 Msix 配置下 WinAppSDK DeploymentManager 未注册类。
3. **`using Microsoft.Maui.Controls.Shapes`**：修复 `RoundRectangle` 编译错误（人生剧情 / 广场动态卡片）。

## 验收标准

- [ ] `/health` 返回 ok
- [ ] 窗口标题为「人生墓碑日记」，五 Tab 可切换
- [ ] 广场在 API 启动时可看到 mock 卡片（含近章标题、无正文）
- [ ] 能产出 2～4 张清晰截图

## 风险与后续

| 项 | 说明 |
|----|------|
| GitHub 在部分自托管网络不稳定 | 可用分支 zip 同步；推送需可连通 origin |
| 最大化窗口偶发不稳定 | 预览保持普通窗口大小 |
| Android/iOS 仍是上架主路径 | Windows 仅作开发预览，不替代双端真机 |
| M2+ | 真 AI Provider、建墓引导、献花；预览流程不变 |

## 相关

- 产品总计划：仓库外 Project `docs/mvp-plan.md` / 本仓 README
- Agent 操作手册：`.cursor/skills/maui-windows-preview/SKILL.md`
