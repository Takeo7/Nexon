using UnityEngine;
using UnityEngine.UI;

public class LanguageLine : MonoBehaviour {

	public Text myText;
	public byte myLine;
	LanguageManager LM;

	private void Start()
	{
		LM = LanguageManager.instance;
		AskForLine();
		LM.texts.Add(this);
	}
	public void AskForLine()
	{
		myText.text = LM.ReturnLine(myLine);
	}
}
