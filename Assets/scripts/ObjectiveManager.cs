using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public string currentTarget;
    public int score = 0;
    public int winScore = 10;

    public TMP_Text objectiveText;

    private string[] kojut =
    {
        "koju1",
        "koju2",
        "koju3",
        "koju4",
        "koju5",
        "koju6",
        "koju7",
        "koju8",
        "koju9",
        "koju10"
    };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AnnaSatunnainenTehtava();
    }

    public void AnnaSatunnainenTehtava()
    {
        string uusiKohde = kojut[Random.Range(0, kojut.Length)];

        currentTarget = uusiKohde;

        objectiveText.text = "Tehtava: kay " + uusiKohde + " luona";
        StopAllCoroutines();
        StartCoroutine(HideTextAfterSeconds());
    }
    private IEnumerator HideTextAfterSeconds()
    {
    yield return new WaitForSeconds(5f);

    objectiveText.text = "";
    }
}