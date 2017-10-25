using UnityEngine;

public class KinectMultiStream : KinectStream {
  public KinectStream[] Streams;


  void Start() {
    JointData = new float[25 * 3];
  }


  void Update() {
    foreach (var stream in Streams) {
      for (int i = 0; i < 25 * 3; ++i) { JointData[i] += stream.JointData[i]; }
    }
    for (int i = 0; i < 25 * 3; ++i) { JointData[i] /= Streams.Length; }
  }
}
