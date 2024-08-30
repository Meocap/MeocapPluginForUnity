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
        public static readonly int[] BONE_PARA = { -1, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 9, 9, 12, 13, 14, 16, 17, 18, 19, 20, 21 };



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
            this.inited = true;
        }

        public SkelBase SyncBonePosToClient()
        {
            List<Vector3> vecs = new List<Vector3>();
            var index = 0;
            foreach (var bone in OrderedHumanBodyBones)
            {
                var transform = this.animator.GetBoneTransform(bone);
                if (transform != null)
                {
                    vecs.Add(transform.localPosition);
                }
                else
                {
                    vecs.Add(new Vector3(0,0,0));
                }
                index++;
            }

            List<SkelJoint> joints = new List<SkelJoint>();
            
            index = 0;
            foreach (var vec in vecs)
            {
                if (index == 0)
                {
                    joints.Add(new SkelJoint { pos0 = vecs[0].x, pos1 = vecs[0].y, pos2 = vecs[0].z });
                }
                else
                {
                    var p = joints[BONE_PARA[index]];
                    joints.Add(new SkelJoint { 
                        pos0 = vecs[index].x + p.pos0,
                        pos1 = vecs[index].y + p.pos1,
                        pos2 = vecs[index].z + p.pos2
                    });

                }
                index++;
            }



            SkelBase ret = new SkelBase { 
                bones0 = joints[0], bones1 = joints[1],bones2 = joints[2],
                bones3 = joints[3], bones4 = joints[4],bones5 = joints[5],
                bones6 = joints[6], bones7 = joints[7],bones8 = joints[8],
                bones9 = joints[9], bones10 = joints[10],
                bones11 = joints[11], bones12 = joints[12],
                bones13 = joints[13], bones14 = joints[14],
                bones15 = joints[15], bones16 = joints[16],
                bones17 = joints[17], bones18 = joints[18],
                bones19 = joints[19], bones20 = joints[20],
                bones21 = joints[21], bones22 = joints[22],
                bones23 = joints[23],
                floor_y = joints[10].pos1 - joints[0].pos1
            };


            return ret;

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
            List<Quaternion> quats = new List<Quaternion>();
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
                    quats.Add(new Quaternion((float)item.glb_rot0, (float)item.glb_rot1, (float)item.glb_rot2, (float)item.glb_rot3));
                }
            }

            int index = 0;
            foreach(var quat in quats)
            {
                var offset = this.tPoseOffsets[index];
                var bone = OrderedHumanBodyBones[index];
                var rot = quat.eulerAngles;
                // Quaternion.Euler(rot.x, -rot.y, -rot.z)
                var bone_tf = this.animator.GetBoneTransform(bone);
                this.PerformBone(bone, Quaternion.Euler(rot.x, -rot.y, -rot.z) * offset,frame);

                index++;
            }

        }
    }

}
