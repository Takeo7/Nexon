using System.Collections.Generic;
using UnityEngine;

namespace Nexon.Networking {
    using UnityEngine.Networking;
    using UnityEngine.Networking.Match;
    using UnityEngine.Networking.Types;

    public class MatchMaking : NetworkLobbyManager
    {
        private bool partidaEncontrada = false;

        public void AbandonarMatchMaking()
        {

        }

	    void Start () {
            CrearOUnirse();
	    }
        
        private void CrearOUnirse()
        {
            StartMatchMaker();
            matchMaker.ListMatches(
                0,                      // Pagina
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


        public override void OnMatchList( bool success , string extendedInfo , List<MatchInfoSnapshot> matches )
        {
            base.OnMatchList( success , extendedInfo , matches );
            // Si no encuentro ninguna partida creo una
            if( matches.Count == 0 )
            {
                Crear();
            }

            // Si encuentro partida, me uno a una de ellas aleatoriamente
            else
            {
                int idx = Random.Range( 0 , matches.Count );

                Unirse( matches[idx].networkId );
            }
        }

        public override GameObject OnLobbyServerCreateGamePlayer( NetworkConnection conn , short playerControllerId )
        {
            transform.GetChild(0).gameObject.SetActive( false ); // Ocultamos la interfaz

            return base.OnLobbyServerCreateGamePlayer( conn , playerControllerId );
        }
    }
}
