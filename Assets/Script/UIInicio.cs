using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIInicio : MonoBehaviour
{
    public static UIInicio instance;
    public Image image;
    public Animator UIAnimator;
    public MusicaManager manager;
    public CambiarEscena cambioEscena;
    public GameObject[] contenedoresConTexto;
    public int idiomaSeleccionado = 0;
    public configuracion config;
    public GameObject canvas;
    public TMP_Text textoRecord;

    bool cargando;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        config.cargarConf();
        RellenarTextos();
        ActualizarTextos();
        ActualizarRecord();

        if (UIAnimator != null)
        {
            UIAnimator.Play("imagenOUr", 0, 0f);
            UIAnimator.Update(0f);
        }
    }

    public void ActualizarRecord()
    {
        if (textoRecord == null)
            return;

        textoRecord.text = PlayerPrefs.GetInt("PalabrasMaximas", 0).ToString();
    }

    public void RellenarTextos()
    {
        contenedoresConTexto = UImanager.BuscarTextos(canvas);
    }

    public void Iniciar()
    {
        if (cargando)
            return;

        cargando = true;
        cambioEscena.CargarEscenaSuave("Ahorcado",UIAnimator,"AnimationBlur");
    }

    public void ActualizarTextos()
    {
        idiomaSeleccionado = PlayerPrefs.GetInt("IdiomaPreference", 0);
        HerramientasGenerales.ObtenerTextoPorId(contenedoresConTexto, UImanager.TextosUI(), 0, idiomaSeleccionado);
    }

    public void SeleccionarDificultad(int dificultad)
    {
        PlayerPrefs.SetInt("Dificultad", dificultad);
        PlayerPrefs.Save();
    }

    public void Salir()
    {
        Application.Quit();
    }
}
