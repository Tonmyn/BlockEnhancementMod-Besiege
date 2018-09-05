using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using Modding.Blocks;


namespace BlockEnhancementMod
{
    public class SuspensionScript : EnhancementBlock
    {

        MMenu HardnessMenu;

        MKey ExtendKey;

        MKey ShrinkKey;

        MToggle HydraulicToggle;

        MSlider FeedSlider;

        MSlider ExtendLimitSlider;

        MSlider ShrinkLimitSlider;

        public int Hardness = 1;

        public bool Hydraulic = false;

        public float Feed = 0.5f;

        public float ExtendLimit = 1f;

        public float RetractLimit = 1f;

        public List<KeyCode> ExtendKeyCodes = new List<KeyCode> { KeyCode.E };

        public List<KeyCode> ShrinkKeyCodes = new List<KeyCode> { KeyCode.F };

        //public static BlockMessage blockMessage = new BlockMessage(ModNetworking.CreateMessageType(new DataType[] { DataType.Block, DataType.Integer, DataType.Boolean, DataType.Single, DataType.Single, DataType.Single }), OnCallBack);

        public override void SafeAwake()
        {

            HardnessMenu = BB.AddMenu(LanguageManager.hardness, Hardness, MetalHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Hardness = HardnessMenu.Value; };

            ExtendKey = BB.AddKey(LanguageManager.extend, "Extend", KeyCode.E);
            ShrinkKey = BB.AddKey(LanguageManager.retract, "Shrink", KeyCode.F);           

            HydraulicToggle = BB.AddToggle(LanguageManager.hydraulicMode, "Pressure", Hydraulic);
            HydraulicToggle.Toggled += (bool value) => { Hydraulic = ExtendKey.DisplayInMapper = ShrinkKey.DisplayInMapper = FeedSlider.DisplayInMapper = ExtendLimitSlider.DisplayInMapper = ShrinkLimitSlider.DisplayInMapper = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Hydraulic = HydraulicToggle.IsActive; };

            FeedSlider = BB.AddSlider(LanguageManager.feedSpeed, "feed", Feed, 0f, 2f);
            FeedSlider.ValueChanged += (float value) => { Feed = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { Feed = FeedSlider.Value; };

            ExtendLimitSlider = BB.AddSlider(LanguageManager.extendLimit, "ExtendLimit", ExtendLimit, 0f, 3f);
            ExtendLimitSlider.ValueChanged += (float value) => { ExtendLimit = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { ExtendLimit = ExtendLimitSlider.Value; };

            ShrinkLimitSlider = BB.AddSlider(LanguageManager.retractLimit, "ShrinkLimit", RetractLimit, 0f, 3f);
            ShrinkLimitSlider.ValueChanged += (float value) => { RetractLimit = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { RetractLimit = ShrinkLimitSlider.Value; };



#if DEBUG
            ConsoleController.ShowMessage("悬挂添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            HardnessMenu.DisplayInMapper = value;
            ExtendKey.DisplayInMapper = value && Hydraulic;
            ShrinkKey.DisplayInMapper = value && Hydraulic;
            HydraulicToggle.DisplayInMapper = value;
            FeedSlider.DisplayInMapper = value && Hydraulic;
            ExtendLimitSlider.DisplayInMapper = value && Hydraulic;
            ShrinkLimitSlider.DisplayInMapper = value && Hydraulic;
        }

        //public override void ChangedProperties()
        //{
        //    if (StatMaster.isClient)
        //    {
        //        ModNetworking.SendToHost(blockMessage.messageType.CreateMessage(new object[] { Block.From(BB), Hardness, Hydraulic, Feed, ExtendLimit, RetractLimit }));
        //    }
        //    else
        //    {
        //        ChangeParameter();
        //    }
        //}

        ConfigurableJoint CJ;

        Rigidbody RB;

        public override void ChangeParameter()
        {

            CJ = GetComponent<ConfigurableJoint>();
            RB = GetComponent<Rigidbody>();

            SoftJointLimit limit = CJ.linearLimit;
            limit.limit = Mathf.Max(ExtendLimit, RetractLimit);
            CJ.linearLimit = limit;

            SwitchMatalHardness(Hardness, CJ);

        }

        public override void SimulateUpdateAlways()
        {
            if (StatMaster.isClient) return;

            if (Hydraulic/* && BB.isSimulating*//* && (StatMaster.isHosting || StatMaster.isLocalSim)*/)
            {
                if (ExtendKey.IsDown /*&& !ExtendKey.ignored*/)
                {

                    RB.WakeUp();
                    if ((CJ.targetPosition.x - Feed * 0.005f) > -ExtendLimit)
                    {
                        CJ.targetPosition -= new Vector3(Feed * 0.005f, 0, 0);
                    }
                    else
                    {
                        CJ.targetPosition = new Vector3(-ExtendLimit, 0, 0);
                    }

                }

                if (ShrinkKey.IsDown /*&& !ExtendKey.ignored*/)
                {
                    RB.WakeUp();
                    if (CJ.targetPosition.x + Feed * 0.005f < RetractLimit)
                    {
                        CJ.targetPosition += new Vector3(Feed * 0.005f, 0, 0);
                    }
                    else
                    {
                        CJ.targetPosition = new Vector3(RetractLimit, 0, 0);
                    }
                }
            }
        }

        //public static void OnCallBack(Message message)
        //{
        //    Block block = (Block)message.GetData(0);

        //    if ((block == null ? false : block.InternalObject != null))
        //    {
        //        var script = block.InternalObject.GetComponent<SuspensionScript>();

        //        script.Hardness = (int)message.GetData(1);
        //        script.Hydraulic = (bool)message.GetData(2);
        //        script.Feed = (float)message.GetData(3);
        //        script.ExtendLimit = (float)message.GetData(4);
        //        script.RetractLimit = (float)message.GetData(5);
        //        //script.ChangeParameter(script.Hardness, script.Hydraulic, script.Feed, script.ExtendLimit, script.RetractLimit);
        //        script.ChangeParameter();
        //    }
        //}
       
    }

  
}
