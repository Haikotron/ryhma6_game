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
            if (GameUI.Instance != null) GameUI.Instance.ShowPrompt("Press <b>SPACE</b> to get a new route");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (GameUI.Instance != null) GameUI.Instance.HidePrompt();
        }
    }

    private void Update()
    {
        if (playerNearby &&
            GameUI.IsPlaying &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.AloitaReitti();
        }
    }
}
