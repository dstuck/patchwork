using UnityEngine;
using UnityEditor;

public class CreateTShapedTile
{
    [MenuItem("Game/Create T-Shaped Tile")]
    public static void CreateTile()
    {
        TileData tileData = ScriptableObject.CreateInstance<TileData>();
        
        // Define T shape
        Vector2Int[] squares = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Center
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0),   // Right
            new Vector2Int(0, 1)    // Up
        };

        // Use reflection or create public setup method to set private fields
        var serializedObject = new SerializedObject(tileData);
        serializedObject.FindProperty("m_Squares").arraySize = squares.Length;
        for (int i = 0; i < squares.Length; i++)
        {
            var element = serializedObject.FindProperty("m_Squares").GetArrayElementAtIndex(i);
            element.vector2IntValue = squares[i];
        }
        serializedObject.FindProperty("m_TileName").stringValue = "T-Shaped Tile";
        serializedObject.ApplyModifiedProperties();

        // Save the asset
        AssetDatabase.CreateAsset(tileData, "Assets/Tiles/T_Shaped_Tile.asset");
        AssetDatabase.SaveAssets();
    }
} 