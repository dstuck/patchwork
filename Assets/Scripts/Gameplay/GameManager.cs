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
            
            if (!m_IsInitialized)
            {
                Initialize();
            }
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
            m_CurrentStage = 1;
            m_CumulativeScore = 0;
            m_IsInitialized = true;
        }

        private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
            // Handle any scene-specific initialization
            Debug.Log($"Scene loaded: {_scene.name}");
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
                        Debug.Log("GameManager instance created from prefab");
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
            Initialize();
            SceneManager.LoadScene(m_GameplaySceneName);
        }

        public void StartNextStage()
        {
            m_CurrentStage++;
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