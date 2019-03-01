using System.Collections;
using UnityEngine;

public class ScreenRecorder : MonoBehaviour {

    int fileNumber = 0;

	IEnumerator Start () {
        Debug.Log( "Guardando capturas en " + Application.persistentDataPath );
        while( true )
        {
            ScreenCapture.CaptureScreenshot( string.Format("Tutorial{0:0000}.png",fileNumber) );
            fileNumber++;
            yield return new WaitForSeconds( 0.5f );
        }
	}
    public void StartRecording()
    {
        StartCoroutine( Start() );
    }
    public void StopRecording()
    {
        StopAllCoroutines();
    }
	
	// Update is called once per frame
	void OnDisable () {
        StopRecording();
	}
}
