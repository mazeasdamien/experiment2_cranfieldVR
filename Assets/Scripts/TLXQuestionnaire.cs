using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

[System.Serializable]
public class QuestionData
{
    public int question_id;
    public string question;
    public List<string> responses;
    public int min_value_slider;
    public int max_value_slider;
}

[System.Serializable]
public class TLXData
{
    public List<QuestionData> Questions;
}

public class TLXQuestionnaire : MonoBehaviour
{    public TextMeshProUGUI questionText;

    private List<QuestionData> questions;
    private int currentQuestionIndex;
    public TextMeshProUGUI right_text;
    public TextMeshProUGUI left_text;
    public TextMeshProUGUI valueText;
    public Slider slider;
    public string folderPath;
    public modalities m;

    private List<string> recordedAnswers;
    public GameObject next;

    public List<string> GetAnswers()
    {
        return recordedAnswers;
    }

    private void Start()
    {
        recordedAnswers = new List<string>();
        LoadQuestions();
        ShowQuestion(0);
    }

    private void Update()
    {
        valueText.text = slider.value.ToString();
        //Debug.Log(folderPath);
    }

    private void LoadQuestions()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "questions.json");
        string jsonString = File.ReadAllText(path);
        TLXData data = JsonUtility.FromJson<TLXData>(jsonString);

        questions = data.Questions;
    }

    private void ShowQuestion(int questionIndex)
    {
        if (questionIndex < 0 || questionIndex >= questions.Count)
        {
            Debug.LogError("Invalid question index!");
            return;
        }

        currentQuestionIndex = questionIndex;  // Add this line

        QuestionData questionData = questions[questionIndex];
        questionText.text = questionData.question;
        left_text.text = questionData.responses[0];
        right_text.text = questionData.responses[1];
        slider.minValue = questionData.min_value_slider;
        slider.maxValue = questionData.max_value_slider;
        if (questionData.max_value_slider == 21)
        { slider.value = 11; }
        else { slider.value = 3; }
    }


    public void RecordAnswer()
    {
        if (currentQuestionIndex < questions.Count)
        {
            string answer = slider.value.ToString();
            recordedAnswers.Add(answer);
        }
    }

    public void ResetQuestionnaire()
    {
        // Record the answer for the current question before resetting
        RecordAnswer();

        // Reset the current question index and clear the recorded answers
        currentQuestionIndex = 0;
        recordedAnswers.Clear();

        // Show the first question
        ShowQuestion(0);
    }

    public void NextQuestion()
    {
        // Check if there are more questions
        if (currentQuestionIndex < questions.Count - 1)
        {
            // Record the answer for the current question
            RecordAnswer();

            // Move to the next question
            currentQuestionIndex++;
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            // Record the answer for the last question
            RecordAnswer();

            // Reset the current question index and move on to the next task
            currentQuestionIndex = 0;
            m.NextTask();

            // Show the first question
            ShowQuestion(0);
        }
    }

}
