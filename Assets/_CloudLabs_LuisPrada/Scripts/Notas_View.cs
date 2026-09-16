using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notas_View : MonoBehaviour
{
    [Header("Referencias de la vista")]
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TMP_Text studentCountText;

    [Header("Navegación")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject classificationWindow;

    [Header("Validación")]
    [SerializeField] private Button validateButton;
    [SerializeField] private GameObject greenCheck;
    [SerializeField] private TMP_Text greenCheckText;
    [SerializeField] private GameObject redCheck;
    [SerializeField] private TMP_Text redCheckText;

    private Transform content;
    private readonly List<Card_View> cards = new List<Card_View>();

    private void Awake()
    {
        content = scrollView != null ? scrollView.content : null;
    }

    private void OnEnable()
    {
        DataManager.OnDataChange += Refresh;

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ShowClassification);
        }

        if (validateButton != null)
        {
            validateButton.onClick.AddListener(ValidateAnswers);
        }

        if (DataManager.HasData)
        {
            Refresh();
        }
    }

    private void OnDisable()
    {
        DataManager.OnDataChange -= Refresh;

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(ShowClassification);
        }

        if (validateButton != null)
        {
            validateButton.onClick.RemoveListener(ValidateAnswers);
        }
    }

    public void ShowClassification()
    {
        if (classificationWindow == null)
        {
            Debug.LogWarning("Notas_View no encontró la ventana de Clasificación.", this);
            return;
        }

        classificationWindow.SetActive(true);
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        content = scrollView != null ? scrollView.content : null;

        if (content == null || cardPrefab == null)
        {
            Debug.LogWarning("Notas_View necesita un Scroll View con Content y un prefab Card.", this);
            return;
        }

        ClearGeneratedCards();
        cards.Clear();
        HideValidationResult();

        foreach (StudentDTO student in DataManager.Students)
        {
            GameObject card = Instantiate(cardPrefab, content, false);
            card.name = $"Card_{student.codigo}";
            card.SetActive(true);

            Card_View cardView = card.GetComponent<Card_View>();
            if (cardView == null)
            {
                Debug.LogError("El prefab Card necesita el componente Card_View.", card);
                continue;
            }

            cardView.SetData(student);
            cards.Add(cardView);
        }

        if (studentCountText != null)
        {
            studentCountText.text = $"{DataManager.Students.Count} estudiantes";
        }
    }

    public void ValidateAnswers()
    {
        int incorrectCount = 0;
        foreach (Card_View card in cards)
        {
            if (card != null && !card.IsCorrectlyClassified)
            {
                incorrectCount++;
            }
        }

        int total = cards.Count;
        bool allCorrect = total > 0 && incorrectCount == 0;
        if (greenCheck != null)
        {
            greenCheck.SetActive(allCorrect);
        }

        if (redCheck != null)
        {
            redCheck.SetActive(!allCorrect);
        }

        if (allCorrect && greenCheckText != null)
        {
            greenCheckText.text =
                $"¡Todo correcto! {total} de {total} estudiantes clasificados correctamente.";
        }
        else if (!allCorrect && redCheckText != null)
        {
            redCheckText.text =
                $"¡Incorrecto! {incorrectCount} de {total} estudiantes mal calificados";
        }
    }

    private void HideValidationResult()
    {
        if (greenCheck != null)
        {
            greenCheck.SetActive(false);
        }

        if (redCheck != null)
        {
            redCheck.SetActive(false);
        }
    }

    private void ClearGeneratedCards()
    {
        for (int index = content.childCount - 1; index >= 0; index--)
        {
            Transform child = content.GetChild(index);
            if (child.gameObject != cardPrefab)
            {
                Destroy(child.gameObject);
            }
        }
    }

}
