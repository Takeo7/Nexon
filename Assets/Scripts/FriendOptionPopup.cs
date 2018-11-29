using UnityEngine;
using UnityEngine.UI;

public class FriendOptionPopup : MonoBehaviour {

    [SerializeField]
    Toggle toggleFavourite = null, toggleBlocked = null;

    Friend friend;

    public void Show ( Friend friend )
    {
        this.friend = friend;
        toggleBlocked.isOn = friend.isBlocked;
        toggleFavourite.isOn = friend.isFavourite;
    }

    public void Hide ()
    {
        Destroy(gameObject);
    }

    public void OnChangeFavorito(bool esFavorito)
    {
        FriendsList.SetFriendFavourite( friend.uid , esFavorito );
    }

    public void OnChangeBloqueado(bool estaBloqueado)
    {
        FriendsList.SetFriendBlocked( friend.uid , estaBloqueado );
    }

    public void InviteToGame()
    {
        Hide();
    }

    public void RemoveFriend()
    {
        FriendsList.RemoveFriend( friend.uid );
        Hide();
    }
}
