using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System;
using System.Reflection;

public class CreateNotWalkVolumesEditor : EditorWindow
{
    GameObject terrainSource = null; // optional: use if terrain is not activeTerrain
    float margin = 2f; // extra padding around terrain bounds
    float height = 20f; // vertical size of the NotWalkable box
    string areaName = "Not Walkable"; // area to set (common name)
    string parentName = "NotWalkVolumes";
    string volumeName = "NotVol_Outer";

    [MenuItem("Tools/TD/Path/Create NotWalk Volume")]
    public static void ShowWindow()
    {
        GetWindow<CreateNotWalkVolumesEditor>("Create NotWalk Volume");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Create a Not-Walkable NavMeshModifierVolume covering the terrain", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        terrainSource = (GameObject)EditorGUILayout.ObjectField("Terrain GameObject (optional)", terrainSource, typeof(GameObject), true);
        margin = EditorGUILayout.FloatField(new GUIContent("Margin (meters)", "Extra padding beyond terrain bounds"), margin);
        height = EditorGUILayout.FloatField(new GUIContent("Height (Y)", "Vertical height of the NotWalkable volume"), height);
        areaName = EditorGUILayout.TextField(new GUIContent("Area Name", "NavMesh area name for not-walkable (default 'Not Walkable')"), areaName);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create NotWalk Volume"))
        {
            CreateNotWalkVolume();
        }

        if (GUILayout.Button("Help: Usage"))
        {
            EditorUtility.DisplayDialog("Usage",
                "This tool creates a big NavMeshModifierVolume that marks the terrain exterior as Not Walkable.\n\n" +
                "1) Optionally assign the Terrain GameObject. If left blank the active Terrain in scene is used.\n" +
                "2) Adjust Margin to add padding outside the terrain bounds.\n" +
                "3) Click Create NotWalk Volume and then rebake your NavMeshSurface.", "OK");
        }
    }

    void CreateNotWalkVolume()
    {
        // Find terrain
        Terrain terrain = null;
        if (terrainSource != null)
        {
            terrain = terrainSource.GetComponent<Terrain>();
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected GameObject does not contain a Terrain component.", "OK");
                return;
            }
        }
        else
        {
            terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                // try to find any Terrain in scene
                terrain = GameObject.FindObjectOfType<Terrain>();
                if (terrain == null)
                {
                    EditorUtility.DisplayDialog("Error", "No Terrain found in scene. Please provide a Terrain GameObject.", "OK");
                    return;
                }
            }
        }

        // Get terrain world bounds
        Vector3 terrainPos = terrain.transform.position;
        Vector3 size = terrain.terrainData.size; // size in local space; terrain transform may influence
        // world extents center and size
        Vector3 worldCenter = terrainPos + new Vector3(size.x * 0.5f, 0f, size.z * 0.5f);
        float worldSizeX = size.x + margin * 2f;
        float worldSizeZ = size.z + margin * 2f;

        // Create parent container if not present
        GameObject parent = GameObject.Find(parentName);
        if (parent == null)
        {
            parent = new GameObject(parentName);
            Undo.RegisterCreatedObjectUndo(parent, "Create NotWalkVolumes Parent");
        }

        // Create volume GameObject
        GameObject volGO = new GameObject(volumeName);
        Undo.RegisterCreatedObjectUndo(volGO, "Create NotWalk Volume");
        volGO.transform.parent = parent.transform;
        // place center at terrain center; Y center should be terrain pos.y + height/2
        float centerY = terrainPos.y + height * 0.5f;
        volGO.transform.position = new Vector3(worldCenter.x, centerY, worldCenter.z);
        volGO.transform.rotation = Quaternion.identity;

        // Find NavMeshModifierVolume type via reflection
        Type navVolType = FindTypeByName("NavMeshModifierVolume");
        if (navVolType == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find NavMeshModifierVolume type. Make sure NavMeshComponents/AI Navigation package is installed.", "OK");
            DestroyImmediate(volGO);
            return;
        }

        // Add component via Undo so it's undoable
        Component navComp = Undo.AddComponent(volGO, navVolType);
        if (navComp == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to add NavMeshModifierVolume component to the object.", "OK");
            DestroyImmediate(volGO);
            return;
        }

        // Compute size vector
        Vector3 sizeVec = new Vector3(worldSizeX, height, worldSizeZ);

        // Try to set center/size/area via reflection (same helper methods as earlier)
        SetVector3Member(navComp, "center", Vector3.zero); // keep local center zero; we positioned transform at world center
        SetVector3Member(navComp, "size", sizeVec);

        int areaIndex = 1; // fallback
        try
        {
            int idx = NavMesh.GetAreaFromName(areaName);
            if (idx >= 0) areaIndex = idx;
        }
        catch { /* ignore and fall back */ }

        SetIntMember(navComp, "area", areaIndex);
        SetBoolMemberIfExists(navComp, "overrideArea", true);

        // Select the created object for convenience
        Selection.activeGameObject = volGO;

        EditorUtility.DisplayDialog("Done", $"Created NotWalk volume '{volGO.name}' covering terrain. Now rebake your NavMeshSurface.", "OK");
    }

    static Type FindTypeByName(string shortName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            Type[] types = null;
            try { types = asm.GetTypes(); } catch { continue; }
            foreach (var t in types)
            {
                if (t.Name == shortName) return t;
            }
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
