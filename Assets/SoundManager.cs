using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	#region Singleton
	public static SoundManager instance;
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

	public AudioSource audioSource;
	public AudioClip[] sounds; // all taps for this moment

	public void PlaySound(AudioClip clip)
	{
		audioSource.Stop();
		audioSource.clip = clip;
		audioSource.Play();
	}
}
