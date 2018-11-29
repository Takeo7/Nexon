using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase;

#if UNITY_EDITOR
using Firebase.Unity.Editor;
#endif


public class UserManager : MonoBehaviour {

    [SerializeField]
    InputField inputUsuario = null, inputPassword = null, inputPassword2 = null;

    [SerializeField]
    Text errorText = null;

    [SerializeField]
    GameObject buttonGotoLogin = null, buttonPlay = null, buttonLogin = null, buttonGotoCreate = null, buttonCreate = null, buttonBack = null, panelLogin = null;

    FirebaseApp app = null;
    FirebaseAuth auth = null;
    FirebaseUser user = null;
    DatabaseReference database = null;
    // Get the root reference location of the database.
    public bool IsReady {        get; private set;    }
    public void GotoInitial()
    {
        Step( 1 );
    }
    public void GotoLogin()
    {
        Step( 2 ); 
    }
    public void GotoRegister()
    {
        Step( 3 );
    }
    public void GotoGame()
    {
        if( user != null )
        {
            SceneManager.LoadScene( "Amigos" ); //Cambiar esto y  poner menu
        }
        else
        {
            if( IsReady )
            {
                //LoginAnonimo();
            }
        }
    }
    /// <summary>
    /// 1. Main ( Login and Play )
    /// 2. Login ( Login and Create )
    /// 3. Register ( Create and Back )
    /// </summary>
    private void Step( int step )
    {
        buttonGotoLogin.SetActive( step == 1 );
        buttonGotoCreate.SetActive( step == 1 );
        buttonPlay.SetActive( step == 1 );

        panelLogin.SetActive( step != 1 );
        buttonBack.SetActive( step != 1 );

        buttonLogin.SetActive( step == 2 );

        buttonCreate.SetActive( step == 3 );
        inputPassword2.transform.parent.gameObject.SetActive( step == 3 );

        errorText.text = "";
    }

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        //TODO Borrar el playerprefs
        PlayerPrefs.DeleteAll();
        Debug.LogWarning( "Start" );
        Step( 1 );

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith( task => {
            var dependencyStatus = task.Result;
            Debug.LogWarning( "Status: " + task.Result );
            if( dependencyStatus == DependencyStatus.Available )
            {
                // Create and hold a reference to your FirebaseApp, i.e.
                app = FirebaseApp.DefaultInstance;
                
                // where app is a Firebase.FirebaseApp property of your application class.

                // Set a flag here indicating that Firebase is ready to use by your
                // application.
                IsReady = true;

                InitializeFirebaseAuth();

#if UNITY_EDITOR
                FirebaseApp.DefaultInstance.SetEditorDatabaseUrl( "https://nexon-game.firebaseio.com/" );
#endif
                database = FirebaseDatabase.DefaultInstance.RootReference;

                Debug.LogWarning( "Getting token of "+auth.CurrentUser.UserId );
                //LoginAnonimo();
            }
            else
            {
                Debug.LogError( System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}" , dependencyStatus ) );
                // Firebase Unity SDK is not safe to use here.
            }
        } );
    }

    public void RegistrarEmail()
    {
        if( inputPassword.text != inputPassword2.text )
        {
            //TODO 
            errorText.text = "Contraseñas no coinciden";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync( inputUsuario.text , inputPassword.text ).ContinueWith( task => {
            if( task.IsCanceled )
            {
                Debug.LogError( "CreateUserWithEmailAndPasswordAsync was canceled." );
                return;
            }
            if( task.IsFaulted )
            {
                Debug.LogError( "CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception );
                return;
            }

            // Firebase user has been created.
            user = task.Result;

            Debug.LogFormat( "Firebase user created successfully: {0} ({1})" ,
                user.DisplayName , user.UserId );
            if( user != null )
            {
                Debug.LogWarning( "Sending verification email to " + user.Email );
                user.SendEmailVerificationAsync();
                User us = new User() {
                    uid = user.UserId,
                    email = user.Email,
                    displayName = user.Email.Substring(0, user.Email.IndexOf('@')), // Nos quedamos de base con el string del email hasta el @ (user@gmail.com sería user)
                };
                FirebaseDatabase.DefaultInstance.GetReference( "/users/" + user.UserId ).SetRawJsonValueAsync( JsonUtility.ToJson(us) );

                Step( 1 );
            }
        } );
    }

    public void AccederConEmail()
    {
        auth.SignInWithEmailAndPasswordAsync( inputUsuario.text , inputPassword.text ).ContinueWith( task => {
            if( task.IsCanceled )
            {
                Debug.LogError( "SignInWithEmailAndPasswordAsync was canceled." );
                return;
            }
            if( task.IsFaulted )
            {
                Debug.LogError( "SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception );
                return;
            }

            user = task.Result;
            PlayerPrefs.SetString( "Email" , inputUsuario.text );
            PlayerPrefs.SetString( "Password" , inputPassword.text );
            PlayerPrefs.SetString("UserName", user.DisplayName);

            Debug.LogFormat( "User signed in successfully: {0} ({1})" ,
                user.DisplayName , user.UserId );

            if (user.IsEmailVerified)
                SceneManager.LoadScene("Menu");
            else
            {
                LogOut();
                Step(1);
            }
        } );
    }

    public void LoginAnonimo()
    {
        auth.SignInAnonymouslyAsync().ContinueWith( task => {
            if( task.IsCanceled )
            {
                Debug.LogError( "SignInAnonymouslyAsync was canceled." );
                return;
            }
            if( task.IsFaulted )
            {
                Debug.LogError( "SignInAnonymouslyAsync encountered an error: " + task.Exception );
                return;
            }

            user = task.Result;
            Debug.LogFormat( "User signed in successfully: {0} ({1})" ,
                user.DisplayName , user.UserId );
        } );
    }

    private void LoginWithCredential()
    {
        Credential credential = EmailAuthProvider.GetCredential( PlayerPrefs.GetString( "Email" , inputUsuario.text ) , PlayerPrefs.GetString( "Password" , inputPassword.text ) );
        auth.SignInWithCredentialAsync( credential ).ContinueWith( task => {
            if( task.IsCanceled )
            {
                Debug.LogError( "SignInWithCredentialAsync was canceled." );
                return;
            }
            if( task.IsFaulted )
            {
                Debug.LogError( "SignInWithCredentialAsync encountered an error: " + task.Exception );
                return;
            }

            user = task.Result;
            Debug.LogFormat( "User signed in successfully: {0} ({1})" ,
                user.DisplayName , user.UserId );

            AddFriendByEmail( "payno.currante@gmail.com" );
        } );
    }

    public void LogOut()
    {
        PlayerPrefs.DeleteKey( "Email" );
        PlayerPrefs.DeleteKey( "Password" );
        auth.SignOut();
    }

    public void AddFriendByUId( string friendUId )
    {
        string currentUserUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        Debug.Log( "CurrentUserId: " + currentUserUid );

        Friend friend = new Friend() {
            uid = friendUId ,
            pending = true
        };

        Debug.Log( friend );
        FirebaseDatabase.DefaultInstance.GetReference( "/users/" + currentUserUid + "/friends/"+friendUId ).SetRawJsonValueAsync( JsonUtility.ToJson( friend ) );
    }

    public void AddFriendByEmail( string email )
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

    // Handle initialization of the necessary firebase modules:
    void InitializeFirebaseAuth()
    {
        Debug.Log( "Setting up Firebase Auth" );
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged( this , null );
    }

    // Track state changes of the auth object.
    void AuthStateChanged( object sender , System.EventArgs eventArgs )
    {
        if( auth.CurrentUser != user )
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if( !signedIn && user != null )
            {
                Debug.Log( "Signed out " + user.UserId );
            }

            Step( 1 );

            user = auth.CurrentUser;
            if( signedIn )
            {
                Debug.Log( "Signed in " + user.UserId );
                buttonGotoLogin.SetActive( false );
                buttonGotoCreate.SetActive( false );
            }
        }
#if UNITY_EDITOR
        else
        {
            if( auth.CurrentUser == null && user == null && PlayerPrefs.HasKey("Email") )
            {
                LoginWithCredential();
            }
        }
#endif
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        //auth = null;
    }
}

[System.Serializable]
public class User
{
    public string uid { get; set; }
    public string email;
    public string displayName;
    public int bestStreak;
    public int totalPoints;
    public float ratio;
    //public Friend[] friends;

    public override string ToString()
    {
        return System.String.Format("[{0}({1})] ==> BestStreak:{2} ==> TotalPoints:{3} ==> Win/Played: {4}",displayName, email, bestStreak, totalPoints, ratio);
    }
}

[System.Serializable]
public class Friend
{
    public string uid { get; set; }
    public string displayName{ get; set; }
    public bool isFavourite;
    public bool isBlocked;
    public bool pending;
    public override string ToString()
    {
        return System.String.Format( "[{0}({1})] ==> Fav:{2} ==> Blocked:{3} ==> Pending:{4}",displayName,uid, isFavourite, isBlocked, pending );
    }
}