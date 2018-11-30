using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using System.Collections;

namespace Prototype.NetworkLobby
{
    public class LobbyServerEntry : MonoBehaviour 
    {
        public Text serverInfoText;
        public Text slotInfo;
        public Button joinButton;

		public void Populate(MatchInfoSnapshot match, LobbyManager lobbyManager, Color c)
		{
            Debug.Log( "Populate: " + match.name );
            PlayerPrefs.SetString( "MachName" , match.name );
            serverInfoText.text = match.name;

            slotInfo.text = match.currentSize.ToString() + "/" + match.maxSize.ToString(); ;

            NetworkID networkID = match.networkId;
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener( () => { JoinMatch(networkID, lobbyManager); });

            GetComponent<Image>().color = c;
        }

        void JoinMatch(NetworkID networkID, LobbyManager lobbyManager)
        {
            if( !string.IsNullOrEmpty( lobbyManager.matchName ) )
            {
                PlayerPrefs.SetString( "MachName" , lobbyManager.matchName );
                Debug.LogWarning( "Lobby Match Name: " + lobbyManager.matchName );
            }
            PlayerPrefs.SetString("MatchID",((ulong)networkID).ToString());
			lobbyManager.matchMaker.JoinMatch(networkID, "", "", "", 0, 0, lobbyManager.OnMatchJoined);
			lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager._isMatchmaking = true;
            lobbyManager.DisplayIsConnecting();
        }
    }
}