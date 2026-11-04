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
    public Toggle fullscreenToggle;
    [SerializeField] float currentVolume;
    [SerializeField] float efectosVolume;
    [SerializeField] float musicasVolume;
    [SerializeField] int Idioma;
    Resolution[] resolutions;
    List<string> resoluciones  = new List<string>();

    const string ClaveResolucion = "ResolutionPreference";
    const string ClaveFullscreen = "FullscreenPreference";

    // Start is called before the first frame update
    public void cargarConf()
    { 
        resolutions = Screen.resolutions;
        resoluciones.Clear();

        for (int i = 0; i < resolutions.Length; i++)
            resoluciones.Add(resolutions[i].width + " x " + resolutions[i].height);

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resoluciones);

        LoadSettings(IndiceResolucionActual());
        AplicarModoVentana();
    }

    int IndiceResolucionActual()
    {
        int indice = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width
                && resolutions[i].height == Screen.currentResolution.height)
                indice = i;
        }

        return indice;
    }

    // La resolucion nativa del monitor. En pantalla completa hay que usarla siempre:
    // si se renderiza a una resolucion menor, el sistema escala la imagen y la UI
    // se pixelea.
    static Resolution ResolucionNativa()
    {
        int ancho = 0;
        int alto = 0;

        Display principal = Display.main;

        if (principal != null)
        {
            ancho = principal.systemWidth;
            alto = principal.systemHeight;
        }

        if (ancho <= 0 || alto <= 0)
        {
            Resolution actual = Screen.currentResolution;
            ancho = actual.width;
            alto = actual.height;
        }

        return new Resolution { width = ancho, height = alto };
    }

    Resolution ResolucionSeleccionada()
    {
        int indice = resolutionDropdown != null ? resolutionDropdown.value : 0;

        if (resolutions != null && indice >= 0 && indice < resolutions.Length)
            return resolutions[indice];

        return Screen.currentResolution;
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
        if (fullscreenToggle != null && fullscreenToggle.isOn != isFullscreen)
            fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);

        // En pantalla completa la resolucion la impone el monitor, asi que el
        // desplegable no tiene sentido y se desactiva.
        if (resolutionDropdown != null)
            resolutionDropdown.interactable = !isFullscreen;

        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

            Resolution nativa = ResolucionNativa();
            AplicarResolucion(nativa.width, nativa.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;

            Resolution r = ResolucionSeleccionada();
            AplicarResolucion(r.width, r.height, FullScreenMode.Windowed);
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex < 0 || resolutionIndex >= resolutions.Length)
            return;

        if (resolutionDropdown != null && resolutionDropdown.value != resolutionIndex)
            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);

        if (Screen.fullScreen)
        {
            // En pantalla completa la resolucion la impone el monitor. Aplicar una
            // menor haria que el sistema escalase la imagen (UI pixelada), asi que
            // se ignora y se deja el desplegable para el modo ventana.
            Resolution nativa = ResolucionNativa();
            AplicarResolucion(nativa.width, nativa.height, FullScreenMode.FullScreenWindow);
            return;
        }

        Resolution r = resolutions[resolutionIndex];
        AplicarResolucion(r.width, r.height, FullScreenMode.Windowed);
    }

    static void AplicarResolucion(int ancho, int alto, FullScreenMode modo)
    {
        // En el editor Screen.SetResolution solo cambia el tamano del Game view,
        // asi que no se fuerza la resolucion del monitor.
        if (Application.isEditor)
            return;

        Screen.SetResolution(ancho, alto, modo);
    }

    void AplicarModoVentana()
    {
        bool pantallaCompleta = PlayerPrefs.HasKey(ClaveFullscreen)
            ? Convert.ToBoolean(PlayerPrefs.GetInt(ClaveFullscreen))
            : true;

        SetFullscreen(pantallaCompleta);
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
        // Se guarda como "ancho x alto" y no como indice: el indice depende de las
        // resoluciones que liste el SO en cada maquina.
        Resolution r = ResolucionSeleccionada();
        PlayerPrefs.SetString(ClaveResolucion, r.width + "x" + r.height);
        PlayerPrefs.SetInt("TextureQualityPreference", 
                textureDropdown.value);
        PlayerPrefs.SetInt("AntiAliasingPreference", 
                aaDropdown.value);
        PlayerPrefs.SetInt(ClaveFullscreen, 
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

    int IndiceResolucionGuardada()
    {
        string guardada = PlayerPrefs.GetString(ClaveResolucion, "");

        if (!string.IsNullOrEmpty(guardada))
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if ((resolutions[i].width + "x" + resolutions[i].height) == guardada)
                    return i;
            }

            return -1;
        }

        // Formato antiguo: indice dentro de Screen.resolutions.
        if (PlayerPrefs.HasKey(ClaveResolucion))
        {
            int indice = PlayerPrefs.GetInt(ClaveResolucion);

            if (indice >= 0 && indice < resolutions.Length)
                return indice;
        }

        return -1;
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        if (PlayerPrefs.HasKey("QualitySettingPreference"))
            qualityDropdown.value = 
                        PlayerPrefs.GetInt("QualitySettingPreference");
        else
            qualityDropdown.value = 3;
        int indiceResolucion = IndiceResolucionGuardada();

        if (indiceResolucion < 0)
            indiceResolucion = currentResolutionIndex;

        // Sin notify: el modo de ventana se aplica despues, en AplicarModoVentana.
        resolutionDropdown.SetValueWithoutNotify(indiceResolucion);
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
