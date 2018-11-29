using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class FriendsList : MonoBehaviour {

    [SerializeField]
    InputField emailAmigo = null;
    [SerializeField]
    GameObject popupAddFriend = null;
    [SerializeField]
    FriendUI amigoPrefab = null;
    [SerializeField]
    Transform amigosContent = null, solicitudesContent = null;

    public Transform AmigosContent {get { return amigosContent; } }
    public Transform SolicitudesContent {get { return solicitudesContent; } }
    private DatabaseReference db;

    void Start()
    {
        if( FirebaseAuth.DefaultInstance == null || FirebaseAuth.DefaultInstance.CurrentUser == null )
            return;

        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        db = FirebaseDatabase.DefaultInstance.GetReference( "users/" + currentUserUid + "/friends" );

        GetFriends();
        db.ChildChanged += OnFriendStatusChanged;
        db.ChildAdded += OnFriendStatusChanged;
        db.ChildRemoved += OnFriendStatusChanged;
    }
    private void OnDestroy()
    {
        if( FirebaseAuth.DefaultInstance == null || FirebaseAuth.DefaultInstance.CurrentUser == null )
            return;
        db.ChildChanged -= OnFriendStatusChanged;
        db.ChildAdded -= OnFriendStatusChanged;
        db.ChildRemoved -= OnFriendStatusChanged;
    }

    private void OnFriendStatusChanged( object sender , ChildChangedEventArgs ev )
    {
        Debug.LogWarning( "Ha cambiado el estado de mis amigos" );
        GetFriends();
    }

    private void GetFriends()
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        FirebaseDatabase.DefaultInstance.GetReference( "users/"+currentUserUid+"/friends" ).GetValueAsync().ContinueWith( task => {
            if( task.IsFaulted )
            {
                // Handle the error...
            }
            else if( task.IsCompleted )
            {

                DataSnapshot snapshot = task.Result;

                List<Friend> friends = new List<Friend>();
                
                foreach( Transform t in amigosContent )
                {
                    Destroy( t.gameObject );
                }
                foreach( Transform t in solicitudesContent )
                {
                    Destroy( t.gameObject );
                }

                Debug.Log( "Actualizando interfaz de amigos" );
                // Do something with snapshot...
                int idx = 0;

                foreach( var user in snapshot.Children )
                {
                    Friend f = JsonUtility.FromJson<Friend>( user.GetRawJsonValue() );
                    f.uid = user.Key;
                    FirebaseDatabase.DefaultInstance.GetReference( "users/"+user.Key+"/displayName" ).GetValueAsync().ContinueWith( task2 => {
                        if( task2.IsFaulted )
                        {
                            // Handle the error...
                        }
                        else if( task2.IsCompleted )
                        {
                            idx++;

                            DataSnapshot snapshot2 = task2.Result;

                            f.displayName = snapshot2.Value.ToString();

                            friends.Add( f );
                            Debug.LogWarning( friends[friends.Count - 1] );

                            if( (f.pending && idx <= solicitudesContent.childCount) || (!f.pending && idx <= amigosContent.childCount) )
                            {
                            }
                            else
                            {
                                FriendUI fui;
                                if( f.pending )
                                    fui = Instantiate( amigoPrefab , solicitudesContent );
                                else
                                    fui = Instantiate( amigoPrefab , amigosContent );

                                fui.data = f;
                                fui.RefreshData();
                            }
                        }
                    } );
                }
            }
        } );
    }

    public void EnviarSolicitud()
    {
        string email = emailAmigo.text;
        AddFriendByEmail( email );
    }

    public static void AcceptFriendRequest(string friendUID)
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/friends/" + friendUID ).Child( "pending" ).RemoveValueAsync();
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + friendUID + "/friends/" + currentUserUid ).Child( "pending" ).RemoveValueAsync();
    }
    public static void RemoveFriendRequest( string friendUID )
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/friends/").Child( friendUID ).RemoveValueAsync();
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + friendUID + "/friends/").Child( currentUserUid ).RemoveValueAsync();
    }
    public static void RemoveFriend( string friendUID )
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/friends/" ).Child( friendUID ).RemoveValueAsync();
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + friendUID + "/friends/" ).Child( currentUserUid ).RemoveValueAsync();
    }
    public static void SetFriendFavourite( string friendUID , bool favourite)
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/friends/" + friendUID + "/isFavourite" ).SetValueAsync(favourite);
    }
    public static void SetFriendBlocked( string friendUID , bool blocked )
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/friends/" + friendUID + "/isBlocked" ).SetValueAsync( blocked );
    }
    public static void AddFriendByUId( string friendUId )
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        
        Friend friend = new Friend() {
            uid = friendUId
        };

        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/friends/" + friendUId ).SetRawJsonValueAsync( JsonUtility.ToJson( friend ) );

        friend.uid = currentUserUid;
        friend.pending = true;

        FirebaseDatabase.DefaultInstance.GetReference("/users/" + friendUId + "/friends/" + friendUId).SetRawJsonValueAsync(JsonUtility.ToJson(friend));
    }
    public static void AddFriendByEmail( string email )
    {
        FirebaseAuth.DefaultInstance.FetchProvidersForEmailAsync( email ).ContinueWith( task => {
            if( task.IsCanceled )
            {
                //TODO
                return;
            }
            if( task.IsFaulted )
            {
                //TODO
                return;
            }
            //User with that email already registered. Can proceed to add as friend
            FirebaseDatabase.DefaultInstance.GetReference( "users" ).GetValueAsync().ContinueWith( task2 => {
                if( task2.IsFaulted )
                {
                    // Handle the error...
                }
                else if( task2.IsCompleted )
                {
                    DataSnapshot snapshot = task2.Result;

                    User us = new User();
                    // Do something with snapshot...
                    foreach( var user in snapshot.Children )
                    {
                        //Debug.LogWarning("JSON value-->"+ user.GetRawJsonValue() );
                        us = JsonUtility.FromJson<User>( user.GetRawJsonValue() );
                        //Debug.LogWarning( us.displayName );
                        if( us.email == email )
                        {
                            AddFriendByUId( user.Key );
                            break;
                        }
                    }
                }
            } );
        } );
    }
}
