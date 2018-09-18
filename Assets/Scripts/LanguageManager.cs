using System.Collections.Generic;
using UnityEngine;

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
