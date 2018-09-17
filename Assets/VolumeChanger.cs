using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeChanger : MonoBehaviour {

	SoundManager SM;
	public Slider slider;

	private void Start()
	{
		SM = SoundManager.instance;
	}
	public void ChangeVolume()
	{
		SM.audioSource.volume = slider.value;
	}
}
