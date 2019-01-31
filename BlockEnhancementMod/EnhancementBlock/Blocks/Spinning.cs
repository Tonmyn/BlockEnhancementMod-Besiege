using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{   [Obsolete]
    class SpinningScript : EnhancementBlock
    {

        //SpinningScript SS;

        MKey RotationKey;

        MToggle LockedToggle;

        MSlider LerpSlider;

        bool Locked = false;

        float Lerp = 12f;

        public override void SafeAwake()
        {

                //RotationKey = new MKey("旋转", "Rotation", KeyCode.R);
                //RotationKey.KeysChanged += ChangedProperties;
                //CurrentMapperTypes.Add(RotationKey);

                //LockedToggle = new MToggle("锁定旋转", "Locked", Locked);
                //LockedToggle.Toggled += (bool value) => { Locked = value; ChangedProperties(); };
                //CurrentMapperTypes.Add(LockedToggle);

                //LerpSlider = new MSlider("插值", "Lerp", Lerp, 0f, 20f, false);
                //LerpSlider.ValueChanged += (float value) => { Lerp = value; ChangedProperties(); };
                //CurrentMapperTypes.Add(LerpSlider);

#if DEBUG
            //ConsoleController.ShowMessage("自转块添加进阶属性");
#endif
        }

        //public override void LoadConfiguration()
        //{
        //    base.LoadConfiguration();

        //    if (Controller.MI == null)
        //    {
        //        return;
        //    }

        //    foreach (var blockinfo in Controller.MI.Blocks)
        //    {
        //        if (blockinfo.Guid == BB.Guid)
        //        {
        //            XDataHolder bd = blockinfo.BlockData;

        //            if (bd.HasKey("bmt-" + RotationKey.Key))
        //            {
        //                int index = 0;
        //                foreach (string str in bd.ReadStringArray("bmt-" + RotationKey.Key))
        //                {
        //                    RotationKey.AddOrReplaceKey(index++, (KeyCode)Enum.Parse(typeof(KeyCode), str, true));
        //                }
        //            }

        //            if (bd.HasKey("bmt-" + LockedToggle.Key)) { LockedToggle.IsActive = Locked = bd.ReadBool("bmt-" + LockedToggle.Key); }

        //            if (bd.HasKey("bmt-" + LerpSlider.Key)) { LerpSlider.Value = Lerp = bd.ReadFloat("bmt-" + LerpSlider.Key); }

        //            break;
        //        }

        //    }
        //}

        //public override void SaveConfiguration(MachineInfo mi)
        //{
        //    base.SaveConfiguration(mi);

        //    foreach (var blockinfo in mi.Blocks)
        //    {
        //        if (blockinfo.Guid == BB.Guid)
        //        {

        //            blockinfo.BlockData.Write("bmt-" + RotationKey.Key, Tools.Get_List_keycode(RotationKey));

        //            blockinfo.BlockData.Write("bmt-" + LockedToggle.Key, LockedToggle.IsActive);

        //            blockinfo.BlockData.Write("bmt-" + LerpSlider.Key, LerpSlider.Value);


        //            break;
        //        }

        //    }
        //}

        //public override void ChangedPropertise()
        //{
        //    base.ChangedPropertise();
        //    SS.Rotation = Tools.Get_List_keycode(RotationKey);
        //    SS.Locked = Locked;
        //    SS.Lerp = Lerp;
        //}

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);
            RotationKey.DisplayInMapper = value;
            LockedToggle.DisplayInMapper = value;
            LerpSlider.DisplayInMapper = value;
        }


            HingeJoint HJ;

            CogMotorControllerHinge CMCH;

        //MKey RotationKey;

        //public List<KeyCode> Rotation;

        //public bool Locked;

        //public float Lerp;

        public override void OnSimulateStart()
        {

            //RotationKey = GetKey(Rotation);

            HJ = GetComponent<HingeJoint>();
            CMCH = GetComponent<CogMotorControllerHinge>();

            CMCH.speedLerpSmooth = Lerp;

            if (Locked)
            {
                HJ.useLimits = true;
            }
            else
            {
                HJ.useLimits = false;
            }

        }

        public override void SimulateUpdateEnhancementEnableAlways()
        {
            MotorFreezeRotation(RotationKey.IsDown);
        }

        private void MotorFreezeRotation(bool value)
        {
            if (value)
            {
                HJ.useMotor = false;
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                HJ.useMotor = true;
            }


        }
    }

}
