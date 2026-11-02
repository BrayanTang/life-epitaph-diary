---
name: maui-windows-preview
description: >-
  Runs Life Epitaph Diary (.NET MAUI + API) locally on Windows WinUI for visual
  preview. Use when the user asks to 本地预览, run MAUI Windows, launch 人生墓碑日记,
  start LifeEpitaph.Api, capture desktop screenshots, or debug unpackaged WinUI
  launch failures (WindowsPackageType, RoundRectangle, SDK 8).
---

# MAUI Windows 本地预览

在 Windows 笔记本上尽快弹出「人生墓碑日记」窗口（WinUI），并可选连带启动本地 API。

## 何时用

- 用户要 **看见** 桌面窗口（墓碑主页 / 今日提问 / 人生剧情 / 墓碑广场 / 我的）
- 修 `dotnet run` / unpackaged exe 起不来（WinAppSDK `REGDB_E_CLASSNOTREG` 等）
- 需要截几张运行中 UI 存到 Project store `media/`

## 前置

- .NET **8** SDK（本仓库 `global.json` 钉住；机器默认若是 SDK 10，勿用 10 直接还原 net8 MAUI）
- Workload：`maui-windows`（VS 安装亦可）
- 仓库路径：优先已有 checkout；不要覆盖无关仓库（GangQi / SiSuo 等）

## 启动顺序

1. **API（后台）**

```powershell
dotnet build src/LifeEpitaph.Api/LifeEpitaph.Api.csproj -c Debug
dotnet run --project src/LifeEpitaph.Api --launch-profile http
```

默认：`http://127.0.0.1:5188/`（`/health`、`/swagger`）。App 内 `Preferences` 键 `api_base_url` 可改；默认已指向该地址。

2. **只编 Windows TFM（不要无意覆盖 Shared 的 TFM）**

```powershell
dotnet restore src/LifeEpitaph.Shared/LifeEpitaph.Shared.csproj
dotnet restore src/LifeEpitaph.App/LifeEpitaph.App.csproj
dotnet build src/LifeEpitaph.App/LifeEpitaph.App.csproj -c Debug -f net8.0-windows10.0.19041.0
```

不要对整仓传 `-p:TargetFrameworks=net8.0-windows...`（会污染 Shared 的 `project.assets.json`）。

3. **启动 UI（unpackaged）**

项目已设 `WindowsPackageType=None`。优先直接跑 exe：

```powershell
src/LifeEpitaph.App/bin/Debug/net8.0-windows10.0.19041.0/win10-x64/LifeEpitaph.App.exe
```

或：

```powershell
dotnet run --project src/LifeEpitaph.App/LifeEpitaph.App.csproj -c Debug -f net8.0-windows10.0.19041.0 --no-build
```

API 不可达时 App 仍可开（本地会话 + 题库回退）；广场等需 API 才有完整数据。

## 已知必改 / 已入库点

| 问题 | 处理 |
|------|------|
| SDK 10 下 `$(MauiVersion)` 空 → NU1015 | 仓库根 `global.json` 使用 SDK 8.0.x |
| `RoundRectangle` CS0246 | `using Microsoft.Maui.Controls.Shapes;` |
| WinAppSDK `REGDB_E_CLASSNOTREG` | csproj：`WindowsPackageType=None`（unpackaged） |
| launchSettings 仅 `MsixPackage` | unpackaged 用 exe / `dotnet run`，勿依赖 Msix 配置文件 |

## 截图

- 目录（本 Project）：`media/local-preview/`
- 优先抓窗口客户区；有微信等浮层时先最小化浮层，或对目标 HWND `PrintWindow`（最大化偶发不稳，保持普通窗口）
- 至少覆盖：主页、今日提问、人生剧情、广场（或我的）

## 约束

- 不要为「能跑」堆兼容分支；缺依赖就装/钉版本，按上表最小改动
- 用户未要求时不要新开 PR；本次若明确要求推仓库则开 PR
- 不要 merge；不要动无关仓库

## 验收

- [ ] `GET http://127.0.0.1:5188/health` 返回 ok
- [ ] 桌面有「人生墓碑日记」窗口且可点五个 Tab
- [ ] 截图路径回报给用户（绝对路径）
