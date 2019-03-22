using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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
        audioSource = GetComponent<AudioSource>();
	}
	#endregion

	public AudioClip[] sounds; // all taps for this moment

	private AudioSource audioSource; // obtained with GetComponent

	public void PlaySound(AudioClip clip)
	{
		audioSource.Stop();
		audioSource.clip = clip;
		audioSource.Play();
	}

    public void SetVolume( float vol )
    {
        audioSource.volume = vol;
    }

    public void Mute( bool muted )
    {
        audioSource.mute = muted;
    }
}
