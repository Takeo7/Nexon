using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendUI : MonoBehaviour {

    public Friend data;
    [SerializeField]
    Image blocked = null, fav = null;
    [SerializeField]
    Button acceptRequestButton = null, removeRequestButton = null;
    [SerializeField]
    Text displayName = null;
    
    [SerializeField]
    FriendOptionPopup popUpPrefab;

    public string DisplayName { get { return data.displayName; } }
    public string UID { get { return data.uid; } }

    private void OnEnable()
    {
        RefreshData();
    }

    public void AceptarSolicitud()
    {
        //transform.SetParent( GetComponentInParent<FriendsList>().AmigosContent );
        //data.pending = false;
        //RefreshData();
        //GetComponent<Button>().enabled = true;
        FriendsList.AcceptFriendRequest( data.uid );
    }

    public void EliminarSolicitud()
    {
        FriendsList.RemoveFriendRequest( data.uid );
        //Destroy( gameObject );
    }

    public void RefreshData()
    {
        displayName.text = data.displayName;
        blocked.gameObject.SetActive(data.isBlocked);
        fav.gameObject.SetActive(data.isFavourite);

        acceptRequestButton.gameObject.SetActive( data.pending );
        removeRequestButton.gameObject.SetActive( data.pending );
        if( data.pending )
        {
            GetComponent<Button>().enabled = false;
        }
    }

    public void ShowPopup()
    {
        Transform canvas = transform;

        while( canvas.parent != null )
        {
            canvas = canvas.parent;
        }

        Instantiate( popUpPrefab , canvas ).Show( data );
    }
}
