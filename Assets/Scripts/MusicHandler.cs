using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviourPun
{
    [System.Serializable]
    public struct Music
    {
        public string musicName;
        public AudioClip musicClip;
        [Range(0f,1f)]public float musicVolume;
    }

    [SerializeField] private Music[] musics;
    [SerializeField] private int defaultMusicIndex;

    private Music currentMusic;

    private AudioSource aSource;
    public static MusicHandler instance;

    private Queue<Music> musicQueue = new Queue<Music>();

    private void Awake()
    {
        instance = this;
        aSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        PlayMusic(musics[defaultMusicIndex]);
    }

    private void Update()
    {
        if(musicQueue.Count > 0)
        {
            if(aSource.time >= aSource.clip.length - .1f)
            {
                CallPlayMusic(musicQueue.Dequeue().musicName);
            }
        }
    }

    public void CallPlayMusic(string musicName)
    {
        photonView.RPC(nameof(RPC_PlayMusic), RpcTarget.All, musicName);
    }

    public void PlayMusic(string musicName)
    {
        Music music = FindMusicByName(musicName);

        PlayMusic(music);
    }

    public void PlayMusic(Music music)
    {
        if (music.musicName == currentMusic.musicName) return;

        aSource.Stop();
        aSource.clip = music.musicClip;
        aSource.volume = music.musicVolume;
        aSource.Play();

        currentMusic = music;
    }

    public void Enqueue(string musicName)
    {
        Music music = FindMusicByName(musicName);

        Enqueue(music);
    }

    public void Enqueue(Music music)
    {
        musicQueue.Enqueue(music);
    }

    private Music FindMusicByName(string musicName)
    {
        for (int i = 0; i < musics.Length; i++)
        {
            if (musics[i].musicName.Replace(" ", "").ToLower() == musicName.Replace(" ", "").ToLower())
            {
                return musics[i];
            }
        }

        return musics[defaultMusicIndex];
    }


    [PunRPC]
    public void RPC_PlayMusic(string musicName)
    {
        PlayMusic(musicName);
    }
}
