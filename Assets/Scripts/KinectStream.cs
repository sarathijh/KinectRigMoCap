using UnityEngine;

public class KinectStream : MonoBehaviour {
  public delegate void JointDataReceivedDelegate (float[] joints);

  public JointDataReceivedDelegate OnJointDataReceived;
}
