using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageChanger : MonoBehaviour {

	public Dropdown dropdown;
	LanguageManager LM;

	private void Start()
	{
		LM = LanguageManager.instance;
	}
	public void ChangeLanguage()
	{
		LM.SelectLanguage((byte)dropdown.value);
	}
}
