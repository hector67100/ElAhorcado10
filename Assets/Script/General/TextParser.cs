using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TextParser
{
    public const string path = "CSV/";

    public static List<CsvParsed> obtenerTexto(string archivo)
    {
        List<CsvParsed> texto = new List<CsvParsed>();
        string ruta = RutaArchivo(archivo);

        if (ruta == null)
        {
            Debug.LogError("TextParser: no se encontró el archivo " + archivo + ".csv");
            return texto;
        }

        string[] lineas = File.ReadAllLines(ruta);
        int id = -1;

        for (int i = 0; i < lineas.Length; i++)
        {
            string[] parts = lineas[i].Split(',');

            if (parts.Length == 1 && parts[0].Trim() == "")
                continue;

            if (parts[0].Trim() != "")
            {
                id++;
                texto.Add(new CsvParsed(parts[0].Trim()));
            }

            if (id < 0)
                continue;

            string[] data = new string[parts.Length - 1];
            for (int j = 1; j < parts.Length; j++)
                data[j - 1] = parts[j].Trim();

            texto[id].texto.Add(data);
        }

        if (texto.Count > 0)
            texto.RemoveAt(0);

        return texto;
    }

    static string RutaArchivo(string archivo)
    {
        string nombre = archivo + ".csv";

        string streaming = Path.Combine(Application.streamingAssetsPath, nombre);
        if (File.Exists(streaming))
            return streaming;

        string enAssets = Path.Combine(Application.dataPath, "CSV", nombre);
        if (File.Exists(enAssets))
            return enAssets;

        return null;
    }
}
