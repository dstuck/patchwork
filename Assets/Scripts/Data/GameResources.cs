using UnityEngine;

namespace Patchwork.Data
{
    [CreateAssetMenu(fileName = "GameResources", menuName = "Game/Game Resources")]
    public class GameResources : ScriptableObject
    {
        #region Singleton Pattern
        private static GameResources s_Instance;
        public static GameResources Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<GameResources>("GameResources");
                    if (s_Instance == null)
                    {
                        Debug.LogError("GameResources not found in Resources folder!");
                    }
                }
                return s_Instance;
            }
        }
        #endregion

        #region Serialized Fields
        [Header("Sprites")]
        [SerializeField] private Sprite m_TileSquareSprite;
        #endregion

        #region Properties
        public Sprite TileSquareSprite => m_TileSquareSprite;
        #endregion
    } 
}