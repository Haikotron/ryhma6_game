using UnityEngine;
using UnityEngine.InputSystem;

public class HomeBase : MonoBehaviour
{
    private bool playerNearby;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
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
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.AloitaReitti();
        }
    }
}
