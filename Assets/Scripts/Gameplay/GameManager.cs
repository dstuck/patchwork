using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Patchwork.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        #region Private Fields
        [Header("Scene Names")]
        [SerializeField] private string m_MainMenuSceneName = "MainMenu";
        [SerializeField] private string m_GameplaySceneName = "GameplayScene";
        [SerializeField] private string m_TransitionSceneName = "TransitionScene";
        
        [Header("References")]
        [SerializeField] private Deck m_Deck;
        
        private static GameManager s_Instance;
        private bool m_IsInitialized;
        #endregion

        #region Game State
        private int m_CurrentStage;
        private int m_CumulativeScore;
        #endregion

        #region Public Properties
        public static GameManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    InitializeInstance();
                }
                return s_Instance;
            }
        }

        public int CurrentStage => m_CurrentStage;
        public int CumulativeScore => m_CumulativeScore;
        public Deck Deck => m_Deck;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            if (m_IsInitialized) return;
            
            if (m_Deck == null)
            {
                Debug.LogError("[GameManager] Deck reference is missing!");
                return;
            }

            if (!m_Deck.IsInitialized)
            {
                m_Deck.Initialize();
            }

            m_CurrentStage = 1;
            m_CumulativeScore = 0;
            m_IsInitialized = true;
        }

        private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
            // Handle any scene-specific initialization
        }

        private static void InitializeInstance()
        {
            // First try to find existing instance
            s_Instance = FindFirstObjectByType<GameManager>();

            if (s_Instance == null)
            {
                // Load and instantiate from Resources if not found
                GameObject prefab = Resources.Load<GameObject>("Prefabs/GameManager");
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab);
                    s_Instance = go.GetComponent<GameManager>();
                    if (s_Instance != null)
                    {
                        s_Instance.name = "GameManager";
                        DontDestroyOnLoad(go);
                    }
                    else
                    {
                        Debug.LogError("GameManager component missing from prefab!");
                    }
                }
                else
                {
                    Debug.LogError("GameManager prefab not found in Resources/Prefabs/GameManager!");
                }
            }
        }
        #endregion

        #region Public Methods
        public void StartNewGame()
        {
            m_CumulativeScore = 0;
            
            if (m_Deck != null)
            {
                m_Deck.ResetDeck();
            }
            else
            {
                Debug.LogError("[GameManager] Cannot start game - Deck is null");
                return;
            }

            SceneManager.LoadScene(m_GameplaySceneName);
        }

        public void StartNextStage()
        {
            m_CurrentStage++;
            if (m_Deck != null)
            {
                m_Deck.ResetForNewStage();
            }
            SceneManager.LoadScene(m_GameplaySceneName);
        }

        public void CompleteStage(int _finalScore)
        {
            StartCoroutine(CompleteStageRoutine(_finalScore));
        }

        private IEnumerator CompleteStageRoutine(int _finalScore)
        {
            // Wait for one second before transitioning
            yield return new WaitForSeconds(1f);
            
            // Store the score
            m_CumulativeScore = _finalScore;
            
            // Load the transition scene
            SceneManager.LoadScene(m_TransitionSceneName);
        }

        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene(m_MainMenuSceneName);
        }
        #endregion

        #if UNITY_EDITOR
        [ContextMenu("Force Initialize")]
        private void ForceInitialize()
        {
            Initialize();
        }
        #endif
    }
} 