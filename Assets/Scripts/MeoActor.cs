using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace Meocap.Perform
{
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
        private List<Quaternion> tPoseOffsets = new List<Quaternion>();
        private bool inited = false;



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
            if (animator == null || !animator.isHuman) return;

            foreach (HumanBodyBones bone in OrderedHumanBodyBones)
            {
                if (bone == HumanBodyBones.LastBone) break;
                var bone_t = animator.GetBoneTransform(bone);
                if (bone_t != null)
                {
                    var bone_rot = bone_t.rotation;
                    this.tPoseOffsets.Add(new Quaternion(bone_rot.x,bone_rot.y,bone_rot.z,bone_rot.w));
                }
                else
                {
                    this.tPoseOffsets.Add(new Quaternion(0,0,0,1));
                }
            }
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
            InitializeBoneOffsets();
            Debug.Log(tPoseOffsets.Count);
            this.inited = true;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PerformBone(HumanBodyBones bone,Quaternion rotation,MeoFrame frame)
        {
            if (this.animatorBones.ContainsKey(bone))
            {
                var transform = this.animatorBones[bone];
                if (transform == null) return;
                if(bone == HumanBodyBones.Hips)
                {
                    transform.position = new Vector3(this.baseHipsPos.x-(float)frame.translation0 * this.transform.localScale.x, this.baseHipsPos.y+(float)frame.translation1 * this.transform.localScale.y, this.baseHipsPos.z+(float)frame.translation2*this.transform.localScale.z);
                }
                transform.rotation = rotation;
            }
        }

        public void Perform(MeoFrame frame)
        {
            if (!this.inited) return;
            List<Matrix4x4> matrices = new List<Matrix4x4>();
            Joint[] joints = { 
                frame.joints0,frame.joints1,frame.joints2,frame.joints3,frame.joints4,
                frame.joints5,frame.joints6,frame.joints7,frame.joints8,frame.joints9,
                frame.joints10,frame.joints11,frame.joints12,frame.joints13,frame.joints14,
                frame.joints15,frame.joints16,frame.joints17,frame.joints18,frame.joints19,
                frame.joints20,frame.joints21,frame.joints22,frame.joints23
            };

            if (joints.Length == 24)
            {
                foreach (var item in joints)
                {
                    Matrix4x4 matrix = new Matrix4x4();
                    matrix.SetColumn(0, new Vector4((float)item.rotation0, (float)item.rotation1, (float)item.rotation2, 0));
                    matrix.SetColumn(1, new Vector4((float)item.rotation3, (float)item.rotation4, (float)item.rotation5, 0));
                    matrix.SetColumn(2, new Vector4((float)item.rotation6, (float)item.rotation7, (float)item.rotation8, 0));
                    matrices.Add(matrix);
                }
            }

            int index = 0;
            foreach(var matrix in matrices)
            {
                var offset = this.tPoseOffsets[index];
                var bone = OrderedHumanBodyBones[index];
                var rot = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1)).eulerAngles;
                // Quaternion.Euler(rot.x, -rot.y, -rot.z)
                var bone_tf = this.animator.GetBoneTransform(bone);
                this.PerformBone(bone, Quaternion.Euler(rot.x, -rot.y, -rot.z) * offset,frame);

                index++;
            }

        }
    }

}
