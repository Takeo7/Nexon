using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Casilla : MonoBehaviour {

    public int value = 0;

    public Vector2 posicion;

    public Text numero;

    GameManager GM;

    private void Start()
    {
        GM = GameManager.instance;
    }

    public void PressNewCoin()
    {
        
        if (GM.OnlineGame == false)
        {
            AddCoin();
            GM.NewCoinPlayed();
        }
        else
        {
            GetComponent<PhotonView>().RPC("AddCoin", PhotonTargets.All);
            GM.pView.RPC("NewCoinPlayed", PhotonTargets.All);
        }
    }

    [PunRPC]
    public void AddCoin()
    {
        value++;
        ShowText();
    }
    [PunRPC]
    public void UpdateValue(int i)
    {
        value = i;
        ShowText();
    }

    [PunRPC]
    public void ChangeParams(int posX, int posY, Vector2 localPos, int coins)
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("Respawn").transform);
        transform.localPosition = localPos;
        AddPosition(posX,posY);
        value = coins;
        ShowText();
    }

    public void AddPosition(int x, int y)
    {
        posicion = new Vector2(x, y);
    }

    void ShowText()
    {
        numero.text = "" + value;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            stream.Serialize(ref pos);
        }
        else
        {
            Vector3 pos = Vector3.zero;
            stream.Serialize(ref pos);  // pos gets filled-in. must be used somewhere
        }
    }

}
