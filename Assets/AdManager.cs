using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour {

	#region Singleton
	public static AdManager instance;
	private void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else if (instance == null)
		{
			instance = this;
		}
	}
	#endregion

	public bool isGame = false;

	private void Start()
	{
		//Advertisement.Initialize("2804256", true);
		StartCoroutine("Ad");
	}
	IEnumerator Ad()
	{
		Debug.Log("ye");
		yield return new WaitForSeconds(60f);
		if (isGame == false)
		{
			if (Advertisement.IsReady("video"))
			{
				Debug.Log("ShowAd");
				Advertisement.Show("video");
			}
		}
		yield return new WaitForSeconds(Random.Range(0.5f, 2f));
		StartCoroutine("Ad");
	}
	public void ShowInstantAd()
	{
		if (Advertisement.IsReady("video"))
		{
			Debug.Log("ShowInstantAd");
			Advertisement.Show("video");
		}
	}
	public void SetIsGame(bool _isGame)
	{
		isGame = _isGame;
	}
}
