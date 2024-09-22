using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsController : MonoBehaviour
{

    public static SoundEffectsController instance;

    public AudioSource audioSource;

    public enum SoundType { UI, Battle}
    public List<AudioClip> uiSounds, battleSounds;
    private Dictionary<string, AudioClip> soundMasterList;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();

        //Build dictionary so sounds can be called by name
        soundMasterList = new Dictionary<string, AudioClip>();
        foreach(AudioClip clip in uiSounds)
            soundMasterList.Add(clip.name, clip);
        foreach (AudioClip clip in battleSounds)
            soundMasterList.Add(clip.name, clip);
    }

    public void PlaySound(string _soundString)
    {
        audioSource.PlayOneShot(soundMasterList[_soundString]);
    }

    public void StopSounds()
    {
        audioSource.Stop();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
