using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class configuracion : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown textureDropdown;
    public TMP_Dropdown aaDropdown;
    public TMP_Dropdown LenguajeDropdown;
    public Slider volumeSlider;
    public Slider musicaSlider;
    public Slider efectosSlider;
    [SerializeField] float currentVolume;
    [SerializeField] float efectosVolume;
    [SerializeField] float musicasVolume;
    [SerializeField] int Idioma;
    Resolution[] resolutions;
    List<string> resoluciones  = new List<string>();
    // Start is called before the first frame update
    public void cargarConf()
    { 
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            resoluciones.Add(resolutions[i].width + " x " + resolutions[i].height);

            if (resolutions[i].width == Screen.currentResolution.width 
                && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(resoluciones);
        LoadSettings(currentResolutionIndex);
    }
    
    public void SetVolumenMaster(float volume)
    {
        currentVolume = VolumenADecibels(volume);
        audioMixer.SetFloat("Volume", currentVolume);
    }

    public void SetMusicaMaster(float volume)
    {
        musicasVolume = VolumenADecibels(volume);
        audioMixer.SetFloat("MusicaVol", musicasVolume);
    }

    public void SetEfectosMaster(float volume)
    {
        efectosVolume = VolumenADecibels(volume);
        audioMixer.SetFloat("EfectosVol", efectosVolume);
    }

    static float VolumenADecibels(float volume)
    {
        float valor = Mathf.Clamp01(volume);

        if (valor < 0.001f)
            valor = 0.001f;

        return 20f * (float)Math.Log10(valor);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, 
        resolution.height, Screen.fullScreen);
    }

    public void SetIdioma(int idiomaIndex)
    {
        Idioma = idiomaIndex;
    }

    public void SetQuality(int qualityIndex)
    {
        if (qualityIndex != 6) // if the user is not using 
                                //any of the presets
            QualitySettings.SetQualityLevel(qualityIndex);
        switch (qualityIndex)
        {
            case 0: // quality level - very low
                textureDropdown.value = 3;
                aaDropdown.value = 0;
                break;
            case 1: // quality level - low
                textureDropdown.value = 2;
                aaDropdown.value = 0;
                break;
            case 2: // quality level - medium
                textureDropdown.value = 1;
                aaDropdown.value = 1;
                break;
            case 3: // quality level - high
                textureDropdown.value = 0;
                aaDropdown.value = 1;
                break;
            case 4: // quality level - very high
                textureDropdown.value = 0;
                aaDropdown.value = 2;
                break;
            case 5: // quality level - ultra
                textureDropdown.value = 0;
                aaDropdown.value = 3;
                break;
        }
        qualityDropdown.value = qualityIndex;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("QualitySettingPreference", 
                qualityDropdown.value);
        PlayerPrefs.SetInt("ResolutionPreference", 
                resolutionDropdown.value);
        PlayerPrefs.SetInt("TextureQualityPreference", 
                textureDropdown.value);
        PlayerPrefs.SetInt("AntiAliasingPreference", 
                aaDropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", 
                Convert.ToInt32(Screen.fullScreen));
        PlayerPrefs.SetFloat("VolumePreference", 
                volumeSlider.value);
        PlayerPrefs.SetFloat("MusicaPreference", 
                musicaSlider.value);
        PlayerPrefs.SetFloat("EfectoPreference", 
                efectosSlider.value);
        PlayerPrefs.SetInt("IdiomaPreference", 
                Idioma);
        PlayerPrefs.Save();                 
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        if (PlayerPrefs.HasKey("QualitySettingPreference"))
            qualityDropdown.value = 
                        PlayerPrefs.GetInt("QualitySettingPreference");
        else
            qualityDropdown.value = 3;
        if (PlayerPrefs.HasKey("ResolutionPreference"))
            resolutionDropdown.value = 
                        PlayerPrefs.GetInt("ResolutionPreference");
        else
            resolutionDropdown.value = currentResolutionIndex;
        if (PlayerPrefs.HasKey("TextureQualityPreference"))
            textureDropdown.value = 
                        PlayerPrefs.GetInt("TextureQualityPreference");
        else
            textureDropdown.value = 0;
        if (PlayerPrefs.HasKey("AntiAliasingPreference"))
            aaDropdown.value = 
                        PlayerPrefs.GetInt("AntiAliasingPreference");
        else
            aaDropdown.value = 1;
        if (PlayerPrefs.HasKey("FullscreenPreference"))
            Screen.fullScreen = 
            Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));
        else
            Screen.fullScreen = true;
        if (PlayerPrefs.HasKey("VolumePreference"))
            volumeSlider.value = 
                        VolumenGuardado("VolumePreference");
        else
            volumeSlider.value = 
                        VolumenGuardado("VolumePreference");
        if (PlayerPrefs.HasKey("MusicaPreference"))
        {  
                        musicaSlider.value = 
                        VolumenGuardado("MusicaPreference");
        }
        else
            musicaSlider.value = 
                        VolumenGuardado("MusicaPreference");
                        
        if (PlayerPrefs.HasKey("EfectoPreference"))
            efectosSlider.value = 
                        VolumenGuardado("EfectoPreference");
        else
            efectosSlider.value = 
                        VolumenGuardado("EfectoPreference");
        if (PlayerPrefs.HasKey("IdiomaPreference"))
            Idioma = PlayerPrefs.GetInt("IdiomaPreference");
        else
            Idioma = 0;
    }

    static float VolumenGuardado(string clave)
    {
        float valor = PlayerPrefs.GetFloat(clave);

        if (valor <= 0f)
            return 0.75f;

        return Mathf.Clamp01(valor);
    }
}
