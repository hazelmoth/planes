using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneEngineSounds : MonoBehaviour
{
    [SerializeField] private PlaneController plane;

    [SerializeField] private AudioClip engineStart;
    [SerializeField] private AudioClip engineSustain;
    [SerializeField] private AudioClip engineOut;
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.1f;
    private AudioSource audio;
    private bool lastEngineState = false;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        if (!audio)
        {
            Debug.LogError("PlaneEngineSounds missing an AudioSource component.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (plane.CurrentThrottle > 0 && !lastEngineState)
        {
            StartEngine();
        }
        if (plane.CurrentThrottle <= 0 && lastEngineState)
        {
            StopEngine();
        }
        lastEngineState = plane.CurrentThrottle > 0;

        audio.pitch = minPitch + plane.CurrentThrottle * (maxPitch - minPitch);
    }

    private void StartEngine ()
    {
        StopAllCoroutines();
        audio.clip = engineStart;
        audio.loop = false;
        audio.Play();
        StartCoroutine(QueueAudioCoroutine(engineSustain, true));
    }

    private void StopEngine ()
    {
        StopAllCoroutines();
        StartCoroutine(QueueAudioCoroutine(engineOut, false));
    }

    private IEnumerator QueueAudioCoroutine (AudioClip nextAudio, bool loop)
    {
        while (audio.isPlaying && !audio.loop)
        {
            yield return null;
        }
        audio.clip = nextAudio;
        audio.loop = loop;
        audio.Play();
    }
}
