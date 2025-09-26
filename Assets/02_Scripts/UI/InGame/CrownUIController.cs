using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownUIController : MonoBehaviour
{
    public GameObject player1Crown;
    public GameObject player1Count;
    public GameObject player2Crown;
    public GameObject player2Count;
    void Start()
    {
        if (UserManager.Instance.FusionPlayerRef == SessionManager.Instance.CurrentGameRoomInfo.ClientPlayer)
        {
            player1Crown.transform.localPosition = new Vector3(365f, 135f, 0f);
            player1Count.transform.localPosition = new Vector3(675f, 130f, 0f);
            player2Crown.transform.localPosition = new Vector3(365f, -45f, 0f);
            player2Count.transform.localPosition = new Vector3(675f, -50f, 0f);
        }
    }
}
