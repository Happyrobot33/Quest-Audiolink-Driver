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
    public RenderTexture croppedRenderTexture;
    public Camera projectionCamera;
    Texture2D rawImage;
    Texture2D croppedImage;
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
        RenderTexture.active = null;
        rawImage.Apply();

        detector.ProcessImage(
            rawImage.GetPixels32(),
            projectionCamera.fieldOfView * Mathf.Deg2Rad,
            projectionCamera.pixelHeight
        );

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

        //crop the image
        croppedImage = CropScale.CropTexture(
            rawImage,
            new Vector2(bounds.width, bounds.height),
            CropOptions.CUSTOM,
            (int)bounds.x,
            (int)bounds.y
        );

        //apply the cropped image to the cropped render texture
        Graphics.Blit(croppedImage, croppedRenderTexture);
    }

    /// <summary> This function takes in a 3d tag positon and converts it to a 2d position on the screen, with the z position being the depth. </summary>
    /// <param name="position"> The 3d position of the tag. </param>
    /// <returns> The 2d position of the tag, with the z position being the depth. </returns>
    private Vector3 GetRawTagPosition(Vector3 position)
    {
        Vector3 camPos = projectionCamera.WorldToScreenPoint(position);
        return camPos;
    }
}
