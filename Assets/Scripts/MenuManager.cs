using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public Toggle puntosT;
    public Toggle fichasT;

    public GameObject botonEstadisticas, botonAmigos, botonJugarOnline;

    public GameObject puntosLimit;
    public GameObject fichasLimit;

    public ToggleGroup GameTypeTG;
    public ToggleGroup PuntosLimitTG;
    public ToggleGroup FichasLimitTG;
    public ToggleGroup DificultadTG;
    AdManager AD;
#if !UNITY_EDITOR
    int ADCount;
#endif

	private void Start()
	{
        bool usuarioRegistrado = PlayerPrefs.HasKey( "UserName" );
        botonJugarOnline.SetActive( usuarioRegistrado );

        usuarioRegistrado = false; //TODO Quitar esta linea
        botonAmigos.SetActive(usuarioRegistrado);
        botonEstadisticas.SetActive(usuarioRegistrado);

        AD = AdManager.instance;
#if !UNITY_EDITOR
        ADCount = PlayerPrefs.GetInt("ADCount");
        ADCount++;
        if (ADCount >= 3)
        {
            AD.ShowInstantAd();
            PlayerPrefs.SetInt("ADCount", 0);
        }
        else
        {
            PlayerPrefs.SetInt("ADCount", ADCount);
        }
#endif
	}

	public void ChangeGameType()
    {
        if (puntosT.isOn)
        {
            PlayerPrefs.SetInt("GameType", 0);
            puntosLimit.SetActive(true);
            fichasLimit.SetActive(false);
            Toggle t = PuntosLimitTG.ActiveToggles().FirstOrDefault();
            PlayerPrefs.SetInt("PuntosLimit", t.GetComponent<ToggleValue>().value);

        }
        else if (fichasT.isOn)
        {
            PlayerPrefs.SetInt("GameType", 1);
            puntosLimit.SetActive(false);
            fichasLimit.SetActive(true);
            Toggle t = FichasLimitTG.ActiveToggles().FirstOrDefault();
            PlayerPrefs.SetInt("FichasLimit", t.transform.GetComponent<ToggleValue>().value);
        }       
    }

    public void ChangeDificultad()
    {
        Toggle t = DificultadTG.ActiveToggles().FirstOrDefault();
        PlayerPrefs.SetInt("DificultadIA", t.GetComponent<ToggleValue>().value);
    }

    public void ChangePointLimit()
    {
        Toggle t = PuntosLimitTG.ActiveToggles().FirstOrDefault();
        PlayerPrefs.SetInt("PuntosLimit", t.GetComponent<ToggleValue>().value);
    }
    public void ChangeFichasLimit()
    {
        Toggle t = FichasLimitTG.ActiveToggles().FirstOrDefault();
        PlayerPrefs.SetInt("FichasLimit", t.transform.GetComponent<ToggleValue>().value);
    }

    public void CargarEscena(string scene )
    {
        SceneManager.LoadScene(scene);
    }

    public void Jugar(int i)
    {
        PlayerPrefs.SetInt("Online", i);
        ChangeGameType();
        ChangeDificultad();
		LanguageManager.instance.ClearTexts();

        AD.SetIsGame(true);

		if (i == 0)
			SceneManager.LoadScene ("Game");
		else 
		    SceneManager.LoadScene("Lobby");
	
    }

}
