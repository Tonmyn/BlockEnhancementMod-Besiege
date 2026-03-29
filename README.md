# BlockEnhancementMod

[![Besiege](https://img.shields.io/badge/Besiege-Mod-brightgreen)](https://besiege/downloads/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

> 为 Besiege 游戏增强方块功能的模组

## 📖 简介

BlockEnhancementMod 是一个为 Besiege 游戏开发的方块功能扩展模组，旨在增强游戏中各种方块的功能，提供更多的自定义选项和高级特性。该模组为原有的游戏方块添加了大量进阶属性和功能，使玩家能够构建更加复杂和精细的机械装置。

## ✨ 主要特性

### 🎯 武器系统增强
- **火箭制导** - 主动/被动雷达、目标锁定、弹道预测
- **加农炮升级** - 自定义子弹、弹道轨迹、延迟碰撞
- **特殊武器** - 喷火器推进、水炮沸腾、分离铰链控制

### 🚗 移动系统优化
- **轮子系统** - 自定义碰撞体、摩擦力调节、弹力设置
- **悬挂控制** - 液压模式、硬度调节、伸缩限制
- **各种轮子** - 小轮、大轮、动力轮、无动力轮全部支持

### 🔧 机械连接增强
- **活塞系统** - 硬度调节、阻尼控制、行程限制
- **弹簧优化** - 阻力精确调节
- **铰链升级** - 弹簧铰链功能、自动复位

### 🎈 浮力与飞行
- **气球控制** - 动态浮力调节、拖拽力控制
- **飞行支持** - 基础飞行块增强

### 📹 视觉与监视
- **摄像机跟踪** - 目标自动锁定、智能搜索、第一人称视角
- **监视器** - 装甲表面显示摄像机画面

### 🧱 结构与材料
- **蒙皮块** - 运动学模式、透明支撑、关节半径
- **摩擦垫** - 精确摩擦力控制、硬度设置
- **装甲** - 内置监视功能

## 📦 安装方法

### 自动安装
1. 下载最新的 BlockEnhancementMod 模组文件
2. 将文件放置到 `Besiege_Data\Mods\` 文件夹中
3. 启动游戏，模组将自动加载

### 手动安装
1. 编译项目生成 `BlockEnhancementMod.dll`
2. 将 DLL 复制到 `Besiege_Data\Mods\BlockEnhancementMod\`
3. 复制 `Resources` 文件夹内容到相应目录
4. 复制 `XML` 文件夹内容到相应目录
5. 启动游戏

## 🎮 使用指南

### 基本使用
1. 在建筑模式下选中任意方块
2. 在方块属性面板找到 "Enhancement" 或 "增强" 开关
3. 启用增强功能后显示额外配置选项
4. 根据需要调整各项参数

### 控制台命令
```
be help                    # 显示所有命令
be srsecon [value]         # 设置火箭烟雾发射率
be srsl [value]            # 设置火箭烟雾持续时间
be rfa                      # 刷新资源文件
```

## 📚 详细文档

完整的 Wiki 文档位于 `readme/` 文件夹中：

### 📖 文档导航
- **[主文档](readme/README.md)** - 完整的模组说明
- **[Wiki 索引](readme/Wiki-Index.md)** - 快速查找指南

### ⚔️ 武器系统
- [火箭 (Rocket)](readme/Rocket.md) - 导弹制导系统详解
- [加农炮 (Cannon)](readme/Cannon.md) - 自定义子弹和弹道

### 🚗 移动系统
- [轮子 (Wheel)](readme/Wheel.md) - 轮子物理优化
- [悬挂 (Suspension)](readme/Suspension.md) - 液压悬挂控制

### 🔧 机械连接
- [活塞-弹簧-铰链](readme/Piston-Spring-Hinge.md) - 连接组件增强

### 🎈 浮力系统
- [气球-飞行块](readme/Balloon-FlyingBlock.md) - 浮力控制

### 📹 视觉系统
- [摄像机 (Camera)](readme/Camera.md) - 目标跟踪系统

### ⚔️ 特殊武器
- [喷火器-水炮-分离铰链](readme/Flamethrower-WaterCannon-Decoupler.md)

### 🧱 结构材料
- [蒙皮块-摩擦垫-其他](readme/BuildSurface-GripPad-Others.md)

## 🏗️ 项目结构

```
BlockEnhancementMod/
├── BlockEnhancementMod/
│   ├── EnhancementBlock/
│   │   ├── Blocks/           # 方块增强脚本
│   │   ├── GenericBlockScript/  # 通用脚本
│   │   └── EnhancementBlock.cs
│   ├── EnhancementEntity/
│   ├── UI/
│   ├── Resources/
│   └── XML/
├── readme/                   # Wiki 文档
│   ├── README.md
│   ├── Wiki-Index.md
│   └── [各方块详细文档]
└── README.md                 # 本文件
```

## 🔧 技术要求

- .NET Framework 3.5
- Unity 引擎
- Besiege Modding API
- Besiege 游戏本体

## ❓ 常见问题

### 模组不工作？
- 确保 DLL 文件已正确放置
- 检查游戏版本兼容性
- 确认没有其他模组冲突

### 如何重置设置？
- 在方块属性面板禁用增强功能
- 重新启用即可恢复默认值

### 支持多人游戏吗？
- 是的，支持多人联机
- 某些高级功能仅在主机端生效

## 🤝 贡献与反馈

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

请参考项目的 LICENSE 文件了解具体的许可信息。

## 🙏 致谢

感谢 Besiege 社区的支持和贡献！

---

**Made with ❤️ for the Besiege Community**

[📖 查看完整文档](readme/Wiki-Index.md) | [🐛 报告问题](../../issues) | [💡 功能建议](../../issues)
