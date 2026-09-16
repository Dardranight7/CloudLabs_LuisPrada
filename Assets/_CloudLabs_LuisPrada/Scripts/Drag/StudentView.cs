using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StudentView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Datos visuales")]
    [SerializeField] private TMP_Text studentNameText;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text initialsText;
    [SerializeField] private Image profileIcon;

    [Header("Arrastre")]
    [SerializeField] private bool canBeDragged = true;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private LayoutElement layoutElement;

    private StudentDTO student;
    private StudentView placeholderPrefab;
    private StudentView activePlaceholder;
    private RectTransform dragLayer;
    private DraggableContainer currentContainer;
    private DraggableContainer originContainer;
    private Camera eventCamera;
    private int originSiblingIndex;
    private bool isDragging;

    public event Action<StudentView> ContainerChanged;

    public StudentDTO Student => student;
    public DraggableContainer.ContainerType CurrentContainerType =>
        currentContainer != null ? currentContainer.Type : DraggableContainer.ContainerType.Pending;

    public bool IsCorrectlyClassified
    {
        get
        {
            if (student == null || currentContainer == null || currentContainer.Type == DraggableContainer.ContainerType.Pending)
            {
                return false;
            }

            bool shouldBeApproved = student.notaFinal >= 3f;
            return shouldBeApproved
                ? currentContainer.Type == DraggableContainer.ContainerType.Approved
                : currentContainer.Type == DraggableContainer.ContainerType.Reproved;
        }
    }

    public void Initialize(StudentDTO studentData, DraggableContainer startingContainer,
        RectTransform placeholderLayer, StudentView dragPlaceholderPrefab)
    {
        student = studentData;
        currentContainer = startingContainer;
        dragLayer = placeholderLayer;
        placeholderPrefab = dragPlaceholderPrefab;
        PopulateVisuals();
    }

    public void SetPreviewData(StudentDTO studentData)
    {
        student = studentData;
        PopulateVisuals();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canBeDragged || isDragging || student == null || placeholderPrefab == null || dragLayer == null)
        {
            return;
        }

        isDragging = true;
        originContainer = currentContainer;
        originSiblingIndex = transform.GetSiblingIndex();
        eventCamera = eventData.pressEventCamera;

        activePlaceholder = Instantiate(placeholderPrefab, dragLayer, false);
        activePlaceholder.name = $"DragCardPlaceHolder_{student.codigo}";
        activePlaceholder.SetPreviewData(student);
        activePlaceholder.gameObject.SetActive(true);
        activePlaceholder.transform.SetAsLastSibling();

        if (activePlaceholder.canvasGroup != null)
        {
            activePlaceholder.canvasGroup.blocksRaycasts = false;
            activePlaceholder.canvasGroup.interactable = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        if (layoutElement != null)
        {
            layoutElement.ignoreLayout = true;
        }

        MovePlaceholder(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        DraggableContainer targetContainer = null;
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            targetContainer = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<DraggableContainer>();
        }

        bool changedContainer = targetContainer != null && targetContainer.AcceptsDrop;
        if (changedContainer)
        {
            transform.SetParent(targetContainer.Content, false);
            currentContainer = targetContainer;
        }
        else if (originContainer != null)
        {
            transform.SetParent(originContainer.Content, false);
            transform.SetSiblingIndex(Mathf.Min(originSiblingIndex, transform.parent.childCount - 1));
            currentContainer = originContainer;
        }

        FinishDragging();

        if (changedContainer)
        {
            ContainerChanged?.Invoke(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            MovePlaceholder(eventData.position);
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            MovePlaceholder(Input.mousePosition);
        }
    }

    private void OnDisable()
    {
        if (isDragging)
        {
            FinishDragging();
        }
    }

    private void MovePlaceholder(Vector2 screenPosition)
    {
        if (activePlaceholder == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragLayer, screenPosition, eventCamera, out Vector3 worldPoint))
        {
            ((RectTransform)activePlaceholder.transform).position = worldPoint;
        }
    }

    private void FinishDragging()
    {
        isDragging = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        if (layoutElement != null)
        {
            layoutElement.ignoreLayout = false;
        }

        if (activePlaceholder != null)
        {
            Destroy(activePlaceholder.gameObject);
            activePlaceholder = null;
        }
    }

    private void PopulateVisuals()
    {
        if (student == null)
        {
            return;
        }

        string fullName = $"{student.nombre} {student.apellido}".Trim();
        string initials = GetInitials(student);

        if (studentNameText != null) studentNameText.text = fullName;
        if (gradeText != null) gradeText.text = $"Nota: {student.notaFinal.ToString("0.0", CultureInfo.InvariantCulture)}";
        if (initialsText != null) initialsText.text = initials;
        if (profileIcon != null) profileIcon.color = GetDeterministicColor(initials);
    }

    private static string GetInitials(StudentDTO studentData)
    {
        string firstInitial = string.IsNullOrWhiteSpace(studentData.nombre) ? string.Empty : studentData.nombre.Substring(0, 1);
        string lastInitial = string.IsNullOrWhiteSpace(studentData.apellido) ? string.Empty : studentData.apellido.Substring(0, 1);
        return (firstInitial + lastInitial).ToUpperInvariant();
    }

    private static Color GetDeterministicColor(string seed)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (char character in seed)
            {
                hash ^= character;
                hash *= 16777619;
            }

            return Color.HSVToRGB((hash % 360u) / 360f, 0.55f, 0.78f);
        }
    }
}
