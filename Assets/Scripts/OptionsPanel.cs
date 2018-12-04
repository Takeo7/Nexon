using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour {

    [SerializeField]
    GameObject textoCambiarNombre;
    [SerializeField]
    InputField cambiarNombre;
    [SerializeField]
    Button logout;
    [SerializeField]
    RectTransform panelNegro;
    
    private void OnEnable()
    {
        Vector2 sd = panelNegro.sizeDelta;
        if( Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null )
        {
            textoCambiarNombre.SetActive( false );
            cambiarNombre.gameObject.SetActive( false );
            logout.gameObject.SetActive( false );
            panelNegro.sizeDelta = new Vector2( sd.x, 550 );
        }
        else
        {
            cambiarNombre.text = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
            panelNegro.sizeDelta = new Vector2( sd.x , 1125 );
        }
    }

    public void ChangeDisplayName( string displayName )
    {
        if( Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null )
        {
            if( displayName.Length >= 5 )
            {
                string currentUserUid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                Firebase.Database.FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/displayName" ).SetValueAsync( displayName );
                Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UpdateUserProfileAsync(
                    new Firebase.Auth.UserProfile { DisplayName = displayName }
                    );
            }
            else
            {
                cambiarNombre.text = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
            }
        }
    }
}
