using System.Collections;
using UnityEngine;

namespace Nexon.Networking
{
    using UnityEngine.Networking;

    public class Jugador : NetworkBehaviour
    {
        #region Puntos
        [SyncVar( hook = "OnPuntosCambiados" )]
        private int puntos;
        public int Puntos {
            get { return puntos; }
            set {
                puntos = value; // Para que el cliente no tenga que esperar a que el servidor actualice los datos
                CmdActualizaPuntos(value);
            }
        }
        private void OnPuntosCambiados( int nuevoValor )
        {
            puntos = nuevoValor;
            if( gm != null )
                gm.ActualizarPuntuacion();
        }
        [Command]
        private void CmdActualizaPuntos( int nuevoValor )
        {
            puntos = nuevoValor;
        }
        #endregion Puntos

        #region TiempoUltimaJugada
        [SyncVar( hook = "OnTiempoCambiado")]
        private float tiempoUltimaJugada;
        public float TiempoUltimaJugada {
            get { return tiempoUltimaJugada; }
            set { CmdActualizaTiempo(value); }
        }
        // Hook
        private void OnTiempoCambiado( float nuevoValor )
        {
            tiempoUltimaJugada = nuevoValor;
        }

        [Command]
        private void CmdActualizaTiempo( float nuevoValor )
        {
            tiempoUltimaJugada = nuevoValor;
        }
        #endregion TiempoUltimaJugada

        #region Vidas
        [SyncVar( hook = "OnVidasCambiadas" )]
        private int vidas = 3;
        public int Vidas {
            get {
                return vidas;
            }
            set {
                CmdActualizaVidas( value );
            }
        }
        private void OnVidasCambiadas( int nuevoValor )
        {
            vidas = nuevoValor;
            if( gm != null )
                gm.ActualizarVidas();
        }
        [Command]
        private void CmdActualizaVidas( int nuevoValor )
        {
            vidas = nuevoValor;
            if( vidas < 0 )
                vidas = 0;
            if( vidas > 3 )
                vidas = 3;
        }
        #endregion Vidas
        /*
        // Mientras uno de los dos jugadores esté explotando sus fichas
        // no se mostrará el mensaje de fin de partida, aunque el tiempo
        // haya concluido.
        [SyncVar( hook = "OnExplotando" )]
        private bool explotando;

        public bool Explotando {
            get { return explotando; }
            set { CmdActualizarEstadoExplotando(value); }
        }
        private void OnExplotando( bool nuevoValor )
        {
            explotando = nuevoValor;
        }
        [Command]
        private void CmdActualizarEstadoExplotando( bool nuevoValor )
        {
            explotando = nuevoValor;
        }
        */

        private GameManager gm;

        private IEnumerator Start()
        {
            GameManager.AddPlayer( this );

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            gm = FindObjectOfType<GameManager>();

        }
    }
}