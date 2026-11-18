using UnityEngine;
using UnityEngine.InputSystem;

public class MarketStall : MonoBehaviour
{
    public string stallName = "Stall";
    [Tooltip("How far (in local units) the interaction trigger reaches outside the solid box.")]
    [SerializeField] private float interactionPadding = 0.3f;
    private bool playerNearby = false;

    private void Awake()
    {
        // The scene's BoxCollider is a trigger: keep it (enlarged) for interaction and add a solid one for collisions.
        BoxCollider trigger = GetComponent<BoxCollider>();
        if (trigger != null && trigger.isTrigger)
        {
            BoxCollider solid = gameObject.AddComponent<BoxCollider>();
            solid.center = trigger.center;
            solid.size = trigger.size;
            trigger.size += Vector3.one * interactionPadding;
        }

        // Kinematic so stalls stay put; the hints depend on their positions.
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
        }
        body.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            if (GameUI.Instance != null) GameUI.Instance.ShowPrompt("Press <b>SPACE</b> to check this stall");
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
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.YritaValitaKoju(stallName);
            }
        }
    }
}