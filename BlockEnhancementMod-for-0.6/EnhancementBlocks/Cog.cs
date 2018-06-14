using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    [Obsolete]
    class Cog : Block
    {
        CogScript CS;

        MToggle TemperingToggle;

        bool Tempering = false;

        public Cog(BlockBehaviour block) : base(block)
        {
            if (BB.GetComponent<CogScript>() == null)
            {

                CS = BB.gameObject.AddComponent<CogScript>();

                TemperingToggle = new MToggle("齿轮调质", "Tempering", Tempering);
                TemperingToggle.Toggled += (bool value) => { Tempering = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(TemperingToggle);

            }
            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            Debug.Log("齿轮添加进阶属性");
#endif

        }

        public static bool IsCog(int id)
        {
            bool result = false;

            switch (id)
            {
                case (int)BlockType.CogMediumPowered:
                    result = true;
                    break;

                case (int)BlockType.CogMediumUnpowered:
                    result = true;
                    break;
                case (int)BlockType.CogLargeUnpowered:
                    result = true;
                    break;

                default: result = false; break;
            }
            return result;

        }
        
        public class CogScript : BlockScript
        {

            ConfigurableJoint CJ;

            SetCogStatic SCS;

            public bool Tempering; 

            private void Start()
            {
                CJ = GetComponent<ConfigurableJoint>();

                //float num = Mathf.Infinity;
                //SoftJointLimitSpring softJointLimitSpring = CJ.angularYZLimitSpring;
                //softJointLimitSpring.damper = num;
                //CJ.angularYZLimitSpring = softJointLimitSpring;

                //float single = Mathf.Infinity;

                //CJ.angularYZDrive = new JointDrive() { positionSpring = single, maximumForce = single };

            }

        }
    }
}
