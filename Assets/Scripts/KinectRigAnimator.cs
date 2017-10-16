using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class KinectRigAnimator : MonoBehaviour {
  enum Joint {
    SpineBase     =  0,    RightShoulder =  8,    RightHip      = 16,
    SpineMid      =  1,    RightElbow    =  9,    RightKnee     = 17,
    Neck          =  2,    RightWrist    = 10,    RightAnkle    = 18,
    Head          =  3,    Unknown2      = 11,    RightFoot     = 19,
    LeftShoulder  =  4,    LeftHip       = 12,    SpineShoulder = 20,
    LeftElbow     =  5,    LeftKnee      = 13,    LeftFingers   = 21,
    LeftWrist     =  6,    LeftAnkle     = 14,    Unknown3      = 22,
    Unknown1      =  7,    LeftFoot      = 15,    RightFingers  = 23,
  }
  static Joint[] Joints = (Joint[])Enum.GetValues(typeof(Joint));

  public KinectStream kinectStream;

  Dictionary<Joint, Vector3> jointPositions;
  Animator animator;

  Transform Hips;
  Transform LeftUpperLeg;
  Transform RightUpperLeg;
  Transform LeftLowerLeg;
  Transform RightLowerLeg;
  Transform LeftFoot;
  Transform RightFoot;
  Transform LeftToes;
  Transform RightToes;
  Transform Spine;
  Transform Chest;
  Transform UpperChest;
  Transform Neck;
  Transform Head;
  Transform LeftShoulder;
  Transform RightShoulder;
  Transform LeftUpperArm;
  Transform RightUpperArm;
  Transform LeftLowerArm;
  Transform RightLowerArm;
  Transform LeftHand;
  Transform RightHand;

  float targetArmLength = 1;


  void Start () {
    jointPositions = new Dictionary<Joint, Vector3>();
    foreach (var j in Joints) {
      jointPositions[j] = Vector3.zero;
    }

    kinectStream.OnJointDataReceived = OnJointDataReceived;

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

    targetArmLength = (LeftShoulder.position - LeftHand.position).magnitude;
  }


  static Vector3 ReadVector3(float[] data, int i) {
    return new Vector3(data[i * 3 + 0],
                       data[i * 3 + 1],
                       data[i * 3 + 2]);
  }


  void OnJointDataReceived(float[] joinData) {
    foreach (var j in Joints) {
      jointPositions[j] = ReadVector3(joinData, (int)j);
    }
  }


  void OnAnimatorIK(int layerIndex) {
    /*animator.SetIKPosition(AvatarIKGoal.LeftHand, jointPositions[Joint.LeftWrist]);
    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);

    animator.SetIKPosition(AvatarIKGoal.RightHand, jointPositions[Joint.RightWrist]);
    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);*/
  }


  void Update() {
    transform.rotation = Quaternion.LookRotation(Vector3.Cross(RightFoot.position - LeftFoot.position, Vector3.up));

    Hips.position  = (jointPositions[Joint.LeftHip] + jointPositions[Joint.RightHip])/2;
    Spine.position = jointPositions[Joint.SpineBase];
    if (Chest)      { Chest.position      = jointPositions[Joint.SpineMid]; }
    if (UpperChest) { UpperChest.position = jointPositions[Joint.SpineShoulder]; }
    if (Neck)       { Neck.position       = jointPositions[Joint.Neck]; }
    Head.position = jointPositions[Joint.Head];

    LeftUpperArm.position = jointPositions[Joint.LeftShoulder];
    LeftLowerArm.position = jointPositions[Joint.LeftElbow];
    LeftHand.position     = jointPositions[Joint.LeftWrist];

    RightUpperArm.position = jointPositions[Joint.RightShoulder];
    RightLowerArm.position = jointPositions[Joint.RightElbow];
    RightHand.position     = jointPositions[Joint.RightWrist];

    LeftUpperLeg.position = jointPositions[Joint.LeftHip];
    LeftLowerLeg.position = jointPositions[Joint.LeftKnee];
    LeftFoot.position     = jointPositions[Joint.LeftAnkle];
    if (LeftToes) { LeftToes.position = jointPositions[Joint.LeftFoot]; }

    RightUpperLeg.position = jointPositions[Joint.RightHip];
    RightLowerLeg.position = jointPositions[Joint.RightKnee];
    RightFoot.position     = jointPositions[Joint.RightAnkle];
    //if (RightToes) { RightToes.position = jointPositions[Joint.RightFoot]; }
  }
}
