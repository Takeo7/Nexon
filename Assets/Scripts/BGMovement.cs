using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGMovement : MonoBehaviour {

	public RawImage img;
	public float speed;

	private void Update()
	{
		Rect rect = img.uvRect;
		rect.x = Mathf.Sin(1 * speed*Time.time);
		img.uvRect = rect;
	}
}
