# 自动机器助手 (Auto Surround Action)

[![SMAPI](https://img.shields.io/badge/SMAPI-4.0.0+-blue.svg)](https://smapi.io/)
[![Stardew Valley](https://img.shields.io/badge/Stardew%20Valley-1.6+-green.svg)](https://stardewvalley.net/)

一个轻量级的星露谷物语 SMAPI 模组，让你按住鼠标滚轮（可自定义）即可自动处理周围 3×3 范围内的机器收获、填充材料，并为耕地施肥，彻底解放双手。

---

## ✨ 功能

- **一键触发**：按住配置的按键（默认鼠标中键），持续对周围 3×3 格子执行操作。
- **智能三合一**：自动识别机器（收获/填充）和耕地（施肥），根据手持物品智能决策。
- **全部可调**：触发按键、执行间隔、三项功能（收获/填充/施肥）均可独立开关。
- **GMCM 集成**：若安装 [通用模组配置菜单](https://www.nexusmods.com/stardewvalley/mods/5098)，可在游戏内直观调整所有设置。
- **多语言支持**：配置文本通过 i18n 支持（需自行提供翻译文件）。

---

## 📦 安装要求

- [SMAPI](https://smapi.io/) 4.0.0 或更高版本
- 星露谷物语 1.6 或更高版本

---

## 🚀 安装步骤

1. 下载本模组压缩包（或自行编译源码）。
2. 解压后，将 `AutoSurroundAction` 文件夹放入游戏的 `Mods` 目录。
3. 通过 SMAPI 启动游戏即可生效。

> 若您从源码编译，请确保项目引用 SMAPI 和 Stardew Valley 的相关 DLL。

---

## 🎮 使用方法

- **默认操作**：按住 `鼠标中键`（滚轮），模组会以 200 毫秒的间隔循环检测周围 3×3 格子。
- **自动执行**：
  - 若格子中有**可收获的机器**（已完成生产）→ 收获产物。
  - 若格子中有**可填充的机器**且手持对应原料 → 填入材料。
  - 若格子中有**未施肥的耕地**且手持肥料 → 施肥。
- 松开滚轮即停止循环。

> 注意：模组不会自动切换手持物品，请提前将对应道具拿在手上。

---

## 🔧 配置说明

### 方式一：通过 GMCM（推荐）

1. 确保已安装 [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)。
2. 在游戏主界面或存档内的模组菜单中找到“自动机器助手”。
3. 调整各项参数（按键、间隔、功能开关）。

### 方式二：手动编辑配置文件

1. 运行一次游戏后，模组会在 `Mods/AutoSurroundAction/` 下生成 `config.json`。
2. 用任意文本编辑器打开，修改对应字段（可用按键见 [SMAPI 按键列表](https://stardewvalleywiki.com/Modding:Key_bindings)）。
3. 保存后重启游戏。

**配置项示例**：

```json
{
  "TriggerKey": "MouseMiddle",
  "IntervalMs": 200,
  "EnableMachineHarvest": true,
  "EnableMachineFill": true,
  "EnableFertilize": true
}