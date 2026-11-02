using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CambiarEscena : MonoBehaviour
{
    public void CargarEscenaSuave(string nombreEscena, Animator anim, string nombreAnimacion)
    {
        StartCoroutine(CargarTrasAnimacion(nombreEscena, anim, nombreAnimacion));
    }

    IEnumerator CargarTrasAnimacion(string escena, Animator anim, string nombreAnimacion)
    {
        AsyncOperation carga = SceneManager.LoadSceneAsync(escena);
        carga.allowSceneActivation = false;

        float duracion = 2f;

        if (anim != null)
        {
            anim.Play(nombreAnimacion);
            yield return null;

            duracion = DuracionAnimacion(anim, nombreAnimacion);
        }

        yield return new WaitForSeconds(duracion);

        while (carga.progress < 0.9f)
            yield return null;

        carga.allowSceneActivation = true;
    }

    float DuracionAnimacion(Animator anim, string nombreAnimacion)
    {
        if (anim.runtimeAnimatorController != null)
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
                if (clip.name == nombreAnimacion)
                    return clip.length;

        AnimatorStateInfo estado = anim.GetCurrentAnimatorStateInfo(0);
        if (estado.length > 0f)
            return estado.length;

        return 2f;
    }
}
