// Assets/Editor/PathToNavVolumesEditor.cs
// Place this file in an "Editor" folder (Assets/Editor/) in your project.
// This version uses reflection to find NavMeshModifierVolume so it compiles across Unity versions.

using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System;
using System.Reflection;

public class PathToNavVolumesEditor : EditorWindow
{
    GameObject pathRoot;
    string parentName = "PathNavVolumes";
    string prefix = "NavVol_";
    float width = 3f;
    float height = 5f;
    float overlapPercent = 0.2f; // 20% overlap
    string areaName = "Walkable";

    [MenuItem("Tools/TD/Path/Create NavMesh Volumes")]
    static void ShowWindow()
    {
        GetWindow<PathToNavVolumesEditor>("Create NavVolumes");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Create NavMeshModifierVolumes along a path", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        pathRoot = (GameObject)EditorGUILayout.ObjectField("Path Root (select)", pathRoot, typeof(GameObject), true);
        parentName = EditorGUILayout.TextField("Parent Name", parentName);
        prefix = EditorGUILayout.TextField("Volume Prefix", prefix);

        EditorGUILayout.Space();
        width = EditorGUILayout.FloatField(new GUIContent("Width (X)", "Width of each box (X axis)"), width);
        height = EditorGUILayout.FloatField(new GUIContent("Height (Y)", "Height of each box (Y axis)"), height);
        overlapPercent = EditorGUILayout.Slider(new GUIContent("Overlap %", "How much extra length to add for overlap between segments"), overlapPercent, 0f, 0.9f);

        EditorGUILayout.Space();
        areaName = EditorGUILayout.TextField(new GUIContent("Area Name", "NavMesh area name (usually 'Walkable')"), areaName);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Volumes"))
        {
            if (pathRoot == null)
            {
                EditorUtility.DisplayDialog("Error", "Select a root GameObject that contains child waypoints (in order).", "OK");
            }
            else
            {
                CreateVolumesFromPath();
            }
        }

        if (GUILayout.Button("Help: Usage"))
        {
            EditorUtility.DisplayDialog("Usage",
                "1) Create an empty GameObject (call it PathNodes) and add child empties placed along the center of the trench in order.\n" +
                "2) Select the PathNodes object here.\n" +
                "3) Tweak Width/Height/Overlap % then press Create Volumes.\n" +
                "The script will create a PathNavVolumes parent with volumes between consecutive children.", "OK");
        }
    }

    void CreateVolumesFromPath()
    {
        var children = pathRoot.GetComponentsInChildren<Transform>(true);
        System.Collections.Generic.List<Transform> nodes = new System.Collections.Generic.List<Transform>();
        foreach (var t in children)
        {
            if (t == pathRoot.transform) continue;
            nodes.Add(t);
        }

        if (nodes.Count < 2)
        {
            EditorUtility.DisplayDialog("Error", "Path root must have at least 2 child nodes placed along the path.", "OK");
            return;
        }

        // Locate the NavMeshModifierVolume type at runtime
        Type navVolType = FindTypeByName("NavMeshModifierVolume");
        if (navVolType == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Could not find type 'NavMeshModifierVolume' in loaded assemblies.\n\n" +
                "Make sure the NavMeshComponents / AI Navigation package is installed. " +
                "If you are using a custom NavMesh package, the class name may differ.",
                "OK");
            return;
        }

        // Create parent container
        GameObject parent = new GameObject(parentName);
        Undo.RegisterCreatedObjectUndo(parent, "Create PathNavVolumes");
        parent.transform.position = Vector3.zero;

        // Resolve area index safely
        int areaIndex = 0;
        try
        {
            int idx = NavMesh.GetAreaFromName(areaName);
            if (idx >= 0) areaIndex = idx;
            else areaIndex = 0;
        }
        catch
        {
            areaIndex = 0;
        }

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector3 a = nodes[i].position;
            Vector3 b = nodes[i + 1].position;
            Vector3 dir = b - a;
            float length = dir.magnitude;
            if (length <= 0.001f) continue;

            Vector3 center = (a + b) * 0.5f;

            GameObject volGO = new GameObject(prefix + (i + 1).ToString("D2"));
            Undo.RegisterCreatedObjectUndo(volGO, "Create NavVol");
            volGO.transform.parent = parent.transform;
            volGO.transform.position = center;

            // Rotate so Z axis aligns with the segment direction
            volGO.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);

            // Use Undo.AddComponent so it registers with the undo system
            Component navComp = Undo.AddComponent(volGO, navVolType);

            if (navComp == null)
            {
                Debug.LogWarning($"Failed to add {navVolType.Name} to GameObject {volGO.name}");
                continue;
            }

            // compute length with overlap (we expand along local Z axis)
            float extra = Mathf.Max(0f, overlapPercent * length);
            float finalLength = length + extra;

            // size: X = width, Y = height, Z = finalLength
            Vector3 size = new Vector3(width, height, finalLength);

            // set fields/properties center and size and area via reflection (try property first then field)
            SetVector3Member(navComp, "center", Vector3.zero);
            SetVector3Member(navComp, "size", size);
            SetIntMember(navComp, "area", areaIndex);

            // if there is a property or field named "overrideArea" set it to true if available (some versions)
            SetBoolMemberIfExists(navComp, "overrideArea", true);
        }

        Selection.activeGameObject = parent;
        EditorUtility.DisplayDialog("Done", "Created " + (nodes.Count - 1) + " NavMeshModifierVolumes under '" + parentName + "'.\n\nTip: slightly adjust individual volumes for curves and rebake the NavMesh.", "OK");
    }

    static Type FindTypeByName(string shortName)
    {
        // Search all loaded assemblies for a type by short name
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            try
            {
                var types = asm.GetTypes();
                foreach (var t in types)
                {
                    if (t.Name == shortName) return t;
                }
            }
            catch { /* some assemblies can't be reflected into, ignore */ }
        }
        return null;
    }

    static void SetVector3Member(Component comp, string name, Vector3 value)
    {
        var type = comp.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(Vector3))
        {
            prop.SetValue(comp, value);
            return;
        }
        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(Vector3))
        {
            field.SetValue(comp, value);
            return;
        }
        // maybe private/internal field - try to find any field with that name ignoring case
        var fld = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fld != null && fld.FieldType == typeof(Vector3))
            fld.SetValue(comp, value);
    }

    static void SetIntMember(Component comp, string name, int value)
    {
        var type = comp.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(int))
        {
            prop.SetValue(comp, value);
            return;
        }
        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(int))
        {
            field.SetValue(comp, value);
            return;
        }
        var fld = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fld != null && fld.FieldType == typeof(int))
            fld.SetValue(comp, value);
    }

    static void SetBoolMemberIfExists(Component comp, string name, bool value)
    {
        var type = comp.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(bool))
        {
            prop.SetValue(comp, value);
            return;
        }
        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(comp, value);
            return;
        }
        var fld = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (fld != null && fld.FieldType == typeof(bool))
            fld.SetValue(comp, value);
    }
}
