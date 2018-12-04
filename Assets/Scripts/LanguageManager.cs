using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LanguageManager : MonoBehaviour {

	#region Singleton
	public static LanguageManager instance;
	private void Awake()
	{
		if(instance != null)
		{
			Destroy(gameObject);
		}
		else if(instance == null)
		{
			instance = this;
		}
	}

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded( Scene arg0 , LoadSceneMode arg1 )
    {
        ClearTexts();
        if( arg0.name == "Menu" )
            AdManager.instance.isGame = false;
        else
            AdManager.instance.isGame = true;
    }
    #endregion

    public Language[] languageInfo;
	public Language languageSelected;

	public List<LanguageLine> texts;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		SelectLanguage((byte)PlayerPrefs.GetInt("language"));
	}
	public void SelectLanguage(byte i)
	{
		languageSelected = languageInfo[i];
		UpdateText();
		PlayerPrefs.SetInt("language", i);
	}
	public void UpdateText()
	{
		int length = texts.Count;
		for (int i = 0; i < length; i++)
		{
			texts[i].AskForLine();
		}
	}
	public string ReturnLine(byte i)
	{
		return languageSelected.lines[i];
	}
	public void ClearTexts()
	{
		texts = new List<LanguageLine>();
	}

}
