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
    LanguageLine errorText = null;

    [SerializeField]
    GameObject buttonGotoLogin = null, buttonPlay = null, buttonLogin = null, buttonGotoCreate = null, buttonCreate = null, buttonBack = null, panelLogin = null, buttonReverificar = null;

    FirebaseApp app = null;
    FirebaseAuth auth = null;
    FirebaseUser user = null;
    DatabaseReference database = null;
    // Get the root reference location of the database.
    public bool IsReady {        get; private set;    }

#if UNITY_EDITOR
    private void Update()
    {
        if( inputPassword != null && inputPassword.isFocused && Input.GetKeyDown( KeyCode.Tab ) )
        {
            if ( inputPassword2 != null && inputPassword2.IsActive())
                inputPassword2.Select();

        }
        if( inputUsuario != null && inputUsuario.isFocused && Input.GetKeyDown( KeyCode.Tab ) )
        {
            inputPassword.Select();
        }
    }
#endif

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
            LanguageManager.instance.ClearTexts();
            SceneManager.LoadScene( "Menu" );
        }
        else
        {
            if( IsReady )
            {
                LoginAnonimo();
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

        buttonReverificar.SetActive(false);
        errorText.myLine = 10; //Texto Vacio
        errorText.AskForLine();
    }

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll(); //TODO Borrar el delete playerprefs
#endif
        Step( 1 );

        Firebase.Messaging.FirebaseMessaging.TokenReceived += FirebaseMessaging_TokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += FirebaseMessaging_MessageReceived;

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

    private void FirebaseMessaging_TokenReceived( object sender , Firebase.Messaging.TokenReceivedEventArgs token )
    {
        Debug.Log( "Received Registration Token: " + token.Token );
    }

    private void FirebaseMessaging_MessageReceived( object sender , Firebase.Messaging.MessageReceivedEventArgs e )
    {
        Debug.Log( "Received a new message from: " + e.Message.From );
    }

    public void RegistrarEmail()
    {
        if( inputPassword.text != inputPassword2.text )
        {
            //TODO 
            errorText.myLine = 57; //Las contraseñas no coinciden
            errorText.AskForLine();
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

                user.UpdateUserProfileAsync( new UserProfile() { DisplayName = us.displayName } );

                Step( 1 );
            }
        } );
    }

    public void AccederConEmail()
    {
        errorText.myLine = 10; //Vacío
        errorText.AskForLine();
        Debug.Log( "Accediendo con Email" );
        auth.SignInWithEmailAndPasswordAsync( inputUsuario.text , inputPassword.text ).ContinueWith( task => {
            if( task.IsCanceled )
            {
                Debug.LogError( "SignInWithEmailAndPasswordAsync was canceled." );
                return;
            }
            if( task.IsFaulted )
            {
                Debug.LogError( "SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception );
                errorText.myLine = 60; //Usuario y/o contraseña no valido
                errorText.AskForLine();
                return;
            }

            user = task.Result;
            string userName = user.DisplayName;
            if( string.IsNullOrEmpty( userName ) )
            {
                userName = inputUsuario.text.Substring( 0 , inputUsuario.text.IndexOf( '@' ) );

                user.UpdateUserProfileAsync( new UserProfile() { DisplayName = userName } ).ContinueWith( task2 => {
                    Debug.LogWarningFormat( "Canceled {0}, Faulted {1} , Completed {2}" , task2.IsCanceled , task2.IsFaulted , task2.IsCompleted );
                    Debug.LogWarning( "Actualizado el nickname" );
                } );
            }
            PlayerPrefs.SetString( "Email" , inputUsuario.text );
            PlayerPrefs.SetString( "Password" , inputPassword.text );
            PlayerPrefs.SetString( "UserName" , userName );

            Debug.LogFormat( "User signed in successfully: {0} ({1})  CustomUserName: {2}" ,
                user.DisplayName , user.UserId , userName );
#if !UNITY_EDITOR
            if (user.IsEmailVerified)
#endif
            {
                LanguageManager.instance.ClearTexts();
                SceneManager.LoadScene( "Menu" );
            }
#if !UNITY_EDITOR
            else
            {
                PlayerPrefs.DeleteKey("UserName");
                PlayerPrefs.DeleteKey("Email");
                PlayerPrefs.DeleteKey("Password");
                auth.SignOut();
                //Step(1);
                errorText.myLine = 58; //Email no verificado
                errorText.AskForLine();
                buttonReverificar.SetActive( true );
            }
#endif
        } );
    }

    public void ReenviarEmailVerificacion()
    {
        user.SendEmailVerificationAsync();
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

            LanguageManager.instance.ClearTexts();
            SceneManager.LoadScene( "Menu" );
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

        } );
    }

    public void LogOut()
    {
        PlayerPrefs.DeleteKey( "UserName" );
        PlayerPrefs.DeleteKey( "Email" );
        PlayerPrefs.DeleteKey( "Password" );
        auth.SignOut();

        //Recargamos la escena login sin dejar rastro de los datos del usuario
        Destroy(gameObject);
        LanguageManager.instance.ClearTexts();
        SceneManager.LoadScene( "Login" );
    }
    
    // Handle initialization of the necessary firebase modules:
    void InitializeFirebaseAuth()
    {
        Debug.Log( "Setting up Firebase Auth" );
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
#if UNITY_EDITOR
        AuthStateChanged( PlayerPrefs.HasKey( "Email" ) , null );
#else
        AuthStateChanged( this , null );
#endif
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

            //Step( 1 );

            user = auth.CurrentUser;
            if( signedIn && !user.IsAnonymous)
            {
                Debug.Log( "[AuthStateChange] Signed in "+user.DisplayName+" : " + user.UserId );
                buttonGotoLogin.SetActive( false );
                buttonGotoCreate.SetActive( false );
            }
        }
#if UNITY_EDITOR
        else
        {
            if( auth.CurrentUser == null && user == null && (sender is bool) && (bool)sender  )
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
    
    public override string ToString()
    {
        return System.String.Format("[DisplayName: {0}  Email({1})]",displayName, email);
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