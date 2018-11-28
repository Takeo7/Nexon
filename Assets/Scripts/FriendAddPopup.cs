using UnityEngine;
using UnityEngine.UI;

public class FriendAddPopup : MonoBehaviour {
    [SerializeField]
    InputField inputEmail = null;

    private void OnEnable()
    {
        inputEmail.text = "";
    }
    public void InvitarAmigo()
    {
        FriendsList.AddFriendByEmail(inputEmail.text);
        gameObject.SetActive(false);
    }
}
