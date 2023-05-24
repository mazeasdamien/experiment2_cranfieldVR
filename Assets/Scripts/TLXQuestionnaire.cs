using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class QuestionData
{
    public string question;
}

[System.Serializable]
public class TLXData
{
    public List<QuestionData> NASA_TLX;
}

public class TLXQuestionnaire : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public GameObject buttonContainer; // make sure to assign this in the editor

    private List<QuestionData> questions;
    private int currentQuestionIndex;

    private void Start()
    {
        LoadQuestions();
        ShowQuestion(0);
    }

    private void LoadQuestions()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "tlx_questions.json");
        string jsonString = File.ReadAllText(path);
        TLXData data = JsonUtility.FromJson<TLXData>(jsonString);

        questions = data.NASA_TLX;
    }

    private void ShowQuestion(int questionIndex)
    {
        if (questionIndex < 0 || questionIndex >= questions.Count)
        {
            Debug.LogError("Invalid question index!");
            return;
        }

        QuestionData questionData = questions[questionIndex];
        questionText.text = questionData.question;

        // Assume that you've already set up buttons for each of the possible responses in the buttonContainer
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.Log("Questionnaire completed!");
            // Implement your questionnaire completion logic here.
        }
    }
}
