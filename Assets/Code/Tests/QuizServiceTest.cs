using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class QuizServiceTests {
    private GameObject quizServiceGO;
    private BoardquizService quizService;
    private const string TEST_QUESTIONS_FILE_PATH = "Data/Testdata/Economic_Questions/questions_example";

    private List<QuestionData> expectedQuestions = new() {
        new QuestionData(1, new List<long> { 1, 2, 3 }, 0, "easy"),
        new QuestionData(2, new List<long> { 4, 5, 6 }, 1, "medium"),
        new QuestionData(3, new List<long> { 7, 8, 9 }, 2, "hard")
    };

    [SetUp]
    public void SetUp() {
        quizServiceGO = new GameObject();
        quizService = quizServiceGO.AddComponent<BoardquizService>();
        quizService.SetDataSourcePath(TEST_QUESTIONS_FILE_PATH);
    }

    [TearDown]
    public void TearDown() {
        Object.DestroyImmediate(quizServiceGO);
    }

    [Test]
    public void GetRandomQuestion_WhenQuestionsAvailable_ReturnsQuestionAndRemovesFromPool() {
        var initialCount = expectedQuestions.Count;
        var drawnQuestions = new List<QuestionData>();

        QuestionData q1 = quizService.GetRandomQuestion();
        drawnQuestions.Add(q1);
        for (var i = 1; i < initialCount; i++) {
            QuestionData q_loop = quizService.GetRandomQuestion();
            drawnQuestions.Add(q_loop);
        }

        Assert.IsNotNull(q1);
        Assert.IsTrue(expectedQuestions.Any(tq => tq.question == q1.question));
        Assert.AreEqual(initialCount, drawnQuestions.Count);
        Assert.AreEqual(initialCount, drawnQuestions.Distinct().Count());
    }

    [Test]
    public void GetRandomQuestion_WhenNoQuestionsAvailable_ReturnsNullOnNextCall() { // Später wahrscheinlich eine Exception werfen
        var initialCount = expectedQuestions.Count + 1;

        for (var i = 1; i < initialCount; i++) {
            _ = quizService.GetRandomQuestion();
        }

        QuestionData result = quizService.GetRandomQuestion();

        Assert.IsNull(result);
    }

    [Test]
    public void CheckAnswer_WithCorrectOption_ReturnsTrue() {
        QuestionData question = new QuestionData(0, new List<long> { 1, 2, 3 }, 0, "easy");
        var selectedOptionIndex = 0;

        var result = quizService.CheckAnswer(question, selectedOptionIndex);

        Assert.IsTrue(result);
    }

    [Test]
    public void CheckAnswer_WithIncorrectOption_ReturnsFalse() {
        QuestionData question = new QuestionData(0, new List<long> { 1, 2, 3 }, 0, "easy");
        int selectedOptionIndex = 1;

        bool result = quizService.CheckAnswer(question, selectedOptionIndex);

        Assert.IsFalse(result);
    }
}