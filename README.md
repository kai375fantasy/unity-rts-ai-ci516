# Unity RTS 游戏 AI 系统（CI516 Artificial Intelligence for Games）

> Unity · C# · 行为树 / A* 寻路 / 有限状态机 ｜ 课程要求：不依赖 Unity NavMesh，自研游戏 AI

一个小型 RTS（即时战略）Demo，重点展示多种游戏 AI 技术的组合实现：单位选择、采集、追击、战斗、逃跑等完整状态驱动的单位行为。

## 技术亮点

- **有限状态机（FSM）**：单位具备 `idle / stepRandom / wander / chase / attack / flee / harvest / deposit` 共 8 种状态，采集-运送形成闭环经济行为，遇敌自动切换战斗/逃跑；
- **自研行为树（Behavior Tree）**：从 0 实现行为树节点体系——`Selector`（选择）、`Sequence`（序列）、`ConditionNode`（条件）、`ActionNode`（动作），以"战斗序列 > 资源序列 > 游荡动作"的优先级驱动单位决策；
- **自研 A\* 寻路**：实现基于曼哈顿距离启发式的 A\* 算法（开放/关闭列表、F/G/H 代价、父节点回溯），配合 25 个路点（waypoint）做地图导航，**全程未使用 Unity NavMesh**；
- **RTS 基础框架**：单位框选/点选、队伍管理、建筑、炮塔、资源点等完整玩法要素。

## 代码结构（Assets/DD/Scripts/）

| 文件 | 职责 |
|---|---|
| `DD_GameManager.cs` | 游戏全局状态与流程管理 |
| `DD_PlayerInputManager.cs` | 玩家输入与单位选择（选中/取消选中） |
| `DD_AI_Class.cs` | A* 寻路核心（Node 类、启发式计算、路径回溯） |
| `GameObjects/DD_Unit.cs` | 单位行为：FSM 状态机 + 行为树决策（约 630 行，本项目的核心） |
| `GameObjects/DD_Building.cs` / `DD_Turret.cs` | 建筑与防御塔逻辑 |
| `GameObjects/DD_Resource.cs` / `DD_Item.cs` | 资源与道具系统 |
| `GameObjects/DD_Team.cs` | 队伍/阵营管理 |

## 行为树决策结构

```
Selector（根节点）
├── Sequence：战斗分支
│   ├── Condition：附近存在敌人且在追击范围内
│   └── Action：切换至 attack 状态
├── Sequence：资源分支
│   ├── Condition：场上存在可采集资源
│   ├── Action：寻找最近资源点
│   ├── Condition：已到达资源范围
│   └── Action：切换至 harvest 状态
└── Action：默认 wander 游荡
```

行为树负责"做什么"的宏观决策，FSM 负责"怎么做"的状态执行，A* 负责"怎么去"的路径规划——三层架构各司其职。

## 开发方式

开发过程中与 GPT 进行 **AI 协同开发**：行为树 + FSM + A* 的组合架构设计讨论、C# 代码框架生成、单位选中状态 bug 的逐层调试定位，最终由本人独立完成实现与整合。

- 引擎：Unity（C#）
- 时间：2024-09 ~ 2025-06
- 说明：本仓库为 Unity 工程的 Assets 导出（.unitypackage 还原），导入空 Unity 工程即可查看
