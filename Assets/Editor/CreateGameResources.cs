using UnityEngine;
using UnityEditor;
using Patchwork.Data;

public class CreateGameResources
{
    [MenuItem("Game/Create Game Resources")]
    public static void Create()
    {
        // Create the asset
        GameResources resources = ScriptableObject.CreateInstance<GameResources>();

        // Create the Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // Save the asset
        AssetDatabase.CreateAsset(resources, "Assets/Resources/GameResources.asset");
        AssetDatabase.SaveAssets();

        // Select the created asset
        Selection.activeObject = resources;
        
        Debug.Log("GameResources asset created in Resources folder");
    }
} 