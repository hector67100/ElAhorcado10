using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CsvParsed
{
    public string id;
    [SerializeField] public List<string[]> texto;
    public CsvParsed(string id)
    {
        this.id = id;
        this.texto = new List<string[]>();
    }

    public void SetId(string id)
    {
        this.id = id;
    }

    public void GetTextoPorIdTexto(string id)
    {
        foreach (string[] linea in texto)
        {
            if (linea[0] == id)
            {
                Debug.Log(linea[1]);
                return;
            }
        }
    }

}