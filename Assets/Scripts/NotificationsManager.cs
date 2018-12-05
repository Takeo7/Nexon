using System.Collections;
using System.Collections.Generic;
using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

public class NotificationsManager : MonoBehaviour {

    static NotificationsManager instance;
    [SerializeField]
    private RectTransform botonNotificacion = null;
    [SerializeField]
    private float botonSpeed = 10;

    private string currentId;
    private ulong netID;
    private string matchName;
    private string notificationText;

    private bool canShowNotification = true;

    private void Awake()
    {
        if( instance == null )
            instance = this;
        else
            Destroy( gameObject );
    }
    // Use this for initialization
    void Start () {
        DontDestroyOnLoad( gameObject );

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        currentId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child( "matches/"+currentId ).ValueChanged += NotificationsManager_ValueChanged;

    }

    private void SceneManager_sceneLoaded( Scene scene , LoadSceneMode mode )
    {
        canShowNotification = scene.name != "Login" && scene.name != "Game";
        if( scene.name == "Lobby" )
        {
            OnClicNotificacion();
        }
    }

    private void NotificationsManager_ValueChanged( object sender , Firebase.Database.ValueChangedEventArgs e )
    {
        if( e.Snapshot.Value == null )
            return;
        Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child( "users/" + currentId + "/friends/" + e.Snapshot.Child( "friendID" ) + "/isBlocked" ).GetValueAsync()
            .ContinueWith( task => {

                if( task.Result.Value == null || task.Result.Value.ToString() == "false" ) // Si no está bloqueado
                {
                    StartCoroutine( "ShowNotification" );
                    ulong.TryParse( "" + e.Snapshot.Child( "networkID" ).Value , out netID );
                    Debug.LogWarning( "El valor obtenido de netid es = " + netID );
                    matchName = "" + e.Snapshot.Child( "matchName" ).Value;
                    Debug.LogWarning( "El valor obtenido del matchName es = " + matchName );
                    notificationText = "" + e.Snapshot.Child( "message" ).Value;
                    GetComponentInChildren<UnityEngine.UI.Text>().text = notificationText;
                }
                else
                {
                    Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child( "matches/" + currentId ).RemoveValueAsync();
                }
            } );
    }

    void JoinMatch( NetworkID networkID , LobbyManager lobbyManager , string matchname )
    {
        Debug.LogError( "Trying to join to " + networkID );
        PlayerPrefs.SetString( "MatchName" , matchname );
        PlayerPrefs.SetString( "MatchID" , ((ulong)networkID).ToString() );
        lobbyManager.matchMaker.JoinMatch( networkID , "" , "" , "" , 0 , 0 , lobbyManager.OnMatchJoined );
        lobbyManager.backDelegate = lobbyManager.StopClientClbk;
        lobbyManager._isMatchmaking = true;
        lobbyManager.DisplayIsConnecting();

        Debug.LogWarning( "Reseteo valores" );
        netID = 0;
        matchName = "";
        notificationText = "";
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child( "matches/" + currentId ).ValueChanged -= NotificationsManager_ValueChanged;
    }

    public static void InviteFriend( string friendID,string yourID, string yourName, string matchName, ulong networkId )
    {
        string message = yourName + " invited you to play.";
        Dictionary<string , object> match = new Dictionary<string,object>{
                {"friendID", friendID },
                { "networkID" , networkId.ToString() },
                { "matchName", matchName },
                { "message", message },
        };
        Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child( "matches/" + friendID ).SetValueAsync( match );
    }


    public void OnClicNotificacion()
    {
        Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child( "matches/" + currentId ).RemoveValueAsync();
        StartCoroutine( HideNotification() );
        Debug.Log( "Net Id = " + netID );
        Debug.Log( "MatchName = " + matchName );

        if( netID != 0 && !string.IsNullOrEmpty( matchName ) )
        {
            LobbyManager LM = FindObjectOfType<LobbyManager>();
            if( LM != null )
            {
                LM.StartMatchMaker();
                JoinMatch( (NetworkID)netID , LM , matchName );
            }
            else
            {
                SceneManager.LoadScene( "Lobby" );
            }
        }
    }

    private IEnumerator ShowNotification()
    {
        Vector2 position = botonNotificacion.anchoredPosition;
        float height = botonNotificacion.sizeDelta.y;

        while( position.y > -height )
        {
            position.y -= Time.deltaTime * botonSpeed;
            botonNotificacion.anchoredPosition = position;
            yield return null;
        }
        position.y = -height;
        botonNotificacion.anchoredPosition = position;
    }

    private IEnumerator HideNotification()
    {
        Vector2 position = botonNotificacion.anchoredPosition;
        float height = botonNotificacion.sizeDelta.y;

        while( position.y < 0 )
        {
            position.y += Time.deltaTime * botonSpeed;
            botonNotificacion.anchoredPosition = position;
            yield return null;
        }
        position.y = 0;
        botonNotificacion.anchoredPosition = position;
    }
}
