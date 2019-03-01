using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[DisallowMultipleComponent]
public class TresPuntitos : MonoBehaviour {

    [SerializeField] private Text texto;

    [Tooltip("Cada cuánto tiempo se añade un punto nuevo")]
    [SerializeField] private float frecuencia = 0.5f;
#if UNITY_EDITOR
    private void OnValidate()
    {
        if( texto == null )
            texto = GetComponent<Text>();
    }
#endif
    private void OnEnable()
    {
        StartCoroutine( Waiting() );
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Waiting()
    {
        texto.text = "...";
        while( true )
        {
            yield return new WaitForSeconds( frecuencia );
            texto.text += ".";
            if( texto.text.Length > 3 )
                texto.text = "";
        }
    }
}
