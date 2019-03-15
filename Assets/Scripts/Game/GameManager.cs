using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Nexon
{
    using Nexon.Networking; 

    public class GameManager : NetworkBehaviour {
#region SyncVars
        private SyncListInt valoresIniciales = new SyncListInt();
        [SyncVar] private float tiempoRestantePartida = 180;
#endregion SyncVars

#region Fields
        [SerializeField] private Casilla casillaPrefab = null;

        [SerializeField] private Transform canvasPanel = null;

        [SerializeField] private GameObject panelBloqueo = null;
        [SerializeField] private GameObject panelCuentaAtras = null;
        [SerializeField] private GameObject panelVictoria = null;

        [SerializeField] private Button botonVolverAlMenu = null;

        [SerializeField] private ParticleSystem[] particulas = null;

        [SerializeField] private Image imagenTiempo = null, imagenTiempoTurno = null;

        [SerializeField] private Text textoTiempoPartida = null, textoTiempoTurno = null;
        [SerializeField] private Text textoPuntosJugador = null;
        [SerializeField] private Text textoPuntosEnemigo = null;
        [SerializeField] private Text textoVictoria = null;
        [SerializeField] private Text player1NameText = null;
        [SerializeField] private Text enemyNameText = null;

        [SerializeField] private GameObject[] fichasTurno = null, vidasDisponibles = null;

        [Tooltip( "Tiempo máximo de la partida en segundos." )]
        [SerializeField] private int tiempoMaximoPartida = 180;
        [Tooltip( "Tiempo máximo del turno en segundos." )]
        [SerializeField] private int tiempoMaximoTurno = 10;
        [Tooltip( "Tiempo que el jugador debe esperar antes de comenzar la partida." )]
        [SerializeField] private int tiempoPrePartida = 5;


        private const int lengthX = 6, lengthY = 6;
        private const int puntosPorExplosion = 100;

        private readonly Casilla[,] casillas = new Casilla[ lengthX , lengthY ];

        private int fichasPuestas = 0;
        private int rachaExplosiones = 0;
        private float tiempoTurno = 10f;
        private int turnosSinPuntuar = 0;
        private bool explotando = false;
        

        private static LinkedList<Jugador> jugador = new LinkedList<Jugador>();
        
        private SoundManager soundManager;
        private LanguageManager languageManager;
        #endregion Fields

        #region Properties

        public Jugador jugadorLocal {
            get {
                if( jugador.First == null )
                    return null;
                return jugador.First.Value;
            }
        }
        public Jugador jugadorRemoto {
            get {
                if( jugador.Last == null )
                    return null;
                return jugador.Last.Value;
            }
        }
        #endregion Properties

        #region Callbacks
        private IEnumerator Start()
        {
            soundManager = FindObjectOfType<SoundManager>();
            languageManager = FindObjectOfType<LanguageManager>();
            player1NameText.text = languageManager.ReturnLine( 4 );
            enemyNameText.text = languageManager.ReturnLine( 6 );

            GetComponent<Canvas>().worldCamera = Camera.main;
            ActualizarTiempo( tiempoMaximoPartida );

            //yield return new WaitForSeconds(1f); // new WaitUntil( () => jugador.Count == NetworkServer.connections.Count );

            ActualizarPuntuacion();
            ActualizarVidas();

            if( isServer )
            {
                ObtenerValoresIniciales(); // Solo debe hacerlo el servidor
            }

            yield return new WaitUntil( () => valoresIniciales.Count == (lengthX * lengthY) );

            CrearTablero();
        }
        #endregion Callbacks

        #region Private-Methods

        #region Start-Match
        private void ComenzarPartida()
        {
            fichasPuestas = 0;
            StartCoroutine( ComenzarCuentaAtras() );
        }

        [Server]
        private void ObtenerValoresIniciales()
        {
            // treses = 19% 7fichas
            // doses = 42% 15fichas
            // unos = 28% 10Fichas
            // ceros = 11% 4fichas

            Dictionary<int , int> cantidades = new Dictionary<int , int>() {
                {3, 7 },
                {2, 15 },
                {1, 10 },
                {0, 4 }
            };

            // TODO Obtener el mismo seed para 2 jugadores emparejados
            //Random.InitState( 42 );

            for( int i = 0 ; i < lengthX ; i++ )
            {
                for( int j = 0 ; j < lengthY ; j++ )
                {
                    List<int> keyList = new List<int>( cantidades.Keys );
                    int numeroCasilla = keyList[ Random.Range( 0 , keyList.Count ) ];

                    cantidades[numeroCasilla]--;
                    if( cantidades[numeroCasilla] == 0 ){
                        cantidades.Remove( numeroCasilla );
                    }
                    valoresIniciales.Add( numeroCasilla );
                }
            }
        }

        private void CrearTablero()
        {
            int k = 0;
            for( int i = 0 ; i < lengthX ; i++ )
            {
                for( int j = 0 ; j < lengthY ; j++ )
                {
                    casillas[i , j] = Instantiate( casillaPrefab , canvasPanel ); //casillas[i , j] = PhotonNetwork.Instantiate( "casillaPrefab" , Vector3.zero , Quaternion.identity , 0 );
                    //casillas[i , j].GetComponent<PhotonView>().RPC( "ChangeParams" , PhotonTargets.AllBuffered , i , j , new Vector2( posX , posY ) , numeroCasilla );
                    casillas[i , j].transform.SetParent( canvasPanel );
                    //casillas[i , j].transform.localScale = Vector3.one;
                    casillas[i , j].posicion = new Vector2Int( i , j );
                    casillas[i , j].Valor = valoresIniciales[k];
                    casillas[i , j].onClick += OnCasillaPulsada;
                    k++;
                }
            }
            ComenzarPartida();
        }
        #endregion Start-Match

        #region Logic
        public void ActualizarVidas()
        {
            if( jugadorLocal.Vidas < 3 && jugadorLocal.Vidas >= 0 && vidasDisponibles[jugadorLocal.Vidas].activeSelf )
            {
                vidasDisponibles[jugadorLocal.Vidas].SetActive( false );
                ParticleSystem partic = Instantiate( particulas[0] , null );
                partic.transform.position = vidasDisponibles[jugadorLocal.Vidas].transform.position;
                partic.Play();
                Destroy( partic.gameObject , 3f );
            }

            if( jugadorRemoto.Vidas < 3 && jugadorRemoto.Vidas >= 0 && vidasDisponibles[3+jugadorRemoto.Vidas].activeSelf )
            {
                vidasDisponibles[3+jugadorRemoto.Vidas].SetActive( false );
                ParticleSystem partic = Instantiate( particulas[1] , null );
                partic.transform.position = vidasDisponibles[3+jugadorRemoto.Vidas].transform.position;
                partic.Play();
                Destroy( partic.gameObject , 3f );
            }


            if( jugadorLocal.Vidas == 0 )
            {
                FinalizarPartida();
            }
        }

        public void ActualizarPuntuacion()
        {
            if ( 0 != string.Compare( textoPuntosJugador.text, jugadorLocal.Puntos.ToString() ) )
                particulas[0].Play();
            //if( 0 != string.Compare( textoPuntosEnemigo.text , jugadorRemoto.Puntos.ToString() ) )
            //    particulas[1].Play();

            textoPuntosJugador.text = jugadorLocal.Puntos.ToString();
            //if ( jugadorRemoto != null )
            //    textoPuntosEnemigo.text = jugadorRemoto.Puntos.ToString();
        }
        private void ActualizarTiempo(float tiempo)
        {
            imagenTiempo.fillAmount = tiempo / (float)tiempoMaximoPartida;
            imagenTiempoTurno.fillAmount = tiempoTurno / (float)tiempoMaximoTurno;
            textoTiempoPartida.text = string.Format( "{0}:{1:00}" , (int)tiempo / 60 , (int)tiempo % 60 );
            textoTiempoTurno.text = string.Format( "{0:00.0}" , tiempoTurno );
        }
        
        private void OnCasillaPulsada(Casilla casilla)
        {
            fichasPuestas++;

            fichasTurno[3 - fichasPuestas].SetActive( false );

            if( fichasPuestas >= 3 )
            {
                fichasPuestas = 0;
                //tiempoTurno = 10f;
                rachaExplosiones = 0;

                panelBloqueo.SetActive( true );

                soundManager.PlaySound( soundManager.sounds[0] );

                StartCoroutine( ComprobarExplosiones(casilla) );
            }
        }

        private IEnumerator ComenzarCuentaAtras()
        {
            #region PanelCuentaAtras
            panelCuentaAtras.SetActive( true );
            float preTime = Time.time + tiempoPrePartida;
            Text numero = panelCuentaAtras.transform.GetChild( 0 ).GetComponent<Text>();
            int tiempo = tiempoPrePartida-1;

            while( Time.time < preTime )
            {
                numero.text = tiempo.ToString();
                numero.gameObject.SetActive( true );
                yield return new WaitForSecondsRealtime(1);
                tiempo--;
            }
            panelCuentaAtras.SetActive( false );
            #endregion PanelCuentaAtras

            panelBloqueo.SetActive( false );
            float tiempoInicioCuentaAtras = Time.realtimeSinceStartup;
            if( isServer )
            {
                tiempoRestantePartida = tiempoMaximoPartida - (Time.realtimeSinceStartup - tiempoInicioCuentaAtras);
                jugadorLocal.Vidas = 3;
                jugadorRemoto.Vidas = 3;
            }

            fichasPuestas = 0;
            turnosSinPuntuar = 0;
            tiempoTurno = 10f;
            

            while( tiempoRestantePartida > 0)
            {
                if ( isServer )
                    tiempoRestantePartida = tiempoMaximoPartida - (Time.realtimeSinceStartup - tiempoInicioCuentaAtras);

                if( jugadorLocal.Vidas > 0 )
                {
                    if ( !explotando )
                        tiempoTurno -= Time.unscaledDeltaTime;
                
                    if( tiempoTurno < 0 )
                    {
                        tiempoTurno = 10f;
                    
                        // TODO Preguntar
                        /*turnosSinPuntuar++;
                        if( turnosSinPuntuar > 2 )
                        {
                            turnosSinPuntuar = 0;
                            jugadorLocal.Vidas--;
                        }*/

                        jugadorLocal.Vidas--;
                        fichasPuestas = 2;

                        fichasTurno[0].SetActive( false );
                        fichasTurno[1].SetActive( false );
                        fichasTurno[2].SetActive( false );

                        OnCasillaPulsada( casillas[0 , 0] );
                        //StartCoroutine( ComprobarExplosiones(casillas[0,0]) );
                    }
                }

                ActualizarTiempo( tiempoRestantePartida );
                yield return null;
            }
            
            panelBloqueo.SetActive( true );
            // Esperamos a que ambos jugadores hayan finalizado sus explosiones para mostrar el mensaje de victoria / derrota
            //yield return new WaitUntil( () => !jugadorLocal.Explotando && !jugadorRemoto.Explotando );

            FinalizarPartida();
        }

        /// <summary>
        /// Comprueba si al menos un vecino tiene valor superior a 3
        /// Hace explotar al primero que cumpla dicha condición
        /// El orden de búsqueda es Arriba, Izquierda, Abajo, Derecha, 
        /// </summary>
        private IEnumerator ComprobarExplosionVecinos( int x, int y )
        {
            int j = y - 1;
            // Casilla Superior
            if( j >= 0 && casillas[x , j].Valor >= 4 )
            {
                yield return ExplosionEnCoordendas( x , j );
                yield break;
            }

            int i = x - 1;
            // Casilla izquierda
            if( i >= 0 && casillas[i , y].Valor >= 4 )
            {
                yield return ExplosionEnCoordendas( i , y );
                yield break;
            }
        
            j = y + 1;
            // Casilla Inferior
            if( j < lengthY && casillas[x , j].Valor >= 4 )
            {
                yield return ExplosionEnCoordendas( x , j );
                yield break;
            }

            i = x + 1;
            // Casilla derecha
            if( i < lengthX && casillas[i , y].Valor >= 4 )
            {
                yield return ExplosionEnCoordendas( i , y );
                yield break;
            }
        }
        /// <summary>
        /// Realiza la explosión en la casilla correspondiente
        /// Tras realizar la explosión incrementa el valor de los vecinos
        /// Y Comprueba si debe explotar los vecinos con los nuevos valores
        /// </summary>
        private IEnumerator ExplosionEnCoordendas( int i , int j )
        {
            if( tiempoRestantePartida <= 0 )
                yield break; // En cuanto acabe el tiempo dejamos de explotar
            //if( jugadorLocal.Vidas <= 0 )
            //    yield break; // Si no tengo vidas, dejamos de explotar
            //if( jugadorRemoto.Vidas <= 0 )
            //    yield break; // Si el oponente no tiene vidas, dejamos de explotar

            // El efecto de partículas de romperse o cambiar de valor se hace en el componente Casilla
            // El sonido se hace aquí
            soundManager.PlaySound( soundManager.sounds[1] );
            // Sumamos los puntos derivados de esta explosion
            int bonus = SumarPuntos() - puntosPorExplosion;
            // Incrementamos la racha de explosiones
            rachaExplosiones++;
            // Incrementamos en 1 el valor de los vecinos de la casilla explotada
            IncrementarVecinos( casillas[i , j].posicion );
            // Ajustamos su valor ( si explotó siendo 4 -> 0, 5->1, 6->2, 7->3 )
            casillas[i , j].AjustarValor(bonus);
            // Esperamos medio segundo hasta la siguiente explosión
            yield return new WaitForSeconds( 0.5f );
            // Comprobamos si tenemos que explotar algún vecino
            yield return ComprobarExplosionVecinos( i , j );
        }

        /// <summary>
        /// Comprueba el tablero una y otra vez 
        /// hasta que no haya ninguna explosión
        /// o se haya acabado el tiempo de la ronda
        /// </summary>
        /// <param name="casilla">Ultima casilla pulsada</param>
        private IEnumerator ComprobarExplosiones(Casilla casilla)
        {
            bool explosion = false;
            bool algunaExplosion = false;

            explotando = true;
            // Comenzamos las explosiones en la última pulsada si su valor es superior a 3
            if( casilla != null && casilla.Valor >= 4 )
            {
                yield return ExplosionEnCoordendas( casilla.posicion.x , casilla.posicion.y );
                algunaExplosion = true;
            }

            // Continuamos con el resto de explosiones
            do
            {
                explosion = false;

                for( int i = 0 ; i < lengthX ; i++ )
                {
                    for( int j = 0 ; j < lengthY ; j++ )
                    {
                        if( casillas[i , j].Valor >= 4 )
                        {
                            yield return ExplosionEnCoordendas( i , j );
                            explosion = true;
                            algunaExplosion = true;
                        }
                    }
                }
            }
            while( explosion && tiempoRestantePartida > 0);

            panelBloqueo.SetActive( false );

            if( !algunaExplosion )
            {
                turnosSinPuntuar++;
                //if( turnosSinPuntuar > 2 ) Version inicial
                {
                    turnosSinPuntuar = 0;

                    jugadorLocal.Vidas--;
                }
            }

            for( int i = 0 ; i < 3 ; i++ )
                fichasTurno[i].SetActive( true );

            // Si no cortásemos las explosiones al finalizar el tiempo
            // Necesitaríamos esta variable para saber cuándo acaban ambos jugadores
            // Antes de mostrar el mensaje de win
            //jugadorLocal.Explotando = false;

            explotando = false;
            if( tiempoRestantePartida > 0 && jugadorLocal.Vidas > 0 )
                tiempoTurno = 10f;
        }

        /// <summary>
        /// Damos por Finalizada la Partida
        /// bloqueando el panel y mostrando el mensaje
        /// de victoria o derrota correspondiente
        /// </summary>
        private void FinalizarPartida()
        {
            panelBloqueo.SetActive( true ); // Bloqueamos la interacción con el tablero

            if ( tiempoRestantePartida <= 0 || jugador.Count == 1)
                StopAllCoroutines(); // Detenemos los contadores de tiempo y explosiones

            // Comprobamos el ganador / perdedor de la partida
            #region ComprobacionVictoriaDerrota
            if( jugadorLocal.Vidas == 0 )
            {
                textoVictoria.text = languageManager.ReturnLine( 16 ); // Derrota
            }
            else if( jugadorRemoto.Vidas == 0 )
            {
                textoVictoria.text = languageManager.ReturnLine( 17 ); // Victoria
            }
            else if( jugadorLocal.Puntos > jugadorRemoto.Puntos )
            {
                textoVictoria.text = languageManager.ReturnLine( 17 ); // Victoria
            }
            else if( jugadorLocal.Puntos < jugadorRemoto.Puntos )
            {
                textoVictoria.text = languageManager.ReturnLine( 16 ); // Derrota
            }
            else if( jugadorLocal.TiempoUltimaJugada < jugadorRemoto.TiempoUltimaJugada )
            {
                textoVictoria.text = languageManager.ReturnLine( 17 ); // Victoria
            }
            else if( jugadorLocal.TiempoUltimaJugada > jugadorRemoto.TiempoUltimaJugada )
            {
                textoVictoria.text = languageManager.ReturnLine( 16 ); // Derrota
            }
            else
            {
                if( isServer )
                    textoVictoria.text = languageManager.ReturnLine( 17 ); // Victoria
                else
                    textoVictoria.text = languageManager.ReturnLine( 16 ); // Derrota
            }
            #endregion ComprobacionVictoriaDerrota

            if ( tiempoRestantePartida > 0 )
                textoVictoria.text = languageManager.ReturnLine( 61 ); // Esperando Jugador
            // Mostramos el mensaje de Victora / Derrota
            panelVictoria.SetActive( true );

            // Solo cuando realmente acaba la partida para ambos se muestra el botón para volver.
            if( tiempoRestantePartida <= 0 || jugador.Count == 1)
            {
                botonVolverAlMenu.gameObject.SetActive( true );
            }
        }

        /// <summary>
        /// Suma los puntos y tiene en cuenta las rachas para sumar bonus
        /// </summary>
        private int SumarPuntos()
        {
            int puntosSumados = puntosPorExplosion;

            if( (rachaExplosiones % 5) == 0 )
            {
                if( rachaExplosiones == 0 )
                    puntosSumados += 150;
                else
                    puntosSumados += 50 * rachaExplosiones; // 250pts *(rachaExplosiones / 5);
            }
            jugadorLocal.Puntos += puntosSumados ;
            jugadorLocal.TiempoUltimaJugada = ( Time.time );
            return puntosSumados;
        }

        /// <summary>
        /// Incrementa en 1 el valor de las 4 casillas que rodean
        /// ortogonalmente a la celda en la posición <param name="posicion">posicion</param>
        /// (Vecindad de von Neumann)
        /// </summary>
        /// <param name="posicion"></param>
        private void IncrementarVecinos( Vector2Int posicion )
        {
            int i = posicion.x - 1;
            int j = posicion.y - 1;
            // Casilla Superior
            if( j >= 0 ) {
                casillas[posicion.x , j].Valor++;
            }

            // Casilla Inferior
            j = posicion.y + 1;
            if( j < lengthY ) {
                casillas[posicion.x , j].Valor++;
            }

            // Casilla izquierda
            i = posicion.x - 1;
            if( i >= 0 ) {
                casillas[i , posicion.y].Valor++;
            }
            
            // Casilla derecha
            i = posicion.x + 1;
            if( i < lengthX ) {
                casillas[i , posicion.y].Valor++;
            }
        }
        #endregion Logic

        #endregion Private-Methods

        #region Public-Methods
        public static void AddPlayer( Jugador j )
        {
            if( j.isLocalPlayer )
                jugador.AddFirst( j );
            else
                jugador.AddLast( j );
        }
        public void IrAlMenu()
        {
            jugador = new LinkedList<Jugador>();
            if( isServer )
                NetworkManager.singleton.matchMaker.DestroyMatch( NetworkManager.singleton.matchInfo.networkId , 0 , OnDestroyMatch );
            else
            {
                NetworkManager.singleton.StopClient();
                NetworkManager.singleton.StopMatchMaker();
                
                Destroy( NetworkManager.singleton.gameObject );
                UnityEngine.SceneManagement.SceneManager.LoadScene( 0 );
            }
        }

        // Volver a jugar una partida con el mismo adversario
        public void Rematch()
        {
            // TODO
        }
        #endregion Public-Methods

        #region OtrosCallbacks
        public void OnDestroyMatch( bool success , string extendedInfo )
        {
            NetworkManager.singleton.OnDestroyMatch( success , extendedInfo );
            if( isServer )
            {
                NetworkManager.singleton.StopMatchMaker();
                NetworkManager.singleton.StopHost();
            }
            Destroy( NetworkManager.singleton.gameObject );
            UnityEngine.SceneManagement.SceneManager.LoadScene( 0 );
        }
        #endregion
    }
}
