/*using UnityEngine;
using System.Collections;

public class AssetBundleManager : MonoBehaviour
{
    public static AssetBundleManager Instance;
    private AssetBundle imageBundle;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator LoadImageBundle()
    {
        string bundleUrl = "http://your-server.com/assetbundles/imagebundle";
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load AssetBundle: " + www.error);
            yield break;
        }

        imageBundle = DownloadHandlerAssetBundle.GetContent(www);
    }

    public Sprite LoadSpriteFromBundle(string spriteName)
    {
        if (imageBundle == null)
        {
            Debug.LogError("Image bundle not loaded!");
            return null;
        }

        return imageBundle.LoadAsset<Sprite>(spriteName);
    }
}
*/
