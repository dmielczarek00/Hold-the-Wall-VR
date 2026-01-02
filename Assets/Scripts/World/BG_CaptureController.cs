using UnityEngine;
using System.IO;

public class CaptureToPng : MonoBehaviour
{
    public Camera captureCamera;
    public RenderTexture renderTexture;
    public string fileName = "bg_capture";
    public int superSize = 1;

    [ContextMenu("Capture PNG")]
    public void Capture()
    {
        if (captureCamera == null || renderTexture == null)
        {
            Debug.LogError("Brak captureCamera lub renderTexture.");
            return;
        }

        var prevRT = RenderTexture.active;
        var prevTarget = captureCamera.targetTexture;

        captureCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;

        captureCamera.Render();

        int w = renderTexture.width * superSize;
        int h = renderTexture.height * superSize;

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        DestroyImmediate(tex);

        string time = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(Application.dataPath, $"{fileName}_{time}.png");
        File.WriteAllBytes(path, png);

        Debug.Log($"Zapisano: {path}");

        captureCamera.targetTexture = prevTarget;
        RenderTexture.active = prevRT;
    }
}