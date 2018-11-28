using System.Collections;
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

    void Start()
    {
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
                // Do something with snapshot...
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
                            DataSnapshot snapshot2 = task2.Result;

                            f.displayName = snapshot2.Value.ToString();

                            friends.Add( f );
                            Debug.LogWarning( friends[friends.Count - 1] );
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

    public static void AddFriendByUId( string friendUId )
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        
        Friend friend = new Friend() {
            uid = friendUId ,
            pending = true
        };

        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/friends/" + friendUId ).SetRawJsonValueAsync( JsonUtility.ToJson( friend ) );
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
                        us = JsonUtility.FromJson<User>( user.GetRawJsonValue() );
                        Debug.LogWarning( us.displayName );
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
