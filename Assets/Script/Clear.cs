using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clear : MonoBehaviour
{
    public GameManager gameManager; 
    public PlayerController player;  

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (player.key == player.maxHasKey)
            {
                gameManager.GameClear();
            }
            else
            {
                Debug.Log("���谡 �����մϴ�!");
            }
        }
    }
}
