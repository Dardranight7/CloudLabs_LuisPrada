using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Clasification_View : MonoBehaviour
{
    [Header("Tarjetas")]
    [SerializeField] private StudentView dragCardPrefab;
    [SerializeField] private StudentView dragCardPlaceholderPrefab;
    [SerializeField] private RectTransform dragLayer;
    [SerializeField] private DraggableContainer pendingContainer;
    [SerializeField] private DraggableContainer approvedContainer;
    [SerializeField] private DraggableContainer reprovedContainer;
    [SerializeField] private TMP_Text studentCountText;

    [Header("Validación")]
    [SerializeField] private Button validateButton;
    [SerializeField] private GameObject greenCheck;
    [SerializeField] private TMP_Text greenCheckText;
    [SerializeField] private GameObject redCheck;
    [SerializeField] private TMP_Text redCheckText;

    private readonly List<StudentView> studentViews = new List<StudentView>();

    private void OnEnable()
    {
        DataManager.OnDataChange += Refresh;

        if (validateButton != null)
        {
            validateButton.onClick.AddListener(ValidateClassification);
        }

        if (DataManager.HasData) Refresh();
        else UpdateValidateButton();
    }

    private void OnDisable()
    {
        DataManager.OnDataChange -= Refresh;

        if (validateButton != null)
        {
            validateButton.onClick.RemoveListener(ValidateClassification);
        }

        UnsubscribeCards();
    }

    private void Refresh()
    {
        if (dragCardPrefab == null || dragCardPlaceholderPrefab == null || dragLayer == null ||
            pendingContainer == null || approvedContainer == null || reprovedContainer == null ||
            pendingContainer.Content == null || approvedContainer.Content == null || reprovedContainer.Content == null)
        {
            Debug.LogWarning("Clasification_View tiene referencias serializadas pendientes de asignar.", this);
            return;
        }

        ClearGeneratedCards();
        HideValidationResult();

        foreach (StudentDTO student in DataManager.Students)
        {
            StudentView card = Instantiate(dragCardPrefab, pendingContainer.Content, false);
            card.name = $"DragCard_{student.codigo}";
            card.gameObject.SetActive(true);
            card.Initialize(student, pendingContainer, dragLayer, dragCardPlaceholderPrefab);
            card.ContainerChanged += OnStudentContainerChanged;
            studentViews.Add(card);
        }

        if (studentCountText != null)
        {
            studentCountText.text = $"{DataManager.Students.Count} estudiantes";
        }

        UpdateValidateButton();
    }

    public void ValidateClassification()
    {
        if (studentViews.Count == 0 || HasPendingStudents()) return;

        int incorrectCount = 0;
        foreach (StudentView studentView in studentViews)
        {
            if (studentView == null || !studentView.IsCorrectlyClassified) incorrectCount++;
        }

        int total = studentViews.Count;
        bool allCorrect = incorrectCount == 0;

        if (greenCheck != null) greenCheck.SetActive(allCorrect);
        if (redCheck != null) redCheck.SetActive(!allCorrect);

        if (allCorrect && greenCheckText != null)
        {
            greenCheckText.text = $"¡Todo correcto! {total} de {total} estudiantes clasificados correctamente.";
        }
        else if (!allCorrect && redCheckText != null)
        {
            redCheckText.text = $"¡Incorrecto! {incorrectCount} de {total} estudiantes mal clasificados.";
        }
    }

    private void OnStudentContainerChanged(StudentView studentView)
    {
        HideValidationResult();
        UpdateValidateButton();
    }

    private void UpdateValidateButton()
    {
        if (validateButton != null)
        {
            validateButton.interactable = studentViews.Count > 0 && !HasPendingStudents();
        }
    }

    private bool HasPendingStudents()
    {
        foreach (StudentView studentView in studentViews)
        {
            if (studentView != null && studentView.CurrentContainerType == DraggableContainer.ContainerType.Pending)
            {
                return true;
            }
        }

        return false;
    }

    private void HideValidationResult()
    {
        if (greenCheck != null) greenCheck.SetActive(false);
        if (redCheck != null) redCheck.SetActive(false);
    }

    private void ClearGeneratedCards()
    {
        UnsubscribeCards();
        ClearContainer(pendingContainer);
        ClearContainer(approvedContainer);
        ClearContainer(reprovedContainer);
        studentViews.Clear();
    }

    private void UnsubscribeCards()
    {
        foreach (StudentView studentView in studentViews)
        {
            if (studentView != null) studentView.ContainerChanged -= OnStudentContainerChanged;
        }
    }

    private static void ClearContainer(DraggableContainer container)
    {
        if (container == null || container.Content == null) return;

        for (int index = container.Content.childCount - 1; index >= 0; index--)
        {
            Transform child = container.Content.GetChild(index);
            if (child.GetComponent<StudentView>() != null) Destroy(child.gameObject);
        }
    }
}
