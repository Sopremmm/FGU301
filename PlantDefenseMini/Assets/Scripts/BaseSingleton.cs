using UnityEngine;

public class BaseSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] bool dontDestroyOnload; // Giữ object này khi đổi scene
    public static T Instance { get; private set; } // Instance singleton hiện tại

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            if (dontDestroyOnload) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
