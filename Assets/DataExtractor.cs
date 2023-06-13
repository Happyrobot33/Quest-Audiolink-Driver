using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AprilTag;

/// <summary> The point of this class is to extract data from different sources and convert it into the format that can be fed back to OSC. </summary>
public class DataExtractor : MonoBehaviour
{
    public RenderTexture windowRenderTexture;
    Texture2D rawImage;
    TagDetector detector;

    // Start is called before the first frame update
    void Start()
    {
        detector = new TagDetector(windowRenderTexture.width, windowRenderTexture.height);
        rawImage = new Texture2D(
            windowRenderTexture.width,
            windowRenderTexture.height,
            TextureFormat.RGB24,
            false
        );
    }

    // Update is called once per frame
    void Update()
    {
        RenderTexture.active = windowRenderTexture;
        rawImage.ReadPixels(
            new Rect(0, 0, windowRenderTexture.width, windowRenderTexture.height),
            0,
            0
        );
        rawImage.Apply();

        var image = rawImage.GetPixels32();
        detector.ProcessImage(image, 0, 0.1f);
        foreach (var tag in detector.DetectedTags)
        {
            Debug.Log(tag.ID);
        }
    }
}
