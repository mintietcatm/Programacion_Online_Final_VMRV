using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public Transform content;
    public EntradaServidor prefabSesion;
    public TMP_Text mensajeError;

    private List<EntradaServidor> entradas = new List<EntradaServidor>();

    public void Actualizar(List<SessionInfo> sesiones)
    {
        for (int i = entradas.Count - 1; i >= 0; i--) //for pero negativo pa quitar en lugar de poner
        {
            bool sigueExistiendo = false;

            for (int j = 0; j < sesiones.Count; j++)
            {
                if (entradas[i].nombreSesion == sesiones[j].Name
                    && sesiones[j].IsVisible)
                {
                    sigueExistiendo = true;
                    break;
                }
            }

            if (!sigueExistiendo)
            {
                entradas[i].gameObject.SetActive(false);
                Destroy(entradas[i].gameObject);
                entradas.RemoveAt(i);
            }
        }

        for (int i = 0; i < sesiones.Count; i++)
        {
            if (sesiones[i].IsVisible)
            {
                bool yaExiste = false;

                // Buscamos si ya tenemos una entrada con ese nombre.
                for (int j = 0; j < entradas.Count; j++)
                {
                    if (entradas[j].nombreSesion == sesiones[i].Name)
                    {
                        entradas[j].SetInfo(sesiones[i]);

                        yaExiste = true;
                        break;
                    }
                }

                if (entradas.Count == 0)
                {
                    mensajeError.text = "no disponible";
                }
                else
                {
                    mensajeError.text = "";
                }

                if (!yaExiste)
                {
                    EntradaServidor nuevaEntrada =
                        Instantiate(prefabSesion, content);

                    nuevaEntrada.SetInfo(sesiones[i]);

                    entradas.Add(nuevaEntrada);
                }
            }
        }

        }
    }

