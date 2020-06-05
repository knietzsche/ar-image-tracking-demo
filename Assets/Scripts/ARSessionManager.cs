using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARSessionManager : MonoBehaviour
{
    [SerializeField] private Augment _augmentPrefab = null;
    [SerializeField] private XRReferenceImageLibrary _referenceLibrary = null;
    [SerializeField] private bool _setActiveOnStart = false;

    public static Transform Origin = null;

    private ARSession _arSession;
    private ARTrackedImageManager _arTrackedImageManager;
    private Camera _camera;

    private bool IsActive => _arSession.enabled;

    private readonly List<ARTrackedImage> _trackedImageList = new List<ARTrackedImage>();
    private ARTrackedImage _trackedImageDistanceMin;
    private Augment _augment;

    private void Awake()
    {
        _arSession = GetComponentInChildren<ARSession>(true);
        _arTrackedImageManager = GetComponentInChildren<ARTrackedImageManager>(true);
        _camera = GetComponentInChildren<Camera>(true);

        Origin = _camera.transform.parent;
    }

    private void OnEnable()
    {
        ARSessionAction.SetActive += OnSetActive;
        _arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        ARSessionAction.AugmentInstantiated += OnAugmentInstantiated;
    }

    private void OnDisable()
    {
        ARSessionAction.SetActive -= OnSetActive;
        _arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        ARSessionAction.AugmentInstantiated -= OnAugmentInstantiated;
    }

    private void Start()
    {
        if (_setActiveOnStart)
        {
            ARSessionAction.SetActive(true);
        }
    }

    private void OnSetActive(bool? valueIn)
    {
        var value = valueIn ?? !IsActive;

        if (value)
        {
            _arSession.enabled = true;
            _arTrackedImageManager.referenceLibrary = _referenceLibrary;
            _arTrackedImageManager.enabled = true;
        }
        else
        {
            _arTrackedImageManager.enabled = false;
            _arTrackedImageManager.referenceLibrary = null;
            _arSession.enabled = false;

            Clear();
        }

        DebugOverlay.Log($"AR Session IsActive == {IsActive}");
    }

    private void Clear()
    {
        _trackedImageList.Clear();
        _augment = null;
        _trackedImageDistanceMin = null;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            AddTrackedImage(trackedImage);
        }
        foreach (var trackedImage in eventArgs.removed)
        {
            _trackedImageList.Remove(trackedImage);
        }
    }

    private void AddTrackedImage(ARTrackedImage trackedImage)
    {
        if (_augment == null)
        {
            Instantiate(_augmentPrefab.gameObject, trackedImage.transform);
        }

        _trackedImageList.Add(trackedImage);

        UpdateTrackedImageDistanceMin(trackedImage);
    }

    private void UpdateTrackedImageDistanceMin(ARTrackedImage trackedImage)
    {
        if (_trackedImageDistanceMin == trackedImage)
        {
            return;
        }

        _trackedImageDistanceMin = trackedImage;
        ARSessionAction.ARTrackableDistanceMinSet?.Invoke(trackedImage.transform, trackedImage.referenceImage.name);
    }

    private void OnAugmentInstantiated(Augment augment)
    {
        _augment = augment;

        DebugOverlay.Log($"Augment {augment.name} instantiated");
    }

    private void Update()
    {
        if (!IsActive)
        {
            return;
        }

        FindTrackedImageDistanceMin();
    }

    private void FindTrackedImageDistanceMin()
    {
        if (_trackedImageList.Count == 1)
        {
            UpdateTrackedImageDistanceMin(_trackedImageList[0]);
        }
        else
        {
            var distanceMin = float.MaxValue;
            ARTrackedImage found = null;

            foreach (var trackedImage in _trackedImageList)
            {
                var distance = Vector3.Distance(_camera.transform.position, trackedImage.transform.position);
                if (distance < distanceMin)
                {
                    distanceMin = distance;
                    found = trackedImage;
                }
            }
            if (found != _trackedImageDistanceMin)
            {
                UpdateTrackedImageDistanceMin(found);
            }
        }
    }

    private void OnValidate()
    {
        Debug.Assert(_augmentPrefab != null, $"{this} is missing reference");
        Debug.Assert(_referenceLibrary != null, $"{this} is missing reference");
    }
}