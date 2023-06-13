using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRCAudioLink;
using extOSC;

public class DataInjector : MonoBehaviour
{
    public string remoteHost = "127.0.0.1";
    public int remotePort = 9000;
    public DataExtractor dataExtractor;
    public AudioLink audioLink;
    OSCTransmitter oscTransmitter;

    public bool inject = false;

    // Start is called before the first frame update
    void Start()
    {
        oscTransmitter = gameObject.AddComponent<OSCTransmitter>();
        oscTransmitter.RemoteHost = remoteHost;
        oscTransmitter.RemotePort = remotePort;
    }

    // Update is called once per frame
    void Update()
    {
        float band1 = 0;
        float band2 = 0;
        float band3 = 0;
        float band4 = 0;
        if (inject)
        {
            band1 = getBand(0);
            band2 = getBand(1);
            band3 = getBand(2);
            band4 = getBand(3);
        }
        else
        {
            band1 = dataExtractor._BAND1;
            band2 = dataExtractor._BAND2;
            band3 = dataExtractor._BAND3;
            band4 = dataExtractor._BAND4;
        }
        sendVRCmessage("/avatar/parameters/QAL/RAW/ALband1", band1);
        sendVRCmessage("/avatar/parameters/QAL/RAW/ALband2", band2);
        sendVRCmessage("/avatar/parameters/QAL/RAW/ALband3", band3);
        sendVRCmessage("/avatar/parameters/QAL/RAW/ALband4", band4);

        //wakeup the remote
        OSCMessage message = new OSCMessage("/avatar/parameters/QAL/SETTINGS/ALMode");
        message.AddValue(OSCValue.Bool(true));
        oscTransmitter.Send(message);
    }

    void sendVRCmessage(string address, float value)
    {
        OSCMessage message = new OSCMessage(address);
        message.AddValue(OSCValue.Float(value));
        oscTransmitter.Send(message);
    }

    float getBand(int band)
    {
        int dataIndex = (band * 128);
        float bandValue = audioLink.audioData[dataIndex].r;
        return bandValue;
    }
}
