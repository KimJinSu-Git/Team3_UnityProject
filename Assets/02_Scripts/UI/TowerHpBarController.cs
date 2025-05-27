using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerHpBarController : MonoBehaviour
{
    public Transform player1TowerHPBar1;
    public Transform player1TowerHPBar2;
    public Transform player1TowerHPBar3;
    public Transform player2TowerHPBar1;
    public Transform player2TowerHPBar2;
    public Transform player2TowerHPBar3;

    private Transform player1TowerHPBar1_Temp;
    private Transform player1TowerHPBar2_Temp;
    private Transform player1TowerHPBar3_Temp;
    // public GameObject player2TowerHPBar1;
    // public GameObject player2TowerHPBar2;
    // public GameObject player2TowerHPBar3;
    private Vector3 addPosition = new Vector3(30f,0f,0f);
    void Start()
    {
        if (UserManager.Instance.FusionPlayerRef == SessionManager.Instance.CurrentGameRoomInfo.ClientPlayer)
        {
            Vector3 temp1 = player1TowerHPBar1.localPosition;
            Vector3 temp2 = player1TowerHPBar2.localPosition;
            Vector3 temp3 = player1TowerHPBar3.localPosition;

            player1TowerHPBar3.localPosition = player2TowerHPBar1.localPosition + addPosition;
            player1TowerHPBar2.localPosition = player2TowerHPBar2.localPosition + addPosition;
            player1TowerHPBar1.localPosition = player2TowerHPBar3.localPosition + addPosition;

            player2TowerHPBar3.localPosition = temp1 + addPosition;
            player2TowerHPBar2.localPosition = temp2 + addPosition;
            player2TowerHPBar1.localPosition = temp3 + addPosition;
        }
    }
}