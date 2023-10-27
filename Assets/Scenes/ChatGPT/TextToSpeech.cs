using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

// using SpeechLib;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    private async void Start()
    {
        var credentials = new BasicAWSCredentials("AKIASH5LD2NA3KVBCU4I", "//Tdw/AFQ4I0+Z4q3fHuMXizidhtPxImqEbIqFOp");
        var client = new AmazonPollyClient(credentials, RegionEndpoint.USEast1);

        var request = new SynthesizeSpeechRequest()
        {
            Text = "Testing amazon polly",
            Engine = Engine.Neural,
            VoiceId = VoiceId.Aria,
            OutputFormat = OutputFormat.Mp3
        };
        
        var response = await client.SynthesizeSpeechAsync(request);
        
        WriteIntoFile(response.AudioStream);

        using (var www = UnityWebRequestMultimedia.GetAudioClip($"{Application.persistentDataPath}/audio.mp3", AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone) await Task.Yield();
            
            var clip = DownloadHandlerAudioClip.GetContent(www);
            
            audioSource.clip = clip;
            audioSource.Play();
        }
        
    }
    
    public async void SpeakText(string textToSpeak)
    {
        
        var credentials = new BasicAWSCredentials("AKIASH5LD2NA3KVBCU4I", "//Tdw/AFQ4I0+Z4q3fHuMXizidhtPxImqEbIqFOp");
        var client = new AmazonPollyClient(credentials, RegionEndpoint.USEast1);
        
        var request = new SynthesizeSpeechRequest()
        {
            Text = textToSpeak,  // This line replaces the hardcoded text
            Engine = Engine.Neural,
            VoiceId = VoiceId.Aria,
            OutputFormat = OutputFormat.Mp3
        };

        var response = await client.SynthesizeSpeechAsync(request);
        WriteIntoFile(response.AudioStream);

        using (var www = UnityWebRequestMultimedia.GetAudioClip($"{Application.persistentDataPath}/audio.mp3", AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone) await Task.Yield();

            var clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void WriteIntoFile(Stream stream)
    {
        using (var fileStream = new FileStream($"{Application.persistentDataPath}/audio.mp3", FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;
            
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
            stream.CopyTo(fileStream);
        }
    }
    
    

}
