using System.Collections;
using UnityEngine;

public class Augment : MonoBehaviour
{
    [SerializeField] private Transform _wrapperMain = null;

    [SerializeField] private bool _detachParent = false;
    [SerializeField] private bool _verticalLock = false;
    [SerializeField] private Transform[] _anchors = null;

    private Coroutine _detachParentWait;

    public void OnEnable()
    {
        ARSessionAction.ARTrackableDistanceMinSet += OnARTrackableDistanceMinSet;
    }

    public void OnDisable()
    {
        ARSessionAction.ARTrackableDistanceMinSet -= OnARTrackableDistanceMinSet;
    }

    private void Start()
    {
        ARSessionAction.AugmentInstantiated?.Invoke(this);
    }

    public void OnARTrackableDistanceMinSet(Transform parent, string anchorName)
    {
        foreach (var anchor in _anchors)
        {
            if (anchor.name == anchorName)
            {
                DebugOverlay.Log($"SetPlacement({anchorName})");
                SetPlacement(parent, anchor);

                break;
            }
        }
    }

    private void SetPlacement(Transform parent, Transform anchor)
    {
        if (_detachParentWait != null)
        {
            StopCoroutine(_detachParentWait);
            _detachParentWait = null;

            transform.parent = ARSessionManager.Origin;
        }

        _wrapperMain.localPosition = Quaternion.Inverse(anchor.localRotation) * (Vector3.zero - anchor.localPosition);
        _wrapperMain.localRotation = Quaternion.Inverse(anchor.localRotation);

        if (transform.parent == ARSessionManager.Origin)
        {
            transform.SetPositionAndRotation(parent.position, parent.rotation);
            transform.SetParent(parent, true);
        }
        else
        {
            transform.SetParent(parent, false);
        }

        if (_detachParent)
        {
            _detachParentWait = StartCoroutine(DetachParentWait());
        }
    }

    private IEnumerator DetachParentWait()
    {
        var counter = 0;
        var position = transform.position;
        while (true)
        {
            yield return new WaitForEndOfFrame();

            counter = Vector3.Distance(position, transform.position) == 0 ? counter + 1 : 0;
            if (counter > 2) { break; }

            position = transform.position;
        }

        transform.parent = ARSessionManager.Origin;

        _detachParentWait = null;
    }

    private void Update()
    {
        if (_verticalLock)
        {
            var eulerAngles = transform.rotation.eulerAngles;
            var contraRotation = new Vector3(-eulerAngles.x, 0f, -eulerAngles.z);
            transform.Rotate(contraRotation);
        }
    }

    private void OnValidate()
    {
        Debug.Assert(_wrapperMain != null, $"{this} is missing reference");
    }
}
