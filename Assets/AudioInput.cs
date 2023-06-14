using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioInput : MonoBehaviour
{
    public string deviceName;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            int deviceMaxFreq,
                deviceMinFreq;
            Microphone.GetDeviceCaps(deviceName, out deviceMinFreq, out deviceMaxFreq);
            audioSource.clip = Microphone.Start(deviceName, true, 10, deviceMaxFreq);
            audioSource.loop = true;
            //while (!(Microphone.GetPosition(null) > 0)) { }
            audioSource.Play();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
}
