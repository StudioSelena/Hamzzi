using UnityEngine;
using UnityEngine.EventSystems;

public class HamsterModelRotate : MonoBehaviour, IDragHandler
{
    private Transform _hamsterRoot;
    [SerializeField] private float rotationSpeed = 0.5f;

    private void Start()
    {
        var modelTransform = ServiceManager.Instance.HamsterModelService.GetModelTransform();
        _hamsterRoot = modelTransform;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_hamsterRoot == null)
            return;

        _hamsterRoot.Rotate(
            Vector3.up,
            -eventData.delta.x * rotationSpeed,
            Space.World
        );
    }
}
