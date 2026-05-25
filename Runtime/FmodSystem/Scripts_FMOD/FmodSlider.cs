#if FMOD_PRESENT

using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.UI;

public class FmodSlider : MonoBehaviour
{
    [Header("Mains Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    public BusSlider[] otherSliders;

    [Serializable]
    public class BusSlider
    {
        [Tooltip("This name will be used in PlayerPrefs. Use uppercase letters at the beginning of each word.")]
        public string name;

        public string busPath;
        public Slider slider;

        [HideInInspector] public Bus bus;
    }

    void Start()
    {
        ResetPrefs();

        // Main buses
        masterBus = RuntimeManager.GetBus("bus:/Master");
        musicBus = RuntimeManager.GetBus("bus:/Master/Music");
        sfxBus = RuntimeManager.GetBus("bus:/Master/SFX");

        SetupSlider(masterSlider, masterBus, "Master");
        SetupSlider(musicSlider, musicBus, "Music");
        SetupSlider(sfxSlider, sfxBus, "SFX");

        // Others sliders
        foreach (BusSlider busSlider in otherSliders)
        {
            busSlider.bus = RuntimeManager.GetBus(busSlider.busPath);

            SetupSlider(busSlider.slider, busSlider.bus, busSlider.name);
        }
    }

    void SetupSlider(Slider slider, Bus bus, string saveName)
    {
        SetPrefs(slider, bus, saveName);

        slider.onValueChanged.AddListener((value) =>
        {
            SetVolume(bus, value);

            PlayerPrefs.SetFloat(saveName, value);
            PlayerPrefs.Save();
        });
    }

    void SetVolume(Bus bus, float value)
    {
        bus.setVolume(value);
    }

    void SetPrefs(Slider slider, Bus bus, string saveName)
    {
        float savedVolume = PlayerPrefs.GetFloat(saveName, 0.5f);

        slider.value = savedVolume;

        bus.setVolume(savedVolume);
    }

    void ResetPrefs()
    {
#if UNITY_EDITOR

        PlayerPrefs.DeleteKey("Master");
        PlayerPrefs.DeleteKey("Music");
        PlayerPrefs.DeleteKey("SFX");

        foreach (BusSlider busSlider in otherSliders)
        {
            PlayerPrefs.DeleteKey(busSlider.name);
        }

        PlayerPrefs.Save();

#endif
    }
}

#endif