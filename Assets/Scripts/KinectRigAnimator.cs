using System;
using System.Collections.Generic;
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

  public enum AnimationTypeEnum {
    DirectPosition,
    RotationFromPosition,
  }
  public AnimationTypeEnum AnimationType = AnimationTypeEnum.DirectPosition;
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
  Vector3 HipOffset;
  float GroundVelocity = 0;


  void Start() {
    jointPositions = new Dictionary<Joint, Vector3>();
    foreach (var j in Joints) {
      jointPositions[j] = Vector3.zero;
    }

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


  void OnAnimatorIK(int layerIndex) {
    UpdateJoints();

    Vector3 rot = Vector3.Cross(GetJoint(Joint.HipLeft) - GetJoint(Joint.HipRight), Vector3.up);
    if (rot == Vector3.zero) {
      transform.rotation = kinectReference.rotation;
    } else {
      transform.rotation = Quaternion.LookRotation(rot) * kinectReference.rotation;
    }
    Vector3 pos = (GetJoint(Joint.HipLeft) + GetJoint(Joint.HipRight)) / 2;

    LeftShoulderPoint.position = Hips.position + modelBoneLengths[Joint.Neck] * (GetJoint(Joint.Neck) - (GetJoint(Joint.HipLeft) + GetJoint(Joint.HipRight)) / 2).normalized +
        modelBoneLengths[Joint.ShoulderLeft] * (GetJoint(Joint.ShoulderLeft) - GetJoint(Joint.Neck)).normalized;
    LeftElbowPoint.position = LeftShoulderPoint.position +
      modelBoneLengths[Joint.ElbowLeft] * (GetJoint(Joint.ElbowLeft) - GetJoint(Joint.ShoulderLeft)).normalized;
    LeftWristPoint.position = LeftElbowPoint.position +
      modelBoneLengths[Joint.WristLeft] * (GetJoint(Joint.WristLeft) - GetJoint(Joint.ElbowLeft)).normalized;

    RightShoulderPoint.position = Hips.position + modelBoneLengths[Joint.Neck] * (GetJoint(Joint.Neck) - (GetJoint(Joint.HipLeft) + GetJoint(Joint.HipRight)) / 2).normalized +
      modelBoneLengths[Joint.ShoulderRight] * (GetJoint(Joint.ShoulderRight) - GetJoint(Joint.Neck)).normalized;
    RightElbowPoint.position = RightShoulderPoint.position +
      modelBoneLengths[Joint.ElbowRight] * (GetJoint(Joint.ElbowRight) - GetJoint(Joint.ShoulderRight)).normalized;
    RightWristPoint.position = RightElbowPoint.position +
      modelBoneLengths[Joint.WristRight] * (GetJoint(Joint.WristRight) - GetJoint(Joint.ElbowRight)).normalized;

    LeftHipPoint.position = Hips.position +
      modelBoneLengths[Joint.HipLeft] * (GetJoint(Joint.HipLeft) - GetJoint(Joint.HipRight)).normalized;
    LeftKneePoint.position = LeftHipPoint.position +
      modelBoneLengths[Joint.KneeLeft] * 1.2f * (GetJoint(Joint.KneeLeft) - GetJoint(Joint.HipLeft)).normalized;
    LeftAnklePoint.position = LeftKneePoint.position +
      modelBoneLengths[Joint.AnkleLeft] * 1.1f * (GetJoint(Joint.AnkleLeft) - GetJoint(Joint.KneeLeft)).normalized;

    RightHipPoint.position = Hips.position +
      modelBoneLengths[Joint.HipRight] * (GetJoint(Joint.HipRight) - GetJoint(Joint.HipLeft)).normalized;
    RightKneePoint.position = RightHipPoint.position +
      modelBoneLengths[Joint.KneeRight] * 1.2f * (GetJoint(Joint.KneeRight) - GetJoint(Joint.HipRight)).normalized;
    RightAnklePoint.position = RightKneePoint.position +
      modelBoneLengths[Joint.AnkleRight] * 1.1f * (GetJoint(Joint.AnkleRight) - GetJoint(Joint.KneeRight)).normalized;

    //const float threshold = 1f;
    //RaycastHit hit;
    //if (Physics.Raycast(pos - HipOffset + Vector3.up * threshold, Vector3.down, out hit, threshold*2)) {
      //pos.y = Mathf.SmoothDamp(pos.y, hit.point.y + HipOffset.y + 0.1f, ref GroundVelocity, 0.3f);
      //pos.y = hit.point.y + HipOffset.y + 0.1f;
    //}

    transform.position = pos;

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

    Spine.localRotation = Quaternion.FromToRotation(Vector3.up, (GetJoint(Joint.SpineMid) - GetJoint(Joint.SpineBase)).normalized);
    Neck.localRotation = Quaternion.FromToRotation((GetJoint(Joint.SpineMid) - GetJoint(Joint.SpineBase)).normalized, (GetJoint(Joint.Head) - GetJoint(Joint.Neck)).normalized);
    //Chest.localRotation = Quaternion.LookRotation((GetJoint(Joint.SpineShoulder) - GetJoint(Joint.SpineMid)).normalized);
  }


  void AnimateDirectPosition() {
    if (Hips)          { Hips.position          = (jointPositions[Joint.HipLeft] + jointPositions[Joint.HipRight]) / 2; }
    if (Spine)         { Spine.position         = jointPositions[Joint.SpineBase]; }
    if (Chest)         { Chest.position         = jointPositions[Joint.SpineMid]; }
    if (UpperChest)    { UpperChest.position    = jointPositions[Joint.SpineShoulder]; }
    if (Neck)          { Neck.position          = jointPositions[Joint.Neck]; }
    if (Head)          { Head.position          = jointPositions[Joint.Head]; }

    if (LeftUpperArm)  { LeftUpperArm.position  = jointPositions[Joint.ShoulderLeft]; }
    if (LeftLowerArm)  { LeftLowerArm.position  = jointPositions[Joint.ElbowLeft]; }
    if (LeftHand)      { LeftHand.position      = jointPositions[Joint.WristLeft]; }

    if (RightUpperArm) { RightUpperArm.position = jointPositions[Joint.ShoulderRight]; }
    if (RightLowerArm) { RightLowerArm.position = jointPositions[Joint.ElbowRight]; }
    if (RightHand)     { RightHand.position     = jointPositions[Joint.WristRight]; }

    if (LeftUpperLeg)  { LeftUpperLeg.position  = jointPositions[Joint.HipLeft]; }
    if (LeftLowerLeg)  { LeftLowerLeg.position  = jointPositions[Joint.KneeLeft]; }
    if (LeftFoot)      { LeftFoot.position      = jointPositions[Joint.AnkleLeft]; }
    if (LeftToes)      { LeftToes.position      = jointPositions[Joint.FootLeft]; }

    if (RightUpperLeg) { RightUpperLeg.position = jointPositions[Joint.HipRight]; }
    if (RightLowerLeg) { RightLowerLeg.position = jointPositions[Joint.KneeRight]; }
    if (RightFoot)     { RightFoot.position     = jointPositions[Joint.AnkleRight]; }
    if (RightToes)     { RightToes.position     = jointPositions[Joint.FootRight]; }
  }


  void AnimateRotationFromPosition() {
    
  }


  void Update() {
    /*UpdateJoints();

    if (PointsOnlyMode) {
      LeftShoulderPoint.position = Hips.position + modelBoneLengths[Joint.Neck] * (GetJoint(Joint.Neck) - (GetJoint(Joint.HipLeft) + GetJoint(Joint.HipRight)) / 2).normalized +
        modelBoneLengths[Joint.ShoulderLeft] * (GetJoint(Joint.ShoulderLeft) - GetJoint(Joint.Neck)).normalized;
      LeftElbowPoint.position = LeftShoulderPoint.position +
        modelBoneLengths[Joint.ElbowLeft] * (GetJoint(Joint.ElbowLeft) - GetJoint(Joint.ShoulderLeft)).normalized;
      LeftWristPoint.position = LeftElbowPoint.position +
        modelBoneLengths[Joint.WristLeft] * (GetJoint(Joint.WristLeft) - GetJoint(Joint.ElbowLeft)).normalized;

      RightShoulderPoint.position = Hips.position + modelBoneLengths[Joint.Neck] * (GetJoint(Joint.Neck) - (GetJoint(Joint.HipLeft) + GetJoint(Joint.HipRight)) / 2).normalized +
        modelBoneLengths[Joint.ShoulderRight] * (GetJoint(Joint.ShoulderRight) - GetJoint(Joint.Neck)).normalized;
      RightElbowPoint.position = RightShoulderPoint.position +
        modelBoneLengths[Joint.ElbowRight] * (GetJoint(Joint.ElbowRight) - GetJoint(Joint.ShoulderRight)).normalized;
      RightWristPoint.position = RightElbowPoint.position +
        modelBoneLengths[Joint.WristRight] * (GetJoint(Joint.WristRight) - GetJoint(Joint.ElbowRight)).normalized;

      LeftHipPoint.position = Hips.position +
        modelBoneLengths[Joint.HipLeft] * (GetJoint(Joint.HipLeft) - GetJoint(Joint.HipRight)).normalized;
      LeftKneePoint.position = LeftHipPoint.position +
        modelBoneLengths[Joint.KneeLeft] * (GetJoint(Joint.KneeLeft) - GetJoint(Joint.HipLeft)).normalized;
      LeftAnklePoint.position = LeftKneePoint.position +
        modelBoneLengths[Joint.AnkleLeft] * (GetJoint(Joint.AnkleLeft) - GetJoint(Joint.KneeLeft)).normalized;

      RightHipPoint.position = Hips.position +
        modelBoneLengths[Joint.HipRight] * (GetJoint(Joint.HipRight) - GetJoint(Joint.HipLeft)).normalized;
      RightKneePoint.position = RightHipPoint.position +
        modelBoneLengths[Joint.KneeRight] * (GetJoint(Joint.KneeRight) - GetJoint(Joint.HipRight)).normalized;
      RightAnklePoint.position = RightKneePoint.position +
        modelBoneLengths[Joint.AnkleRight] * (GetJoint(Joint.AnkleRight) - GetJoint(Joint.KneeRight)).normalized;

      transform.rotation = Quaternion.LookRotation(Vector3.Cross(jointPositions[Joint.HipRight] - jointPositions[Joint.HipLeft], Vector3.up));

      if (LeftUpperArm) { LeftUpperArm.position = LeftShoulderPoint.position; }
      if (LeftLowerArm) { LeftLowerArm.position = LeftElbowPoint.position; }
      if (LeftHand) { LeftHand.position = LeftWristPoint.position; }

      if (RightUpperArm) { RightUpperArm.position = RightShoulderPoint.position; }
      if (RightLowerArm) { RightLowerArm.position = RightElbowPoint.position; }
      if (RightHand) { RightHand.position = RightWristPoint.position; }

      if (LeftUpperLeg) { LeftUpperLeg.position = LeftHipPoint.position; }
      if (LeftLowerLeg) { LeftLowerLeg.position = LeftKneePoint.position; }
      if (LeftFoot) { LeftFoot.position = LeftAnklePoint.position; }

      if (RightUpperLeg) { RightUpperLeg.position = RightHipPoint.position; }
      if (RightLowerLeg) { RightLowerLeg.position = RightKneePoint.position; }
      if (RightFoot) { RightFoot.position = RightAnklePoint.position; }
    } else {
      switch (AnimationType) {
        case AnimationTypeEnum.DirectPosition: AnimateDirectPosition(); break;
        case AnimationTypeEnum.RotationFromPosition: AnimateRotationFromPosition(); break;
      }
    }*/
  }
}
