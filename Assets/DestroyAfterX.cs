using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterX : MonoBehaviour {

	public float x;

	private void Start()
	{
		StartCoroutine("DestroyAfterXs");
	}
	IEnumerator DestroyAfterXs()
	{
		yield return new WaitForSeconds(x);
		Destroy(gameObject);
	}
}
