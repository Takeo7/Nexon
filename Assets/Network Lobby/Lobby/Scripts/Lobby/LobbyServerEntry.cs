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

            int pos = match.name.LastIndexOf( ' ' )+1;

            string customName = match.name.Substring( 0, pos);

            if( match.name[pos] == '0' )
                customName += " P ";//Partida a PUNTOS
            else
            {
                switch( LanguageManager.instance.languageSelected._language )
                {
                    case language.Deutsch:
                        customName += " S ";//Partida a FICHAS
                        break;
                    case language.English:
                        customName += " T ";//Partida a FICHAS
                        break;
                    case language.Español:
                        customName += " F ";//Partida a FICHAS
                        break;
                    case language.Français:
                        customName += " J ";//Partida a FICHAS
                        break;
                }
            }

            customName += " "+match.name.Substring( pos+1 );

            serverInfoText.text = customName;

            slotInfo.text = match.currentSize.ToString() + "/" + match.maxSize.ToString(); ;

            NetworkID networkID = match.networkId;

            lobbyManager.matchName = match.name;
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener( () => { JoinMatch(networkID, lobbyManager, match.name); });

            GetComponent<Image>().color = c;
        }

        void JoinMatch(NetworkID networkID, LobbyManager lobbyManager, string matchname)
        {
            PlayerPrefs.SetString( "MatchName" , matchname );
            PlayerPrefs.SetString("MatchID",((ulong)networkID).ToString());
			lobbyManager.matchMaker.JoinMatch(networkID, "", "", "", 0, 0, lobbyManager.OnMatchJoined);
			lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager._isMatchmaking = true;
            lobbyManager.DisplayIsConnecting();
        }
    }
}