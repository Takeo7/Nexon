using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Casilla : MonoBehaviour {

    public int value = 0;

    public Vector2 posicion;

    public Text numero;

	public Image image;


    GameManager GM;

    private void Start()
    {
        GM = GameManager.instance;
		UpdateColor();
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
		//Instantiate Tap 
    }

    [PunRPC]
    public void AddCoin()
    {
        value++;
        ShowText();
		UpdateColor();
	}
    [PunRPC]
    public void UpdateValue(int i)
    {
        value = i;
        ShowText();
		UpdateColor();
		GameObject temp = Instantiate(GM.particles[0]);
		temp.transform.position = transform.position;
	}

    [PunRPC]
    public void ChangeParams(int posX, int posY, Vector2 localPos, int coins)
    {
        AddPosition(posX,posY);
        value = coins;
        ShowText();
    }

	void UpdateColor()
	{
		switch (value)
		{
			case 0:
				image.color = GM.colorNUM[0];
				break;
			case 1:
				image.color = GM.colorNUM[1];
				break;
			case 2:
				image.color = GM.colorNUM[2];
				break;
			case 3:
				image.color = GM.colorNUM[3];
				break;
			case 4:
				image.color = GM.colorNUM[4];
				break;
		}
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
