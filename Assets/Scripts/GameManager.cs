using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    #region Singleton
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    #endregion

    public string GameVersion = "0.0.1"; // Version del juego para que coincidan todos en la misma

    public PhotonView pView; //Componente PhotonView para hacer llamadas del tipo RPC por la red

    public GameObject[,] casillas = new GameObject[6,6]; // Array de 2º Grado para el tablero
    public Casilla[,] casillasScripts = new Casilla[6, 6];

    public GameObject casillaPrefab; // Prefab de cada casilla

    public GameObject[] fichasUsables;

    public Transform canvasPanel; // Canvas principal

    public GameObject winnerGO; // GameObject del panel de Fin de partida
    bool endGame = false; // Bool para check si ha acabado la partida en las corrutinas


    public delegate void DelegadoPeso(int i);

    public DelegadoPeso delegadoPeso;

    //Textos varios
    public Text turnoText;
    public Text PlayerPointsText;
    public Text IAPointsText;
    public Text winnerText;
    public Text turnTimerText;
    public Text LimitText;
	public Text player1NameText;
	public Text enemyNameText;

    public bool MiTurno = true;
    public bool OnlineGame = false;
    public bool exploding;

    public int player1Points = 0;
    public int player2Points = 0;
    public int IAPoints = 0;

    int NumJugadores = 0;

    public GameObject nonTouch; //GameObject invisible para no permitir poner fichas cuando no es su turno

    int fichasPuestas = 0;
    int totalFichas = 0;

    int fichasMaximas = 0;
    int puntosMaximos = 0;

    GameType gameType;

    DificultadIA dificultadIA;

	public Color[] colorNUM;
	public GameObject[] particles;//0 explosion / 1 tap / 2 points update 0 / 3 points update 
	public GameObject blackBG;
	public Image timeBar;

	LanguageManager LM;
	SoundManager SM;
	
    //DELETE FOR TEST
    public GameObject[] casillastest = new GameObject[3];

    private bool fromGameToMenu = false;

    private void Start()
    {
		LM = LanguageManager.instance;
		SM = SoundManager.instance;
		SetOnlineGame();
		Connect();
        SetGameType();
        SetMaxLimits();
        ShowPoints(false);

        SceneManager.sceneLoaded += CheckSceneLoaded;
    }

    #region Network

    void Connect() // funcion para conectar a la Network
    {
        if (PlayerPrefs.GetInt("Online") == 0) // If para ver si ejecutamos en Offline
        {
            NumJugadores = 1;
            PhotonNetwork.offlineMode = true; // Ejecutamos el OfflineMode
            OnJoinedLobby(); // Conectamos directamente a una Room sin pasar por la Network
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings(GameVersion + PlayerPrefs.GetInt( "GameType" ) + PlayerPrefs.GetInt("PuntosLimit"));
            //PhotonNetwork.ConnectUsingSettings(PlayerPrefs.GetString("MatchId"));
            Debug.LogWarning("MatchName: " + PlayerPrefs.GetString("MatchName"));
            //PlayerPrefs.DeleteKey("MatchID");
             // Conectamos usando los settings Default del Usuario
        }
    }

    void OnConnectedToMaster() // Funcion para saber si hemos conectado al Master
    {
        Debug.LogWarning( "Nos conectamos al master" );
        PhotonNetwork.JoinLobby(); // Nos añadimos a la Lobby
    }

    /*void OnGUI() // para que aparezca informacion en la pantalla de la conexion
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString()); // Informacion de la conexion 
    }*/

    void OnJoinedLobby() // Si hemos conectado al Lobby -->
    {
        Debug.Log("OnJoinedLobby"); // Debug.Log para saber cuando se conecta al Lobby
                                    //PhotonNetwork.JoinRandomRoom(); // Conecta a una Room    

        if( !PhotonNetwork.connected )
            return;
        RoomOptions roomOptions = new RoomOptions() {
            CleanupCacheOnLeave = true,
            IsOpen = true,
            MaxPlayers = 2,
            PublishUserId = true
        };
        PhotonNetwork.JoinOrCreateRoom( PlayerPrefs.GetString( "MatchID" ) , roomOptions , TypedLobby.Default );
        Debug.LogWarning( "MatchID: " + PlayerPrefs.GetString( "MatchID" ) );
    }

    void OnPhotonJoinRoomFailed()
    {
        Debug.Log( "OnPhotonJoinRoomFailed" );
    }
    void OnPhotonRandomJoinFailed() // Si falla a conectar a una Room aleatoria
    {
        Debug.Log("OnPhotonRandomJoinFailed"); // Debug.Log para saber cuando falla en entrar a una Room
        PhotonNetwork.CreateRoom(null); // Creamos una Room puesto que no existe ninguna       
    }

    void OnJoinedRoom() // Cuando conecte a una Room
    {
        Debug.Log("OnJoinedRoom"); // Debug.Log para saber cuando conecta a una Room
        GetNumPlayer();      
    }
	[PunRPC]
    private void Disconnect()
    {
        PhotonNetwork.Disconnect();
		if(PlayerPrefs.GetInt("Online") == 1)
		{
			PhotonNetwork.room.IsVisible = false;
		}
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            stream.Serialize(ref pos);
        }
        else
        {
            Vector3 pos = Vector3.zero;
            stream.Serialize(ref pos);  // pos gets filled-in. must be used somewhere
        }
    }
    #endregion
    #region StartingGame
    public void SetFichasUsables()
    {
        if (MiTurno)
        {
            int length = fichasUsables.Length;
            for (int i = 0; i < length; i++)
            {
                fichasUsables[i].SetActive(true);
            }
        }
		else
		{
			int length = fichasUsables.Length;
			for (int i = 0; i < length; i++)
			{
				fichasUsables[i].SetActive(false);
			}
		}
    }
    
    [PunRPC]
    public void StartMatch()
    {
        if (PhotonNetwork.player.ID == 1)
        {
            MiTurno = true;
            nonTouch.SetActive(false);
            fichasPuestas = 0;
            SetFichasUsables();
            turnoText.text = LM.ReturnLine(9);
            StartCoroutine("TurnTime");
			StartCoroutine("BarTime");
        }
        else if (PhotonNetwork.player.ID == 2)
        {
            MiTurno = false;
            nonTouch.SetActive(true);
            turnoText.text = LM.ReturnLine(8);
        }
    }
    public void GetNumPlayer()
    {
        Debug.Log("Player ID: " + PhotonNetwork.player.ID);
        switch (PhotonNetwork.player.ID)
        {
            case 1:
                //soy player 1
                CreateTable();
                break;
            case 2:
                //soy player 2
                pView.RPC("StartMatch", PhotonTargets.All);
                PhotonNetwork.room.IsVisible = false;
                break;
            default:
                Disconnect();
                break;
        }
        if (PhotonNetwork.player.ID != 1)
        {
            turnTimerText.text = "";
            MiTurno = false;
            nonTouch.SetActive(true);
            turnoText.text = LM.ReturnLine(8);
            fichasPuestas = 0;
        }
        GetComponent<PhotonView>().RPC("LastPlayerArrived", PhotonTargets.All, PhotonNetwork.player.ID);
    }
    private void CreateTable()
    {
        MiTurno = true;

        int lengthX = casillas.GetLength(0);
        int lengthY = casillas.GetLength(1);

        float posX = -330;
        float posY = 450;

        // rojas = 20% 7fichas
        // verdes = 40% 15fichas
        // azules = 30% 10Fichas
        // vacias = 10% 4fichas

        int rojas = 7;
        int verdes = 15;
        int azules = 10;
        int vacias = 4;

        for (int i = 0; i < lengthX; i++)
        {
            posY = 450;
            for (int j = 0; j < lengthY; j++)
            {
                int rand = Random.Range(0, 4);
                int coinsDef = 0;
                switch (rand)
                {
                    case 0:
                        if (vacias <= 0)
                        {
                            goto case 1;
                        }
                        coinsDef = 0;
                        vacias--;

                        break;
                    case 1:
                        if (azules <= 0)
                        {
                            goto case 2;
                        }
                        coinsDef = 1;
                        azules--;

                        break;
                    case 2:
                        if (verdes <= 0)
                        {
                            goto case 3;
                        }
                        coinsDef = 2;
                        verdes--;

                        break;
                    case 3:
                        if (rojas <= 0)
                        {
                            goto case 0;
                        }
                        coinsDef = 3;
                        rojas--;

                        break;
                    case 4:
                        goto case 0;

                }
                casillas[i, j] = PhotonNetwork.Instantiate("casillaPrefab", Vector3.zero,Quaternion.identity,0);
				casillas[i, j].GetComponent<PhotonView>().RPC("ChangeParams", PhotonTargets.AllBuffered, i, j, new Vector2(posX, posY),coinsDef);
				casillas[i, j].transform.SetParent(canvasPanel);
				casillas[i, j].transform.localScale= Vector3.one;
                casillasScripts[i, j] = casillas[i, j].GetComponent<Casilla>();
                casillasScripts[i,j].AddPosition(i, j);
                posY += -100;
            }
            posX += 100;
        }
        if (OnlineGame == false)
        {
            int rand = Random.Range(0, 2);
            if (rand == 2)
            {
                rand = 0;
            }
            switch (rand)
            {
                case 0:
                    MiTurno = true;
                    nonTouch.SetActive(false);
                    fichasPuestas = 0;
                    SetFichasUsables();
                    turnoText.text = LM.ReturnLine(9);
                    turnoText.gameObject.SetActive(true);
                    StartCoroutine("TurnTime");
                    StartCoroutine("BarTime");
                    break;
                case 1:
                    StopCoroutine("TurnTime");
                    StartCoroutine("BarTime");
                    timeBar.fillAmount = 1;
                    MiTurno = false;
                    turnTimerText.text = "";
                    nonTouch.SetActive(true);
                    fichasPuestas = 0;
                    turnoText.text = LM.ReturnLine(7);
                    turnoText.gameObject.SetActive(true);
                    StartCoroutine("IA");
                    break;
                default:
                    break;
            }
		}
    }
    void SetOnlineGame()
    {
        switch (PlayerPrefs.GetInt("Online"))
        {
            case 0:
                OnlineGame = false;
                PhotonNetwork.offlineMode = OnlineGame;
                SetDificultad();
                break;

            case 1:
                OnlineGame = true;
                break;
            default:
                break;
        }
    }
    void SetGameType()
        {
            switch (PlayerPrefs.GetInt("GameType"))
            {
                case 0:
                    gameType = GameType.Puntuacion;
                    break;
                case 1:
                    gameType = GameType.Fichas;
                    break;
                default:
                    gameType = GameType.Puntuacion;
                    break;
            }
        }
    void SetMaxLimits()
    {
        switch (gameType)
        {
            case GameType.Puntuacion:
                puntosMaximos = PlayerPrefs.GetInt("PuntosLimit");
                LimitText.text = LM.ReturnLine(3)+ " " + puntosMaximos + "p";
                break;
            case GameType.Fichas:
                fichasMaximas = PlayerPrefs.GetInt("FichasLimit");
                LimitText.text = LM.ReturnLine(13)+ " " + fichasMaximas + LM.ReturnLine(2);
                break;
            default:
                break;
        }
    }
    void UpdateLimitText()
    {
        switch (gameType)
        {
            case GameType.Puntuacion:
                break;
            case GameType.Fichas:
                LimitText.text = LM.ReturnLine(14)+ " " + fichasMaximas + LM.ReturnLine(2);
                break;
            default:
                break;
        }
    }
    #endregion
    #region IA
    void ResetChecked()
    {
        int lengthX = casillas.GetLength(0);
        int lengthY = casillas.GetLength(1);

        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                casillasScripts[i, j].hasChecked = false;
            }
        }
    }
    GameObject IAHard()
    {
        GameObject maxNumber = new GameObject();

        int lengthX = casillas.GetLength(0);
        int lengthY = casillas.GetLength(1);

        //Check max number


        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                if (casillas[i, j].GetComponent<Casilla>().value == 2)
                {
                    casillas[i, j].GetComponent<Casilla>().hasChecked = true;
                    casillas[i, j].GetComponent<Casilla>().SetPeso(IACheckColindants(new Vector2(i, j)));
                    ResetChecked();
                    if (fichasPuestas <= 1)
                    {
                        if (maxNumber.GetComponent<Casilla>() == false)
                        {
                            maxNumber = casillas[i, j];
                        }
                        else if (maxNumber.GetComponent<Casilla>().GetPeso() < casillas[i,j].GetComponent<Casilla>().GetPeso())
                        {
                            maxNumber = casillas[i, j];
                        }
                    }
                }
                else if(casillas[i, j].GetComponent<Casilla>().value == 3 && fichasPuestas == 2)
                {                    
                    casillas[i, j].GetComponent<Casilla>().SetPeso(1 + IACheckColindants(new Vector2(i, j)));
                    ResetChecked();
                    if (maxNumber.GetComponent<Casilla>() == false)
                    {
                        maxNumber = casillas[i, j];
                    }
                    else if (maxNumber.GetComponent<Casilla>().GetPeso() < casillas[i, j].GetComponent<Casilla>().GetPeso())
                    {
                        maxNumber = casillas[i, j];
                    }
                }               
                else if (casillas[i,j].GetComponent<Casilla>().value <= 1)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (maxNumber.GetComponent<Casilla>() == false)
                        {
                            maxNumber = casillas[i, j];
                        }
                    }
                }
            }
        }
        return maxNumber;
    }
    GameObject IAMid()
    {
        GameObject maxNumber = new GameObject();

        int lengthX = casillas.GetLength(0);
        int lengthY = casillas.GetLength(1);

        //Check max number


        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                if (casillas[i, j].GetComponent<Casilla>().value == 2)
                {
                    casillas[i, j].GetComponent<Casilla>().hasChecked = true;
                    casillas[i, j].GetComponent<Casilla>().SetPeso(IACheckColindants(new Vector2(i, j)));
                    ResetChecked();
                    if (fichasPuestas <= 1)
                    {
                        if (maxNumber.GetComponent<Casilla>() == false)
                        {
                            maxNumber = casillas[i, j];
                        }
                        else if (maxNumber.GetComponent<Casilla>().GetPeso() < casillas[i, j].GetComponent<Casilla>().GetPeso())
                        {
                            maxNumber = casillas[i, j];
                        }
                    }
                }
                else if (casillas[i, j].GetComponent<Casilla>().value == 3)
                {
                    casillas[i, j].GetComponent<Casilla>().SetPeso(1 + IACheckColindants(new Vector2(i, j)));
                    ResetChecked();
                    if (maxNumber.GetComponent<Casilla>() == false)
                    {
                        maxNumber = casillas[i, j];
                    }
                    else if (maxNumber.GetComponent<Casilla>().GetPeso() < casillas[i, j].GetComponent<Casilla>().GetPeso())
                    {
                        maxNumber = casillas[i, j];
                    }
                }
                else if (casillas[i, j].GetComponent<Casilla>().value <= 1)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (maxNumber.GetComponent<Casilla>() == false)
                        {
                            maxNumber = casillas[i, j];
                        }
                    }
                }
            }
        }
        return maxNumber;
    }
    GameObject IAEasy()
    {
        GameObject maxNumber = casillas[1, 1];

        int lengthX = casillas.GetLength(0);
        int lengthY = casillas.GetLength(1);

        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                if (casillas[i, j].GetComponent<Casilla>().value == 3)
                {
                    if (casillas[i, j].GetComponent<Casilla>().value > maxNumber.GetComponent<Casilla>().value)
                    {
                        maxNumber = casillas[i, j];
                    }
                }
                else if (casillas[i, j].GetComponent<Casilla>().value >= 2 && fichasPuestas <= 2)
                {
                    if (casillas[i, j].GetComponent<Casilla>().value > maxNumber.GetComponent<Casilla>().value)
                    {
                        int rand = Random.Range(0, 4);
                        if (rand == 0)
                        {
                            maxNumber = casillas[i, j];
                        }
                    }
                }
                else if (casillas[i, j].GetComponent<Casilla>().value >= 1 && fichasPuestas <= 0)
                {
                    if (casillas[i, j].GetComponent<Casilla>().value > maxNumber.GetComponent<Casilla>().value)
                    {
                        int rand = Random.Range(0, 4);
                        if (rand == 0)
                        {
                            maxNumber = casillas[i, j];
                        }
                    }
                }
                else if (casillas[i, j].GetComponent<Casilla>().value <= 1)
                {
                    if (casillas[i, j].GetComponent<Casilla>().value >= maxNumber.GetComponent<Casilla>().value)
                    {
                        int rand = Random.Range(0, 2);
                        if (rand == 0)
                        {
                            maxNumber = casillas[i, j];
                        }
                    }
                }
            }
        }
        return maxNumber;
    }
    public int IACheckColindants(Vector2 posicion)
    {

        int posicionX = (int)posicion.x;
        int posicionY = (int)posicion.y;

        int peso = 0;

        if (posicionX - 1 >= 0)
        {
            if (casillas[posicionX - 1, posicionY].GetComponent<Casilla>().value == 3 && casillas[posicionX - 1, posicionY].GetComponent<Casilla>().hasChecked == false)
            {
                peso++;
                casillas[posicionX - 1, posicionY].GetComponent<Casilla>().hasChecked = true;
                peso += IACheckColindants(new Vector2(posicionX - 1, posicionY));
            }
            else if (casillas[posicionX - 1, posicionY].GetComponent<Casilla>().value == 4)
            {
                return 0;
            }
        }
        if (posicionX + 1 <= 5)
        {
            if (casillas[posicionX + 1, posicionY].GetComponent<Casilla>().value == 3 && casillas[posicionX + 1, posicionY].GetComponent<Casilla>().hasChecked == false)
            {
                peso++;
                casillas[posicionX + 1, posicionY].GetComponent<Casilla>().hasChecked = true;
                peso += IACheckColindants(new Vector2(posicionX + 1, posicionY));
            }
            else if (casillas[posicionX + 1, posicionY].GetComponent<Casilla>().value == 4)
            {
                return 0;
            }
        }
        if (posicionY - 1 >= 0)
        {
            if (casillas[posicionX, posicionY - 1].GetComponent<Casilla>().value == 3 && casillas[posicionX, posicionY - 1].GetComponent<Casilla>().hasChecked == false)
            {
                peso++;
                casillas[posicionX, posicionY - 1].GetComponent<Casilla>().hasChecked = true;
                peso += IACheckColindants(new Vector2(posicionX, posicionY - 1));
            }
            else if (casillas[posicionX, posicionY - 1].GetComponent<Casilla>().value == 4)
            {
                return 0;
            }
        }
        if (posicionY + 1 <= 5)
        {
            if (casillas[posicionX, posicionY + 1].GetComponent<Casilla>().value == 3 && casillas[posicionX, posicionY + 1].GetComponent<Casilla>().hasChecked == false)
            {
                peso++;
                casillas[posicionX, posicionY + 1].GetComponent<Casilla>().hasChecked = true;
                peso += IACheckColindants(new Vector2(posicionX, posicionY + 1));
            }
            else if (casillas[posicionX, posicionY + 1].GetComponent<Casilla>().value == 4)
            {
                return 0;
            }
        }
        return peso;

    }
    public int IACheckColindantsSec(Vector2 posicion)
    {

        int posicionX = (int)posicion.x;
        int posicionY = (int)posicion.y;

        int peso = 0;

        if (posicionX - 1 >= 0)
        {
            if (casillas[posicionX - 1, posicionY].GetComponent<Casilla>().value == 3)
            {
                peso++;
            }
            else if (casillas[posicionX - 1, posicionY].GetComponent<Casilla>().value == 4)
            {
                return 0;
            }
        }
        if (posicionX + 1 <= 5)
        {
            if (casillas[posicionX + 1, posicionY].GetComponent<Casilla>().value == 3)
            {
                peso++;
            }
            else if (casillas[posicionX + 1, posicionY].GetComponent<Casilla>().value == 4)
            {
                return 0;
            }
        }
        if (posicionY - 1 >= 0)
        {
            if (casillas[posicionX, posicionY - 1].GetComponent<Casilla>().value == 3)
            {
                peso++;
            }
            else if (casillas[posicionX, posicionY - 1].GetComponent<Casilla>().value == 4)
            {
                return 0;
            }
        }
        if (posicionY + 1 <= 5)
        {
            if (casillas[posicionX, posicionY + 1].GetComponent<Casilla>().value == 3)
            {
                peso++;
            }
            else if (casillas[posicionX, posicionY + 1].GetComponent<Casilla>().value == 4)
            {
                return 0;
            }
        }
        return peso;

    }
    void SetDificultad()
    {
        switch (PlayerPrefs.GetInt("DificultadIA"))
        {
            case 1:
            case 2:
            case 3:
                dificultadIA = DificultadIA.Easy;
                break;
            case 4:
            case 5:
            case 6:
            case 7:
                dificultadIA = DificultadIA.Mid;
                break;
            case 8:
            case 9:
            case 10:
                dificultadIA = DificultadIA.Hard;
                break;
            default:
                dificultadIA = DificultadIA.Easy;
                break;
        }
    }

    IEnumerator IA()
    {
        switch (dificultadIA)
        {
            case DificultadIA.Easy:
                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForSeconds(2);
                    MiTurno = false;
                    GameObject g = IAEasy();
                    g.GetComponent<Casilla>().AddCoin();
                    NewCoinPlayed();
                    Debug.Log("IA placed coin");
                }
                break;
            case DificultadIA.Mid:
                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForSeconds(2);
                    MiTurno = false;
                    GameObject g = IAMid();
                    g.GetComponent<Casilla>().AddCoin();
                    NewCoinPlayed();
                    Debug.Log("IA placed coin");
                }
                break;
            case DificultadIA.Hard:
                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForSeconds(2);
                    delegadoPeso(0);
                    GameObject gG = IAHard();
                    MiTurno = false;
                    gG.GetComponent<Casilla>().AddCoin();
                    NewCoinPlayed();
                    Debug.Log("IA placed coin");
                }
                break;
            default:
                break;
        }
    }
    #endregion
    #region NetworkEnemy 

    [PunRPC]
    public void ChangeTurn(int playerID)
    {
        Debug.Log("Change Turn " + playerID);
        if (playerID != PhotonNetwork.player.ID)
        {
            turnoText.text = LM.ReturnLine(9);
			turnoText.gameObject.SetActive(true);
            MiTurno = true;
            SetFichasUsables();
            fichasPuestas = 0;
            nonTouch.SetActive(false);
            StartCoroutine("TurnTime");
			StartCoroutine("BarTime");

		}
        else
        {
            StopCoroutine("TurnTime");
			StartCoroutine("BarTime");
			turnTimerText.text = "";
            MiTurno = false;
            nonTouch.SetActive(true);
            turnoText.text = LM.ReturnLine(8);
			turnoText.gameObject.SetActive(true);
			fichasPuestas = 0;
        }
    }

    [PunRPC]
    public void LastPlayerArrived(int playerID)
    {
        if (PhotonNetwork.player.ID == 1 && playerID == 2)
        {
            GetComponent<PhotonView>().RPC("ChangeTurn", PhotonTargets.All, 2);
        }
    }

    public void CheckStartMatch()
    {

    }

    #endregion
    #region Points
    void SetLimits()
    {
        switch (gameType)
        {
            case GameType.Puntuacion:
                puntosMaximos = PlayerPrefs.GetInt("PuntosMaximos");
                break;
            case GameType.Fichas:
                fichasMaximas = PlayerPrefs.GetInt("FichasMaximas");
                break;
            default:
                break;
        }
    }
    [PunRPC]
    public void AddPoints()
    {
        Debug.Log( "Adding Points" );
        if (OnlineGame == false)
        {
            if (MiTurno == true)
            {
                player1Points++;
                ShowPoints(true);
                if (player1Points >= puntosMaximos && gameType == GameType.Puntuacion)
                {
                    endGame = true;
                    StopAllCoroutines();
                    EndGame(LM.ReturnLine(4));
                }
            }
            else
            {
                IAPoints++;
                ShowPoints(true);
                if (IAPoints >= puntosMaximos && gameType == GameType.Puntuacion)
                {
                    endGame = true;
                    StopAllCoroutines();
                    EndGame(LM.ReturnLine(5));
                }
            }
        }
        else if (OnlineGame == true)
        {
            if (MiTurno == true && PhotonNetwork.player.ID == 1)
            {
                player1Points++;
                ShowPoints(true);
                if (player1Points >= puntosMaximos && gameType == GameType.Puntuacion)
                {
                    endGame = true;
                    StopAllCoroutines();
                    Estadisticas.SumaPartida( true );
                    EndGame(LM.ReturnLine(4)); //JUGADOR
                }
            }
            else if(MiTurno == false && PhotonNetwork.player.ID == 1)
            {
                IAPoints++;
                ShowPoints(true);
                if (IAPoints >= puntosMaximos && gameType == GameType.Puntuacion)
                {
                    endGame = true;
                    StopAllCoroutines();
                    Estadisticas.SumaPartida( false );
                    EndGame(LM.ReturnLine(6)); //ENEMIGO
                }
            }else if (MiTurno == true && PhotonNetwork.player.ID == 2)
            {
                player1Points++;
                ShowPoints(true);
                if (player1Points >= puntosMaximos && gameType == GameType.Puntuacion)
                {
                    endGame = true;
                    StopAllCoroutines();
                    Estadisticas.SumaPartida( true );
                    EndGame(LM.ReturnLine(4)); //JUGADOR
                }
            }
            else if (MiTurno == false && PhotonNetwork.player.ID == 2)
            {
                IAPoints++;
                ShowPoints(true);
                if (IAPoints >= puntosMaximos && gameType == GameType.Puntuacion)
                {
                    endGame = true;
                    StopAllCoroutines();
                    Estadisticas.SumaPartida( false );
                    EndGame(LM.ReturnLine(6)); //ENEMIGO
                }
            }
        }
        
    }
    void ShowPoints(bool b)
    {
		if (MiTurno && b)
		{
			particles[2].SetActive(false);
			particles[2].SetActive(true);
		}
		else if(MiTurno == false && b)
		{
			particles[3].SetActive(false);
			particles[3].SetActive(true);
		}
		player1NameText.text = LM.ReturnLine(4);
        PlayerPointsText.text = /*"Player: "*/ + player1Points + "p";
        if (OnlineGame == false)
        {
			enemyNameText.text = LM.ReturnLine(5);
            IAPointsText.text = /*"IA: "*/ + IAPoints + "p";
        }
        else
        {
			enemyNameText.text = LM.ReturnLine(6);
            IAPointsText.text = /*"Enemy: "*/ + IAPoints + "p";
        }        
    }
    #endregion
    #region Coins
    [PunRPC]
    public void LessFichasUsables()
    {
        Debug.Log("LessFichasUsables");
        if (MiTurno)
        {
            Debug.Log("LessFichasUsables - MiTurno");
            int length = fichasUsables.Length;
            for (int i = 0; i < length; i++)
            {
                Debug.Log("LessFichasUsables - for");
                if (fichasUsables[i].GetActive() == true)
                {
                    Debug.Log("LessFichasUsables - SetActive");
                    fichasUsables[i].SetActive(false);
                    break;
                }
            }
            Debug.Log("LessFichasUsables - End For");
        }
    }
	[PunRPC]
    public void NewCoinPlayed()
    {
        fichasPuestas++;
        LessFichasUsables();
        fichasMaximas--;
        UpdateLimitText();
        if (gameType == GameType.Fichas)
        {
            if (fichasMaximas <= 0)
            {

                if (player1Points > IAPoints)
                {
                    StopAllCoroutines();
                    if( OnlineGame )
                    {
                        Estadisticas.SumaPartida( true );
                    }
                    EndGame(LM.ReturnLine(4));//Jugador
                }
                else if(IAPoints > player1Points)
                {
                    StopAllCoroutines();
                    if( OnlineGame )
                        Estadisticas.SumaPartida( false );
                    EndGame(LM.ReturnLine(5)); //IA
                }
                else
                {
                    StopAllCoroutines();
                    if( OnlineGame )
                        Estadisticas.SumaPartida( false );
                    EndGame("Empate");
                }
            }
        }
        Debug.Log("Fichas puestas: " + fichasPuestas);
        if (fichasPuestas>=3 && endGame == false && exploding == false)
        {
            fichasPuestas = 0;
            StopCoroutine("TurnTime");
            StopCoroutine("IA");
            turnTimerText.text = LM.ReturnLine(15);
            nonTouch.SetActive(true);
            //pView.RPC("NetCheckExplosions", PhotonTargets.All);
            NetCheckExplosions();
        }
    }

    [PunRPC]
    public void NetCheckExplosions()
    {
        if (OnlineGame == false)
        {
            StartCoroutine("CheckExplosionsCoroutine");
        }
        else if (PhotonNetwork.player.ID == 1)
        {
            StartCoroutine("CheckExplosionsCoroutine");
        }       
    }
    [PunRPC]
    public void UpdateRachaExplosiones(int rachaExplosiones)
    {
        Debug.LogWarning( "He recibido una racha de " + rachaExplosiones );
        if( MiTurno )
            Estadisticas.MejorRacha( rachaExplosiones );
    }
    [PunRPC]
    public void UpdatePuntosPartida( int puntos )
    {
        Estadisticas.SumaPuntos( puntos );
    }
    [PunRPC]
    public void UpdateResultadoPartida(bool ganada )
    {
        Estadisticas.SumaPartida( ganada );
    }
    IEnumerator CheckExplosionsCoroutine()
    {
        exploding = true;
        bool checking = true;
        int rachaExplosiones = -1;
        while (checking)
        {
            checking = CheckExplosions();
            rachaExplosiones++;
            yield return new WaitForSeconds(0.5f);
        }


        Debug.LogWarning( "Hubo un total de " + rachaExplosiones + " explosiones. Tu modo es "+(OnlineGame?"Online":"Offline")+ " y es el turno "+(MiTurno?"tuyo":"del oponente") );

        if (OnlineGame == false)
        {
            EndOfTurn();
        }
        else if (OnlineGame == true)
        {
            pView.RPC( "UpdateRachaExplosiones" , PhotonTargets.All , rachaExplosiones );
            pView.RPC( "EndOfTurn", PhotonTargets.All);
        }
    }

    public bool CheckExplosions()
    {
        int lengthX = casillas.GetLength(0);
        int lengthY = casillas.GetLength(1);
        for (int i = 0; i < lengthX; i++)
        {
            for (int j = 0; j < lengthY; j++)
            {
                if (casillas[i, j].GetComponent<Casilla>().value == 4)
                {
					SM.PlaySound(SM.sounds[1]);
					if (OnlineGame == false)
                    {
                        AddCoinNext(new Vector2(i,j));
                        casillas[i, j].GetComponent<Casilla>().UpdateValue(0);
                    }
                    else
                    {
                        AddCoinNext(new Vector2(i, j));
                        casillas[i, j].GetComponent<PhotonView>().RPC("UpdateValue", PhotonTargets.All, 0);
                        //pView.RPC("AddCoinNext", PhotonTargets.All, new Vector2(i, j));
                    }
                    
                    return true;
                }
                else if (casillas[i, j].GetComponent<Casilla>().value == 5)
                {
                    if (OnlineGame == false)
                    {
                        AddCoinNext(new Vector2(i, j));
                        casillas[i, j].GetComponent<Casilla>().UpdateValue(1);
                    }
                    else
                    {
                        AddCoinNext(new Vector2(i, j));
                        casillas[i, j].GetComponent<PhotonView>().RPC("UpdateValue", PhotonTargets.All, 1);
                        //pView.RPC("AddCoinNext", PhotonTargets.All, new Vector2(i, j));
                    }
                    
                    return true;
                }
                else if (casillas[i, j].GetComponent<Casilla>().value == 6)
                {
                    if (OnlineGame == false)
                    {
                        AddCoinNext(new Vector2(i, j));
                        casillas[i, j].GetComponent<Casilla>().UpdateValue(2);
                    }
                    else
                    {
                        AddCoinNext(new Vector2(i, j));
                        casillas[i, j].GetComponent<PhotonView>().RPC("UpdateValue", PhotonTargets.All, 2);
                        //pView.RPC("AddCoinNext", PhotonTargets.All, new Vector2(i, j));
                    }
                    
                    return true;
                }

            }
        }
        return false;
    }
    [PunRPC]
    public void AddCoinNext(Vector2 posicion)
    {
        if (OnlineGame == false)
        {
            AddPoints();
        }
		else
        {
            pView.RPC("AddPoints", PhotonTargets.All);
        }
        
        int posicionX = (int)posicion.x;
        int posicionY = (int)posicion.y;
        Debug.Log("Casilla explotada: " + posicionX + "," + posicionY);
        if (posicionX - 1 >= 0)
        {
            if (casillas[posicionX - 1, posicionY] != null)
            {
                if (OnlineGame == true)
                {
                    casillas[posicionX - 1, posicionY].GetComponent<PhotonView>().RPC("AddCoin", PhotonTargets.All);
                }
                else
                {
                    casillas[posicionX - 1, posicionY].GetComponent<Casilla>().AddCoin();
                }

            }
        }
        if (posicionX + 1 <= 5)
        {
            if (casillas[posicionX + 1, posicionY] != null)
            {
                if (OnlineGame == true)
                {
                    casillas[posicionX + 1, posicionY].GetComponent<PhotonView>().RPC("AddCoin", PhotonTargets.All);
                }
                else
                {
                    casillas[posicionX + 1, posicionY].GetComponent<Casilla>().AddCoin();
                }
            }
        }
        if (posicionY - 1 >= 0)
        {
            if (casillas[posicionX, posicionY - 1] != null)
            {
                if (OnlineGame == true)
                {
                    casillas[posicionX, posicionY - 1].GetComponent<PhotonView>().RPC("AddCoin", PhotonTargets.All);
                }
                else
                {
                    casillas[posicionX, posicionY - 1].GetComponent<Casilla>().AddCoin();
                }
            }
        }
        if (posicionY + 1 <= 5)
        {
            if (casillas[posicionX, posicionY + 1] != null)
            {
                if (OnlineGame == true)
                {
                    casillas[posicionX, posicionY + 1].GetComponent<PhotonView>().RPC("AddCoin", PhotonTargets.All);
                }
                else
                {
                    casillas[posicionX, posicionY + 1].GetComponent<Casilla>().AddCoin();
                }
            }
        }

    }
    #endregion
    #region TurnsManagement
    [PunRPC]
    public void EndOfTurn()
    {
        Debug.Log( "End of turn: " + PhotonNetwork.player.ID +" Online : "+OnlineGame );
        StopAllCoroutines();
        exploding = false;
        if (endGame == false && OnlineGame == true)
        {
            Debug.Log("End of turn: " + PhotonNetwork.player.ID);
            if (PhotonNetwork.player.ID == 1 && MiTurno == true)
            {
				StopCoroutine("TurnTime");
				StartCoroutine("BarTime");
				Debug.Log("Soy player 1");
                pView.RPC("ChangeTurn", PhotonTargets.All, 1);
            }
            else if (PhotonNetwork.player.ID == 2 && MiTurno == true)
            {
				StopCoroutine("TurnTime");
				StartCoroutine("BarTime");
				Debug.Log("Soy player 2");
                pView.RPC("ChangeTurn", PhotonTargets.All, 2);
            }

        }
        else if (endGame == false && OnlineGame == false && MiTurno == true)
        {
            Debug.LogWarning( "Mi turno y finalizada" );
            StopCoroutine("TurnTime");
			StartCoroutine("BarTime");
			timeBar.fillAmount = 1;
			MiTurno = false;
            turnTimerText.text = "";
            nonTouch.SetActive(true);
            fichasPuestas = 0;
            turnoText.text = LM.ReturnLine(7);
			turnoText.gameObject.SetActive(true);
            StartCoroutine("IA");
        }
        else if (endGame == false && OnlineGame == false && MiTurno == false)
        {
            delegadoPeso(0);
            StopCoroutine("IA");
            MiTurno = true;
            SetFichasUsables();
            nonTouch.SetActive(false);
            fichasPuestas = 0;
            turnoText.text = LM.ReturnLine(9);
			turnoText.gameObject.SetActive(true);
			StartCoroutine("BarTime");
			StartCoroutine("TurnTime");
		}
    }
	IEnumerator BarTime()
	{
		float duration = 40f; // 3 seconds you can change this 
							 //to whatever you want
		float normalizedTime = 1;
		while (normalizedTime >= 0f)
		{
			timeBar.fillAmount = normalizedTime;
			normalizedTime -= Time.deltaTime / duration;
			yield return null;
		}
        Debug.LogWarning( "Fin cuenta atrás" );
    }

    IEnumerator TurnTime()
    {
        int time = 40;
        turnTimerText.text = time + "s";
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            turnTimerText.text = time + "s";
            //timeBar.fillAmount = time / 40f;
        }
        Debug.LogWarning( "Fin cuenta atrás de TurnTime"+endGame );
        nonTouch.SetActive(true);
        turnoText.text = "Turno IA";
        fichasPuestas = 0;
        if (endGame == false && OnlineGame == false)
        {
            StartCoroutine("IA");
        }
		else if(endGame == false && OnlineGame == true)
		{
			//NetCheckExplosions();
            pView.RPC( "NetCheckExplosions" , PhotonTargets.All );
		}
        StopCoroutine("TurnTime");
    }

    void EndGame(string s)
    {
        endGame = true;
        StopCoroutine("IA");
        StopCoroutine("TurnTime");
        winnerGO.SetActive(true);
		blackBG.SetActive(true);
        nonTouch.SetActive(true);
        if (s == "Empate")
        {
            winnerText.text = LM.ReturnLine(16);
        }
        else
        {
            winnerText.text = s + "\n"+ LM.ReturnLine(17);
        }

        if (OnlineGame == true)
        {
            PhotonNetwork.room.IsVisible = false;
        }
        else
        {
            GameObject.Find("lobbyButton").SetActive(false);
        }
		
    }
    #endregion
    public void NewGame()
    {
		LM.ClearTexts();
        SceneManager.LoadScene("Game");
    }

    public void GoToLobby()
    {
        LM.ClearTexts();
        pView.RPC("Disconnect", PhotonTargets.All);

        UnityEngine.Networking.NetworkIdentity identity = FindObjectOfType<UnityEngine.Networking.NetworkIdentity>();
        if (identity != null)
        {
            Prototype.NetworkLobby.LobbyManager lb = FindObjectOfType<Prototype.NetworkLobby.LobbyManager>();
            if (lb != null)
                lb.StopHostClbk();

        }
    }

    public void GoToMenu()
    {
        if (OnlineGame)
        {
            fromGameToMenu = true;
            GoToLobby();
        }
        else
        {
            LM.ClearTexts();
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("Menu");
        }

    }

    private void CheckSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals("Lobby") && fromGameToMenu)
        {
            fromGameToMenu = false;
            Prototype.NetworkLobby.LobbyManager.s_Singleton.GoHomeButton();
        }
    }

    enum DificultadIA { Easy, Mid, Hard}
    public enum GameType { Puntuacion, Fichas}
}
