#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AdvancedTPSCharacter.Editor
{
    [InitializeOnLoad]
    internal static class Phase4HudAssetMaterializer
    {
        private const string ResourcesFolderPath = "Assets/AdvancedTPSCharacter/Resources";
        private const string HudResourcesFolderPath = ResourcesFolderPath + "/AdvancedTPSUI";
        private const string PanelSettingsAssetPath = HudResourcesFolderPath + "/HUDPanelSettings.asset";

        static Phase4HudAssetMaterializer()
        {
            EditorApplication.delayCall += EnsureHudPanelSettingsAsset;
        }

        [MenuItem("Advanced TPS/Setup/Materialize HUD Panel Settings")]
        private static void MaterializeHudPanelSettingsAssetMenu()
        {
            EnsureHudPanelSettingsAsset(forceLog: true);
        }

        private static void EnsureHudPanelSettingsAsset()
        {
            EnsureHudPanelSettingsAsset(forceLog: false);
        }

        private static void EnsureHudPanelSettingsAsset(bool forceLog)
        {
            if (AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsAssetPath) != null)
            {
                if (forceLog)
                {
                    Debug.Log("[Advanced TPS] HUD PanelSettings asset already exists at " + PanelSettingsAssetPath);
                }

                return;
            }

            EnsureFolder(ResourcesFolderPath);
            EnsureFolder(HudResourcesFolderPath);

            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.name = "HUDPanelSettings";
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            panelSettings.sortingOrder = 100;

            AssetDatabase.CreateAsset(panelSettings, PanelSettingsAssetPath);
            EditorUtility.SetDirty(panelSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Advanced TPS] Created HUD PanelSettings asset at " + PanelSettingsAssetPath);
        }

        private static void EnsureFolder(string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parentFolder = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(parentFolder))
            {
                return;
            }

            EnsureFolder(parentFolder);

            string folderName = Path.GetFileName(folderPath);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }
    }
}
#endif
