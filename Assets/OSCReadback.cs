using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRCAudioLink;
using extOSC;

public class OSCReadback : MonoBehaviour
{
    public string serverHost = "127.0.0.1";
    public int serverPort = 9001;
    public AudioLink audioLink;
    public AudioLinkController audioLinkController;
    OSCReceiver OSCReceiver;

    // Start is called before the first frame update
    void Start()
    {
        OSCReceiver = gameObject.AddComponent<OSCReceiver>();
        OSCReceiver.LocalHost = serverHost;
        OSCReceiver.LocalPort = serverPort;
        OSCReceiver.Bind("/avatar/parameters/QAL/*", receiveMessage);
    }

    void receiveMessage(OSCMessage message)
    {
        //Debug.Log(message.Address);
        ///All float values will come in as 0 to 1, with 0 being the max value and 1 being the min value due to the way inputs on the avatar work
        switch (message.Address)
        {
            case "/avatar/parameters/QAL/CONTROLLER/GAIN_Squish":
                audioLink.gain = 2 - (message.Values[0].FloatValue * 2);
                Debug.Log(2 - (message.Values[0].FloatValue * 2));
                Debug.Log(audioLink.gain);
                break;
            case "/avatar/parameters/QAL/CONTROLLER/TREBLE":
                audioLink.treble = message.Values[0].FloatValue;
                break;
            case "/avatar/parameters/QAL/CONTROLLER/BASS":
                audioLink.bass = message.Values[0].FloatValue;
                break;
            case "/avatar/parameters/QAL/CONTROLLER/THRESHOLD0":
                audioLink.threshold0 = message.Values[0].FloatValue;
                break;
            case "/avatar/parameters/QAL/CONTROLLER/THRESHOLD1":
                audioLink.threshold1 = message.Values[0].FloatValue;
                break;
            case "/avatar/parameters/QAL/CONTROLLER/THRESHOLD2":
                audioLink.threshold2 = message.Values[0].FloatValue;
                break;
            case "/avatar/parameters/QAL/CONTROLLER/THRESHOLD3":
                audioLink.threshold3 = message.Values[0].FloatValue;
                break;
            case "/avatar/parameters/QAL/CONTROLLER/RESET":
                if (message.Values[0].BoolValue)
                    audioLinkController.ResetSettings();
                break;
        }
        //update audioLink
        audioLink.UpdateSettings();
        audioLinkController.GetSettings();
    }
}
