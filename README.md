# 人生墓碑日记 · life-epitaph-diary

安静的卡通墓碑日记：每日轻提问、碎片短答、每周人生剧情；默认私密，广场仅匿名墓志铭 + 皮肤 + 近章标题。

本仓库为 **MVP M1 骨架**：可还原构建的 .NET 解决方案（MAUI 客户端 + ASP.NET Core API + Shared），匿名会话、本地 SQLite、API stub、Mock AI。

## 解决方案结构

| 项目 | 说明 |
|------|------|
| `src/LifeEpitaph.App` | .NET MAUI 客户端（中文 Tab：墓碑主页 / 今日提问 / 人生剧情 / 墓碑广场 / 我的） |
| `src/LifeEpitaph.Api` | ASP.NET Core Minimal API stubs |
| `src/LifeEpitaph.Shared` | 共享 DTO 与 AI 抽象接口 |

## 前置条件

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Windows（推荐跑 MAUI）**：Visual Studio 2022 + .NET MAUI workload（Android 模拟器 / Windows 目标）
- **macOS**：同上 + Xcode（**iOS / Mac Catalyst 仅能在 macOS 上构建**）
- **Linux**：可构建 `Api` + `Shared`；可选安装 `maui-android` workload 尝试 Android（需 Android SDK / JDK）

```bash
# 可选：Linux / CI 上的 Android 工作负载
dotnet workload install maui-android
```

## 还原 / 构建

```bash
dotnet restore LifeEpitaph.sln

# 始终应可通过（Linux CI 最小集）
dotnet build src/LifeEpitaph.Shared/LifeEpitaph.Shared.csproj -c Release
dotnet build src/LifeEpitaph.Api/LifeEpitaph.Api.csproj -c Release

# MAUI（需对应平台 workload；Windows 可打 Android / WinUI；iOS 需 macOS）
dotnet build src/LifeEpitaph.App/LifeEpitaph.App.csproj -c Debug
```

## 运行 API（stub）

```bash
dotnet run --project src/LifeEpitaph.Api --launch-profile http
# http://127.0.0.1:5188/swagger
# GET /health
```

主要路由：

| Method | Path | 说明 |
|--------|------|------|
| GET | `/health` | 健康检查 |
| POST | `/auth/anonymous` | 设备 id → 匿名会话 |
| GET | `/prompts/today` | 今日提问（题库） |
| GET | `/prompts/{date}` | 指定日提问 |
| GET/PUT | `/answers` · `/answers/today` | 短答读写（Bearer `anon.{userId:N}`） |
| GET | `/tombstone/me` | 当前用户墓碑 |
| GET | `/plaza` | 广场 mock 卡片（含近章标题） |
| POST | `/chapters/generate` | Mock 周章节生成 |

## 运行 MAUI（Windows）

1. 先启动 API（见上），默认基址 `http://127.0.0.1:5188/`（App 内 Preferences 键 `api_base_url` 可改）。
2. 用 Visual Studio 打开 `LifeEpitaph.sln`，将启动项目设为 `LifeEpitaph.App`。
3. 选择 **Windows Machine** 或 **Android** 仿真器 / 设备后运行。
4. 应用启动即匿名会话，无登录墙；API 不可达时回退本地设备会话 + 本地题库。

**iOS**：请在 macOS + Xcode 上构建；本 Linux 环境无法产出 iOS 包。

## M1 已实现 / 仍为 stub

**已有**

- 中文五 Tab 壳与安静配色
- 匿名会话（设备 id → API 或本地）
- 本地 SQLite：墓碑、当日短答自动保存、章节列表占位
- API stubs + 中文静态题库
- `IDailyPromptService` / `IChapterGenerator` / `IAiTextProvider`（Mock + Null provider）

**仍为 stub / 未做**

- 真实 LLM 调用与 API Key（刻意不做）
- 首次建墓引导 UI 打磨
- 广场献花互动与限频
- 周章节批处理任务、真实皮肤资源
- 商店打包与推送

## 安全

- 不要提交密钥：使用 `.gitignore`；未来真实 AI Key 仅放服务端配置 / 密钥库，不上客户端。
- 当前无第三方密钥依赖。
