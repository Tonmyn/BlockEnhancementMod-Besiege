using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    [Obsolete]
    class SteeringHinge : Block
    {
        SteeringHingeScript SHS;

        MToggle FreezeToggle;

        bool Freeze = false;

        public SteeringHinge(BlockBehaviour block) : base(block)
        {

            if (BB.GetComponent<SteeringHingeScript>() == null)
            {
                SHS = BB.gameObject.AddComponent<SteeringHingeScript>();

                FreezeToggle = new MToggle("关节僵化", "Freeze", Freeze);
                FreezeToggle.Toggled += (bool value) => { Freeze = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(FreezeToggle);
            }
            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            Debug.Log("转向铰链添加进阶属性");
#endif

        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();

            if (Controller.MI == null)
            {
                return;
            }

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    if (bd.HasKey("bmt-" + FreezeToggle.Key)) { FreezeToggle.IsActive = Freeze = bd.ReadBool("bmt-" + FreezeToggle.Key); }

                    break;
                }

            }
        }

        public override void ChangedPropertise()
        {
            base.ChangedPropertise();
            SHS.Freeze = Freeze;
        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            FreezeToggle.DisplayInMapper = value;
        }

        class SteeringHingeScript : BlockScript
        {
            ConfigurableJoint CJ;

            MKey Key;

            List<KeyCode> KeyCode = new List<KeyCode>();

            public bool Freeze;

            //private void Start()
            //{
            //    KeyCode.AddRange(GetComponent<BlockBehaviour>().Keys.Find(match => match.Key == "left").KeyCode);
            //    KeyCode.AddRange(GetComponent<BlockBehaviour>().Keys.Find(match => match.Key == "right").KeyCode);

            //    Key = GetKey(KeyCode);

            //    CJ = GetComponent<ConfigurableJoint>();
            //    CJ.projectionAngle = 180;
            //    CJ.projectionMode = Freeze ? JointProjectionMode.PositionAndRotation : JointProjectionMode.None;


            //}
            //private void Update()
            //{
            //    if (StatMaster.isSimulating && Freeze)
            //    {
            //        //if (Key.IsDown)
            //        //{
            //        //    CJ.angularYMotion = ConfigurableJointMotion.Free;
            //        //   // CJ.projectionMode = JointProjectionMode.None;
            //        //}
            //        //else
            //        //{
            //        //    //CJ.angularYMotion = ConfigurableJointMotion.Locked;

            //        //    CJ.projectionMode = JointProjectionMode.PositionAndRotation;
            //        //    CJ.targetRotation =  Quaternion.Euler(GetComponent<SteeringWheel>().axis * 40);
            //        //}
            //        CJ.angularYZDrive = new JointDrive() { positionSpring = 10000, maximumForce = 10000 };
            //    }


            //}
        }
    }


}
