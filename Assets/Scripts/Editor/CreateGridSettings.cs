using UnityEngine;
using UnityEditor;
using Patchwork.Data;

namespace Patchwork.Editor
{
    public class CreateGridSettings
    {
        [MenuItem("Game/Create Grid Settings")]
        public static void Create()
        {
            // Create Resources folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Check if GridSettings already exists
            GridSettings existingSettings = Resources.Load<GridSettings>("GridSettings");
            if (existingSettings != null)
            {
                Debug.Log("GridSettings already exists in Resources folder");
                Selection.activeObject = existingSettings;
                return;
            }

            // Create new GridSettings
            GridSettings settings = ScriptableObject.CreateInstance<GridSettings>();
            
            // Save the asset
            AssetDatabase.CreateAsset(settings, "Assets/Resources/GridSettings.asset");
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = settings;
            Debug.Log("GridSettings asset created in Resources folder");
        }
    }
} 