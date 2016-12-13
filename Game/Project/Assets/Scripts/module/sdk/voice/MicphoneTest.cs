using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MicphoneTest : MonoBehaviour
{
    AudioSource _audio;
    int deviceCount;
    string sFrequency = "10000";
    string sLog = "";

    void Start()
    {
        _audio = gameObject.AddComponent<AudioSource>();
        string[] ms = Microphone.devices;
        deviceCount = ms.Length;
        if (deviceCount == 0)
        {
            Log("no microphone found");
        }
        Debug.Log("Start");
    }

    void Log(string log)
    {
        sLog += log;
        sLog += "\r\n";
    }
    void OnGUI()
    {
        if (deviceCount > 0)
        {
            if (!Microphone.IsRecording(null) && GUILayout.Button("Start", GUILayout.Height(Screen.height / 20), GUILayout.Width(Screen.width / 5)))
            {
                StartRecord();
            }
            if (Microphone.IsRecording(null) && GUILayout.Button("Stop", GUILayout.Height(Screen.height / 20), GUILayout.Width(Screen.width / 5)))
            {
                StopRecord();
            }
            if (!Microphone.IsRecording(null) && GUILayout.Button("Play", GUILayout.Height(Screen.height / 20), GUILayout.Width(Screen.width / 5)))
            {
                PlayRecord();
            }
            if (!Microphone.IsRecording(null) && GUILayout.Button("Print", GUILayout.Height(Screen.height / 20), GUILayout.Width(Screen.width / 5)))
            {
                PrintRecord();
            }
            sFrequency = GUILayout.TextField(sFrequency, GUILayout.Width(Screen.width / 5), GUILayout.Height(Screen.height / 20));
        }
        GUILayout.Label(sLog);
    }
    void StartRecord()
    {
        _audio.Stop();
        _audio.loop = false;
       _audio.mute = true;
       _audio.clip = Microphone.Start(null, false, 10, int.Parse(sFrequency));
      //  Microphone.
       /* while (!(Microphone.GetPosition(null) > 0))
        {
        }*/
        //_audio.Play();
        Log("StartRecord");
    }
    void StopRecord()
    {
        if (!Microphone.IsRecording(null))
        {
            return;
        }
        Microphone.End(null); 
        _audio.Stop();
    }
    void PrintRecord()
    {
        if (Microphone.IsRecording(null))
        {
            return;
        }
        byte[] data = GetClipData();
        string slog = "total length:" + data.Length + " time:" + _audio.time;
        Log(data.ToString());
        Log(slog);
    }
    void PlayRecord()
    {
        if (Microphone.IsRecording(null))
        {
            return;
        }
        if (_audio.clip == null)
        {
            return;
        }
        _audio.mute = false;
        _audio.loop = false;
        _audio.Play();
    }
    public byte[] GetClipData()
    {
        if (_audio.clip == null)
        {
            Debug.Log("GetClipData audio.clip is null");
            return null;
        }

        float[] samples = new float[_audio.clip.samples];
        _audio.clip.GetData(samples, 0);


        byte[] outData = new byte[samples.Length * 2];

        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            short temshort = (short)(samples[i] * rescaleFactor);

            byte[] temdata = System.BitConverter.GetBytes(temshort);

            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];


        }
        if (outData == null || outData.Length <= 0)
        {
            Debug.Log("GetClipData intData is null");
            return null;
        }        
        return outData;
    }

   /* public static byte[] GetData(this AudioClip clip)
    {
        var data = new float[clip.samples * clip.channels];

        clip.GetData(data, 0);

        byte[] bytes = new byte[data.Length * 4];
        Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

        return SVZip.ConvertBytesZlib(bytes, Ionic.Zlib.CompressionMode.Compress);
    }

    public static void SetData(this AudioClip clip, byte[] bytes)
    {
        bytes = SVZip.ConvertBytesZlib(bytes, Ionic.Zlib.CompressionMode.Decompress);

        float[] data = new float[bytes.Length / 4];
        Buffer.BlockCopy(bytes, 0, data, 0, data.Length);

        clip.SetData(data, 0);
    }

    public static byte[] ConvertBytesZlib(byte[] data, CompressionMode compressionMode)
    {
        CompressionMode mode = compressionMode;
        if (mode != CompressionMode.Compress)
        {
            if (mode != CompressionMode.Decompress)
            {
                throw new NotImplementedException();
            }
            return ZlibStream.UncompressBuffer(data);
        }
        return ZlibStream.CompressBuffer(data);
    }*/

}
