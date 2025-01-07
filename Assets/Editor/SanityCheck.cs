using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Patchwork.Gameplay;

public class SanityCheck
{
    private const float c_TestTimeout = 10f; // Timeout in seconds
    private static bool s_TestsComplete = false;
    private static List<string> s_Errors = new List<string>();

    [MenuItem("Tools/Run Sanity Check")]
    public static void RunSanityCheck()
    {
        // Check if Unity is already running this project
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Cannot run sanity check while Unity is in Play mode");
            EditorApplication.Exit(1);
            return;
        }

        // Force close any existing Unity processes for this project
        if (EditorPrefs.HasKey("UnityProjectOpenedPID"))
        {
            EditorPrefs.DeleteKey("UnityProjectOpenedPID");
        }

        // Start coroutine via EditorApplication.update
        EditorApplication.update += RunSanityCheckCoroutine;
        Debug.Log("Starting sanity check...");
    }

    private static void RunSanityCheckCoroutine()
    {
        if (s_TestsComplete)
        {
            EditorApplication.update -= RunSanityCheckCoroutine;
            
            if (s_Errors.Count > 0)
            {
                Debug.LogError("Sanity check failed with errors:");
                foreach (string error in s_Errors)
                {
                    Debug.LogError(error);
                }
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log("Sanity check completed successfully!");
                EditorApplication.Exit(0);
            }
            return;
        }

        try
        {
            // Load the gameplay scene
            Scene scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                scene = EditorSceneManager.OpenScene("Assets/Scenes/GameplayScene.unity", OpenSceneMode.Single);
            }

            // Basic scene validation
            if (!scene.IsValid())
            {
                s_Errors.Add("Failed to load gameplay scene");
                s_TestsComplete = true;
                return;
            }

            // Test game initialization
            var gameManager = GameObject.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                s_Errors.Add("GameManager not found in scene");
                s_TestsComplete = true;
                return;
            }

            // Test GameManager initialization
            if (gameManager.Deck == null)
            {
                s_Errors.Add("GameManager's Deck reference is missing");
                s_TestsComplete = true;
                return;
            }

            s_TestsComplete = true;
        }
        catch (System.Exception e)
        {
            s_Errors.Add($"Exception during sanity check: {e.Message}");
            s_TestsComplete = true;
        }
    }
} 