using System.IO;
using UnityEditor;
using UnityEngine;

// Menu: Alien Game > Save Game View Screenshot (use it while the game is playing).
// Saves a PNG into the Screenshots folder next to Assets.
public static class ScreenshotTool
{
    [MenuItem("Alien Game/Save Game View Screenshot")]
    public static void Save()
    {
        Directory.CreateDirectory("Screenshots");
        string path = "Screenshots/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + System.DateTime.Now.ToString("HHmmss") + ".png";
        ScreenCapture.CaptureScreenshot(path, 2);
        Debug.Log("Saved screenshot: " + path);
    }
}
