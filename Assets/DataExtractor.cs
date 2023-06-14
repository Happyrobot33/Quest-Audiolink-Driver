using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AprilTag;
using UnityEngine.Profiling;

/// <summary> The point of this class is to extract data from different sources and convert it into the format that can be fed back to OSC. </summary>
public class DataExtractor : MonoBehaviour
{
    public int detectRate = 1;
    public bool debug = false;
    public RenderTexture windowRenderTexture;
    public RenderTexture croppedRenderTexture;
    public Camera projectionCamera;
    Texture2D rawImage;
    Texture2D croppedImage;
    TagDetector detector;

    public float _BAND1 = 0;
    public float _BAND2 = 0;
    public float _BAND3 = 0;
    public float _BAND4 = 0;
    public bool runExtractor = true;

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
        croppedImage = new Texture2D(
            croppedRenderTexture.width,
            croppedRenderTexture.height,
            TextureFormat.RGB24,
            false
        );

        //lock camera aspect ratio to the same as the render texture
        projectionCamera.aspect = windowRenderTexture.width / windowRenderTexture.height;
        //set camera resolution to the same as the render texture
        projectionCamera.targetTexture = windowRenderTexture;
        //ensure the camera is disabled
        projectionCamera.enabled = false;

        //run a single frame to ensure all variables are initialized
        DetectTags();
    }

    // Update is called once per frame
    void Update()
    {
        if (!runExtractor)
        {
            return;
        }
        DetectTags();

        if (detector.DetectedTags.Count() != 2)
        {
            return;
        }

        Vector3 LeftTagPosition = GetRawTagPosition(detector.DetectedTags.ElementAt(0).Position);
        Vector3 RightTagPosition = GetRawTagPosition(detector.DetectedTags.ElementAt(1).Position);

        //create bounds between the tags
        Rect bounds = new Rect(
            LeftTagPosition.x,
            LeftTagPosition.y,
            RightTagPosition.x - LeftTagPosition.x,
            20
        );

        //change the bounds to start top right instead of bottom left
        bounds.y = windowRenderTexture.height - bounds.y - bounds.height;
        bounds.x = windowRenderTexture.width - bounds.x - bounds.width;

        //crop the image
        croppedImage = CropScale.CropTexture(
            rawImage,
            new Vector2(bounds.width, bounds.height),
            CropOptions.CUSTOM,
            (int)bounds.x,
            (int)bounds.y
        );

        if (debug)
        {
            //check if we need to resize the render texture
            if (
                croppedRenderTexture.width != (int)bounds.width
                || croppedRenderTexture.height != (int)bounds.height
            )
            {
                //change the render texture to the cropped render texture
                croppedRenderTexture.Release();
                croppedRenderTexture.width = (int)bounds.width;
                croppedRenderTexture.height = (int)bounds.height;
                croppedRenderTexture.Create();
            }

            //apply the cropped image to the cropped render texture
            Graphics.Blit(croppedImage, croppedRenderTexture);
        }

        //extract the data from the cropped image
        _BAND1 = ExtractData(croppedImage, 0f, 0.25f);
        _BAND2 = ExtractData(croppedImage, 0.25f, 0.5f);
        _BAND3 = ExtractData(croppedImage, 0.5f, 0.75f);
        _BAND4 = ExtractData(croppedImage, 0.75f, 1f);
    }

    private float ExtractData(Texture2D InputImage, float lowerBound, float upperBound)
    {
        // Get the width of the image
        int width = InputImage.width;
        int height = InputImage.height;

        //get a the pixel between the bounds
        int pixel = (int)(width * (lowerBound + upperBound) / 2);

        //get the color of the pixel
        Color pixelColor = InputImage.GetPixel(pixel, height / 2);

        //return the red value of the pixel as 0 to 1
        return pixelColor.grayscale;
    }

    private void DetectTags()
    {
        //begin profiling
        Profiler.BeginSample("Tag Detection");
        //only run on a interval
        if (Time.frameCount % detectRate != 0)
        {
            RenderTexture.active = windowRenderTexture;
            rawImage.ReadPixels(
                new Rect(0, 0, windowRenderTexture.width, windowRenderTexture.height),
                0,
                0
            );
            RenderTexture.active = null;
            rawImage.Apply();
            return;
        }

        detector.ProcessImage(
            rawImage.GetPixels32(),
            projectionCamera.fieldOfView * Mathf.Deg2Rad,
            projectionCamera.pixelHeight
        );
        Profiler.EndSample();
    }

    /// <summary> This function takes in a 3d tag positon and converts it to a 2d position on the screen, with the z position being the depth. </summary>
    /// <param name="position"> The 3d position of the tag. </param>
    /// <returns> The 2d position of the tag, with the z position being the depth. </returns>
    private Vector3 GetRawTagPosition(Vector3 position)
    {
        Vector3 camPos = projectionCamera.WorldToScreenPoint(position);
        return camPos;
    }

    public void inverseEnable(bool value)
    {
        runExtractor = !value;
    }
}
