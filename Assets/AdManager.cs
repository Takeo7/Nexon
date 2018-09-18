using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour {

	private void Start()
	{
		StartCoroutine("Ad");
	}
	IEnumerator Ad()
	{
		Debug.Log("ye");
		yield return new WaitForSeconds(60f);
		if (Advertisement.IsReady("downbanner"))
		{
			Advertisement.Show("downbanner");
		}
		yield return new WaitForSeconds(Random.Range(0.5f, 2f));
		StartCoroutine("Ad");
	}
}
