using System;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Modding.Mapper;

namespace BlockEnhancementMod
{
    public static class Messages
    {
        //For rockets
        public static MessageType rocketTargetBlockBehaviourMsg;
        public static MessageType rocketTargetEntityMsg;
        public static MessageType rocketRayToHostMsg;

        //For cameras
        public static MessageType cameraTargetBlockBehaviourMsg;
        public static MessageType cameraTargetEntityMsg;
        public static MessageType cameraRayToHostMsg;
    }

    public class BlockEnhancementMod : ModEntryPoint
    {

        public static GameObject mod;

        public override void OnLoad()
        {
            mod = new GameObject("Block Enhancement Mod");
            Controller.Instance.transform.SetParent(mod.transform);
            MessageInit();
        }

        private void MessageInit()
        {
            //Create message received callbacks
            Messages.rocketTargetBlockBehaviourMsg = ModNetworking.CreateMessageType(DataType.Block);
            Messages.rocketTargetEntityMsg = ModNetworking.CreateMessageType(DataType.Entity);
            Messages.rocketRayToHostMsg = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Vector3);

            Messages.cameraTargetBlockBehaviourMsg = ModNetworking.CreateMessageType(DataType.Block);
            Messages.cameraTargetEntityMsg = ModNetworking.CreateMessageType(DataType.Entity);
            Messages.cameraRayToHostMsg = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Vector3);
        }
    }

}
