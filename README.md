# Unity RTS 游戏 AI 系统（CI516 Artificial Intelligence for Games）

> Unity · C# · 行为树 / A* 寻路 / 有限状态机 ｜ 课程要求：不依赖 Unity NavMesh，自研游戏 AI

一个小型 RTS（即时战略）Demo，重点展示多种游戏 AI 技术的组合实现：单位选择、采集、追击、战斗、逃跑等完整状态驱动的单位行为。

## 技术亮点

- **有限状态机（FSM）**：单位具备 `idle / wander / chase / attack / flee / harvest / deposit` 等完整状态链，采集-运送形成闭环经济行为；
- **行为树（Behavior Tree）**：自行实现行为树节点结构（选择/序列/条件/动作），驱动复杂决策；
- **A\* 寻路**：自行实现 A\* 算法并与单位 AI 结合，**未使用 Unity NavMesh**；
- **RTS 基础框架**：单位框选/点选、队伍管理、建筑、炮塔、资源点等完整玩法要素。

## 代码结构（Assets/DD/Scripts/）

| 文件 | 职责 |
|---|---|
| `DD_GameManager.cs` | 游戏全局状态与流程管理 |
| `DD_PlayerInputManager.cs` | 玩家输入与单位选择（选中/取消选中） |
| `DD_AI_Class.cs` | AI 决策核心 |
| `GameObjects/DD_Unit.cs` | 单位行为与状态机 |
| `GameObjects/DD_Building.cs` / `DD_Turret.cs` | 建筑与防御塔逻辑 |
| `GameObjects/DD_Resource.cs` / `DD_Item.cs` | 资源与道具系统 |
| `GameObjects/DD_Team.cs` | 队伍/阵营管理 |

## 开发方式

开发过程中与 GPT 进行 **AI 协同开发**：行为树 + FSM + A* 的组合架构设计讨论、C# 代码框架生成、单位选中状态 bug 的逐层调试定位，最终由本人独立完成实现与整合。

- 引擎：Unity（C#）
- 时间：2024-09 ~ 2025-06
- 说明：本仓库为 Unity 工程的 Assets 导出（.unitypackage 还原），导入空 Unity 工程即可查看
