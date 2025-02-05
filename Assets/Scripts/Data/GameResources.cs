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
        [Header("Board Elements")]
        [SerializeField] private Sprite m_TileSquareSprite;
        [SerializeField] private Sprite m_DrawGemSprite;
        [SerializeField, Range(0.1f, 5f)] private float m_DrawGemScale = 4f;
        
        [Header("Collectibles")]
        [SerializeField] private Sprite m_SparkSprite;
        [SerializeField] private Sprite m_FlameSprite;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject m_UpgradeTooltipPrefab;
        #endregion

        #region Public Properties
        public Sprite TileSquareSprite => m_TileSquareSprite;
        public Sprite DrawGemSprite => m_DrawGemSprite;
        public float DrawGemScale => m_DrawGemScale;
        public Sprite SparkSprite => m_SparkSprite;
        public Sprite FlameSprite => m_FlameSprite;
        public GameObject UpgradeTooltipPrefab => m_UpgradeTooltipPrefab;
        #endregion
    } 
}