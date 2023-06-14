using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRCAudioLink;
using extOSC;

public class OSCReadback : MonoBehaviour
{
    public AudioLink audioLink;
    public AudioLinkController audioLinkController;
    public OSCReceiver OSCReceiver;
    public OSCTransmitter OSCTransmitter;

    // Start is called before the first frame update
    void Start()
    {
        OSCReceiver.Bind("/avatar/parameters/QAL/CONTROLLER/*", receiveMessage);
        OSCReceiver.Bind("/avatar/change", avatarChange);
    }

    bool avatarInitialized = false;

    void avatarChange(OSCMessage message)
    {
        avatarInitialized = false;
        Debug.Log("Avatar is not initialized");
    }

    /// <summary> This is the current setting that the value slider will change </summary>
    int setting = 0;
    const int numSettings = 6;
    bool MovingSettingSlider = true;

    void receiveMessage(OSCMessage message)
    {
        //Debug.Log(message.Address);
        ///All float values will come in as 0 to 1, with 0 being the max value and 1 being the min value due to the way inputs on the avatar work
        //we use 1 slider to select a setting, and then another slider to select a value for that setting
        switch (message.Address)
        {
            //this ensures that the avatar is initialized before we start using anything from it
            case "/avatar/parameters/QAL/CONTROLLER/INITIALIZED":
                if (message.Values[0].BoolValue)
                {
                    avatarInitialized = true;
                    Debug.Log("Avatar is initialized");
                }
                break;
            case "/avatar/parameters/QAL/CONTROLLER/SETTING_IsGrabbed":
                MovingSettingSlider = message.Values[0].BoolValue;
                break;
            case "/avatar/parameters/QAL/CONTROLLER/SETTING_Squish":
                setting = (int)remap(message.Values[0].FloatValue, 1, 0, 0, numSettings);
                //writeback the current seting
                //we do this since its easier to round here correctly than in the avatar
                var messageSend = new OSCMessage("/avatar/parameters/QAL/CONTROLLER/SETTING");
                messageSend.AddValue(OSCValue.Int(setting));
                OSCTransmitter.Send(messageSend);
                //initialize the avatars slider to the current value
                messageSend = new OSCMessage("/avatar/parameters/QAL/CONTROLLER/VALUE_Current");
                messageSend.AddValue(OSCValue.Float(getSettingValue(setting)));
                OSCTransmitter.Send(messageSend);
                //tell the avatar to update the slider
                messageSend = new OSCMessage("/avatar/parameters/QAL/CONTROLLER/VALUE_Control");
                messageSend.AddValue(OSCValue.Bool(true));
                OSCTransmitter.Send(messageSend);
                break;
            case "/avatar/parameters/QAL/CONTROLLER/VALUE_Squish":
                //release control
                messageSend = new OSCMessage("/avatar/parameters/QAL/CONTROLLER/VALUE_Control");
                messageSend.AddValue(OSCValue.Bool(false));
                OSCTransmitter.Send(messageSend);
                if (!MovingSettingSlider && avatarInitialized)
                {
                    switch (setting)
                    {
                        case 0:
                            audioLink.gain = remap(message.Values[0].FloatValue, 1, 0, 0, 2);
                            break;
                        case 1:
                            audioLink.bass = remap(message.Values[0].FloatValue, 1, 0, 0, 2);
                            break;
                        case 2:
                            audioLink.treble = remap(message.Values[0].FloatValue, 1, 0, 0, 2);
                            break;
                        case 3:
                            audioLink.threshold0 = remap(message.Values[0].FloatValue, 1, 0, 0, 1);
                            break;
                        case 4:
                            audioLink.threshold1 = remap(message.Values[0].FloatValue, 1, 0, 0, 1);
                            break;
                        case 5:
                            audioLink.threshold2 = remap(message.Values[0].FloatValue, 1, 0, 0, 1);
                            break;
                        case 6:
                            audioLink.threshold3 = remap(message.Values[0].FloatValue, 1, 0, 0, 1);
                            break;
                    }
                }
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

    float getSettingValue(int setting)
    {
        switch (setting)
        {
            case 0:
                return remap(audioLink.gain, 0, 2, 0, 1);
            case 1:
                return remap(audioLink.bass, 0, 2, 0, 1);
            case 2:
                return remap(audioLink.treble, 0, 2, 0, 1);
            case 3:
                return audioLink.threshold0;
            case 4:
                return audioLink.threshold1;
            case 5:
                return audioLink.threshold2;
            case 6:
                return audioLink.threshold3;
        }
        return 0;
    }

    float remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
