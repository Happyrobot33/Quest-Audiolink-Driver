using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRCAudioLink;
using extOSC;

public class DataInjector : MonoBehaviour
{
    public DataExtractor dataExtractor;
    public AudioLink audioLink;
    public OSCTransmitter oscTransmitter;

    public bool inject = false;

    [Range(0, 15)]
    public int smoothing = 0;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        float band1 = 0;
        float band2 = 0;
        float band3 = 0;
        float band4 = 0;
        if (inject)
        {
            band1 = getBand(0, smoothing);
            band2 = getBand(1, smoothing);
            band3 = getBand(2, smoothing);
            band4 = getBand(3, smoothing);
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

    //smoothed bands start at index 3584
    //3584 is the smoothest and 3599 is the most recent
    //each band is offset by the 128
    float getBand(int band, int smoothing = 0)
    {
        //invert smoothing so it makes sense
        smoothing = 15 - smoothing;
        //cap smoothing
        if (smoothing < 0)
            smoothing = 0;
        if (smoothing > 15)
            smoothing = 15;
        int dataIndex = 3584 + (band * 128) + smoothing;
        float bandValue = audioLink.audioData[dataIndex].grayscale;
        return bandValue;
    }

    //public so it can be called from the UI
    public void Inject(bool value)
    {
        inject = value;
    }

    public void setSmoothing(float value)
    {
        smoothing = (int)value;
    }
}
