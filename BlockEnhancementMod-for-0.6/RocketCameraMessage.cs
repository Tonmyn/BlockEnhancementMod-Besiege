using Modding;
using UnityEngine;

namespace BlockEnhancementMod
{
    public static class Messages
    {
        //For rockets
        public static MessageType rocketTargetBlockBehaviourMsg;
        public static MessageType rocketTargetEntityMsg;
        public static MessageType rocketTargetNullMsg;
        public static MessageType rocketRayToHostMsg;
        public static MessageType rocketHighExploPosition;

        //For cameras
        public static MessageType cameraTargetBlockBehaviourMsg;
        public static MessageType cameraTargetEntityMsg;
        public static MessageType cameraRayToHostMsg;
    }

    public class RocketCameraMessage
    {
        public void MessageInit()
        {
            //Create message received callbacks
            //Rocket
            Messages.rocketTargetBlockBehaviourMsg = ModNetworking.CreateMessageType(DataType.Block, DataType.Block);
            Messages.rocketTargetEntityMsg = ModNetworking.CreateMessageType(DataType.Entity, DataType.Block);
            Messages.rocketTargetNullMsg = ModNetworking.CreateMessageType(DataType.Block);
            Messages.rocketRayToHostMsg = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Vector3);
            Messages.rocketHighExploPosition = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Single);

            ModNetworking.Callbacks[Messages.rocketHighExploPosition] += (Message msg) =>
            {
                if (StatMaster.isClient)
                {
                    Vector3 position = (Vector3)msg.GetData(0);
                    float bombExplosiveCharge = (float)msg.GetData(1);
                    int levelBombCategory = 4;
                    int levelBombID = 5001;
                    float radius = 7f;
                    float power = 3600f;
                    float torquePower = 100000f;
                    float upPower = 0.25f;
                    try
                    {
                        GameObject bomb = UnityEngine.Object.Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject);
                        bomb.transform.position = position;
                        ExplodeOnCollide bombControl = bomb.GetComponent<ExplodeOnCollide>();
                        bomb.transform.localScale = Vector3.one * bombExplosiveCharge;
                        bombControl.radius = radius * bombExplosiveCharge;
                        bombControl.power = power * bombExplosiveCharge;
                        bombControl.torquePower = torquePower * bombExplosiveCharge;
                        bombControl.upPower = upPower;
                        bombControl.Explodey();
                    }
                    catch { }
                }
            };

            //Camera
            Messages.cameraTargetBlockBehaviourMsg = ModNetworking.CreateMessageType(DataType.Block);
            Messages.cameraTargetEntityMsg = ModNetworking.CreateMessageType(DataType.Entity);
            Messages.cameraRayToHostMsg = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Vector3);
        }
    }
}
