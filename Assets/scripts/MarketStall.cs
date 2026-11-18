using UnityEngine;
using UnityEngine.InputSystem;

public class MarketStall : MonoBehaviour
{
    public string stallName = "Koju";
    private bool playerNearby = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Pelaaja tuli kojulle");
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
        if (playerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
        if (stallName == ObjectiveManager.Instance.currentTarget)
        {
           ObjectiveManager.Instance.score++;
           if (ObjectiveManager.Instance.score >= ObjectiveManager.Instance.winScore)
    {
        ObjectiveManager.Instance.objectiveText.text =
            "VOITIT PELIN!";

        return;
    }
 
         ObjectiveManager.Instance.objectiveText.text =
        "Oikea koju! Pisteet: " +
        ObjectiveManager.Instance.score;

        Invoke(nameof(AnnaUusiTehtava), 2f);
    }
    else
    {
        ObjectiveManager.Instance.score =
    Mathf.Max(0, ObjectiveManager.Instance.score - 1);

        ObjectiveManager.Instance.objectiveText.text =
        "Vaara koju! Pisteet: " +
        ObjectiveManager.Instance.score;

        Invoke(nameof(TyhjennaTeksti), 2f);
    }
        }
    }
        private void AnnaUusiTehtava()
    {
        ObjectiveManager.Instance.AnnaSatunnainenTehtava();
    }
    private void TyhjennaTeksti()
    {
        ObjectiveManager.Instance.objectiveText.text = "";
}
}