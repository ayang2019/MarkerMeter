///*************************************************
// * 工具名: 打点测距器 (MarkerMeter)
// * 作者  : ayangbing@hotmail.com
// * 仓库  : https://github.com/ayang2019/MarkerMeter/
// * 许可  : MIT License
// * 版本  : 1.0.0
// * 日期  : <最后修改日期：2025-06-25>
// * 
// * 功能  : Unity Editor 插件，一键标注网格点并实时测量多段距离
// * 说明  : 详见 GitHub README
// *************************************************/

using UnityEngine;
using UnityEditor;
using System.Linq;

public class MeshPickerEditor : EditorWindow
{
    [MenuItem("Tools/Mesh测点距工具 &m")]
    static void Open() => GetWindow<MeshPickerEditor>("MeshPicker");

    #region 状态字段
    int markerCount = 0;
    string statusMsg = "等待操作…";
    MessageType statusType = MessageType.Info;

    GameObject[] selectedMarkers = new GameObject[0];
    float maxDistance;

    /* 1 秒冷却，防止连发 */
    const float COOLDOWN = 1f;
    double lastPlaceTime = -1f;
    #endregion

    #region 生命周期
    void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        ClearAllMarkers();
    }
    #endregion

    #region 窗口 UI
    void OnGUI()
    {
        GUILayout.Space(5);
        if (selectedMarkers.Length >= 2)
        {
            GUIStyle big = new GUIStyle(GUI.skin.label)
            { fontSize = 24, alignment = TextAnchor.MiddleCenter };
            EditorGUILayout.LabelField($"最大间距 = {maxDistance:F3} m", big, GUILayout.Height(30));
        }

        EditorGUILayout.HelpBox(
            "1. Scene 视图中点选物体后按 M 放置标记（1 秒冷却，一按一个）\n" +
            "2. 在 Hierarchy 选中任意两个及以上标记球 → 显示最远距离\n" +
            "3. 关闭窗口或点击【清除全部标记】自动清场",
            MessageType.Info);

        GUILayout.Space(5);
        EditorGUILayout.HelpBox(statusMsg, statusType);

        GUILayout.Space(5);
        if (GUILayout.Button("清除全部标记", GUILayout.Height(25)))
            ClearAllMarkers();

        Repaint();
    }
    #endregion

    #region Scene 交互
    void OnSceneGUI(SceneView sv)
    {
        UpdateSelectionAndDistance();
        foreach (var m in GameObject.FindGameObjectsWithTag("EditorOnly"))
            if (m.name.StartsWith("__Marker_"))
                DrawAxis(m.transform.position);

        Event e = Event.current;
        if (e.type != EventType.KeyDown || e.keyCode != KeyCode.M) return;

        double now = EditorApplication.timeSinceStartup;
        if (now - lastPlaceTime < COOLDOWN)
        {
            SetStatus("冷却中… 请稍后再按 M", MessageType.Warning);
            e.Use();
            return;
        }

        GameObject go = Selection.activeGameObject;
        if (!go) { SetStatus("先点选一个物体再按 M", MessageType.Warning); return; }

        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh) { SetStatus("选中物体缺少 MeshFilter 或 Mesh", MessageType.Warning); return; }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        MeshCollider tmp = null;
        bool had = go.TryGetComponent(out MeshCollider col);
        if (!had)
        {
            tmp = go.AddComponent<MeshCollider>();
            tmp.sharedMesh = mf.sharedMesh;
            col = tmp;
        }

        if (col.Raycast(ray, out RaycastHit hit, float.MaxValue))
        {
            lastPlaceTime = now;
            CreateMarker(hit.point);
            SetStatus($"已放置标记  #{markerCount - 1}", MessageType.Info);
        }
        else
        {
            SetStatus("射线未与网格相交", MessageType.Warning);
        }

        if (tmp) DestroyImmediate(tmp);
        e.Use();
    }
    #endregion

    #region 标记球管理
    void CreateMarker(Vector3 pos)
    {
        GameObject m = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m.name = $"__Marker_{markerCount++}";
        m.tag = "EditorOnly";
        m.transform.localScale = Vector3.one * 0.1f;
        var mr = m.GetComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(Shader.Find("Unlit/Color")) { color = Color.red };
        m.transform.position = pos;
        DestroyImmediate(m.GetComponent<Collider>());
    }

    void ClearAllMarkers()
    {
        var marks = GameObject.FindGameObjectsWithTag("EditorOnly")
                              .Where(g => g.name.StartsWith("__Marker_")).ToArray();
        foreach (var m in marks) DestroyImmediate(m);
        markerCount = 0;
        selectedMarkers = new GameObject[0];
        SetStatus("已清除全部标记", MessageType.Info);
    }
    #endregion

    #region 距离计算与画轴线
    void UpdateSelectionAndDistance()
    {
        selectedMarkers = Selection.gameObjects
                                  .Where(g => g.name.StartsWith("__Marker_"))
                                  .ToArray();
        maxDistance = 0f;
        if (selectedMarkers.Length < 2) return;
        for (int i = 0; i < selectedMarkers.Length - 1; i++)
            for (int j = i + 1; j < selectedMarkers.Length; j++)
            {
                float d = Vector3.Distance(selectedMarkers[i].transform.position,
                                          selectedMarkers[j].transform.position);
                if (d > maxDistance) maxDistance = d;
            }
    }

    void DrawAxis(Vector3 pos)
    {
        float inf = 100000f;
        Debug.DrawRay(pos, Vector3.right * inf, Color.red, 0f);
        Debug.DrawRay(pos, Vector3.up * inf, Color.green, 0f);
        Debug.DrawRay(pos, Vector3.forward * inf, Color.blue, 0f);
    }

    void SetStatus(string msg, MessageType t)
    {
        statusMsg = msg;
        statusType = t;
    }
    #endregion
}
