#if FMOD_PRESENT
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente legado de slider que controla volumes de buses pelo BetterFMOD.
/// </summary>
public class FmodSlider : MonoBehaviour
{
    [Header("Mains Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    public BusSlider[] otherSliders;

    /// <summary>
    /// Representa um slider vinculado a um path de bus do FMOD.
    /// </summary>
    [Serializable]
    public class BusSlider
    {
        [Tooltip("This name will be used in PlayerPrefs. Use uppercase letters at the beginning of each word.")]
        public string name;

        public string busPath;
        public Slider slider;
    }

    private void Start()
    {
        ResetPrefs();
        TrySetupSlider(masterSlider, "bus:/Master", "Master");
        TrySetupSlider(musicSlider, "bus:/Master/Music", "Music");
        TrySetupSlider(sfxSlider, "bus:/Master/SFX", "SFX");

        foreach (BusSlider busSlider in otherSliders)
            TrySetupSlider(busSlider.slider, busSlider.busPath, busSlider.name);
    }

    private void TrySetupSlider(Slider slider, string busPath, string saveName)
    {
        if (slider == null || string.IsNullOrWhiteSpace(busPath) || string.IsNullOrWhiteSpace(saveName))
            return;

        float savedVolume = PlayerPrefs.GetFloat(saveName, 0.5f);
        slider.value = savedVolume;
        Fmod.SetBusVolume(busPath, savedVolume);

        slider.onValueChanged.AddListener(value =>
        {
            Fmod.SetBusVolume(busPath, value);
            PlayerPrefs.SetFloat(saveName, value);
            PlayerPrefs.Save();
        });
    }

    private void ResetPrefs()
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteKey("Master");
        PlayerPrefs.DeleteKey("Music");
        PlayerPrefs.DeleteKey("SFX");

        foreach (BusSlider busSlider in otherSliders)
            PlayerPrefs.DeleteKey(busSlider.name);

        PlayerPrefs.Save();
#endif
    }
}
#endif
