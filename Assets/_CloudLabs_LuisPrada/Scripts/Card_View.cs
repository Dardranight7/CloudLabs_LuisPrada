using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card_View : MonoBehaviour
{
    private const float PassingGrade = 3f;

    [Header("Campos del estudiante")]
    [SerializeField] private TMP_Text initialsText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text statusText;

    [Header("Color del perfil")]
    [SerializeField] private Image profileIcon;

    [Header("Clasificación")]
    [SerializeField] private Button approvalButton;
    [SerializeField] private Image statusIcon;
    [SerializeField] private Image gradeIcon;
    [SerializeField] private Sprite approvedSprite;
    [SerializeField] private Sprite repprovedSprite;
    [SerializeField] private Color approvedGradeColor = new Color32(0x27, 0x98, 0x5E, 0xFF);
    [SerializeField] private Color repprovedGradeColor = new Color32(0xCE, 0x44, 0x4A, 0xFF);
    [SerializeField] private bool startsApproved = true;

    private float grade;
    private bool isApproved;

    public bool IsApproved => isApproved;
    public bool IsCorrectlyClassified => isApproved == (grade >= PassingGrade);

    private void Awake()
    {
        if (approvalButton != null)
        {
            approvalButton.onClick.AddListener(ToggleApproval);
        }
    }

    private void OnDestroy()
    {
        if (approvalButton != null)
        {
            approvalButton.onClick.RemoveListener(ToggleApproval);
        }
    }

    public void SetData(StudentDTO student)
    {
        if (student == null)
        {
            Debug.LogWarning("Card_View recibió un estudiante nulo.", this);
            return;
        }

        string fullName = $"{student.nombre} {student.apellido}".Trim();

        grade = student.notaFinal;
        SetText(initialsText, GetInitials(student));
        SetText(nameText, fullName);
        SetText(codeText, student.codigo);
        SetText(emailText, student.correo);
        SetText(gradeText, grade.ToString("0.0", CultureInfo.InvariantCulture));

        SetProfileColor(fullName);
        SetApprovalState(startsApproved);
    }

    public void ToggleApproval()
    {
        SetApprovalState(!isApproved);
    }

    private void SetApprovalState(bool approved)
    {
        isApproved = approved;
        SetText(statusText, approved ? "Aprobado" : "Reprobado");

        if (statusIcon != null)
        {
            statusIcon.sprite = approved ? approvedSprite : repprovedSprite;
        }

        if (gradeIcon != null)
        {
            gradeIcon.color = approved ? approvedGradeColor : repprovedGradeColor;
        }
    }

    private void SetProfileColor(string seed)
    {
        if (profileIcon == null)
        {
            return;
        }

        uint hash = 2166136261;
        foreach (char character in seed)
        {
            hash ^= char.ToUpperInvariant(character);
            hash *= 16777619;
        }

        float hue = (hash % 360) / 360f;
        float saturation = 0.5f + ((hash >> 8) % 20) / 100f;
        Color color = Color.HSVToRGB(hue, saturation, 0.85f);
        color.a = profileIcon.color.a;
        profileIcon.color = color;
    }

    private static void SetText(TMP_Text field, string value)
    {
        if (field != null)
        {
            field.text = value;
        }
    }

    private static string GetInitials(StudentDTO student)
    {
        string firstInitial = string.IsNullOrWhiteSpace(student.nombre)
            ? string.Empty
            : student.nombre.Substring(0, 1);
        string lastInitial = string.IsNullOrWhiteSpace(student.apellido)
            ? string.Empty
            : student.apellido.Substring(0, 1);

        return (firstInitial + lastInitial).ToUpperInvariant();
    }
}
