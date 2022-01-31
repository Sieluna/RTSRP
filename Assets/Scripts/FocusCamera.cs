using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class FocusCamera : MonoBehaviour
{
    public float FocusDistance = 10.0f;
    public float Aperture = 1.0f;

    [NonSerialized] public Vector3 LeftBottomCorner;
    [NonSerialized] public Vector3 RightTopCorner;
    [NonSerialized] public Vector2 Size;

    private Camera m_Camera;

    public void Update()
    {
        m_Camera ??= GetComponent<Camera>();

        var theta = m_Camera.fieldOfView * Mathf.Deg2Rad;
        var halfHeight = Mathf.Tan(theta * 0.5f);
        var halfWidth = m_Camera.aspect * halfHeight;
        LeftBottomCorner = transform.position + transform.forward * FocusDistance -
                           transform.right * FocusDistance * halfWidth -
                           transform.up * FocusDistance * halfHeight;
        Size = new Vector2(FocusDistance * halfWidth * 2.0f, FocusDistance * halfHeight * 2.0f);
        RightTopCorner = LeftBottomCorner + transform.right * Size.x + transform.up * Size.y;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var pt1 = LeftBottomCorner;
        var pt2 = pt1 + transform.right * Size.x;
        var pt3 = RightTopCorner;
        var pt4 = pt1 + transform.up * Size.y;
        Gizmos.DrawLine(pt1, pt2);
        Gizmos.DrawLine(pt2, pt3);
        Gizmos.DrawLine(pt3, pt4);
        Gizmos.DrawLine(pt4, pt1);
        Gizmos.DrawLine(pt1, pt3);
        Gizmos.DrawLine(pt2, pt4);
        Gizmos.DrawWireSphere(transform.position, Aperture * 0.5f);
        Gizmos.color = Color.white;
    }
}