using UnityEngine;

public class KinectGizmo : MonoBehaviour {
  void OnDrawGizmos() {
    Gizmos.DrawIcon(transform.position, "Kinect_Symbol.png", true);
  }
}