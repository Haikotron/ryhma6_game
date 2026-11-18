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
            hints = new[] { "red" }
        },
        new StallHintSet
        {
            stallName = "koju2",
            hints = new[] { "green" }
        },
        new StallHintSet
        {
            stallName = "koju3",
            hints = new[] { "purple" }
        },
        new StallHintSet
        {
            stallName = "koju4",
            hints = new[] { "blue" }
        },
        new StallHintSet
        {
            stallName = "koju5",
            hints = new[] { "yellow" }
        },
        new StallHintSet
        {
            stallName = "koju6",
            hints = new[] { "white" }
        },
        new StallHintSet
        {
            stallName = "koju7",
            hints = new[] { "pink" }
        },
        new StallHintSet
        {
            stallName = "koju8",
            hints = new[] { "orange" }
        },
        new StallHintSet
        {
            stallName = "koju9",
            hints = new[] { "black" }
        },
        new StallHintSet
        {
            stallName = "koju10",
            hints = new[] { "grey" }
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
        UpdateUI("Walk to the Home Base and press SPACE to get your first route.");
    }

    public void AloitaReitti()
    {
        if (routeActive)
        {
            ShowMessage("Finish the current route first.");
            Toast("Finish the current route first!", GameUI.InfoColor);
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
        UpdateUI("From the <color=#FFC233>" + firstHint + "</color> to the <color=#FFC233>" + destinationHint + "</color>");
        Toast("New route!", GameUI.InfoColor);
    }

    public void YritaValitaKoju(string stallName)
    {
        if (!routeActive)
        {
            ShowMessage("Get a new hint from the Home Base.");
            Toast("Get a route from the Home Base first!", GameUI.InfoColor);
            return;
        }

        if (stallName != currentTarget)
        {
            score = Mathf.Max(0, score - 1);
            ShowMessage("Wrong stall!");
            UpdateUI(null);
            Toast("Wrong stall!  -1", GameUI.BadColor);
            return;
        }

        if (!firstStallVisited)
        {
            firstStallVisited = true;
            currentTarget = destinationTarget;
            ShowMessage("Yes, go on...");
            UpdateUI("Now go to the <color=#FFC233>" + destinationHint + "</color>");
            Toast("Correct! Keep going...", GameUI.GoodColor);
            return;
        }

        score++;
        routeActive = false;
        currentTarget = "";

        if (score >= winScore)
        {
            ShowMessage("YOU WON!");
            UpdateUI("You won!");
            if (GameUI.Instance != null) GameUI.Instance.ShowWin(score);
            return;
        }

        ShowMessage("Route complete! Return to the Home Base.");
        UpdateUI("Return to the Home Base for a new route.");
        Toast("Route complete!  +1", GameUI.GoodColor);
    }

    // Passing null keeps the current objective text and only refreshes the score.
    private void UpdateUI(string objective)
    {
        if (GameUI.Instance == null) return;
        GameUI.Instance.SetScore(score, winScore);
        if (objective != null) GameUI.Instance.SetObjective(objective);
    }

    private static void Toast(string message, Color color)
    {
        if (GameUI.Instance != null) GameUI.Instance.ShowToast(message, color);
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