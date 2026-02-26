using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{


    [SerializeField] private AudioMixer audiomixer;
    public static AudioManager Instance;
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgSource;
    [SerializeField] private AudioSource interactionSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] clips;

    public enum SFX { Flip, Match, Mismatch,GameStart,GameWin, GameOver }


   

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlayBG()
    {
        if (bgSource != null && !bgSource.isPlaying)
            bgSource.Play();
    }

    public void StopBG()
    {
        if (bgSource != null)
            bgSource.Stop();
    }


    public void PlayInteraction(AudioClip clip)
    {
        if (interactionSource == null || clip == null) return;

        interactionSource.Stop();
        interactionSource.PlayOneShot(clip);
        interactionSource.Play();
    }

    public void PlayInteraction(int index)
    {
        if (interactionSource == null) return;
        if (index < 0 || index >= clips.Length) return;

        interactionSource.Stop();
        interactionSource.PlayOneShot(clips[index]);
        interactionSource.Play();
    }


    public void Play(SFX sfx)
    {
        if (sfxSource == null) return;

        int index = (int)sfx;

        if (index < 0 || index >= clips.Length) return;

        sfxSource.PlayOneShot(clips[index]);
    }
}

