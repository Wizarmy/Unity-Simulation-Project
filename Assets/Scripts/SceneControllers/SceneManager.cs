using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events;

public class SceneManager : MonoBehaviour
{
    #region Singleton
    private static SceneManager _instance;
    public static SceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SceneManager");
                _instance = go.AddComponent<SceneManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [Header("Settings")]
    [Tooltip("Time in seconds to wait before starting the load (useful for fade out)")]
    public float preLoadDelay = 0.5f;
    
    [Tooltip("Minimum time the loading screen should be visible")]
    public float minimumLoadTime = 1.0f;

    // Events (easy to hook into from UI or other systems)
    public UnityEvent onSceneLoadStarted = new UnityEvent();
    public UnityEvent onSceneLoadCompleted = new UnityEvent();
    public UnityEvent<float> onLoadProgress = new UnityEvent<float>(); // 0.0 to 1.0

    private bool _isLoading;

    /// <summary>
    /// Load a scene by name (additive or single)
    /// </summary>
    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (_isLoading) return;
        StartCoroutine(LoadSceneCoroutine(sceneName, mode));
    }

    /// <summary>
    /// Load a scene by build index
    /// </summary>
    public void LoadScene(int sceneBuildIndex, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (_isLoading) return;
        StartCoroutine(LoadSceneCoroutine(sceneBuildIndex, mode));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode mode)
    {
        _isLoading = true;
        onSceneLoadStarted.Invoke();

        // Optional pre-load delay (e.g., for fade-out animation)
        yield return new WaitForSeconds(preLoadDelay);

        var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
        if (operation != null)
        {
            operation.allowSceneActivation = false;

            var timer = 0f;

            while (!operation.isDone)
            {
                var progress = Mathf.Clamp01(operation.progress / 0.9f); // 0.9 is the real completion point
                onLoadProgress.Invoke(progress);

                timer += Time.deltaTime;

                // Wait until minimum load time is reached AND progress is almost complete
                if (progress >= 0.99f && timer >= minimumLoadTime)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        // Scene is fully loaded
        onSceneLoadCompleted.Invoke();
        _isLoading = false;
    }

    private IEnumerator LoadSceneCoroutine(int sceneBuildIndex, LoadSceneMode mode)
    {
        _isLoading = true;
        onSceneLoadStarted.Invoke();

        yield return new WaitForSeconds(preLoadDelay);

        var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneBuildIndex, mode);
        if (operation != null)
        {
            operation.allowSceneActivation = false;

            var timer = 0f;

            while (!operation.isDone)
            {
                var progress = Mathf.Clamp01(operation.progress / 0.9f);
                onLoadProgress.Invoke(progress);

                timer += Time.deltaTime;

                if (progress >= 0.99f && timer >= minimumLoadTime)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        onSceneLoadCompleted.Invoke();
        _isLoading = false;
    }

    /// <summary>
    /// Reload the current scene (useful for restarts)
    /// </summary>
    public void ReloadCurrentScene()
    {
        LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Quick way to go to next scene in build order
    /// </summary>
    public void LoadNextScene()
    {
        var nextIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
            LoadScene(nextIndex);
        else
            Debug.LogWarning("No next scene in build settings.");
    }
}