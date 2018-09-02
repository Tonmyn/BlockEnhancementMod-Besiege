using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class LanguageManager
    {
        static readonly bool ifChinese = (Application.systemLanguage == SystemLanguage.Chinese);

        //Enhancement Block
        public static string enhancement = ifChinese ? "进阶属性" : "Enhancement";

        //Rocket & Camera
        public static string trackTarget = ifChinese ? "搜索目标" : "Search Target";

        public static string proximityFuze = ifChinese ? "近炸" : "Proximity Fuze";

        public static string noSmoke = ifChinese ? "无烟" : "No Smoke";

        public static string highExplo = ifChinese ? "高爆" : "High-Explosive";

        public static string searchAngle = ifChinese ? "搜索角度" : "Search Angle";

        public static string closeRange = ifChinese ? "近炸距离" : "Proximity" + Environment.NewLine + "Range";

        public static string closeAngle = ifChinese ? "近炸角度" : "Proximity" + Environment.NewLine + "Angle";

        public static string torqueOnRocket = ifChinese ? "扭转力度" : "Turning" + Environment.NewLine + "Torque";

        public static string rocketStability = ifChinese ? "发射后气动" : "Aerodynamics" + Environment.NewLine + "After Fired";

        public static string guideDelay = ifChinese ? "追踪延迟" : "Guide Delay";

        public static string lockTarget = ifChinese ? "锁定目标" : "Lock Target";

        public static string switchGuideMode = ifChinese ? "自动/手动切换" : "Switch" + Environment.NewLine + "Auto/Manual";

        public static string recordTarget = ifChinese ? "记录目标" : "Save Target";

        public static string firstPersonSmooth = ifChinese ? "第一人称" + Environment.NewLine + "平滑" : "FP Smooth";

        public static string pauseTracking = ifChinese ? "暂停/恢复追踪" : "Pause/Resume" + Environment.NewLine + "Tracking";

        //CV Joint
        public static string cvJoint = ifChinese ? "万向节" : "CV Joint";

        //Cannon
        public static string fireInterval = ifChinese ? "发射间隔" : "Fire Interval";

        public static string randomDelay = ifChinese ? "随机延迟" : "Random Delay";

        public static string recoil = ifChinese ? "后坐力" : "Recoil";

        public static string customBullet = ifChinese ? "自定子弹" : "Custom Cannonball";

        public static string inheritSize = ifChinese ? "继承尺寸" : "Inherit Size";

        public static string bulletMass = ifChinese ? "炮弹质量" : "Cannonball Mass";

        public static string bulletDrag = ifChinese ? "炮弹阻力" : "Cannonball Drag";

        public static string trail = ifChinese ? "显示尾迹" : "Trail";

        public static string trailLength = ifChinese ? "尾迹长度" : "Trail Length";

        public static string trailColor = ifChinese ? "尾迹颜色" : "Trail Color";

        //Decoupler
        public static string explodeForce = ifChinese ? "爆炸力" : "Exploding" + Environment.NewLine + "Force";

        public static string explodeTorque = ifChinese ? "爆炸扭矩" : "Exploding" + Environment.NewLine + "Torque";


        //Grip Pad & Piston & Slider & Suspension
        public static string hardness = ifChinese ? "硬度" : "Hardness";

        public static string friction = ifChinese ? "摩擦力" : "Friction";

        public static string softWood = ifChinese ? "朽木" : "Soft Wood";

        public static string midSoftWood = ifChinese ? "桦木" : "Median-Soft Wood";

        public static string hardWood = ifChinese ? "梨木" : "Hard Wood";

        public static string veryHardWood = ifChinese ? "檀木" : "Very Hard Wood";

        public static string lowCarbonSteel = ifChinese ? "低碳钢" : "Low Carbon Steel";

        public static string midCarbonSteel = ifChinese ? "中碳钢" : "Mid Carbon Steel";

        public static string highCarbonSteel = ifChinese ? "高碳钢" : "High Carbon Steel";

        public static string limit = ifChinese ? "限制" : "Limit";

        public static string extend = ifChinese ? "伸出" : "Extend";

        public static string retract = ifChinese ? "收缩" : "Retract";

        public static string hydraulicMode = ifChinese ? "液压模式" : "Hydraulic Mode ";

        public static string feedSpeed = ifChinese ? "进给速度" : "Feed Speed";

        public static string extendLimit = ifChinese ? "伸出限制" : "Extension" + Environment.NewLine + "Limit";

        public static string retractLimit = ifChinese ? "收缩限制" : "Retraction" + Environment.NewLine + "Limit";



        //Small Wheel
        public static string rotatingSpeed = ifChinese ? "旋转速度" : "Rotating" + Environment.NewLine + "Speed";

        //Spring 
        public static string drag = ifChinese ? "阻力" : "Drag";

        //Steering
        public static string returnToCenter = ifChinese ? "自动回中" : "ReturnToCenter";

        //FlameThrower
        public static string thrustForce = ifChinese ? "推力" : "Thrust Force";
        public static string flameColor = ifChinese ? "火焰颜色" : "Flame Color";

    }
}