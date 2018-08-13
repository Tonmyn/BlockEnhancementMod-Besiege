using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class LanguageManager : MonoBehaviour
    {
        static readonly bool ifChinese = (Application.systemLanguage == SystemLanguage.Chinese);

        //Enhancement Block
        public static String enhancement = ifChinese ? "进阶属性" : "Enhancement";

        //Rocket & Camera
        public static String trackTarget = ifChinese ? "追踪目标" : "Track Target";

        public static String proximityFuze = ifChinese ? "近炸" : "Proximity Fuze";

        public static String noSmoke = ifChinese ? "无烟" : "No Smoke";

        public static String highExplo = ifChinese ? "高爆" : "High-Explosive";

        public static String searchAngle = ifChinese ? "搜索角度" : "Search Angle";

        public static String closeRange = ifChinese ? "近炸距离" : "Proximity" + Environment.NewLine + "Range";

        public static String closeAngle = ifChinese ? "近炸角度" : "Proximity" + Environment.NewLine + "Angle";

        public static String torqueOnRocket = ifChinese ? "扭转力度" : "Turning" + Environment.NewLine + "Torque";

        public static String rocketStability = ifChinese ? "气动大小" : "Aerodynamic" + Environment.NewLine + "Strength";

        public static String guideDelay = ifChinese ? "追踪延迟" : "Guide Delay";

        public static String lockTarget = ifChinese ? "锁定目标" : "Lock Target";

        public static String activeGuideKeys = ifChinese ? "自动/手动切换" : "Switch" + Environment.NewLine + "Auto/Manual";

        public static String recordTarget = ifChinese ? "记录目标" : "Save Target";

        public static String firstPersonSmooth = ifChinese ? "第一人称平滑" : "FP Smooth";

        public static String pauseTracking = ifChinese ? "暂停/恢复追踪" : "Pause/Resume" + Environment.NewLine + "Tracking";

        //CV Joint
        public static String cvJoint = ifChinese ? "万向节" : "CV Joint";

        //Cannon
        public static String fireInterval = ifChinese ? "发射间隔" : "Fire Interval";

        public static String randomDelay = ifChinese ? "随机延迟" : "Random Delay";

        public static String recoil = ifChinese ? "后坐力" : "Recoil";

        public static String customBullet = ifChinese ? "自定子弹" : "Custom Cannonball";

        public static String inheritSize = ifChinese ? "继承尺寸" : "Inherit Size";

        public static String bulletMass = ifChinese ? "炮弹质量" : "Cannonball Mass";

        public static String bulletDrag = ifChinese ? "炮弹阻力" : "Cannonball Drag";

        public static String trail = ifChinese ? "显示尾迹" : "Trail";

        public static String trailLength = ifChinese ? "尾迹长度" : "Trail Length";

        public static String trailColor = ifChinese ? "尾迹颜色" : "Trail Color";

        //Decoupler
        public static String explodeForce = ifChinese ? "爆炸力" : "Exploding" + Environment.NewLine + "Force";

        public static String explodeTorque = ifChinese ? "爆炸扭矩" : "Exploding" + Environment.NewLine + "Torque";


        //Grip Pad & Piston & Slider & Suspension
        public static String hardness = ifChinese ? "硬度" : "Hardness";

        public static String friction = ifChinese ? "摩擦力" : "Friction";

        public static String softWood = ifChinese ? "朽木" : "Soft Wood";

        public static String midSoftWood = ifChinese ? "桦木" : "Midian-Soft Wood";

        public static String hardWood = ifChinese ? "梨木" : "Hard Wood";

        public static String veryHardWood = ifChinese ? "檀木" : "Very Hard Wood";

        public static String lowCarbonSteel = ifChinese ? "低碳钢" : "Low Carbon Steel";

        public static String midCarbonSteel = ifChinese ? "中碳钢" : "Mid Carbon Steel";

        public static String highCarbonSteel = ifChinese ? "高碳钢" : "High Carbon Steel";

        public static String limit = ifChinese ? "限制" : "Limit";

        public static String extend = ifChinese ? "伸出" : "Extend";

        public static String retract = ifChinese ? "收缩" : "Retract";

        public static String hydraulicMode = ifChinese ? "液压模式" : "Hydraulic Mode ";

        public static String feedSpeed = ifChinese ? "进给速度" : "Feed Speed";

        public static String extendLimit = ifChinese ? "伸出限制" : "Extension" + Environment.NewLine + "Limit";

        public static String retractLimit = ifChinese ? "收缩限制" : "Retraction" + Environment.NewLine + "Limit";



        //Small Wheel
        public static String rotatingSpeed = ifChinese ? "旋转速度" : "Rotating" + Environment.NewLine + "Speed";

        //Spring 
        public static String drag = ifChinese ? "阻力" : "Drag";



    }
}