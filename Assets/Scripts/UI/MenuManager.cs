using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public void CargarEscena(string scene )
    {
        SceneManager.LoadScene(scene);
    }

    public void Jugar()
    {
        SceneManager.LoadScene( 1 );
    }
}
