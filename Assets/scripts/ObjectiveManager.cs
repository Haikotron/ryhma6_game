using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class StallHintSet
{
    public string stallName;
    public string[] hints = { "Hint 1", "Hint 2", "Hint 3" };
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public string currentTarget;
    public int score = 0;
    public int winScore = 10;

    public TMP_Text objectiveText;
    private string destinationHint;
    private string destinationTarget;
    private bool routeActive;
    private bool firstStallVisited;

    private static readonly StallHintSet[] stallHints =
    {
        new StallHintSet
        {
            stallName = "koju1",
            hints = new[] { "first on the left", "red", "#1" }
        },
        new StallHintSet
        {
            stallName = "koju2",
            hints = new[] { "second on the left", "green", "#2" }
        },
        new StallHintSet
        {
            stallName = "koju3",
            hints = new[] { "third on the left", "purple", "#3" }
        },
        new StallHintSet
        {
            stallName = "koju4",
            hints = new[] { "fourth on the left", "blue", "#4" }
        },
        new StallHintSet
        {
            stallName = "koju5",
            hints = new[] { "fifth on the left", "yellow", "#5" }
        },
        new StallHintSet
        {
            stallName = "koju6",
            hints = new[] { "first on the right", "white", "#6" }
        },
        new StallHintSet
        {
            stallName = "koju7",
            hints = new[] { "second on the right", "pink", "#7" }
        },
        new StallHintSet
        {
            stallName = "koju8",
            hints = new[] { "third on the right", "orange", "#8" }
        },
        new StallHintSet
        {
            stallName = "koju9",
            hints = new[] { "fourth on the right", "black", "#9" }
        },
        new StallHintSet
        {
            stallName = "koju10",
            hints = new[] { "fifth on the right", "grey", "#10" }
        }
    };
    private readonly List<string> messageHistory = new List<string>();
    private const int MaxVisibleMessages = 5;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentTarget = "";
        objectiveText.text = "";
    }

    public void AloitaReitti()
    {
        if (routeActive)
        {
            ShowMessage("Finish the current route first.");
            return;
        }

        if (stallHints == null || stallHints.Length < 2)
        {
            Debug.LogError("A route requires at least two stalls.");
            return;
        }

        int firstIndex = Random.Range(0, stallHints.Length);
        int destinationIndex = Random.Range(0, stallHints.Length - 1);
        if (destinationIndex >= firstIndex)
        {
            destinationIndex++;
        }

        StallHintSet firstStall = stallHints[firstIndex];
        StallHintSet destinationStall = stallHints[destinationIndex];
        currentTarget = firstStall.stallName;
        destinationTarget = destinationStall.stallName;
        string firstHint = ArvoVihje(firstStall);
        destinationHint = ArvoVihje(destinationStall);
        firstStallVisited = false;
        routeActive = true;

        ShowMessage("From the " + firstHint + " to the " + destinationHint);
    }

    public void YritaValitaKoju(string stallName)
    {
        if (!routeActive)
        {
            ShowMessage("Get a new hint from the Home Base.");
            return;
        }

        if (stallName != currentTarget)
        {
            score = Mathf.Max(0, score - 1);
            ShowMessage("Wrong stall!");
            return;
        }

        if (!firstStallVisited)
        {
            firstStallVisited = true;
            currentTarget = destinationTarget;
            ShowMessage("Yes, go on...");
            return;
        }

        score++;
        routeActive = false;
        currentTarget = "";

        if (score >= winScore)
        {
            ShowMessage("YOU WON!");
            return;
        }

        ShowMessage("Route complete! Return to the Home Base.");
    }

    private string ArvoVihje(StallHintSet stall)
    {
        if (stall == null || stall.hints == null || stall.hints.Length == 0)
        {
            Debug.LogError("A stall is missing a hint.");
            return "unknown stall";
        }

        return stall.hints[Random.Range(0, stall.hints.Length)];
    }

    private void ShowMessage(string message)
    {
        messageHistory.Add(message);

        if (messageHistory.Count > MaxVisibleMessages)
        {
            messageHistory.RemoveAt(0);
        }

        objectiveText.text = string.Join("\n", messageHistory);
    }
}