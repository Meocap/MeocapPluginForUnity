using System.Collections.Generic;
using UnityEngine;
namespace Meocap.Perform
{
    [System.Serializable]
    public class UniversalFrame
    {
        public int frame_id;
        public List<double> optimized_pose;
        public List<double> translation;
    }

    [System.Serializable]
    public class PlainTrackerReport
    {
        public float[] rotation;
        public ulong timestamp;
        public float[] acc;
    }
    public class MeoActor : MonoBehaviour
    {
        // Start is called before the first frame update

        static readonly HumanBodyBones[] OrderedHumanBodyBones =
        {
            HumanBodyBones.Hips, // 0
            HumanBodyBones.LeftUpperLeg, // 1
            HumanBodyBones.RightUpperLeg, // 2
            HumanBodyBones.Spine,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.RightLowerLeg, // 5
            HumanBodyBones.Chest,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot, // 8
            HumanBodyBones.UpperChest, // 9
            HumanBodyBones.LeftToes,
            HumanBodyBones.RightToes,
            HumanBodyBones.Neck, // 12
            HumanBodyBones.LeftShoulder,// 13 
            HumanBodyBones.RightShoulder, // 14
            HumanBodyBones.Head,//15
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftMiddleDistal,
            HumanBodyBones.RightMiddleDistal,// 23
        };


        public Animator animator;
        public Transform target;
        public Dictionary<HumanBodyBones,Transform> animatorBones = new Dictionary<HumanBodyBones, Transform>();
        public Vector3 baseHipsPos;



        /// <summary>
        /// Get Transform from a given HumanBodyBones.
        /// </summary>
        private Transform GetBone(HumanBodyBones bone)
        {
            return animatorBones[bone];
        }


        /// <summary>
        /// Calculate Character's offset based on its T Pose and Newton's T Pose.
        /// </summary>
        protected void InitializeBoneOffsets()
        {

        }

        /// <summary>
        /// Cache the bone transforms from Animator.
        /// </summary>
        protected void InitializeAnimatorHumanBones()
        {
            if (animator == null || !animator.isHuman) return;
            this.animatorBones.Clear();

            foreach (HumanBodyBones bone in OrderedHumanBodyBones)
            {
                if (bone == HumanBodyBones.LastBone) break;
                this.animatorBones.Add(bone, animator.GetBoneTransform(bone));
                if(bone == HumanBodyBones.Hips)
                {
                    var p = GetBone(bone).position;
                    this.baseHipsPos = new Vector3(p.x,p.y,p.z);
                }
            }

        }

        void Start()
        {
            InitializeAnimatorHumanBones();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PerformBone(HumanBodyBones bone,Quaternion rotation,UniversalFrame frame)
        {
            if (this.animatorBones.ContainsKey(bone))
            {
                var transform = this.animatorBones[bone];
                if (transform == null) return;
                if(bone == HumanBodyBones.Hips)
                {
                    transform.rotation = rotation;
                    transform.position = new Vector3(this.baseHipsPos.x-(float)frame.translation[0], this.baseHipsPos.y+(float)frame.translation[1], this.baseHipsPos.z+(float)frame.translation[2]);
                }else
                {
                    transform.localRotation = rotation;
                }
            }
        }

        public void Perform(UniversalFrame frame)
        {
            List<Matrix4x4> matrices = new List<Matrix4x4>();
            if (frame.optimized_pose.Count == 216)
            {
                for (int i = 0; i < frame.optimized_pose.Count; i += 9)
                {
                    Matrix4x4 matrix = new Matrix4x4();
                    matrix.SetColumn(0, new Vector4((float)frame.optimized_pose[i], (float)frame.optimized_pose[i + 1], (float)frame.optimized_pose[i + 2], 0));
                    matrix.SetColumn(1, new Vector4((float)frame.optimized_pose[i + 3], (float)frame.optimized_pose[i + 4], (float)frame.optimized_pose[i + 5], 0));
                    matrix.SetColumn(2, new Vector4((float)frame.optimized_pose[i + 6], (float)frame.optimized_pose[i + 7], (float)frame.optimized_pose[i + 8], 0));
                    matrices.Add(matrix);
                }
            }

            int index = 0;
            foreach(var matrix in matrices)
            {
                var bone = OrderedHumanBodyBones[index];
                var rot = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1)).eulerAngles;
                PerformBone(bone, Quaternion.Euler(rot.x, -rot.y, -rot.z),frame);
                index++;
            } 


        }
    }

}
