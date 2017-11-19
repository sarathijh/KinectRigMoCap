using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UnityEngine;

public class KinectSocketStream : KinectStream {
  public string IP = "127.0.0.1";
  public int Port = 16000;
  public string OutputFile;

  UdpClient udpClient;
  IPEndPoint ipEndPoint;
  Thread thread;

  StreamWriter outputWriter;

  bool threadDone = false;


  void Start() {
    udpClient = new UdpClient(Port);
    ipEndPoint = new IPEndPoint(IPAddress.Any, Port);

    JointData = new float[25*3];

    outputWriter = new StreamWriter("Assets/Resources/" + OutputFile);

    thread = new Thread(new ThreadStart(ReadJointData));
    thread.IsBackground = true;
    thread.Start();
  }


  void ReadJointData() {
    try {
      while (!threadDone) {
        // Each coord is a 32 bit float
        Byte[] buffer = udpClient.Receive(ref ipEndPoint);
        Buffer.BlockCopy(buffer, 0, JointData, 0, buffer.Length);
        foreach (float val in JointData) {
          outputWriter.Write(val.ToString());
          outputWriter.Write(" ");
        }
        outputWriter.Write("\n");
      }
    } catch (Exception e) {
      Debug.Log("UDP Client Error: " + e);
    }
  }


  void OnApplicationQuit() {
    if (thread.IsAlive) {
      Debug.Log("Killing Kinect Stream Thread...");
      threadDone = true;
      thread.Abort();
    }
    udpClient.Close();
  }
}
