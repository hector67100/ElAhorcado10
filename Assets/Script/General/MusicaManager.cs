using System.Collections;
using UnityEngine;

public class MusicaManager : MonoBehaviour
{
    [Header("Fuente")]
    public AudioSource fuente;
    public AudioClip cancionInicial;
    public float duracionFade = 1f;
    public float volumenMaximo = 1f;

    [Range(1, 5)]
    [Tooltip("Suavidad de la curva: 2 = suave, 3 = muy suave (cúbica)")]
    public int suavidad = 3;

    [Header("Lista")]
    public AudioClip[] canciones;
    public bool cambiarAlAcabar = true;
    public bool repetirLista = true;

    int indiceCancion;
    Coroutine cambioActual;
    Coroutine esperaFin;

    [Header("Aleatorio")]
    public bool aleatorioIncial = true;
    public bool ordenAleatorio = false;


    void Start()
    {
        if (fuente == null)
        {
            Debug.LogError("MusicaManager: asigna el AudioSource.");
            return;
        }

        AudioClip inicial = cancionInicial;

        if (canciones != null && canciones.Length > 0)
        {
            indiceCancion = aleatorioIncial ? IndiceAleatorio(-1) : 0;
            inicial = canciones[indiceCancion];
        }

        if (inicial != null)
        {
            fuente.clip = inicial;
            fuente.loop = !cambiarAlAcabar || canciones == null || canciones.Length == 0;
            fuente.volume = volumenMaximo;
            fuente.Play();
        }
    }

    public void CambiarCancion(AudioClip nuevaCancion)
    {
        if (fuente == null || nuevaCancion == null)
            return;

        DetenerEspera();

        if (canciones != null)
            for (int i = 0; i < canciones.Length; i++)
                if (canciones[i] == nuevaCancion)
                    indiceCancion = i;

        if (cambioActual != null)
            StopCoroutine(cambioActual);

        cambioActual = StartCoroutine(FadeCambio(nuevaCancion));
    }

    public void SiguienteCancion()
    {
        if (canciones == null || canciones.Length == 0)
            return;

        if (ordenAleatorio)
        {
            indiceCancion = IndiceAleatorio(indiceCancion);
            CambiarCancion(canciones[indiceCancion]);
            return;
        }

        indiceCancion++;
        if (indiceCancion >= canciones.Length)
        {
            if (!repetirLista)
            {
                indiceCancion = canciones.Length - 1;
                DetenerEspera();
                return;
            }

            indiceCancion = 0;
        }

        CambiarCancion(canciones[indiceCancion]);
    }

    public void AnteriorCancion()
    {
        if (canciones == null || canciones.Length == 0)
            return;

        indiceCancion--;
        if (indiceCancion < 0)
            indiceCancion = canciones.Length - 1;

        CambiarCancion(canciones[indiceCancion]);
    }

    IEnumerator FadeCambio(AudioClip nuevaCancion)
    {
        if (fuente.isPlaying)
        {
            float volumenInicial = fuente.volume;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, duracionFade);
                fuente.volume = volumenInicial * (1f - EaseOut(Mathf.Clamp01(t)));
                yield return null;
            }
        }

        fuente.volume = 0f;
        fuente.Stop();
        fuente.clip = nuevaCancion;
        fuente.Play();

        yield return FadeIn();
    }

    IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duracionFade);
            fuente.volume = Mathf.Clamp01(volumenMaximo * EaseIn(Mathf.Clamp01(t)));
            yield return null;
        }

        fuente.volume = volumenMaximo;
        cambioActual = null;
        IniciarEsperaFin();
    }

    void IniciarEsperaFin()
    {
        if (!cambiarAlAcabar || fuente.clip == null)
            return;

        if (esperaFin != null)
            StopCoroutine(esperaFin);

        esperaFin = StartCoroutine(EsperarFin());
    }

    IEnumerator EsperarFin()
    {
        float restante = fuente.clip.length - fuente.time;
        float espera = Mathf.Max(0f, restante - duracionFade);

        yield return new WaitForSeconds(espera);

        esperaFin = null;
        SiguienteCancion();
    }

    void DetenerEspera()
    {
        if (esperaFin != null)
        {
            StopCoroutine(esperaFin);
            esperaFin = null;
        }
    }

    float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, suavidad);
    }

    float EaseIn(float t)
    {
        return Mathf.Pow(t, suavidad);
    }

    int IndiceAleatorio(int evitar)
    {
        if (canciones == null || canciones.Length == 0)
            return 0;

        if (canciones.Length == 1)
            return 0;

        int elegido;
        do
        {
            elegido = Random.Range(0, canciones.Length);
        } while (elegido == evitar);

        return elegido;
    }
}
