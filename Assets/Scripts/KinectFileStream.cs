using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class KinectFileStream : KinectStream {
  public TextAsset File;

  string[] lines;
  int i;
  Regex jointDataMatcher;


  void Start() {
    jointDataMatcher = new Regex("(?<x>-?\\d+(?:\\.\\d+)?),\\s*(?<y>-?\\d+(?:\\.\\d+)?),\\s*(?<z>-?\\d+(?:\\.\\d+)?)");

    var splitFile = new string[] { "\r\n", "\r", "\n" };
    lines = File.text.Split(splitFile, StringSplitOptions.None);
    i = 0;

    JointData = new float[25 * 3];
  }


  void Update() {
    if (i < lines.Length) {
      string line = lines[i]; ++i;
      MatchCollection matches = jointDataMatcher.Matches(line);
      int g = 0;
      foreach(Match match in matches) {
        if (g >= 25) {
          break;
        }
        JointData[g * 3 + 0] = float.Parse(match.Groups["x"].Value);
        JointData[g * 3 + 1] = float.Parse(match.Groups["y"].Value);
        JointData[g * 3 + 2] = float.Parse(match.Groups["z"].Value);
        ++g;
      }
    }
  }
}
