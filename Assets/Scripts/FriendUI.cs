using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendUI : MonoBehaviour {

    public Friend data;
    [SerializeField]
    Image blocked, fav;
    
    [SerializeField]
    GameObject popUpPrefab;

    public void ShowPopup()
    {
        Transform canvas = transform;

        while( canvas.parent != null )
        {
            canvas = canvas.parent;
        }

        Instantiate( popUpPrefab , canvas );
    }
}
