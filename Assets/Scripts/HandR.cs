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

public class HandR : MonoBehaviour {
    // 第一遍改
    private Vector3 initPos;

    Animator animator;
    private Transform RHand;

    private Transform rIndexFinger1, rIndexFinger2, rIndexFinger3;
    private Transform rLittleFinger1, rLittleFinger2, rLittleFinger3;
    private Transform rMiddleFinger1, rMiddleFinger2, rMiddleFinger3;
    private Transform rRingFinger1, rRingFinger2, rRingFinger3;
    private Transform rThumb0, rThumb1, rThumb2;

    private Quaternion IndexFinger1, IndexFinger2, IndexFinger3;
    private Quaternion LittleFinger1, LittleFinger2, LittleFinger3;
    private Quaternion MiddleFinger1, MiddleFinger2, MiddleFinger3;
    private Quaternion RingFinger1, RingFinger2, RingFinger3;
    private Quaternion Thumb0, Thumb1, Thumb2;

    private Quaternion midRHand;
    private Quaternion midRIndexFinger1, midRIndexFinger2;
    private Quaternion midRLittleFinger1, midRLittleFinger2;
    private Quaternion midRMiddleFinger1, midRMiddleFinger2;
    private Quaternion midRRingFinger1, midRRingFinger2;
    private Quaternion midRThumb0, midRThumb1;


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
    void Start() {
        animator = this.GetComponent<Animator>();

        RHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        rIndexFinger1 = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
        rIndexFinger2 = animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        rIndexFinger3 = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);

        rLittleFinger1 = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
        rLittleFinger2 = animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        rLittleFinger3 = animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);

        rMiddleFinger1 = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        rMiddleFinger2 = animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
        rMiddleFinger3 = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

        rRingFinger1 = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
        rRingFinger2 = animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        rRingFinger3 = animator.GetBoneTransform(HumanBodyBones.RightRingDistal);

        rThumb0 = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
        rThumb1 = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        rThumb2 = animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);

        initPos = RHand.position;

        Vector3 handForward = TriangleNormal(RHand.position, rMiddleFinger1.position, rThumb0.position);
        /*midLHand = Quaternion.Inverse(LHand.rotation) * Quaternion.LookRotation(
            lThumb0.position - lMiddleFinger1.position,
            handForward
            );*/

        //食指
        midRIndexFinger1 = Quaternion.Inverse(rIndexFinger1.rotation) * Quaternion.LookRotation(
            rIndexFinger1.position - rIndexFinger2.position, handForward);
        midRIndexFinger2 = Quaternion.Inverse(rIndexFinger2.rotation) * Quaternion.LookRotation(
            rIndexFinger2.position - rIndexFinger3.position, handForward);
        IndexFinger1 = rIndexFinger1.rotation;
        IndexFinger2 = rIndexFinger2.rotation;

        //小拇指
        midRLittleFinger1 = Quaternion.Inverse(rLittleFinger1.rotation) * Quaternion.LookRotation(
            rLittleFinger1.position - rLittleFinger2.position, handForward);
        midRLittleFinger2 = Quaternion.Inverse(rLittleFinger2.rotation) * Quaternion.LookRotation(
            rLittleFinger2.position - rLittleFinger3.position, handForward);
        LittleFinger1 = rLittleFinger1.rotation;
        LittleFinger2 = rLittleFinger2.rotation;

        //中指
        midRMiddleFinger1 = Quaternion.Inverse(rMiddleFinger1.rotation) * Quaternion.LookRotation(
            rMiddleFinger1.position - rMiddleFinger2.position, handForward);
        midRMiddleFinger2 = Quaternion.Inverse(rMiddleFinger2.rotation) * Quaternion.LookRotation(
            rMiddleFinger2.position - rMiddleFinger3.position, handForward);
        MiddleFinger1 = rMiddleFinger1.rotation;
        MiddleFinger2 = rMiddleFinger2.rotation;

        //无名指
        midRRingFinger1 = Quaternion.Inverse(rRingFinger1.rotation) * Quaternion.LookRotation(
            rRingFinger1.position - rRingFinger2.position, handForward);
        midRRingFinger2 = Quaternion.Inverse(rRingFinger2.rotation) * Quaternion.LookRotation(
            rRingFinger2.position - rRingFinger3.position, handForward);
        RingFinger1 = rRingFinger1.rotation;
        RingFinger2 = rRingFinger2.rotation;

        //大拇指
        Vector3 handForward2 = TriangleNormal(RHand.position, rIndexFinger1.position, rThumb0.position);
        midRThumb0 = Quaternion.Inverse(rThumb0.rotation) * Quaternion.LookRotation(
            rThumb0.position - rThumb1.position, handForward2);
        midRThumb1 = Quaternion.Inverse(rThumb1.rotation) * Quaternion.LookRotation(
            rThumb1.position - rThumb2.position, handForward2);
        Thumb0 = rThumb0.rotation;
        Thumb1 = rThumb1.rotation;

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
    void updatePose() {
        //实时传输
        string data = udpReceive.data;
        if (data.Length == 0) {
            rIndexFinger1.rotation = IndexFinger1;
            rIndexFinger2.rotation = IndexFinger2;

            rLittleFinger1.rotation = LittleFinger1;
            rLittleFinger2.rotation = LittleFinger2;

            rMiddleFinger1.rotation = MiddleFinger1;
            rMiddleFinger2.rotation = MiddleFinger2;

            rRingFinger1.rotation = RingFinger1;
            rRingFinger2.rotation = RingFinger2;

            rThumb0.rotation = Thumb0;
            rThumb1.rotation = Thumb1;
            return;
        }
        string[] points = data.Split(',');
        if (points.Length == 0) {
            rIndexFinger1.rotation = IndexFinger1;
            rIndexFinger2.rotation = IndexFinger2;

            rLittleFinger1.rotation = LittleFinger1;
            rLittleFinger2.rotation = LittleFinger2;

            rMiddleFinger1.rotation = MiddleFinger1;
            rMiddleFinger2.rotation = MiddleFinger2;

            rRingFinger1.rotation = RingFinger1;
            rRingFinger2.rotation = RingFinger2;

            rThumb0.rotation = Thumb0;
            rThumb1.rotation = Thumb1;
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

        rIndexFinger1.rotation = Quaternion.LookRotation(pred3d[1] - pred3d[2], handForward) * Quaternion.Inverse(midRIndexFinger1);
        rIndexFinger2.rotation = Quaternion.LookRotation(pred3d[2] - pred3d[3], handForward) * Quaternion.Inverse(midRIndexFinger2);

        rLittleFinger1.rotation = Quaternion.LookRotation(pred3d[4] - pred3d[5], handForward) * Quaternion.Inverse(midRLittleFinger1);
        rLittleFinger2.rotation = Quaternion.LookRotation(pred3d[5] - pred3d[6], handForward) * Quaternion.Inverse(midRLittleFinger2);

        rMiddleFinger1.rotation = Quaternion.LookRotation(pred3d[7] - pred3d[8], handForward) * Quaternion.Inverse(midRMiddleFinger1);
        rMiddleFinger2.rotation = Quaternion.LookRotation(pred3d[8] - pred3d[9], handForward) * Quaternion.Inverse(midRMiddleFinger2);

        rRingFinger1.rotation = Quaternion.LookRotation(pred3d[10] - pred3d[11], handForward) * Quaternion.Inverse(midRRingFinger1);
        rRingFinger2.rotation = Quaternion.LookRotation(pred3d[11] - pred3d[12], handForward) * Quaternion.Inverse(midRRingFinger2);

        rThumb0.rotation = Quaternion.LookRotation(pred3d[13] - pred3d[14], handForward2) * Quaternion.Inverse(midRThumb0);
        rThumb1.rotation = Quaternion.LookRotation(pred3d[14] - pred3d[15], handForward2) * Quaternion.Inverse(midRThumb1);
    }

    void Update() {
        updatePose();

    }
}
