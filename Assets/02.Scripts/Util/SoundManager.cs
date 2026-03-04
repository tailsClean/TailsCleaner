using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum ClipName
{
    Stage_BGM
}

[System.Serializable]
public struct ClipInfo
{
    public ClipName name;
    public AudioClip source;
}


public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance{get => instance; private set => instance = value;}

    [SerializeField] private AudioSource bgmPlayer;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }
    private void Start()
    {
        bgmPlayer.Play();
    }
    public void SetVolume (float volume)
    {
        bgmPlayer.volume = volume;
    }
}
