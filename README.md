# MarkerMeter

# MarkerMeter for Unity Editor  
**一键标注场景中任意网格点 + 实时测距**

---

## 1. 功能  
在 **Scene 视图** 里用 `M` 键连续给网格打标记（带编号小球 + RGB 无限长轴线），  
在 **Hierarchy** 选中任意 ≥2 个标记即可在窗口查看 **两点距离**，  
关闭窗口或点击 **"清除全部标记"** 一键清场，场景恢复原样。

---

## 2. 安装即用  
1. 把脚本文件 `MeshPickerEditor.cs` 拖进项目 **任意 Editor 文件夹**（必须 Editor 文件夹）。  
2. 菜单栏 → `Tools / Mesh Point Picker` 打开窗口。  
3. **无需额外预制体、无需运行时脚本，零侵入**。

---

## 3. 使用步骤  
| 操作 | 结果 |
|---|---|
| **Scene 视图** 鼠标左键点选目标物体 | 设为当前操作对象 |
| 按一次 `M` | 在命中点生成一个 **红色小球**（`__Marker_0`...）并附带 **XYZ 无限长轴线**（RGB） |
| 1 秒后可继续按 `M` | 继续放置 `__Marker_1 / 2 / 3...`（冷却防止连发） |
| 在 **Hierarchy** 选中任意 ≥2 个标记球 | 窗口顶部实时显示 **最远两点距离**（大字号） |
| 点击 **"清除全部标记"** 或关闭窗口 | 所有小球 + 轴线 **全部销毁**，场景干净 |

---

## 4. 细节与限制
- **冷却时间**：1 秒（可在源码 `COOLDOWN` 字段自由调整）。  
- **轴线**：理论上 100 km，足够任何规模场景；永久存在，随小球一起销毁。  
- **距离计算**：只计算 **选中的标记球**，未选中的不参与。  
- **依赖**：仅依赖 `UnityEditor` 与 `UnityEngine`；URP/HDRP/内置管线通用。  
- **版本要求**：Unity 2019.1+（用了 `SceneView.duringSceneGui`）。

---

## 5. 源码结构
```
+-- Editor
    |
    +-- MeshPickerEditor.cs
```
| 区域 | 用途 |
|---|---|
| `#region 状态字段` | 冷却、计数、选集、距离缓存 |
| `#region 生命周期` | 窗口打开/关闭事件 |
| `#region 窗口 UI` | 距离大字号、提示、清除按钮 |
| `#region Scene 交互` | 按键冷却、射线检测、放置标记 |
| `#region 标记球管理` | 创建/清除所有标记 |
| `#region 距离计算与画轴线` | 实时最远距 + 每帧重绘无限轴线 |

---

## 6. 可二次开发
- 修改 `COOLDOWN` 调整放置间隔；  
- 修改 `inf` 长度或 `DrawAxis` 换成 `Gizmos` / `Handles`；  
- 把距离结果写成 `ScriptableObject` 或 `Json` 供运行时读取；  
- 支持 `Undo`：把 `Undo.RegisterCreatedObjectUndo(marker, "Place Marker");` 插进 `CreateMarker` 即可。

---

## 7. License
MIT – 随意商用、修改、分发，保留原作者信息即可。

---

**Happy Measuring!** 🎯
