using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.UI;

public class FriendOptionPopup : MonoBehaviour {

    [SerializeField]
    Toggle toggleFavourite = null, toggleBlocked = null;

    [SerializeField]
    LobbyManager lobbyPrefab;

    Friend friend;

    public void Show ( Friend friend )
    {
        this.friend = friend;
        toggleBlocked.isOn = friend.isBlocked;
        toggleFavourite.isOn = friend.isFavourite;
    }

    public void Hide ()
    {
        Destroy(gameObject);
    }

    public void OnChangeFavorito(bool esFavorito)
    {
        FriendsList.SetFriendFavourite( friend.uid , esFavorito );
    }

    public void OnChangeBloqueado(bool estaBloqueado)
    {
        FriendsList.SetFriendBlocked( friend.uid , estaBloqueado );
    }

    public void InviteToGame()
    {
        //TODO Crear partida online y enviar notificación push con el id de la partida (hacer la room privada)
        LobbyManager LM = FindObjectOfType<LobbyManager>();
        if( LM == null )
            LM = Instantiate( lobbyPrefab );
        if( LM != null )
        {
            StartCoroutine(CreateMatchMaking(LM));
        }
    }
    
    private System.Collections.IEnumerator CreateMatchMaking(LobbyManager lobbyManager)
    {
        PlayerPrefs.SetInt( "Online" , 1 );
        yield return new WaitForSeconds( 0.5f );

        string user = PlayerPrefs.GetString( "UserName" , "" );
        string matchName = user;
        ulong networkId = 0;

        if( PlayerPrefs.GetInt( "GameType" ) == 0 )
            matchName += " 0" + PlayerPrefs.GetInt( "PuntosLimit" ).ToString();
        else
            matchName += " 1" + PlayerPrefs.GetInt( "FichasLimit" ).ToString();

        Debug.Log( "Filter string => " + matchName );
        PlayerPrefs.SetString( "MatchName" , matchName );

        lobbyManager.StartMatchMaker();
        lobbyManager.matchMaker.CreateMatch(
            matchName ,
            (uint)lobbyManager.maxPlayers ,
            false ,
            "" , "" , "" , 0 , 0 ,
            ( success , extendedInfo , matchInfo ) => {
                lobbyManager.OnMatchCreate(success, extendedInfo, matchInfo);
                networkId = (System.UInt64)matchInfo.networkId;
                Debug.LogWarning( "Creada la partida "+networkId );
                NotificationsManager.InviteFriend( friend.uid , Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId, Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.DisplayName , matchName , networkId );
            }
        );

        lobbyManager.backDelegate = lobbyManager.StopHost;
        lobbyManager._isMatchmaking = true;
        lobbyManager.DisplayIsConnecting();

        lobbyManager.SetServerInfo( "Matchmaker Host" , lobbyManager.matchHost );
        Hide();

    }

    public void RemoveFriend()
    {
        FriendsList.RemoveFriend( friend.uid );
        Hide();
    }
}
