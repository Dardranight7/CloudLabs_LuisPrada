using UnityEngine;

public class DraggableContainer : MonoBehaviour
{
    public enum ContainerType
    {
        Pending,
        Approved,
        Reproved
    }

    [SerializeField] private ContainerType containerType;
    [SerializeField] private RectTransform content;

    public ContainerType Type => containerType;
    public RectTransform Content => content;
    public bool AcceptsDrop => containerType != ContainerType.Pending;
}
