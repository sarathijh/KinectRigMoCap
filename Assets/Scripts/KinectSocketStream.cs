using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class KinectSocketStream : KinectStream {
  public int Port = 16000;

  UdpClient udpClient;
  IPEndPoint ipEndPoint;
  Thread thread;

  bool threadDone = false;


  void Start() {
    udpClient = new UdpClient(Port);
    ipEndPoint = new IPEndPoint(IPAddress.Any, Port);

    thread = new Thread(new ThreadStart(ReadJointData));
    thread.IsBackground = true;
    thread.Start();
  }


  void ReadJointData() {
    try {
      while (!threadDone) {
        // Each coord is a 32 bit float
        Byte[] buffer = udpClient.Receive(ref ipEndPoint);
        float[] jointData = new float[buffer.Length / 4];
        Buffer.BlockCopy(buffer, 0, jointData, 0, buffer.Length);
        OnJointDataReceived(jointData);
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
