using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;

public class DataBase : MonoBehaviour {

    private void Start()
    {
        if( FirebaseAuth.DefaultInstance == null || FirebaseAuth.DefaultInstance.CurrentUser == null )
            return;

    }

    public void EscribirLoteCompleto()
    {
        Dictionary<string , object> lote;
    }

    public void EscribirVariosAmigosCompletos()
    {

    }

    public void EscribirUnAmigoCompleto()
    {

    }

    public void EscribirNombre( string nombre )
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/displayName" ).SetValueAsync( nombre );
    }

    public void EscribirEmail( string email )
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/email" ).SetValueAsync( email );
    }

    public void EscribirGamesCompleto(bool ganada)
    {
        Estadisticas.SumaPartida( ganada );
    }

    public void EscribirTotalPoints( int puntos )
    {
        Estadisticas.SumaPuntos( puntos );
    }

    public void EscribirRacha( int racha )
    {
        Estadisticas.MejorRacha( racha );
    }

}
