using System.Collections;
using UnityEngine;

namespace Nexon.Networking {

    using UnityEngine.Networking;

    public class LobbyPlayer : NetworkLobbyPlayer
    {
        private IEnumerator Start()
        {
            DontDestroyOnLoad( gameObject );

            if ( isServer )
#if UNITY_EDITOR
                yield return new WaitForSecondsRealtime( 20f );
#else
                yield return new WaitForSecondsRealtime( 10f );
#endif
            SendReadyToBeginMessage();
        }
    }
}
