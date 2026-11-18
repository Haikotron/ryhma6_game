using UnityEngine;
using UnityEngine.InputSystem;

public class MarketStall : MonoBehaviour
{
    public string stallName = "Stall";
    private bool playerNearby = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Player entered the stall");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    private void Update()
    {
        if (playerNearby &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.YritaValitaKoju(stallName);
            }
        }
    }
}