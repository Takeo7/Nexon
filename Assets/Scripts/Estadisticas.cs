using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Estadisticas : MonoBehaviour {
    [SerializeField]
    Dropdown tipoRanking = null;
    [SerializeField]
    Transform nombres = null, puntos = null;
    [SerializeField]
    Text nombrePropio = null, puntosPropios = null;

    static string currentUserUid = "";
    
    public static int Puntuacion { get; private set; }
    public static int RachaMejor { get; private set; }
    public static int PartidasTotales { get; private set; }
    public static int PartidasGanadas { get; private set; }

    IEnumerator Start()
    {
        while ( FirebaseAuth.DefaultInstance == null || FirebaseAuth.DefaultInstance.CurrentUser == null )
            yield return null;

        currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        nombrePropio.text = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
        ActualizaValores( tipoRanking.value );
    }

    public void SumaPuntosUI ( int puntos ){ SumaPuntos( puntos ); }
    public void MejorRachaUI (int cantidad) { MejorRacha( cantidad ); }
    public void SumaPartidaUI (bool ganada) { SumaPartida( ganada ); }
    public void VolverAMenu(){ UnityEngine.SceneManagement.SceneManager.LoadScene( "Menu" ); }


    public void ActualizaValores(int valor)
    {
        switch( valor )
        {
            case 0:
                SumaPuntos( 0 );
                GetTopPuntos();
                break;
            case 1:
                GetTopRatio();
                break;
            default:
                MejorRacha( 0 );
                GetTopRacha();
                break;
        }
    }

    public void GetTopRatio()
    {
        FirebaseDatabase.DefaultInstance.GetReference( "users" ).OrderByChild( "games/ratio" ).LimitToLast( 10 ).ValueChanged += TopRatioChanged;

        FirebaseDatabase.DefaultInstance.GetReference( "users/" + currentUserUid + "/games/ratio" ).GetValueAsync().ContinueWith( task => {

            puntosPropios.text = string.Format( "{0:0.00}" , task.Result.Value.ToString());
        } );
    }

    private void TopRatioChanged( object sender , ValueChangedEventArgs args )
    {
        if( args.DatabaseError != null )
        {
            Debug.LogError( args.DatabaseError.Message );
            return;
        }

        Text[] nombresT = nombres.GetComponentsInChildren<Text>();
        Text[] puntosT = puntos.GetComponentsInChildren<Text>();
        for( int a = 0 ; a < nombresT.Length ; a++ )
        {
            nombresT[a].text = "";
            puntosT[a].text = "";
        }

        int i = (int)args.Snapshot.ChildrenCount;
        foreach( var e in args.Snapshot.Children )
        {
            i--;
            if( e.HasChild( "displayName" ) )
                nombresT[i].text = e.Child( "displayName" ).Value.ToString();

            float ratio = 0;
            if( e.HasChild( "games/ratio" ) )
                float.TryParse( e.Child( "games/ratio" ).Value.ToString() , out ratio );
            
            puntosT[i].text = string.Format( "{0:0.00}" ,  ratio );
        }

    }
    public void GetTopRacha()
    {
        FirebaseDatabase.DefaultInstance.GetReference( "users" ).OrderByChild( "bestStreak" ).LimitToLast( 10 ).ValueChanged += TopRachaChanged;

    }

    private void TopRachaChanged( object sender , ValueChangedEventArgs args )
    {
        if( args.DatabaseError != null )
        {
            Debug.LogError( args.DatabaseError.Message );
            return;
        }

        Text[] nombresT = nombres.GetComponentsInChildren<Text>();
        Text[] puntosT = puntos.GetComponentsInChildren<Text>();
        for( int a = 0 ; a < nombresT.Length ; a++ )
        {
            nombresT[a].text = "";
            puntosT[a].text = "";
        }
        int i = (int)args.Snapshot.ChildrenCount;
        foreach( var e in args.Snapshot.Children )
        {
            i--;
            if( e.HasChild( "displayName" ) )
                nombresT[i].text = e.Child( "displayName" ).Value.ToString();
            if( e.HasChild( "bestStreak" ) )
                puntosT[i].text = e.Child( "bestStreak" ).Value.ToString();
        }
        puntosPropios.text = RachaMejor.ToString();
    }

    public void GetTopPuntos()
    {
        FirebaseDatabase.DefaultInstance.GetReference( "users" ).OrderByChild( "totalPoints" ).LimitToLast( 10 ).ValueChanged += TopPuntosChanged;
        
    }

    private void TopPuntosChanged( object sender , ValueChangedEventArgs args )
    {
        if( args.DatabaseError != null )
        {
            Debug.LogError( args.DatabaseError.Message );
            return;
        }

        Text[] nombresT = nombres.GetComponentsInChildren<Text>();
        Text[] puntosT = puntos.GetComponentsInChildren<Text>();
        for( int a = 0 ; a < nombresT.Length ; a++ )
        {
            nombresT[a].text = "";
            puntosT[a].text = "";
        }
        int i = (int)args.Snapshot.ChildrenCount;
        foreach( var e in args.Snapshot.Children )
        {
            i--;
            if( e.HasChild( "displayName" ) )
                nombresT[i].text = e.Child("displayName").Value.ToString();
            if( e.HasChild( "totalPoints" ) ) 
                puntosT[i].text = e.Child( "totalPoints" ).Value.ToString();
        }
        puntosPropios.text = Puntuacion.ToString();
    }

    public static void SumaPuntos( int puntos )
    {
        if( string.IsNullOrEmpty( currentUserUid ) )
        {
            currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            if( string.IsNullOrEmpty( currentUserUid ) )
                return;
        }

        int oldValue = 0;

        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/totalPoints" ).GetValueAsync().ContinueWith
            ( task => {
                if( task.IsCanceled )
                    return;
                DataSnapshot ds = task.Result;
                if( ds != null && ds.Value != null)
                    int.TryParse( (ds.Value.ToString()) , out oldValue );
                Puntuacion = oldValue;

                FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/totalPoints" ).SetValueAsync( oldValue + puntos );

            }
            );

    }

    public static void MejorRacha( int cantidad )
    {
        Debug.LogWarning( "Intentando Guardar Racha de " + cantidad );
        if( string.IsNullOrEmpty( currentUserUid ) )
        {
            currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            if( string.IsNullOrEmpty( currentUserUid ) )
                return;
        }
        Debug.LogWarning( "Guardando Racha de " + cantidad );
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/bestStreak" ).GetValueAsync().ContinueWith
            ( task => {
                Debug.LogWarning( "Consulta completada" );

                if( task.IsCanceled )
                    return;
                DataSnapshot ds = task.Result;
                int oldValue;
                Debug.LogWarning( "Obteniendo valor anterior" );
                if( ds != null && ds.Value != null )
                    int.TryParse( (ds.Value.ToString()) , out oldValue );
                else
                    oldValue = 0;
                RachaMejor = Mathf.Max( oldValue, cantidad) ;
                Debug.LogWarning( "Racha anterior = "+oldValue + " y mejor racha = "+RachaMejor );
                if( cantidad > oldValue )
                {
                    Debug.LogWarning( "Es mayor la nueva, de modo que se procede a guardar" );
                    FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/bestStreak" ).SetValueAsync( cantidad );
                }

            }
            );
    }

    public static void SumaPartida( bool ganada )
    {
        if( string.IsNullOrEmpty( currentUserUid ) )
        {
            currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            if( string.IsNullOrEmpty( currentUserUid ) )
                return;
        }
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/games" ).GetValueAsync().ContinueWith
            ( task => {
                Debug.Log( "Sumando Partida" );
                if( task.IsCanceled )
                    return;
                
                DataSnapshot ds = null;
                int oldPlayed = 0;
                Debug.Log( "Algo" );
               //if( task.Result.HasChild( "total" ) )
                    ds = task.Result.Child( "total" );
                Debug.Log( "Otra cosa" );
                if( ds != null && ds.Value != null )
                    int.TryParse((ds.Value.ToString()), out oldPlayed );
                
                FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/games/total" ).SetValueAsync( oldPlayed + 1  );

                PartidasTotales = oldPlayed + 1;

                ds = task.Result.Child( "won" );
                
                int oldWon = 0;
                if( ds != null && ds.Value != null )
                    int.TryParse( (ds.Value.ToString()) , out oldWon );

                PartidasGanadas = oldWon;

                if( ganada )
                {
                    PartidasGanadas = oldWon + 1;
                    FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/games/won" ).SetValueAsync( oldWon + 1 );
                }

                FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/games/ratio" ).SetValueAsync( (100*PartidasGanadas / PartidasTotales) / 100f );
            }
            );
    }
}
