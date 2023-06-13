using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyToRenderTex : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        //get the material from the renderer
        material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        //copy the material to the render texture, flipping it vertically
        Graphics.Blit(material.mainTexture, renderTexture, new Vector2(1, -1), new Vector2(0, 1));
    }
}
