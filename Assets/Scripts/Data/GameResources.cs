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

        #region Serialized Fields
        [Header("Board Elements")]
        [SerializeField] private Sprite m_TileSquareSprite;
        [SerializeField] private Sprite m_DrawGemSprite;
        [SerializeField, Range(0.1f, 5f)] private float m_DrawGemScale = 4f;
        
        [Header("Collectibles")]
        [SerializeField] private Sprite m_SparkSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_SparkScale = 0.1f;
        [SerializeField] private Sprite m_GhostSparkSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_GhostSparkScale = 0.1f;
        [SerializeField] private Sprite m_JumpingSparkSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_JumpingSparkScale = 0.1f;
        [SerializeField] private Sprite m_FlameSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_FlameScale = 0.1f;
        [SerializeField] private Sprite m_HeartPieceSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_HeartPieceScale = 0.1f;
        
        [SerializeField] private Sprite m_ScoreBonusSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_ScoreBonusScale = 0.1f;
        [SerializeField] private Sprite m_MultiplierBonusSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_MultiplierBonusScale = 0.1f;
        [SerializeField] private Sprite m_PaintSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_PaintScale = 0.1f;
        [SerializeField] private Sprite m_NewSquareSprite;
        [SerializeField, Range(0.01f, 5f)] private float m_NewSquareScale = 0.8f;

        [Header("UI Elements")]
        [SerializeField] private GameObject m_UpgradeTooltipPrefab;
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip m_PickupSoundFX;
        [SerializeField] private AudioClip m_DamageSoundFX;
        [SerializeField] private AudioClip m_LoseSoundFX;
        [SerializeField] private AudioClip[] m_PlaceTileSoundFX;
        [SerializeField] private AudioClip[] m_MoveSoundFX;
        [SerializeField] private AudioClip m_RotateLeftSoundFX;
        [SerializeField] private AudioClip m_RotateRightSoundFX;
        #endregion

        #region Public Properties
        public Sprite TileSquareSprite => m_TileSquareSprite;
        public Sprite DrawGemSprite => m_DrawGemSprite;
        public float DrawGemScale => m_DrawGemScale;
        public Sprite SparkSprite => m_SparkSprite;
        public float SparkScale => m_SparkScale;
        public Sprite GhostSparkSprite => m_GhostSparkSprite;
        public float GhostSparkScale => m_GhostSparkScale;
        public Sprite JumpingSparkSprite => m_JumpingSparkSprite;
        public float JumpingSparkScale => m_JumpingSparkScale;
        public Sprite FlameSprite => m_FlameSprite;
        public float FlameScale => m_FlameScale;
        public Sprite ScoreBonusSprite => m_ScoreBonusSprite;
        public float ScoreBonusScale => m_ScoreBonusScale;
        public Sprite MultiplierBonusSprite => m_MultiplierBonusSprite;
        public float MultiplierBonusScale => m_MultiplierBonusScale;
        public Sprite HeartPieceSprite => m_HeartPieceSprite;
        public float HeartPieceScale => m_HeartPieceScale;
        public GameObject UpgradeTooltipPrefab => m_UpgradeTooltipPrefab;
        public Sprite PaintSprite => m_PaintSprite;
        public float PaintScale => m_PaintScale;
        public Sprite NewSquareSprite => m_NewSquareSprite;
        public float NewSquareScale => m_NewSquareScale;
        public AudioClip PickupSoundFX => m_PickupSoundFX;
        public AudioClip DamageSoundFX => m_DamageSoundFX;
        public AudioClip LoseSoundFX => m_LoseSoundFX;
        public AudioClip[] PlaceTileSoundFX => m_PlaceTileSoundFX;
        public AudioClip[] MoveSoundFX => m_MoveSoundFX;
        public AudioClip RotateLeftSoundFX => m_RotateLeftSoundFX;
        public AudioClip RotateRightSoundFX => m_RotateRightSoundFX;
        #endregion
    } 
}