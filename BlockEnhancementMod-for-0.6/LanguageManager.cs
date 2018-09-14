using Localisation;
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
        public static readonly bool isChinese = LocalisationManager.Instance.currLangName.Contains("中文");

        //Enhancement Block
        public static string enhancement = isChinese ? "进阶属性" : "Enhancement";

        //Rocket & Camera
        public static string trackTarget = isChinese ? "搜索目标" : "Search Target";

        public static string proximityFuze = isChinese ? "近炸" : "Proximity Fuze";

        public static string noSmoke = isChinese ? "无烟" : "No Smoke";

        public static string highExplo = isChinese ? "高爆" : "High-Explosive";

        public static string searchAngle = isChinese ? "搜索角度" : "Search Angle";

        public static string closeRange = isChinese ? "近炸距离" : "Proximity" + Environment.NewLine + "Range";

        public static string closeAngle = isChinese ? "近炸角度" : "Proximity" + Environment.NewLine + "Angle";

        public static string torqueOnRocket = isChinese ? "扭转力度" : "Turning" + Environment.NewLine + "Torque";

        public static string rocketStability = isChinese ? "发射后气动" : "Aerodynamics" + Environment.NewLine + "After Fired";

        public static string guideDelay = isChinese ? "追踪延迟" : "Guide Delay";

        public static string lockTarget = isChinese ? "锁定目标" : "Lock Target";

        public static string switchGuideMode = isChinese ? "自动/手动切换" : "Switch" + Environment.NewLine + "Auto/Manual";

        public static string recordTarget = isChinese ? "记录目标" : "Save Target";

        public static string firstPersonSmooth = isChinese ? "第一人称" + Environment.NewLine + "平滑" : "FP Smooth";

        public static string pauseTracking = isChinese ? "暂停/恢复追踪" : "Pause/Resume" + Environment.NewLine + "Tracking";

        //CV Joint
        public static string cvJoint = isChinese ? "万向节" : "CV Joint";

        //Cannon
        public static string fireInterval = isChinese ? "发射间隔" : "Fire Interval";

        public static string randomDelay = isChinese ? "随机延迟" : "Random Delay";

        public static string recoil = isChinese ? "后坐力" : "Recoil";

        public static string customBullet = isChinese ? "自定子弹" : "Custom Cannonball";

        public static string inheritSize = isChinese ? "继承尺寸" : "Inherit Size";

        public static string bulletMass = isChinese ? "炮弹质量" : "Cannonball Mass";

        public static string bulletDrag = isChinese ? "炮弹阻力" : "Cannonball Drag";

        public static string trail = isChinese ? "显示尾迹" : "Trail";

        public static string trailLength = isChinese ? "尾迹长度" : "Trail Length";

        public static string trailColor = isChinese ? "尾迹颜色" : "Trail Color";

        //Decoupler
        public static string explodeForce = isChinese ? "爆炸力" : "Exploding" + Environment.NewLine + "Force";

        public static string explodeTorque = isChinese ? "爆炸扭矩" : "Exploding" + Environment.NewLine + "Torque";


        //Grip Pad & Piston & Slider & Suspension
        public static string hardness = isChinese ? "硬度" : "Hardness";

        public static string friction = isChinese ? "摩擦力" : "Friction";

        public static string softWood = isChinese ? "朽木" : "Soft Wood";

        public static string midSoftWood = isChinese ? "桦木" : "Median-Soft Wood";

        public static string hardWood = isChinese ? "梨木" : "Hard Wood";

        public static string veryHardWood = isChinese ? "檀木" : "Very Hard Wood";

        public static string lowCarbonSteel = isChinese ? "低碳钢" : "Low Carbon Steel";

        public static string midCarbonSteel = isChinese ? "中碳钢" : "Mid Carbon Steel";

        public static string highCarbonSteel = isChinese ? "高碳钢" : "High Carbon Steel";

        public static string limit = isChinese ? "限制" : "Limit";

        public static string extend = isChinese ? "伸出" : "Extend";

        public static string retract = isChinese ? "收缩" : "Retract";

        public static string hydraulicMode = isChinese ? "液压模式" : "Hydraulic Mode ";

        public static string feedSpeed = isChinese ? "进给速度" : "Feed Speed";

        public static string extendLimit = isChinese ? "伸出限制" : "Extension" + Environment.NewLine + "Limit";

        public static string retractLimit = isChinese ? "收缩限制" : "Retraction" + Environment.NewLine + "Limit";



        //Small Wheel
        public static string rotatingSpeed = isChinese ? "旋转速度" : "Rotating" + Environment.NewLine + "Speed";

        //Spring 
        public static string drag = isChinese ? "阻力" : "Drag";

        //Steering
        public static string returnToCenter = isChinese ? "自动回中" : "ReturnToCenter";

        //FlameThrower
        public static string thrustForce = isChinese ? "推力" : "Thrust Force";
        public static string flameColor = isChinese ? "火焰颜色" : "Flame Color";

    }
}