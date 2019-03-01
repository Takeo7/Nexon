using UnityEngine;
using UnityEngine.UI;

public class LanguageLine : MonoBehaviour {

	public Text myText;
	public byte myLine;
	//LanguageManager LM;

	private void Start()
	{
		//LM = LanguageManager.instance;
		AskForLine();
        if ( LanguageManager.instance != null )
            LanguageManager.instance.texts.Add(this);
	}
	public void AskForLine()
	{
        if ( LanguageManager.instance != null )
		    myText.text = LanguageManager.instance.ReturnLine(myLine);
	}
}
