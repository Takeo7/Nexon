using UnityEngine;
using UnityEngine.UI;

public class FriendOptionPopup : MonoBehaviour {

    [SerializeField]
    Toggle favorito = null, bloqueado = null;

    public void Show ( Friend friend )
    {

    }

    public void Hide ()
    {
        Destroy(gameObject);
    }

    public void OnChangeFavorito(bool esFavorito)
    {

    }

    public void OnChangeBloqueado(bool estaBloqueado)
    {

    }
}
