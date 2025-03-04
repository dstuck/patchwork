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
                        Debug.LogError("GameResources asset not found in Resources folder!");
                    }
                }
                return s_Instance;
            }
        }
        #endregion

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Game/Game Resources")]
        public static void CreateAsset()
        {
            if (!System.IO.Directory.Exists("Assets/Resources"))
            {
                System.IO.Directory.CreateDirectory("Assets/Resources");
            }

            var asset = Resources.Load<GameResources>("GameResources");
            if (asset == null)
            {
                asset = CreateInstance<GameResources>();
                UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/Resources/GameResources.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }
        #endif

        #region Serialized Fields
        [Header("Board Elements")]
        [SerializeField] private Sprite m_TileSquareSprite;
        [SerializeField] private Sprite m_DrawGemSprite;
        [SerializeField, Range(0.1f, 5f)] private float m_DrawGemScale = 4f;
        
        [Header("Collectibles")]
        [SerializeField] private Sprite m_SparkSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_SparkScale = 0.1f;
        [SerializeField] private Sprite m_FlameSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_FlameScale = 0.1f;
        
        [SerializeField] private Sprite m_ScoreBonusSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_ScoreBonusScale = 0.1f;
        [SerializeField] private Sprite m_MultiplierBonusSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_MultiplierBonusScale = 0.1f;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject m_UpgradeTooltipPrefab;
        #endregion

        #region Public Properties
        public Sprite TileSquareSprite => m_TileSquareSprite;
        public Sprite DrawGemSprite => m_DrawGemSprite;
        public float DrawGemScale => m_DrawGemScale;
        public Sprite SparkSprite => m_SparkSprite;
        public float SparkScale => m_SparkScale;
        public Sprite FlameSprite => m_FlameSprite;
        public float FlameScale => m_FlameScale;
        public Sprite ScoreBonusSprite => m_ScoreBonusSprite;
        public float ScoreBonusScale => m_ScoreBonusScale;
        public Sprite MultiplierBonusSprite => m_MultiplierBonusSprite;
        public float MultiplierBonusScale => m_MultiplierBonusScale;
        public GameObject UpgradeTooltipPrefab => m_UpgradeTooltipPrefab;
        #endregion
    } 
}