using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioInput : MonoBehaviour
{
    public string deviceName;
    public TMP_Dropdown dropdown;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        initMic();

        // Populate dropdown with available devices
        dropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (string device in Microphone.devices)
        {
            options.Add(device);
        }
        dropdown.AddOptions(options);
    }

    void initMic()
    {
        try
        {
            int deviceMaxFreq,
                deviceMinFreq;
            Microphone.GetDeviceCaps(deviceName, out deviceMinFreq, out deviceMaxFreq);
            audioSource.clip = Microphone.Start(deviceName, true, 1, deviceMaxFreq);
            audioSource.loop = true;
            audioSource.Play();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void changeMicInput(int deviceIndex)
    {
        deviceName = Microphone.devices[deviceIndex];
        initMic();
    }
}
