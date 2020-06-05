using System;
using UnityEngine;

public static class ARSessionAction
{
    public static Action<bool?> SetActive;
    public static Action<Augment> AugmentInstantiated;
    public static Action<Transform, string> ARTrackableDistanceMinSet;
}
