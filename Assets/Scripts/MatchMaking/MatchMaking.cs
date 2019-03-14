using System.Collections.Generic;
using UnityEngine;

namespace Nexon.Networking {
    using UnityEngine.Networking;
    using UnityEngine.Networking.Match;
    using UnityEngine.Networking.Types;
    using UnityEngine.SceneManagement;

    [RequireComponent(typeof(NetworkMigrationManager))]
    public class MatchMaking : NetworkLobbyManager
    {
        private bool partidaEncontrada = false;

        private bool matchmaking = false;
        private bool isHosting = false;
        private bool _disconnectServer = false;
        private int page = 0;

        public void AbandonarMatchMaking()
        {

            if( isHosting )
            {
                matchMaker.DestroyMatch( matchInfo.networkId , 0 , OnDestroyMatch );
                _disconnectServer = true;
            }
            else
            {
                StopClient();

                if( matchmaking )
                {
                    StopMatchMaker();
                }
                Destroy( gameObject );
                UnityEngine.SceneManagement.SceneManager.LoadScene( 0 );
            }

        }

        public override void OnLobbyServerPlayersReady()
        {
            // Evitamos que esta partida se liste a otras personas una vez comienza
            matchMaker.SetMatchAttributes( matchInfo.networkId , false , 0 , OnSetMatchAttributes );
            base.OnLobbyServerPlayersReady();
        }

        public override void OnDropConnection( bool success , string extendedInfo )
        {
            // TODO gestionar el host migration
            base.OnDropConnection( success , extendedInfo );
        }

        public override void OnClientDisconnect( NetworkConnection conn )
        {
            base.OnClientDisconnect( conn );
            GetComponent<NetworkMigrationManager>().BecomeNewHost( matchPort );
        }

        public override void OnClientSceneChanged( NetworkConnection conn )
        {
            base.OnClientSceneChanged( conn );
            if( SceneManager.GetSceneAt( 0 ).name == playScene )
            {
                transform.GetChild( 0 ).gameObject.SetActive( false );
            }
            else
            {
                Debug.Log( "ClienteSceneChanged" );
                AbandonarMatchMaking();
            }
        }


        public override void OnServerSceneChanged( string sceneName )
        {
            base.OnServerSceneChanged( sceneName );
            if( sceneName == "Game" )
            {
                Debug.LogWarning( "Entrando en la escena de juego" );
                foreach( var p in spawnPrefabs )
                {
                    Debug.LogWarning( "Spawneando "+p );
                    GameObject go = Instantiate( p );
                    NetworkServer.Spawn( go );
                }
            }
        }

        public override void OnDestroyMatch( bool success , string extendedInfo )
        {
            base.OnDestroyMatch( success , extendedInfo );
            if( _disconnectServer )
            {
                StopMatchMaker();
                StopHost();
            }
            Destroy( gameObject );
            UnityEngine.SceneManagement.SceneManager.LoadScene( 0 );
        }

        void Start () {
            CrearOUnirse();
	    }
        
        private void CrearOUnirse()
        {
            StartMatchMaker();
            matchMaker.ListMatches(
                page,                      // Pagina
                10,                     // Partidas por pagina
                Application.version ,   // Sin filtros
                true,                   // No buscar partidas privadas
                0,                      // Elo = 0
                0,                      // RequestDomain
                OnMatchList
                );
        }


        private void Unirse( NetworkID networkID)
        {
            matchmaking = true;
            matchMaker.JoinMatch( 
                networkID , 
                "" , 
                "" , 
                "" ,
                0 ,
                0 ,
                OnMatchJoined
                );
        }

        private void Crear()
        {
            matchMaker.CreateMatch(
                string.Format( "{1}{0:000000}" , Random.Range( 0 , 100000 ) , Application.version ) ,
                    (uint)maxPlayers ,
                    true ,
                    "" , "" , "" , 0 , 0 ,
                    OnMatchCreate
                );
        }
        public override void OnMatchCreate( bool success , string extendedInfo , MatchInfo matchInfo )
        {
            matchmaking = true;
            isHosting = true;
            base.OnMatchCreate( success , extendedInfo , matchInfo );
        }


        public override void OnMatchList( bool success , string extendedInfo , List<MatchInfoSnapshot> matches )
        {
            Debug.LogWarning( " OnMatchList::ExtendedInfo-> " + extendedInfo );
            base.OnMatchList( success , extendedInfo , matches );
            // Si no encuentro ninguna partida creo una
            if( matches.Count == 0 )
            {
                Debug.LogWarning( "Creando partida porque no encuentro ninguna a la que unirme." );
                Crear();
            }

            // Si encuentro partida, me uno a una de ellas aleatoriamente
            else
            {
#if UNITY_EDITOR
                foreach( var m in matches )
                {
                    Debug.LogWarningFormat( " Listando {0} con {1} slots " , m.name , m.currentSize );
                }
#endif
                int idx = Random.Range( 0 , matches.Count );
                while( matches.Count > 0 && matches[idx].currentSize > 1 )
                {
                    matches.RemoveAt( idx );
                    idx = Random.Range( 0 , matches.Count );
                }

                if( matches.Count == 0 )
                {
                    page++;
                    matchMaker.ListMatches(
                        page ,                      // Pagina
                        10 ,                     // Partidas por pagina
                        Application.version ,   // Sin filtros
                        true ,                   // No buscar partidas privadas
                        0 ,                      // Elo = 0
                        0 ,                      // RequestDomain
                        OnMatchList
                        );
                }
                else
                {
                    Debug.LogWarningFormat( " Uniéndome a la partida {0}." , matches[idx].name );
                    Unirse( matches[idx].networkId );
                }
            }
        }

        public override GameObject OnLobbyServerCreateGamePlayer( NetworkConnection conn , short playerControllerId )
        {
            transform.GetChild(0).gameObject.SetActive( false ); // Ocultamos la interfaz

            return base.OnLobbyServerCreateGamePlayer( conn , playerControllerId );
        }
    }
}
