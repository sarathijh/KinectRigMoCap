using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class KinectRigAnimator : MonoBehaviour {
  enum Joint {
    SpineBase     =  0,    ShoulderRight =  8,    HipRight      = 16,
    SpineMid      =  1,    ElbowRight    =  9,    KneeRight     = 17,
    Neck          =  2,    WristRight    = 10,    AnkleRight    = 18,
    Head          =  3,    HandRight     = 11,    FootRight     = 19,
    ShoulderLeft  =  4,    HipLeft       = 12,    SpineShoulder = 20,
    ElbowLeft     =  5,    KneeLeft      = 13,    HandTipLeft   = 21,
    WristLeft     =  6,    AnkleLeft     = 14,    ThumbLeft     = 22,
    HandLeft      =  7,    FootLeft      = 15,    HandTipRight  = 23,
    ThumbRight    = 24,
  }
  static Joint[] Joints = (Joint[])Enum.GetValues(typeof(Joint));

  static int FilterWindow = 11;
  static float SmoothingFactor = 30;


  public Transform kinectReference;
  KinectStream kinectStream;

  public Transform LeftShoulderPoint;
  public Transform LeftElbowPoint;
  public Transform LeftWristPoint;

  public Transform RightShoulderPoint;
  public Transform RightElbowPoint;
  public Transform RightWristPoint;

  public Transform LeftHipPoint;
  public Transform LeftKneePoint;
  public Transform LeftAnklePoint;

  public Transform RightHipPoint;
  public Transform RightKneePoint;
  public Transform RightAnklePoint;
  
  public bool PointsOnlyMode = false;


  Animator animator;

  Transform Head;
  Transform Neck;
  Transform UpperChest;
  Transform Chest;
  Transform Spine;
  Transform Hips;
  Transform LeftShoulder;
  Transform LeftUpperArm;
  Transform LeftLowerArm;
  Transform LeftHand;
  Transform RightShoulder;
  Transform RightUpperArm;
  Transform RightLowerArm;
  Transform RightHand;
  Transform LeftUpperLeg;
  Transform LeftLowerLeg;
  Transform LeftFoot;
  Transform LeftToes;
  Transform RightUpperLeg;
  Transform RightLowerLeg;
  Transform RightFoot;
  Transform RightToes;

  Dictionary<Joint, Vector3> jointPositions;
  Dictionary<Joint, float> modelBoneLengths;
  Dictionary<Joint, Vector3[]> filteredJointPositions;
  float[] filterCoefficients;
  Vector3 HipOffset;
  float GroundVelocity = 0;


  void Start() {
    jointPositions = new Dictionary<Joint, Vector3>();
    foreach (var j in Joints) {
      jointPositions[j] = Vector3.zero;
    }

    filteredJointPositions = new Dictionary<Joint, Vector3[]>();
    foreach (Joint joint in Joints) {
      filteredJointPositions[joint] = new Vector3[FilterWindow];
    }
    filterCoefficients = new float[] { 0.197413f,  0.174666f,  0.120978f,  0.065591f,  0.027835f,  0.009245f,  0.002403f,  0.000489f,  0.000078f,  0.00001f, 0.000001f, };
    for (int i = 0; i < filterCoefficients.Length; ++i) {
      filterCoefficients[i] *= 5;
    }
    /*filterCoefficients[0] = 1f;
    for (int i = 1; i < FilterWindow; ++i) {
      filterCoefficients[i] = 0.5f / Mathf.Pow(2, i);
    }*/

    kinectStream = kinectReference.GetComponent<KinectStream>();

    animator = GetComponent<Animator>();

    Hips          = animator.GetBoneTransform (HumanBodyBones.Hips         );
    LeftUpperLeg  = animator.GetBoneTransform (HumanBodyBones.LeftUpperLeg );
    RightUpperLeg = animator.GetBoneTransform (HumanBodyBones.RightUpperLeg);
    LeftLowerLeg  = animator.GetBoneTransform (HumanBodyBones.LeftLowerLeg );
    RightLowerLeg = animator.GetBoneTransform (HumanBodyBones.RightLowerLeg);
    LeftFoot      = animator.GetBoneTransform (HumanBodyBones.LeftFoot     );
    RightFoot     = animator.GetBoneTransform (HumanBodyBones.RightFoot    );
    LeftToes      = animator.GetBoneTransform (HumanBodyBones.LeftToes     );
    RightToes     = animator.GetBoneTransform (HumanBodyBones.RightToes    );
    Spine         = animator.GetBoneTransform (HumanBodyBones.Spine        );
    Chest         = animator.GetBoneTransform (HumanBodyBones.Chest        );
    UpperChest    = animator.GetBoneTransform (HumanBodyBones.UpperChest   );
    Neck          = animator.GetBoneTransform (HumanBodyBones.Neck         );
    Head          = animator.GetBoneTransform (HumanBodyBones.Head         );
    LeftShoulder  = animator.GetBoneTransform (HumanBodyBones.LeftShoulder );
    RightShoulder = animator.GetBoneTransform (HumanBodyBones.RightShoulder);
    LeftUpperArm  = animator.GetBoneTransform (HumanBodyBones.LeftUpperArm );
    RightUpperArm = animator.GetBoneTransform (HumanBodyBones.RightUpperArm);
    LeftLowerArm  = animator.GetBoneTransform (HumanBodyBones.LeftLowerArm );
    RightLowerArm = animator.GetBoneTransform (HumanBodyBones.RightLowerArm);
    LeftHand      = animator.GetBoneTransform (HumanBodyBones.LeftHand     );
    RightHand     = animator.GetBoneTransform (HumanBodyBones.RightHand    );

    modelBoneLengths = new Dictionary<Joint, float>();

    modelBoneLengths[Joint.Neck] = (Neck.position - Hips.position).magnitude;
    modelBoneLengths[Joint.ShoulderLeft] = (Neck.position - LeftUpperArm.position).magnitude;
    modelBoneLengths[Joint.ElbowLeft] = (LeftUpperArm.position - LeftLowerArm.position).magnitude;
    modelBoneLengths[Joint.WristLeft] = (LeftLowerArm.position - LeftHand.position).magnitude;
    modelBoneLengths[Joint.ShoulderRight] = (Neck.position - RightUpperArm.position).magnitude;
    modelBoneLengths[Joint.ElbowRight] = (RightUpperArm.position - RightLowerArm.position).magnitude;
    modelBoneLengths[Joint.WristRight] = (RightLowerArm.position - RightHand.position).magnitude;
    modelBoneLengths[Joint.HipLeft] = (Hips.position - LeftUpperLeg.position).magnitude;
    modelBoneLengths[Joint.KneeLeft] = (LeftUpperLeg.position - LeftLowerLeg.position).magnitude;
    modelBoneLengths[Joint.AnkleLeft] = (LeftLowerLeg.position - LeftFoot.position).magnitude;
    modelBoneLengths[Joint.HipRight] = (Hips.position - RightUpperLeg.position).magnitude;
    modelBoneLengths[Joint.KneeRight] = (RightUpperLeg.position - RightLowerLeg.position).magnitude;
    modelBoneLengths[Joint.AnkleRight] = (RightLowerLeg.position - RightFoot.position).magnitude;

    HipOffset = Hips.position - transform.position;
  }


  static Vector3 ReadVector3(float[] data, int i) {
    return new Vector3(data[i * 3 + 0],
                       data[i * 3 + 1],
                       data[i * 3 + 2]);
  }


  Vector3 GetJoint(Joint joint) {
    return Vector3.Scale(kinectReference.TransformPoint(jointPositions[joint]), new Vector3(-1, 1, 1));//Hips.position + (jointPositions[joint] - hips) * modelBoneLengths[joint] / kinectBoneLengths[joint];
  }


  void UpdateJoints() {
    foreach (var j in Joints) {
      jointPositions[j] = ReadVector3(kinectStream.JointData, (int)j);
    }
  }


  private static float FIR(float[] b, float[] x) {
    int M = b.Length;
    int n = x.Length;
    //y[n]=b0x[n]+b1x[n-1]+....bmx[n-M]
    var y = new float[n];
    for (int yi = 0; yi < n; yi++) {
      float t = 0.0f;
      for (int bi = M - 1; bi >= 0; bi--) {
        if (yi - bi < 0) continue;

        t += b[bi] * x[yi - bi];
      }
      y[yi] = t;
    }
    return y[0];
  }


  Vector3 UpdateJointPosition(Joint joint, Vector3 position) {
    for (int i = 1; i < FilterWindow; ++i) {
      filteredJointPositions[joint][i] = filteredJointPositions[joint][i-1];
    }
    filteredJointPositions[joint][0] = position;
    var filtered = new Vector3(
      FIR(filterCoefficients, filteredJointPositions[joint].Select(v => v.x).ToArray()),
      FIR(filterCoefficients, filteredJointPositions[joint].Select(v => v.y).ToArray()),
      FIR(filterCoefficients, filteredJointPositions[joint].Select(v => v.z).ToArray()));
    return filtered;
  }


  void OnAnimatorIK(int layerIndex) {
    UpdateJoints();

    Vector3 rot = Vector3.Cross(GetJoint(Joint.HipLeft) - GetJoint(Joint.HipRight), Vector3.up);
    if (rot == Vector3.zero) {
      transform.rotation = Quaternion.Lerp(transform.rotation, kinectReference.rotation, Time.deltaTime*30);
    } else {
      transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(rot) * kinectReference.rotation, Time.deltaTime * SmoothingFactor);
    }
    Vector3 pos = (GetJoint(Joint.HipLeft) + GetJoint(Joint.HipRight)) / 2;

    LeftShoulderPoint.position = Vector3.Lerp(LeftShoulderPoint.position, Hips.position + modelBoneLengths[Joint.Neck] * (GetJoint(Joint.Neck) - (GetJoint(Joint.HipLeft) + GetJoint(Joint.HipRight)) / 2).normalized +
        modelBoneLengths[Joint.ShoulderLeft] * (GetJoint(Joint.ShoulderLeft) - GetJoint(Joint.Neck)).normalized, Time.deltaTime * SmoothingFactor);
    LeftElbowPoint.position = Vector3.Lerp(LeftElbowPoint.position, LeftShoulderPoint.position +
      modelBoneLengths[Joint.ElbowLeft] * (GetJoint(Joint.ElbowLeft) - GetJoint(Joint.ShoulderLeft)).normalized, Time.deltaTime * SmoothingFactor);
    LeftWristPoint.position = Vector3.Lerp(LeftWristPoint.position, LeftElbowPoint.position +
      modelBoneLengths[Joint.WristLeft] * (GetJoint(Joint.WristLeft) - GetJoint(Joint.ElbowLeft)).normalized, Time.deltaTime * SmoothingFactor);

    RightShoulderPoint.position = Hips.position + modelBoneLengths[Joint.Neck] * (GetJoint(Joint.Neck) - (GetJoint(Joint.HipLeft) + GetJoint(Joint.HipRight)) / 2).normalized +
      modelBoneLengths[Joint.ShoulderRight] * (GetJoint(Joint.ShoulderRight) - GetJoint(Joint.Neck)).normalized;
    RightElbowPoint.position = RightShoulderPoint.position +
      modelBoneLengths[Joint.ElbowRight] * (GetJoint(Joint.ElbowRight) - GetJoint(Joint.ShoulderRight)).normalized;
    RightWristPoint.position = RightElbowPoint.position +
      modelBoneLengths[Joint.WristRight] * (GetJoint(Joint.WristRight) - GetJoint(Joint.ElbowRight)).normalized;

    LeftHipPoint.position = Vector3.Lerp(LeftHipPoint.position, Hips.position +
      modelBoneLengths[Joint.HipLeft] * (GetJoint(Joint.HipLeft) - GetJoint(Joint.HipRight)).normalized, Time.deltaTime * SmoothingFactor);
    LeftKneePoint.position = Vector3.Lerp(LeftKneePoint.position, LeftHipPoint.position +
      modelBoneLengths[Joint.KneeLeft] * 1.2f * (GetJoint(Joint.KneeLeft) - GetJoint(Joint.HipLeft)).normalized, Time.deltaTime * SmoothingFactor);
    LeftAnklePoint.position = Vector3.Lerp(LeftAnklePoint.position, LeftKneePoint.position +
      modelBoneLengths[Joint.AnkleLeft] * 1.1f * (GetJoint(Joint.AnkleLeft) - GetJoint(Joint.KneeLeft)).normalized, Time.deltaTime * SmoothingFactor);

    RightHipPoint.position = Hips.position +
      modelBoneLengths[Joint.HipRight] * (GetJoint(Joint.HipRight) - GetJoint(Joint.HipLeft)).normalized;
    RightKneePoint.position = RightHipPoint.position +
      modelBoneLengths[Joint.KneeRight] * 1.2f * (GetJoint(Joint.KneeRight) - GetJoint(Joint.HipRight)).normalized;
    RightAnklePoint.position = RightKneePoint.position +
      modelBoneLengths[Joint.AnkleRight] * 1.1f * (GetJoint(Joint.AnkleRight) - GetJoint(Joint.KneeRight)).normalized;

    LeftWristPoint.position = UpdateJointPosition(Joint.WristLeft, LeftWristPoint.position);
    LeftAnklePoint.position = UpdateJointPosition(Joint.AnkleLeft, LeftAnklePoint.position);
    RightWristPoint.position = UpdateJointPosition(Joint.WristRight, RightWristPoint.position);
    RightAnklePoint.position = UpdateJointPosition(Joint.AnkleRight, RightAnklePoint.position);

    float CurrentHipOffset = (RightHipPoint.position.y + LeftHipPoint.position.y) / 2f - Mathf.Min(LeftAnklePoint.position.y, RightAnklePoint.position.y);

    /*const float threshold = 0.2f;
    RaycastHit hit;
    if (Physics.Raycast(pos + Vector3.down * CurrentHipOffset + Vector3.up * threshold, Vector3.down, out hit, threshold * 2)) {
      pos.y = hit.point.y + CurrentHipOffset + 0.2f; // TODO: Mark bottom of feet with bone
    }*/

    transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime*30);

    animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftWristPoint.position);
    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);

    animator.SetIKHintPosition(AvatarIKHint.LeftElbow, LeftElbowPoint.position);
    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0.5f);

    animator.SetIKPosition(AvatarIKGoal.RightHand, RightWristPoint.position);
    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);

    animator.SetIKHintPosition(AvatarIKHint.RightElbow, RightElbowPoint.position);
    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0.5f);

    animator.SetIKPosition(AvatarIKGoal.LeftFoot, LeftAnklePoint.position);
    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

    animator.SetIKHintPosition(AvatarIKHint.LeftKnee, LeftKneePoint.position);
    animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 0.5f);

    animator.SetIKPosition(AvatarIKGoal.RightFoot, RightAnklePoint.position);
    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

    animator.SetIKHintPosition(AvatarIKHint.RightKnee, RightKneePoint.position);
    animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 0.5f);
  }


  void LateUpdate() {
    LeftHand.transform.localRotation = Quaternion.identity;
    RightHand.transform.localRotation = Quaternion.identity;

    Spine.localRotation = Quaternion.Lerp(Spine.localRotation, Quaternion.FromToRotation(Vector3.up, (GetJoint(Joint.SpineMid) - GetJoint(Joint.SpineBase)).normalized), Time.deltaTime*30);
    Neck.localRotation = Quaternion.Lerp(Neck.localRotation, Quaternion.FromToRotation((GetJoint(Joint.SpineMid) - GetJoint(Joint.SpineBase)).normalized, (GetJoint(Joint.Head) - GetJoint(Joint.Neck)).normalized), Time.deltaTime * SmoothingFactor);
    Chest.localRotation = Quaternion.Lerp(Chest.localRotation, Quaternion.FromToRotation(GetJoint(Joint.SpineMid), GetJoint(Joint.SpineShoulder)), Time.deltaTime * SmoothingFactor);
  }
}
