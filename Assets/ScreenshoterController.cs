using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScreenshoterController : MonoBehaviour
{
    [SerializeField]
    private string path = "screenshot_";
    [SerializeField, Range(1, 5)]
    private int size = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the P key is pressed and we are in the editor
        if (Input.GetKeyDown(KeyCode.P) && Application.isEditor)
        {
            TakeScreenshot();
        }
    }

    // Method to take a screenshot
    private void TakeScreenshot()
    {
        string currentPath = path;
        string gameVersionString = StageManager.Instance.gameVersion+"";
        string folderPath = Path.Combine(path, gameVersionString);
        
        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        string fullPath = Path.Combine(folderPath, fileName);

        ScreenCapture.CaptureScreenshot(fullPath, size);
        Debug.Log("Screenshot taken: " + fullPath);
    }

#if UNITY_EDITOR
    // Button in the inspector to take a screenshot
    [ContextMenu("Take Screenshot")]
    private void TakeScreenshotButton()
    {
        TakeScreenshot();
    }
#endif
}
