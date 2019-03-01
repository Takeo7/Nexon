using System.Collections;
using UnityEngine;

namespace Nexon.Networking {

    using UnityEngine.Networking;

    public class LobbyPlayer : NetworkLobbyPlayer
    {
        private IEnumerator Start()
        {
            DontDestroyOnLoad( gameObject );
            yield return new WaitForSecondsRealtime( 5f );
            SendReadyToBeginMessage();
        }
    }
}
