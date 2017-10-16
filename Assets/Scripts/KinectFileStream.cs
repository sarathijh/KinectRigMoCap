using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class KinectFileStream : KinectStream {
  public TextAsset File;

  string[] lines;
  int i;
  Regex jointDataMatcher;


  void Start() {
    jointDataMatcher = new Regex("(?:\\((-?\\d+\\.\\d+),\\s*(-?\\d+\\.\\d+),\\s*(-?\\d+\\.\\d+)\\)\\s*){25}");

    var splitFile = new string[] { "\r\n", "\r", "\n" };
    lines = File.text.Split(splitFile, StringSplitOptions.None);
    i = 0;
  }


  void Update() {
    if (i < lines.Length) {
      float[] joints = new float[25 * 3];

      string line = lines[i]; ++i;
      Match match = jointDataMatcher.Match(line);

      int g = -1;
      foreach (Group group in match.Groups) {
        if (g >= 0 && g < 25) {
          int c = 0;
          foreach (Capture cap in group.Captures) {
            joints[c * 3 + g] = float.Parse(cap.ToString());
            ++c;
          }
        }
        ++g;
      }

      OnJointDataReceived(joints);
    }
  }
}
