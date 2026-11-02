using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public static class HerramientasGenerales
{
    // Start is called before the first frame update

    public static string ListAJson<T>(List<T> lista)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = lista.ToArray();
        string json = JsonUtility.ToJson(wrapper);
        return json;
    }

    public static List<T> FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return new List<T>(wrapper.Items);
    }

    public static Transform ObtenerHijo(Transform padre, string nombreHijo)
    {
        Transform hijo = null;
        foreach (Transform child in padre)
        {
            if (child.name.Equals(nombreHijo, StringComparison.OrdinalIgnoreCase))
            {
                hijo = child;
                return hijo;
            }
        }
        return hijo;
    }

    public static void ObtenerTextoPorId(GameObject[] textosEnContenedores, List<CsvParsed> csvParsedList, int indexTextoEnNovela, int idiomaSeleccionado)
    {
        if (textosEnContenedores == null || csvParsedList == null)
            return;

        if (indexTextoEnNovela < 0 || indexTextoEnNovela >= csvParsedList.Count)
            return;

        List<string[]> lineas = csvParsedList[indexTextoEnNovela].texto;

        foreach(GameObject contenedor in textosEnContenedores)
        {
            if (contenedor == null)
                continue;

            TMP_Text texto = contenedor.GetComponent<TMP_Text>();
            if (texto == null)
                continue;

            string idPadre = null;
            if (texto.transform.parent != null && texto.transform.parent.GetComponent<UnityEngine.UI.Button>() != null)
                idPadre = texto.transform.parent.name;

            string[] lineaTexto = lineas.Find(x => x != null && x.Length > 0 && x[0] != null &&
                (Normalizar(x[0]) == Normalizar(texto.name) || (idPadre != null && Normalizar(x[0]) == Normalizar(idPadre))));

            if (lineaTexto != null)
            {
                if (idiomaSeleccionado + 1 < lineaTexto.Length)
                    texto.text = lineaTexto[idiomaSeleccionado+1];
                else
                    Debug.LogWarning("El ID '" + texto.name + "' no tiene la columna del idioma " + idiomaSeleccionado);
            }
            else
            {
                Debug.LogWarning("No se encontró texto para el ID: " + texto.name);
            }
        }
    }

    public static string ObtenerTextoPorId(string id, List<CsvParsed> csvParsedList, int indexTextoEnNovela, int idiomaSeleccionado)
    {
        if (csvParsedList == null || indexTextoEnNovela < 0 || indexTextoEnNovela >= csvParsedList.Count)
            return null;

        string[] lineaTexto = csvParsedList[indexTextoEnNovela].texto.Find(x => x != null && x.Length > 0 && x[0] != null && Normalizar(x[0]) == Normalizar(id));

        if (lineaTexto != null)
        {
            if (idiomaSeleccionado + 1 < lineaTexto.Length)
                return lineaTexto[idiomaSeleccionado + 1];

            Debug.LogWarning("El ID '" + id + "' no tiene la columna del idioma " + idiomaSeleccionado);
            return null;
        }

        Debug.LogWarning("No se encontró texto para el ID: " + id);
        return null;
    }

    static string Normalizar(string texto)
    {
        if (texto == null)
            return "";

        return texto.Trim().Trim('"').Trim('\'').Trim().ToLowerInvariant();
    }
    

}

//clase para Guardarstatic  cualquier Lista o array a un json
    [Serializable]
    public class Wrapper<T>
    {
        public T[] Items;
    }
