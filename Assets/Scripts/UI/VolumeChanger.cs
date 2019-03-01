using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeChanger : MonoBehaviour {

	SoundManager SM;

	private void Start()
	{
		SM = SoundManager.instance;
	}
	public void ChangeVolume(float value)
	{
		SM.audioSource.volume = value;
	}
    public void Mute(bool muted)
    {
        SM.Mute( muted );
    }
}
