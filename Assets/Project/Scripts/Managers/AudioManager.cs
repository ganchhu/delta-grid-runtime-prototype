using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

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
    [Header("Volume Controls (0 - 1)")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgVolume = 0.1f;
    [Range(0f, 1f)] public float interactionVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;
    [Header("Slider References")]
    [SerializeField] private UnityEngine.UI.Slider masterSlider;
    [SerializeField] private UnityEngine.UI.Slider bgSlider;
    [SerializeField] private UnityEngine.UI.Slider interactionSlider;
    [SerializeField] private UnityEngine.UI.Slider SFXSlider;


   
     string masterVolumeParam = "Master_Volume";
     string bgVolumeParam = "Background_Volume";
     string interactionVolumeParam = "Interaction_Volume";
     string SFXVolumeParam = "SFX_Volume";
   
    private float MV_v, BG_v, IN_v, SFX_v;
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
            return;
        }

        LoadVolumeSettings();
    }
    private void Start()
    {
        ApplyAllVolumes();
        InitializeSliders();
    }
    private void InitializeSliders()
    {
        if (masterSlider != null)
        {
            masterSlider.value = masterVolume;
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (bgSlider != null)
        {
            bgSlider.value = bgVolume;
            bgSlider.onValueChanged.AddListener(SetBackgroundVolume);
        }

        if (interactionSlider != null)
        {
            interactionSlider.value = interactionVolume;
            interactionSlider.onValueChanged.AddListener(SetInteractionVolume);
        }

        if (SFXSlider != null)
        {
            SFXSlider.value = SFXVolume;
            SFXSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    private void LoadVolumeSettings()
    {
        //masterVolume = PlayerPrefs.GetFloat("MasterVolume", masterVolume);
        //bgVolume = PlayerPrefs.GetFloat("BGVolume", bgVolume);
        //interactionVolume = PlayerPrefs.GetFloat("InteractionVolume", interactionVolume);
        //SFXVolume = PlayerPrefs.GetFloat("SFXVolume", SFXVolume);
    }
    public void SetMasterVolume(float value)
    {
        masterVolume = value;
      //  PlayerPrefs.SetFloat("MasterVolume", value);
        SetVolume(masterVolumeParam, value);
    }
    private void OnValidate()
    {
        Reset();
        if (audiomixer != null)
        {
            ApplyAllVolumes();
        }
    }
   
    public void SetBackgroundVolume(float value)
    {
        bgVolume = value;
        SetVolume(bgVolumeParam, bgVolume);

    }

    public void SetInteractionVolume(float value)
    {
        interactionVolume = value;
        SetVolume(interactionVolumeParam, interactionVolume);

    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        SetVolume(SFXVolumeParam, SFXVolume);

    }

    private void ApplyAllVolumes()
    {
        SetVolume(masterVolumeParam, masterVolume);
        SetVolume(bgVolumeParam, bgVolume);
        SetVolume(interactionVolumeParam, interactionVolume);
        SetVolume(SFXVolumeParam, SFXVolume);

    }

    private void SetVolume(string parameterName, float normalizedValue)
    {
        if (audiomixer == null) return;

        float volumeInDb = Mathf.Log10(Mathf.Max(normalizedValue, 0.0001f)) * 20f;
        audiomixer.SetFloat(parameterName, volumeInDb);
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
    }

    public void PlayInteraction(int index)
    {
        if (interactionSource == null) return;
        if (index < 0 || index >= clips.Length) return;

        interactionSource.Stop();
        interactionSource.PlayOneShot(clips[index]);
    }


    public void Reset()
    {

        SetVolume(masterVolumeParam, masterVolume);
        SetVolume(bgVolumeParam, bgVolume);
        SetVolume(interactionVolumeParam, interactionVolume);
        SetVolume(SFXVolumeParam, SFXVolume);
    }

    public void Play(SFX sfx)
    {
        if (sfxSource == null) return;

        int index = (int)sfx;

        if (index < 0 || index >= clips.Length) return;

        sfxSource.PlayOneShot(clips[index]);
    }
}

