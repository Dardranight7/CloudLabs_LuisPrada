using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DataManager : MonoBehaviour
{
    private const string DataFileName = "estudiantes.json";
    private static readonly List<StudentDTO> EmptyStudents = new List<StudentDTO>();

    public static event Action OnDataChange;

    public static DataManager Instance { get; private set; }
    public static IReadOnlyList<StudentDTO> Students =>
        Instance != null && Instance.classroom != null && Instance.classroom.estudiantes != null
            ? Instance.classroom.estudiantes
            : EmptyStudents;

    public static bool HasData { get; private set; }

    [SerializeField] private ClassroomDTO classroom = new ClassroomDTO();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (Instance != null)
        {
            return;
        }

        DataManager existingInstance = FindFirstObjectByType<DataManager>();
        if (existingInstance != null)
        {
            return;
        }

        GameObject dataManagerObject = new GameObject(nameof(DataManager));
        dataManagerObject.AddComponent<DataManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ReloadData();
    }

    public void ReloadData()
    {
        StopAllCoroutines();
        StartCoroutine(LoadData());
    }

    private IEnumerator LoadData()
    {
        HasData = false;
        string dataPath = BuildDataPath();

        using (UnityWebRequest request = UnityWebRequest.Get(dataPath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"No se pudo cargar '{DataFileName}' desde StreamingAssets. {request.error}", this);
                yield break;
            }

            ClassroomDTO loadedClassroom;
            try
            {
                loadedClassroom = JsonUtility.FromJson<ClassroomDTO>(request.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Debug.LogError($"El archivo '{DataFileName}' contiene un JSON inválido.\n{exception}", this);
                yield break;
            }

            if (loadedClassroom == null || loadedClassroom.estudiantes == null)
            {
                Debug.LogError($"El archivo '{DataFileName}' no contiene una lista 'estudiantes' válida.", this);
                yield break;
            }

            classroom = loadedClassroom;
            HasData = true;
            OnDataChange?.Invoke();
        }
    }

    private static string BuildDataPath()
    {
        string path = $"{Application.streamingAssetsPath.TrimEnd('/', '\\')}/{DataFileName}";

        if (path.Contains("://") || path.StartsWith("jar:", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return new Uri(path).AbsoluteUri;
    }

}
