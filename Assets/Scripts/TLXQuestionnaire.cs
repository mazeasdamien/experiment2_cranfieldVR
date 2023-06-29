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


    public void NextQuestion()
    {
        RecordAnswer();

        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            m.NextTask();
            SaveAnswersToCSV();
            next.SetActive(false);
            questionText.text = "Questionnaire completed!";
        }
    }

    private void SaveAnswersToCSV()
    {
        string folderPath = Path.Combine(Application.dataPath, $"Participants_data/participant_{m.par_ID}");
        Directory.CreateDirectory(folderPath);

        string fileName = $"participant_{m.par_ID}_modality_{m.oredr_ID}.csv";
        string filePath = Path.Combine(folderPath, fileName);

        using (StreamWriter sw = new StreamWriter(filePath))
        {
            sw.WriteLine("Question ID,Answer");

            for (int i = 0; i < recordedAnswers.Count; i++)
            {
                if (i < recordedAnswers.Count)
                {
                    string line = $"{questions[i].question_id},{recordedAnswers[i]}";
                    sw.WriteLine(line);
                }
            }
        }
    }
}
