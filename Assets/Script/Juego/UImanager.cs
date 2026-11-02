using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static UImanager instance;
    public GameObject Teclas;
    public AudioSource tecladoEfecto;
    public Sprite letraEquivocada;
    public GameObject[] vidas;
    public CambiarEscena cambioEscena;
    public GameObject[] contenedoresConTexto;
    public int idiomaSeleccionado = 0;
    public configuracion config;
    public GameObject canvas;
    public TMP_Text textoPalabra;
    public TMP_Text numPalabra;
    public TMP_Text numMaximoPalabraTexto;
    public TMP_Text numMaximoPalabra;
    public Animator animCanvas;
    public AudioClip[] clips;
    public AudioSource uiEfectos;
    public GameObject botonReiniciar;
    bool cargando;
    static List<CsvParsed> textosUI;
    public Animator transicionAnim;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        } 

        foreach( Transform child in Teclas.transform)
        {
            child.GetComponent<Button>().onClick.AddListener(PlaySound);
        }

        RellenarTextos();
        ActualizarTextos();
        ReproducirImagenOUr();
    }

    void ReproducirImagenOUr()
    {
        if (transicionAnim == null)
            return;

        transicionAnim.Play("imagenOUr", 0, 0f);
        transicionAnim.Update(0f);
    }

    void PlaySound()
    {
        tecladoEfecto.Play();
    }

    public void CambiarVidaUI(int index)
    {
        if (vidas == null || index < 0 || index >= vidas.Length || vidas[index] == null)
            return;

        Animator anim = vidas[index].GetComponent<Animator>();
        if (anim != null)
            anim.Play("cambioVida");
    }

    public void ReiniciarVidaUI(int erroresActuales)
    {
        if (vidas == null)
            return;

        for (int i = 0; i < vidas.Length; i++)
        {
            if (vidas[i] == null)
                continue;

            Animator anim = vidas[i].GetComponent<Animator>();

            if (anim == null)
                continue;

            if (i >= erroresActuales)
                anim.Play("idle");
        }
    }

    public static List<CsvParsed> TextosUI()
    {
        if (textosUI == null || textosUI.Count == 0)
            textosUI = TextParser.obtenerTexto("UI");

        return textosUI;
    }

    public static GameObject[] BuscarTextos(GameObject canvasRaiz)
    {
        if (canvasRaiz == null)
            return new GameObject[0];

        TMP_Text[] textos = canvasRaiz.GetComponentsInChildren<TMP_Text>(true);
        List<GameObject> lista = new List<GameObject>(textos.Length);

        foreach (TMP_Text texto in textos)
            lista.Add(texto.gameObject);

        return lista.ToArray();
    }

    public string ObtenerTexto(string id)
    {
        return BuscarTexto(id, idiomaActual());
    }

    public static string BuscarTexto(string id, int idioma)
    {
        List<CsvParsed> csv = TextosUI();

        if (csv == null || csv.Count == 0)
            return id;

        string texto = HerramientasGenerales.ObtenerTextoPorId(id, csv, 0, idioma);

        return string.IsNullOrEmpty(texto) ? id : texto;
    }

    int idiomaActual()
    {
        idiomaSeleccionado = PlayerPrefs.GetInt("IdiomaPreference", 0);
        return idiomaSeleccionado;
    }

    public void RellenarTextos()
    {
        contenedoresConTexto = BuscarTextos(canvas);
    }

    public void ActualizarTextos()
    {
        idiomaSeleccionado = idiomaActual();
        HerramientasGenerales.ObtenerTextoPorId(contenedoresConTexto, TextosUI(), 0, idiomaSeleccionado);
    }

    public void SetPalabraPantallaTerminadoUI(string text)
    {
        if (textoPalabra == null)
            return;

        textoPalabra.text = text;
    }

    public void SetNumPalabraPantallaTerminadoUI(string text)
    {
        if (numPalabra == null)
            return;

        numPalabra.text = text;
    }

    public void ReproducirAnimacion()
    {
        if (animCanvas == null)
            return;

        animCanvas.Play("entrarPantallaFin");
    }

    public void ReproducirAnimacionReinicio()
    {
        if (animCanvas == null)
            return;

        animCanvas.Play("salidaPantallaFin");
    }

    public void ReproducirEfecto(int index)
    {
        uiEfectos.clip = clips[index];
        uiEfectos.Play();
    }

    public void Iniciar()
    {
        if (cargando)
            return;

        cargando = true;
        cambioEscena.CargarEscenaSuave("Inicio",transicionAnim,"AnimationBlur");
    }

    public void OcultarBoton()
    {
        botonReiniciar.SetActive(false);
    }

    public void PonerPalabrasMaximas(string text)
    {
        if (numMaximoPalabraTexto != null)
            numMaximoPalabraTexto.gameObject.SetActive(true);

        if (numMaximoPalabra != null)
        {
            numMaximoPalabra.gameObject.SetActive(true);
            numMaximoPalabra.text = text;
        }
    }

    public void OcultarPalabrasMaximas()
    {
        if (numMaximoPalabraTexto != null)
            numMaximoPalabraTexto.gameObject.SetActive(false);

        if (numMaximoPalabra != null)
            numMaximoPalabra.gameObject.SetActive(false);
    }
}
