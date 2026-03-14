#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ReloadWeaponPrefabValidator
{
    [MenuItem("Tools/Advanced TPS/Validate ReloadWeapon Prefabs")]
    public static void ValidateReloadWeaponPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/AdvancedTPSCharacter/Prefabs" });
        var missingRefs = new List<string>();

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            try
            {
                ReloadWeapon[] reloadComponents = prefabRoot.GetComponentsInChildren<ReloadWeapon>(true);
                foreach (ReloadWeapon reloadWeapon in reloadComponents)
                {
                    SerializedObject serializedReloadWeapon = new SerializedObject(reloadWeapon);

                    SerializedProperty rigController = serializedReloadWeapon.FindProperty("rigController");
                    SerializedProperty animationEvents = serializedReloadWeapon.FindProperty("animationEvents");
                    SerializedProperty lefthand = serializedReloadWeapon.FindProperty("lefthand");
                    SerializedProperty ammoWidget = serializedReloadWeapon.FindProperty("ammoWidget");

                    if (rigController.objectReferenceValue == null
                        || animationEvents.objectReferenceValue == null
                        || lefthand.objectReferenceValue == null
                        || ammoWidget.objectReferenceValue == null)
                    {
                        missingRefs.Add($"{assetPath} -> {GetHierarchyPath(reloadWeapon.transform)} " +
                                        $"(rigController: {rigController.objectReferenceValue != null}, " +
                                        $"animationEvents: {animationEvents.objectReferenceValue != null}, " +
                                        $"lefthand: {lefthand.objectReferenceValue != null}, " +
                                        $"ammoWidget: {ammoWidget.objectReferenceValue != null})");
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        if (missingRefs.Count == 0)
        {
            Debug.Log($"ReloadWeapon prefab validation passed. Checked {prefabGuids.Length} prefabs.");
            return;
        }

        Debug.LogWarning("ReloadWeapon prefab validation found missing references:\n" + string.Join("\n", missingRefs));
    }

    private static string GetHierarchyPath(Transform target)
    {
        if (target == null)
        {
            return "<null>";
        }

        string path = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
#endif
