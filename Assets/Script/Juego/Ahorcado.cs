using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Ahorcado : MonoBehaviour
{
    public static Ahorcado instance;

    [Header("Referencias")]
    public GameObject teclas;
    public TMP_Text textoPalabra;
    public TMP_Text textoEstado;
    public GameObject[] etapasAhorcado;

    [Header("Fin de partida")]
    public TMP_Text textoFinal;
    public string nombreAnimacionFinal = "entrarPantallaFin";

    [Header("Ajustes")]
    public int maximoErrores = 6;
    public string[] palabras =
    {
        "UNIDAD", "TECLADO", "MONITOR", "PROGRAMACION", "AHORCADO",
        "COMPUTADORA", "VENTANA", "CONTROL", "JUEGO", "MARIPOSA",
        "ADIVINAR", "LETRA", "PANTALLA", "DISCO", "MOUSE"
    };
    public List<CsvParsed> palabrasCsv = new List<CsvParsed>();

    [Header("Dificultad")]
    public int[] erroresPorDificultad = { 6, 6, 6 };
    public int[] longitudMinima = { 4, 7, 9 };
    public int[] longitudMaxima = { 6, 8, 99 };
    public int nivelMaximo = 2;

    [Tooltip("Palabras completadas necesarias para subir un nivel de dificultad")]
    public int palabrasPorNivel = 10;

    [Header("Colores")]
    public Color colorAcierto = new Color(0.25f, 0.8f, 0.35f);
    public Color colorError = new Color(0.9f, 0.25f, 0.25f);

    readonly HashSet<char> acertadas = new HashSet<char>();
    readonly HashSet<char> falladas = new HashSet<char>();
    readonly Dictionary<Button, char> botones = new Dictionary<Button, char>();
    readonly Dictionary<Button, Color> coloresOriginales = new Dictionary<Button, Color>();
    readonly Dictionary<Button, Sprite> spritesOriginales = new Dictionary<Button, Sprite>();
    int idioma;
    int dificultad;

    string palabraSecreta;
    int errores;
    bool terminado;
    int numPalabrasCompletadas = 0;
    int maxPalabrasCompletadas;

    void Awake()
    {
        instance = this;

        if (teclas == null && UImanager.instance != null)
            teclas = UImanager.instance.Teclas;

        idioma = PlayerPrefs.GetInt("IdiomaPreference", 0);
        dificultad = PlayerPrefs.GetInt("Dificultad", 0);
        maxPalabrasCompletadas = PlayerPrefs.GetInt("PalabrasMaximas", 0);
        palabrasCsv = TextParser.obtenerTexto("Palabras");
        CrearTextosSiFaltan();
        VincularBotones();
    }

    void Start()
    {
        Reiniciar();
    }

    void Update()
    {
        if (terminado)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                Reiniciar();
#endif
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            for (int i = (int)Key.A; i <= (int)Key.Z; i++)
            {
                Key tecla = (Key)i;
                if (Keyboard.current[tecla].wasPressedThisFrame)
                    AdivinarLetra((char)('A' + i - (int)Key.A));
            }
        }
#endif
    }

    public void Reiniciar()
    {
        acertadas.Clear();
        falladas.Clear();
        errores = Mathf.Max(0, errores - 3);
        terminado = false;

        if (UImanager.instance != null)
            UImanager.instance.ReiniciarVidaUI(errores);

        if (UImanager.instance != null)
            UImanager.instance.OcultarPalabrasMaximas();

        palabraSecreta = ObtenerPalabra();
        
        maximoErrores = ErroresSegunDificultad();
        RevelarLetra();

        foreach (KeyValuePair<Button, char> par in botones)
        {
            par.Key.interactable = true;
            if (par.Key.image != null)
            {
                par.Key.image.color = coloresOriginales[par.Key];
                par.Key.image.sprite = spritesOriginales[par.Key];
            }
        }

        if (etapasAhorcado != null)
            foreach (GameObject etapa in etapasAhorcado)
                if (etapa != null)
                    etapa.SetActive(false);

        if (textoEstado != null)
        {
            textoEstado.text = "";
            textoEstado.color = Color.white;
        }

        ActualizarTexto();
    }

    public void AdivinarLetra(char letra)
    {
        if (terminado)
            return;

        letra = char.ToUpperInvariant(letra);

        if (acertadas.Contains(letra) || falladas.Contains(letra))
            return;

        bool esta = palabraSecreta.IndexOf(letra) >= 0;

        if (esta)
            acertadas.Add(letra);
        else
        {
            falladas.Add(letra);
            errores++;

            if (etapasAhorcado != null && errores - 1 < etapasAhorcado.Length && etapasAhorcado[errores - 1] != null)
                etapasAhorcado[errores - 1].SetActive(true);

            if (UImanager.instance != null)
                UImanager.instance.CambiarVidaUI(errores - 1);
        }

        BotonDeLetra(letra, esta);
        ActualizarTexto();

        if (PalabraCompleta())
            Ganar();
        else if (errores >= maximoErrores)
            Perder();
    }

    void VincularBotones()
    {
        if (teclas == null)
        {
            Debug.LogWarning("Ahorcado: asigna el objeto de las teclas en el Inspector.");
            return;
        }

        foreach (Button boton in teclas.GetComponentsInChildren<Button>(true))
        {
            string nombre = boton.gameObject.name.Trim();
            if (string.IsNullOrEmpty(nombre))
                continue;

            char letra = char.ToUpperInvariant(nombre[0]);
            if (letra < 'A' || letra > 'Z')
                continue;

            botones[boton] = letra;
            coloresOriginales[boton] = boton.image != null ? boton.image.color : Color.white;
            spritesOriginales[boton] = boton.image != null ? boton.image.sprite : null;

            char letraCapturada = letra;
            boton.onClick.AddListener(() => AdivinarLetra(letraCapturada));
        }
    }

    void BotonDeLetra(char letra, bool acerto)
    {
        foreach (KeyValuePair<Button, char> par in botones)
        {
            if (par.Value != letra)
                continue;

            par.Key.interactable = false;

            if (par.Key.image == null)
                continue;

            par.Key.image.color = acerto ? colorAcierto : colorError;

            if (!acerto && UImanager.instance != null && UImanager.instance.letraEquivocada != null)
                par.Key.image.sprite = UImanager.instance.letraEquivocada;
        }
    }

    public void SetDificultad(int nivel)
    {
        if (nivel < 0)
            return;

        dificultad = nivel;
        PlayerPrefs.SetInt("Dificultad", nivel);
        PlayerPrefs.Save();

        Reiniciar();
    }

    int ErroresSegunDificultad()
    {
        if (erroresPorDificultad == null || erroresPorDificultad.Length == 0)
            return maximoErrores;

        int nivel = Mathf.Clamp(dificultad, 0, erroresPorDificultad.Length - 1);
        return erroresPorDificultad[nivel];
    }

    int NivelEfectivo()
    {
        int porNivel = Mathf.Max(1, palabrasPorNivel);
        return dificultad + (numPalabrasCompletadas / porNivel);
    }

    int LongitudMinima(int nivel)
    {
        if (longitudMinima == null || longitudMinima.Length == 0)
            return 0;

        return longitudMinima[Mathf.Clamp(nivel, 0, longitudMinima.Length - 1)];
    }

    int LongitudMaxima(int nivel)
    {
        if (longitudMaxima == null || longitudMaxima.Length == 0)
            return 99;

        return longitudMaxima[Mathf.Clamp(nivel, 0, longitudMaxima.Length - 1)];
    }

    string ObtenerPalabra()
    {
        if (palabrasCsv != null && palabrasCsv.Count > 0)
        {
            List<string[]> lineas = palabrasCsv[0].texto;

            List<string> candidatas = Candidatas(lineas, true);

            if (candidatas.Count > 0)
                return candidatas[Random.Range(0, candidatas.Count)];

            candidatas = Candidatas(lineas, false);

            if (candidatas.Count > 0)
                return candidatas[Random.Range(0, candidatas.Count)];
        }

        if (palabras != null && palabras.Length > 0)
            return palabras[Random.Range(0, palabras.Length)].ToUpperInvariant();

        return "";
    }

    int ContarLetra(string palabra, char letra)
    {
        int apariciones = 0;

        foreach (char c in palabra)
            if (c == letra)
                apariciones++;

        return apariciones;
    }

    char LetraParaRevelar(string palabra)
    {
        if (string.IsNullOrEmpty(palabra))
            return '\0';

        foreach (char letra in palabra)
            if (ContarLetra(palabra, letra) == 1)
                return letra;

        return palabra[0];
    }

    void RevelarLetra()
    {
        if (string.IsNullOrEmpty(palabraSecreta))
            return;

        char letra = LetraParaRevelar(palabraSecreta);

        acertadas.Add(letra);
        BotonDeLetra(letra, true);
    }

    List<string> Candidatas(List<string[]> lineas, bool filtrarPorNivel)
    {
        List<string> candidatas = new List<string>();

        if (lineas == null)
            return candidatas;

        int nivelActual = NivelEfectivo();
        bool filtrar = filtrarPorNivel && nivelActual <= nivelMaximo;

        foreach (string[] linea in lineas)
        {
            if (linea == null || linea.Length == 0)
                continue;

            int columna = idioma + 1;

            if (columna >= linea.Length || string.IsNullOrEmpty(linea[columna]))
                columna = 1;

            if (columna >= linea.Length)
                continue;

            string palabra = linea[columna].ToUpperInvariant();

            if (!SoloLetras(palabra))
                continue;

            if (filtrar)
            {
                int nivelPalabra = NivelDeLinea(linea);

                if (nivelPalabra >= 0)
                {
                    if (nivelPalabra != nivelActual)
                        continue;
                }
                else if (palabra.Length < LongitudMinima(nivelActual) || palabra.Length > LongitudMaxima(nivelActual))
                {
                    continue;
                }
            }

            candidatas.Add(palabra);
        }

        return candidatas;
    }

    int NivelDeLinea(string[] linea)
    {
        if (linea.Length < 4)
            return -1;

        int nivel;

        if (!int.TryParse(linea[3], out nivel))
            return -1;

        return nivel - 1;
    }

    bool SoloLetras(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return false;

        foreach (char letra in texto)
            if (letra < 'A' || letra > 'Z')
                return false;

        return true;
    }

    bool PalabraCompleta()
    {

        foreach (char letra in palabraSecreta)
            if (!acertadas.Contains(letra))
                return false;

        return true;
    }

    void Ganar()
    {
        terminado = true;
        numPalabrasCompletadas++;

        if (UImanager.instance != null)
            UImanager.instance.SetNumPalabraPantallaTerminadoUI(numPalabrasCompletadas.ToString());

        UImanager.instance.SetPalabraPantallaTerminadoUI(palabraSecreta);
        ActualizarRecord();
        
        if (textoEstado != null)
        {
            textoEstado.text = "";
            textoEstado.color = colorAcierto;
        }

        if (textoFinal != null)
            textoFinal.text = Texto("victoria");

        if (UImanager.instance != null)
        {
            if (UImanager.instance.textoPalabra != null)
                UImanager.instance.SetPalabraPantallaTerminadoUI(palabraSecreta);

            if (UImanager.instance.animCanvas != null)
                UImanager.instance.ReproducirAnimacion();
                 UImanager.instance.ReproducirEfecto(1);
        }
    }

    void ActualizarRecord()
    {
        if (numPalabrasCompletadas <= maxPalabrasCompletadas)
            return;

        maxPalabrasCompletadas = numPalabrasCompletadas;
        PlayerPrefs.SetInt("PalabrasMaximas", maxPalabrasCompletadas);
        PlayerPrefs.Save();
    }

    void Perder()
    {
        UImanager.instance.SetPalabraPantallaTerminadoUI(palabraSecreta);
        UImanager.instance.SetNumPalabraPantallaTerminadoUI(numPalabrasCompletadas.ToString());
        terminado = true;
        UImanager.instance.OcultarBoton();

        if (textoEstado != null)
        {
            textoEstado.text = "";
            textoEstado.color = colorError;
        }

        if (textoFinal != null)
            textoFinal.text = Texto("derrota");

        if (UImanager.instance != null)
        {
            if (UImanager.instance.textoPalabra != null)
                UImanager.instance.SetPalabraPantallaTerminadoUI(Texto("palabra") + " " + palabraSecreta);

            UImanager.instance.PonerPalabrasMaximas(maxPalabrasCompletadas.ToString());

            if (UImanager.instance.animCanvas != null)
                UImanager.instance.ReproducirAnimacion();
                UImanager.instance.ReproducirEfecto(0);
        }
    }

    string Texto(string id)
    {
        if (UImanager.instance == null)
            return id;

        return UImanager.instance.ObtenerTexto(id);
    }

    void ActualizarTexto()
    {
        if (textoPalabra == null)
            return;

        string linea = "";
        foreach (char letra in palabraSecreta)
            linea += (acertadas.Contains(letra) ? letra.ToString() : "_") + " ";

        textoPalabra.text = linea.TrimEnd();
    }

    void CrearTextosSiFaltan()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        if (textoPalabra == null)
        {
            textoPalabra = CrearTexto(canvas.transform, "TextoPalabra", new Vector2(0f, -40f), 80);
            textoPalabra.transform.SetAsFirstSibling();
        }

        if (textoEstado == null)
            textoEstado = CrearTexto(canvas.transform, "TextoEstado", new Vector2(0f, -130f), 40);
    }

    TMP_Text CrearTexto(Transform padre, string nombre, Vector2 posicion, float tamano)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(1200f, 120f);

        TMP_Text texto = go.AddComponent<TextMeshProUGUI>();
        texto.fontSize = tamano;
        texto.alignment = TextAlignmentOptions.Center;
        texto.raycastTarget = false;
        texto.text = "";

        return texto;
    }
}
