using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class CreateRankRequest
{
    public int level;
    public string name;
    public int score;
}

[System.Serializable]
public class RankData
{
    public int id;
    public int level;
    public string name;
    public int score;
}

[System.Serializable]
public class MyRankData
{
    public int rank;
    public int total;
}

[System.Serializable]
public class UpdateRankRequest
{
    public int score;
}

[System.Serializable]
public class RankDataArray
{
    public RankData[] items;
}

public class kangtoe99_RankApi : MonoBehaviour
{
    public static kangtoe99_RankApi Instance { get; private set; }

    [SerializeField] private string baseUrl = "https://bob-ranking-api.devman11.xyz";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // POST /rank
    public IEnumerator CreateRank(int level, string name, int score,
        Action<RankData> onSuccess, Action<string> onError = null)
    {
        var body = JsonUtility.ToJson(new CreateRankRequest
        {
            level = level,
            name = name,
            score = score
        });

        using var request = new UnityWebRequest($"{baseUrl}/rank", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var rank = JsonUtility.FromJson<RankData>(request.downloadHandler.text);
        onSuccess?.Invoke(rank);
    }

    // GET /rank/my-rank/:id
    public IEnumerator GetMyRank(int id,
        Action<MyRankData> onSuccess, Action<string> onError = null)
    {
        using var request = UnityWebRequest.Get($"{baseUrl}/rank/my-rank/{id}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var myRank = JsonUtility.FromJson<MyRankData>(request.downloadHandler.text);
        onSuccess?.Invoke(myRank);
    }

    // GET /rank/:id
    public IEnumerator GetRank(int id,
        Action<RankData> onSuccess, Action<string> onError = null)
    {
        using var request = UnityWebRequest.Get($"{baseUrl}/rank/{id}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var rank = JsonUtility.FromJson<RankData>(request.downloadHandler.text);
        onSuccess?.Invoke(rank);
    }

    // GET /rank
    public IEnumerator GetAllRanks(
        Action<RankData[]> onSuccess, Action<string> onError = null)
    {
        using var request = UnityWebRequest.Get($"{baseUrl}/rank");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        // JsonUtility는 최상위 배열을 파싱할 수 없으므로 래핑
        var json = "{\"items\":" + request.downloadHandler.text + "}";
        var array = JsonUtility.FromJson<RankDataArray>(json);
        onSuccess?.Invoke(array.items);
    }

    // PATCH /rank/:id
    public IEnumerator UpdateRank(int id, int score,
        Action<RankData> onSuccess, Action<string> onError = null)
    {
        var body = JsonUtility.ToJson(new UpdateRankRequest { score = score });

        using var request = new UnityWebRequest($"{baseUrl}/rank/{id}", "PATCH");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        var rank = JsonUtility.FromJson<RankData>(request.downloadHandler.text);
        onSuccess?.Invoke(rank);
    }

    // DELETE /rank/:id
    public IEnumerator DeleteRank(int id,
        Action onSuccess, Action<string> onError = null)
    {
        using var request = UnityWebRequest.Delete($"{baseUrl}/rank/{id}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        onSuccess?.Invoke();
    }
}
