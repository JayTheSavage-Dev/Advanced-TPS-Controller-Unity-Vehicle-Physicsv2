#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
[AddComponentMenu("Advanced TPS/Debug/Weapon Alignment Debug Gizmo")]
public class WeaponAlignmentDebugGizmo : MonoBehaviour
{
    [Serializable]
    private class SlotBindPose
    {
        public ActiveWeapon.WeaponSlot slot;
        public Vector3 leftGripLocalPosition;
        public Vector3 leftGripLocalEuler;
        public Vector3 rightGripLocalPosition;
        public Vector3 rightGripLocalEuler;
    }

    [SerializeField] private ActiveWeapon activeWeapon;
    [SerializeField] private ReloadWeapon reloadWeapon;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform weaponRoot;
    [SerializeField] private float maxHandDistanceMeters = 0.09f;
    [SerializeField] private float maxRotationDeltaDegrees = 20f;
    [SerializeField] private bool logWarnings = true;
    [SerializeField] private List<SlotBindPose> expectedBindPose = new List<SlotBindPose>();

    private double _nextLogTime;

    private void Reset()
    {
        activeWeapon = GetComponent<ActiveWeapon>();
        reloadWeapon = GetComponent<ReloadWeapon>();
        if (activeWeapon != null)
        {
            weaponRoot = activeWeapon.transform;
        }

        if (leftHand == null)
        {
            leftHand = FindByNameContains(transform, "lefthand") ?? FindByNameContains(transform, "left hand");
        }

        if (rightHand == null)
        {
            rightHand = FindByNameContains(transform, "righthand") ?? FindByNameContains(transform, "right hand");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (activeWeapon == null || activeWeapon.GetActiveWeapon() == null)
        {
            return;
        }

        Transform leftGrip = activeWeapon.WeaponLeftGrip;
        Transform rightGrip = activeWeapon.WeaponRightGrip;

        DrawPair(leftHand, leftGrip, Color.cyan, "L");
        DrawPair(rightHand, rightGrip, Color.magenta, "R");

        DrawMagazineHelpers(activeWeapon.GetActiveWeapon());
        CheckBindPose(activeWeapon.GetActiveWeapon().WeaponSlotType, leftGrip, rightGrip);
    }

    private void DrawPair(Transform hand, Transform grip, Color color, string label)
    {
        if (hand == null || grip == null)
        {
            return;
        }

        Gizmos.color = color;
        Gizmos.DrawSphere(hand.position, 0.01f);
        Gizmos.DrawWireSphere(grip.position, 0.0125f);
        Gizmos.DrawLine(hand.position, grip.position);
        Handles.Label(grip.position, $"{label} grip");

        float handDistance = Vector3.Distance(hand.position, grip.position);
        float rotationDelta = Quaternion.Angle(hand.rotation, grip.rotation);
        if (handDistance > maxHandDistanceMeters || rotationDelta > maxRotationDeltaDegrees)
        {
            WarnThrottled($"[{name}] {label} hand mismatch: dist={handDistance:F3}m rot={rotationDelta:F1}°");
        }
    }

    private void DrawMagazineHelpers(GunController weapon)
    {
        if (weapon == null || weaponRoot == null)
        {
            return;
        }

        Transform magazine = weapon.Magazine != null ? weapon.Magazine.transform : FindByNameContains(weapon.transform, "mag");
        if (magazine == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(magazine.position, Vector3.one * 0.03f);
        Handles.Label(magazine.position, "magazine");

        Transform release = FindByNameContains(weapon.transform, "magrelease");
        if (release != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(release.position, 0.009f);
            Gizmos.DrawLine(release.position, magazine.position);
            Handles.Label(release.position, "mag release");
        }
    }

    private void CheckBindPose(ActiveWeapon.WeaponSlot slot, Transform leftGrip, Transform rightGrip)
    {
        SlotBindPose bindPose = null;
        for (int i = 0; i < expectedBindPose.Count; i++)
        {
            if (expectedBindPose[i] != null && expectedBindPose[i].slot == slot)
            {
                bindPose = expectedBindPose[i];
                break;
            }
        }

        if (bindPose == null || leftGrip == null || rightGrip == null)
        {
            return;
        }

        float leftPosDelta = Vector3.Distance(leftGrip.localPosition, bindPose.leftGripLocalPosition);
        float rightPosDelta = Vector3.Distance(rightGrip.localPosition, bindPose.rightGripLocalPosition);
        float leftRotDelta = Quaternion.Angle(leftGrip.localRotation, Quaternion.Euler(bindPose.leftGripLocalEuler));
        float rightRotDelta = Quaternion.Angle(rightGrip.localRotation, Quaternion.Euler(bindPose.rightGripLocalEuler));

        if (leftPosDelta > maxHandDistanceMeters || rightPosDelta > maxHandDistanceMeters || leftRotDelta > maxRotationDeltaDegrees || rightRotDelta > maxRotationDeltaDegrees)
        {
            WarnThrottled($"[{name}] Bind pose mismatch for {slot}: LP {leftPosDelta:F3} RP {rightPosDelta:F3} LR {leftRotDelta:F1} RR {rightRotDelta:F1}");
        }
    }

    private void WarnThrottled(string msg)
    {
        if (!logWarnings)
        {
            return;
        }

        if (EditorApplication.timeSinceStartup < _nextLogTime)
        {
            return;
        }

        _nextLogTime = EditorApplication.timeSinceStartup + 0.5d;
        Debug.LogWarning(msg, this);
    }

    private static Transform FindByNameContains(Transform root, string partial)
    {
        if (root == null)
        {
            return null;
        }

        partial = partial.ToLowerInvariant();
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            if (current.name.ToLowerInvariant().Contains(partial))
            {
                return current;
            }

            for (int i = 0; i < current.childCount; i++)
            {
                queue.Enqueue(current.GetChild(i));
            }
        }

        return null;
    }
}
#endif
