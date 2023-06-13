using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioInput : MonoBehaviour
{
    public string deviceName;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        int deviceMaxFreq,
            deviceMinFreq;
        Microphone.GetDeviceCaps(deviceName, out deviceMinFreq, out deviceMaxFreq);
        //int sampleRate = AudioSettings.outputSampleRate / deviceMaxFreq;
        int sampleRate = AudioSettings.outputSampleRate;
        audioSource.clip = Microphone.Start(deviceName, true, 10, sampleRate);
        audioSource.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        audioSource.Play();
    }
}
