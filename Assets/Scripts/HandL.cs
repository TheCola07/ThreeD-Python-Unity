using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using System.IO;

public class HandL : MonoBehaviour
{
    // 第一遍改
    private Vector3 initPos;

    Animator animator;
    private Transform LHand;

    private Transform lIndexFinger1, lIndexFinger2, lIndexFinger3;
    private Transform lLittleFinger1, lLittleFinger2, lLittleFinger3;
    private Transform lMiddleFinger1, lMiddleFinger2, lMiddleFinger3;
    private Transform lRingFinger1, lRingFinger2, lRingFinger3;
    private Transform lThumb0, lThumb1, lThumb2;

    private Quaternion IndexFinger1, IndexFinger2, IndexFinger3;
    private Quaternion LittleFinger1, LittleFinger2, LittleFinger3;
    private Quaternion MiddleFinger1, MiddleFinger2, MiddleFinger3;
    private Quaternion RingFinger1, RingFinger2, RingFinger3;
    private Quaternion Thumb0, Thumb1, Thumb2;

    private Quaternion midLHand;
    private Quaternion midLIndexFinger1, midLIndexFinger2;
    private Quaternion midLLittleFinger1, midLLittleFinger2;
    private Quaternion midLMiddleFinger1, midLMiddleFinger2;
    private Quaternion midLRingFinger1, midLRingFinger2;
    private Quaternion midLThumb0, midLThumb1;


    public UDPReceive udpReceive;

    //Hand 0
    //Index1 1, Index2 2, Index3 3,
    //Little1 4, Little2 5, Little3 6,
    //Middle1 7, Middle2 8, Middle3 9,
    //Ring1 10, Ring2 11, Ring3 12,
    //Thumb0 13, Thumb1 14, Thumb2 15

    //mediapipe 到 posenet
    Dictionary<int, int> media2pose_index = new Dictionary<int, int> {
        {0, 0},
        {1, 5},
        {2, 6},
        {3, 7},
        {4, 17},
        {5, 18},
        {6, 19},
        {7, 9},
        {8, 10},
        {9, 11},
        {10, 13},
        {11, 14},
        {12, 15},
        {13, 1},
        {14, 2},
        {15, 3}
    };

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();

        LHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        lIndexFinger1 = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        lIndexFinger2 = animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        lIndexFinger3 = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);

        lLittleFinger1 = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        lLittleFinger2 = animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        lLittleFinger3 = animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);

        lMiddleFinger1 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        lMiddleFinger2 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        lMiddleFinger3 = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);

        lRingFinger1 = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        lRingFinger2 = animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        lRingFinger3 = animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);

        lThumb0 = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        lThumb1 = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        lThumb2 = animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);

        initPos = LHand.position;

        Vector3 handForward = TriangleNormal(LHand.position, lMiddleFinger1.position, lThumb0.position);
        /*midLHand = Quaternion.Inverse(LHand.rotation) * Quaternion.LookRotation(
            lThumb0.position - lMiddleFinger1.position,
            handForward
            );*/

        //食指
        midLIndexFinger1 = Quaternion.Inverse(lIndexFinger1.rotation) * Quaternion.LookRotation(
            lIndexFinger1.position - lIndexFinger2.position ,handForward);
        midLIndexFinger2 = Quaternion.Inverse(lIndexFinger2.rotation) * Quaternion.LookRotation(
            lIndexFinger2.position - lIndexFinger3.position, handForward);
        IndexFinger1 = lIndexFinger1.rotation;
        IndexFinger2 = lIndexFinger2.rotation;

        //小拇指
        midLLittleFinger1 = Quaternion.Inverse(lLittleFinger1.rotation) * Quaternion.LookRotation(
            lLittleFinger1.position - lLittleFinger2.position, handForward);
        midLLittleFinger2 = Quaternion.Inverse(lLittleFinger2.rotation) * Quaternion.LookRotation(
            lLittleFinger2.position - lLittleFinger3.position, handForward);
        LittleFinger1 = lLittleFinger1.rotation;
        LittleFinger2 = lLittleFinger2.rotation;

        //中指
        midLMiddleFinger1 = Quaternion.Inverse(lMiddleFinger1.rotation) * Quaternion.LookRotation(
            lMiddleFinger1.position - lMiddleFinger2.position, handForward);
        midLMiddleFinger2 = Quaternion.Inverse(lMiddleFinger2.rotation) * Quaternion.LookRotation(
            lMiddleFinger2.position - lMiddleFinger3.position, handForward);
        MiddleFinger1 = lMiddleFinger1.rotation;
        MiddleFinger2 = lMiddleFinger2.rotation;

        //无名指
        midLRingFinger1 = Quaternion.Inverse(lRingFinger1.rotation) * Quaternion.LookRotation(
            lRingFinger1.position - lRingFinger2.position, handForward);
        midLRingFinger2 = Quaternion.Inverse(lRingFinger2.rotation) * Quaternion.LookRotation(
            lRingFinger2.position - lRingFinger3.position, handForward);
        RingFinger1 = lRingFinger1.rotation;
        RingFinger2 = lRingFinger2.rotation;

        //大拇指
        Vector3 handForward2 = TriangleNormal(LHand.position, lIndexFinger1.position, lThumb0.position);
        midLThumb0 = Quaternion.Inverse(lThumb0.rotation) * Quaternion.LookRotation(
            lThumb0.position - lThumb1.position, handForward2);
        midLThumb1 = Quaternion.Inverse(lThumb1.rotation) * Quaternion.LookRotation(
            lThumb1.position - lThumb2.position, handForward2);
        Thumb0 = lThumb0.rotation;
        Thumb1 = lThumb1.rotation;

    }

    // 计算三角形法向量
    Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }

    // Update is called once per frame
    void updatePose()
    {
        //实时传输
        string data = udpReceive.data;
        if (data.Length == 0) {
            lIndexFinger1.rotation = IndexFinger1;
            lIndexFinger2.rotation = IndexFinger2;

            lLittleFinger1.rotation = LittleFinger1;
            lLittleFinger2.rotation = LittleFinger2;

            lMiddleFinger1.rotation = MiddleFinger1;
            lMiddleFinger2.rotation = MiddleFinger2;

            lRingFinger1.rotation = RingFinger1;
            lRingFinger2.rotation = RingFinger2;

            lThumb0.rotation = Thumb0;
            lThumb1.rotation = Thumb1;
            return;
        }
        string[] points = data.Split(',');
        if (points.Length == 0) {


            //Vector3 handForward = TriangleNormal(LHand.position, lMiddleFinger1.position, lThumb0.position);
            //Vector3 handForward2 = TriangleNormal(LHand.position, lIndexFinger1.position, lThumb0.position);

            /*LHand.rotation = Quaternion.LookRotation(pred3d[13] - pred3d[7]) * Quaternion.Inverse(midLHand);*/

            lIndexFinger1.rotation = IndexFinger1;
            lIndexFinger2.rotation = IndexFinger2;

            lLittleFinger1.rotation = LittleFinger1;
            lLittleFinger2.rotation = LittleFinger2;

            lMiddleFinger1.rotation = MiddleFinger1;
            lMiddleFinger2.rotation = MiddleFinger2;

            lRingFinger1.rotation = RingFinger1;
            lRingFinger2.rotation = RingFinger2;

            lThumb0.rotation = Thumb0;
            lThumb1.rotation = Thumb1;
            return;
        }
        List<Vector3> pred3d = new List<Vector3>();
        foreach (KeyValuePair<int, int> kvp in media2pose_index) {
            print(kvp.Key);
            print(kvp.Value);
            float x = float.Parse(points[0 + (kvp.Value * 3)]) / 3;
            float y = float.Parse(points[1 + (kvp.Value * 3)]) / 3;
            float z = float.Parse(points[2 + (kvp.Value * 3)]) / 10;
            pred3d.Add(new Vector3(x, y, z));
        }

        //Hand 0
        //Index1 1, Index2 2, Index3 3,
        //Little1 4, Little2 5, Little3 6,
        //Middle1 7, Middle2 8, Middle3 9,
        //Ring1 10, Ring2 11, Ring3 12,
        //Thumb0 13, Thumb1 14, Thumb2 15
        Vector3 handForward = TriangleNormal(pred3d[0], pred3d[7], pred3d[1]);
        Vector3 handForward2 = TriangleNormal(pred3d[0], pred3d[1], pred3d[13]);

        /*LHand.rotation = Quaternion.LookRotation(pred3d[13] - pred3d[7]) * Quaternion.Inverse(midLHand);*/

        lIndexFinger1.rotation = Quaternion.LookRotation(pred3d[1] - pred3d[2], handForward) * Quaternion.Inverse(midLIndexFinger1);
        lIndexFinger2.rotation = Quaternion.LookRotation(pred3d[2] - pred3d[3], handForward) * Quaternion.Inverse(midLIndexFinger2);

        lLittleFinger1.rotation = Quaternion.LookRotation(pred3d[4] - pred3d[5], handForward) * Quaternion.Inverse(midLLittleFinger1);
        lLittleFinger2.rotation = Quaternion.LookRotation(pred3d[5] - pred3d[6], handForward) * Quaternion.Inverse(midLLittleFinger2);

        lMiddleFinger1.rotation = Quaternion.LookRotation(pred3d[7] - pred3d[8], handForward) * Quaternion.Inverse(midLMiddleFinger1);
        lMiddleFinger2.rotation = Quaternion.LookRotation(pred3d[8] - pred3d[9], handForward) * Quaternion.Inverse(midLMiddleFinger2);

        lRingFinger1.rotation = Quaternion.LookRotation(pred3d[10] - pred3d[11], handForward) * Quaternion.Inverse(midLRingFinger1);
        lRingFinger2.rotation = Quaternion.LookRotation(pred3d[11] - pred3d[12], handForward) * Quaternion.Inverse(midLRingFinger2);

        lThumb0.rotation = Quaternion.LookRotation(pred3d[13] - pred3d[14], handForward2) * Quaternion.Inverse(midLThumb0);
        lThumb1.rotation = Quaternion.LookRotation(pred3d[14] - pred3d[15], handForward2) * Quaternion.Inverse(midLThumb1);
    }

    void Update() {
        updatePose();

    }
}
