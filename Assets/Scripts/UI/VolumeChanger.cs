using UnityEngine;

public class VolumeChanger : MonoBehaviour {

	SoundManager SM;

	private void Start()
	{
		SM = SoundManager.instance;
	}
	public void ChangeVolume(float value)
	{
        SM.SetVolume( value );
	}
    public void Mute(bool muted)
    {
        SM.Mute( muted );
    }
}
