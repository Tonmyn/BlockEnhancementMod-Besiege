using System;
using Localisation;
using System.Collections.Generic;
using System.Linq;
using Modding.Common;
using Modding;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class LanguageManager : SingleInstance<LanguageManager>
    {
        public override string Name { get; } = "Language Manager";

        public static bool IsChinese { get; set; }

        private bool wasChinese = IsChinese;

        //Game
        public static string modSettings = IsChinese ? "扩展模组设置" : "Enhancement Mod Settings";
        public static string unifiedFriction = IsChinese ? " 统一摩擦" : " Unified Friction";

        //Enhancement Block
        public static string enhancement = IsChinese ? "进阶属性" : "Enhancement";
        public static string additionalFunction = IsChinese ? " 增强性能" : " Enhance More";

        //Rocket & Camera
        public static string displayWarning = IsChinese ? " 第一人称下显示火箭警告" : " Rocket Warning in First Person Camera";

        public static string markTarget = IsChinese ? " 标记火箭目标" : " Mark Rocket Target";

        public static string displayRocketCount = IsChinese ? " 显示剩余火箭量" : " Display Rocket Count";

        public static string remainingRockets = IsChinese ? " 残余火箭" : " Rocket Count";

        public static string trackTarget = IsChinese ? "搜索目标" : "Search Target";

        public static string groupedFire = IsChinese ? "同组依次发射" : "Grouped Launch";

        public static string groupFireRate = IsChinese ? "同组发射间隔" : "Goruped Launch Rate";

        public static string autoGrabberRelease = IsChinese ? "自动释放钩爪" : "Auto Grabber Release";

        public static string searchMode = IsChinese ? "搜索模式" : "Search Mode";

        public static string defaultAuto = IsChinese ? "默认自动搜索" : "Default " + Environment.NewLine + "Auto Search";

        public static string defaultManual = IsChinese ? "默认手动搜索" : "Default " + Environment.NewLine + " Manual Search";

        public static string zoomControlMode = IsChinese ? "变焦控制" : "Zoom Contorol";

        public static string mouseWheelZoomControl = IsChinese ? "鼠标滚轮变焦" : "Zoom" + Environment.NewLine + "Mouse Wheel";

        public static string keyboardZoomControl = IsChinese ? "按键变焦" : "Zoom" + Environment.NewLine + "Keyboard";

        public static string prediction = IsChinese ? "预测" : "Prediction";

        public static string impactFuze = IsChinese ? "碰炸" : "Impact Fuze";

        public static string proximityFuze = IsChinese ? "近炸" : "Proximity Fuze";

        public static string noSmoke = IsChinese ? "无烟" : "No Smoke";

        public static string highExplo = IsChinese ? "高爆" : "High-Explosive";

        public static string searchAngle = IsChinese ? "搜索角度" : "Search Angle";

        public static string closeRange = IsChinese ? "近炸距离" : "Proximity" + Environment.NewLine + "Range";

        public static string closeAngle = IsChinese ? "近炸角度" : "Proximity" + Environment.NewLine + "Angle";

        public static string torqueOnRocket = IsChinese ? "扭转力度" : "Turning" + Environment.NewLine + "Torque";

        public static string rocketStability = IsChinese ? "发射后气动" : "Aerodynamics" + Environment.NewLine + "After Launch";

        public static string guideDelay = IsChinese ? "追踪延迟" : "Guide Delay";

        public static string lockTarget = IsChinese ? "锁定目标" : "Lock Target";

        public static string switchGuideMode = IsChinese ? "自动/手动切换" : "Switch" + Environment.NewLine + "Auto/Manual";

        public static string recordTarget = IsChinese ? "记录目标" : "Save Target";

        public static string firstPersonSmooth = IsChinese ? "第一人称" + Environment.NewLine + "平滑" : "FP Smooth";

        public static string zoomIn = IsChinese ? "增加焦距" : "Zoom In";

        public static string zoomOut = IsChinese ? "减小焦距" : "Zoom Out";

        public static string zoomSpeed = IsChinese ? "变焦速度" : "Zoom Speed";

        public static string pauseTracking = IsChinese ? "暂停/恢复追踪" : "Pause/Resume" + Environment.NewLine + "Tracking";

        //CV Joint
        public static string cvJoint = IsChinese ? "万向节" : "Universal Joint";

        //Cannon
        public static string fireInterval = IsChinese ? "发射间隔" : "Fire Interval";

        public static string randomDelay = IsChinese ? "随机延迟" : "Random Delay";

        public static string recoil = IsChinese ? "后坐力" : "Recoil";

        public static string customBullet = IsChinese ? "自定子弹" : "Custom Cannonball";

        public static string inheritSize = IsChinese ? "继承尺寸" : "Inherit Size";

        public static string bulletMass = IsChinese ? "炮弹质量" : "Cannonball Mass";

        public static string bulletDrag = IsChinese ? "炮弹阻力" : "Cannonball Drag";

        public static string trail = IsChinese ? "显示尾迹" : "Trail";

        public static string trailLength = IsChinese ? "尾迹长度" : "Trail Length";

        public static string trailColor = IsChinese ? "尾迹颜色" : "Trail Color";

        //Decoupler
        public static string explodeForce = IsChinese ? "爆炸力" : "Exploding" + Environment.NewLine + "Force";

        public static string explodeTorque = IsChinese ? "爆炸扭矩" : "Exploding" + Environment.NewLine + "Torque";


        //Grip Pad & Piston & Slider & Suspension
        public static string hardness = IsChinese ? "硬度" : "Hardness";

        public static string friction = IsChinese ? "摩擦力" : "Friction";

        public static string bounciness = IsChinese ? "弹力" : "Bounciness";

        public static string softWood = IsChinese ? "朽木" : "Soft Wood";

        public static string midSoftWood = IsChinese ? "桦木" : "Median-Soft Wood";

        public static string hardWood = IsChinese ? "梨木" : "Hard Wood";

        public static string veryHardWood = IsChinese ? "檀木" : "Very Hard Wood";

        public static string lowCarbonSteel = IsChinese ? "低碳钢" : "Low Carbon Steel";

        public static string midCarbonSteel = IsChinese ? "中碳钢" : "Mid Carbon Steel";

        public static string highCarbonSteel = IsChinese ? "高碳钢" : "High Carbon Steel";

        public static string limit = IsChinese ? "限制" : "Limit";

        public static string extend = IsChinese ? "伸出" : "Extend";

        public static string retract = IsChinese ? "收缩" : "Retract";

        public static string hydraulicMode = IsChinese ? "液压模式" : "Hydraulic Mode ";

        public static string feedSpeed = IsChinese ? "进给速度" : "Feed Speed";

        public static string extendLimit = IsChinese ? "伸出限制" : "Extension" + Environment.NewLine + "Limit";

        public static string retractLimit = IsChinese ? "收缩限制" : "Retraction" + Environment.NewLine + "Limit";

        //Small Wheel
        public static string rotatingSpeed = IsChinese ? "旋转速度" : "Rotating" + Environment.NewLine + "Speed";
        public static string customCollider = IsChinese ? "自定碰撞" : "Custom Collider";
        public static string showCollider = IsChinese ? "显示碰撞" : "Show Collider";

        //Spring 
        public static string drag = IsChinese ? "阻力" : "Drag";

        //Steering
        public static string returnToCenter = IsChinese ? "自动回正" : "ReturnToCenter";
        public static string near = IsChinese ? "就近" : "Near";

        //FlameThrower
        public static string thrustForce = IsChinese ? "推力" : "Thrust Force";
        public static string flameColor = IsChinese ? "火焰颜色" : "Flame Color";

        //WaterCannon
        public static string boiling = IsChinese ? "沸腾" : "Boiling";

        void Update()
        {
            IsChinese = LocalisationManager.Instance.currLangName.Contains("中文");

            if ((IsChinese && !wasChinese) || (!IsChinese && wasChinese))
            {
                wasChinese = IsChinese;

                //Game
                modSettings = IsChinese ? "扩展模组设置" : "Enhancement Mod Settings";
                unifiedFriction = IsChinese ? " 统一摩擦" : " Unified Friction";

                //Enhancement Block
                enhancement = IsChinese ? "进阶属性" : "Enhancement";
                additionalFunction = IsChinese ? " 增强性能" : " Enhance More";

                //Rocket & Camera
                displayWarning = IsChinese ? " 第一人称下显示火箭警告" : " Rocket Warning in First Person Camera";

                markTarget = IsChinese ? " 标记火箭目标" : " Mark Rocket Target";

                displayRocketCount = IsChinese ? " 显示剩余火箭量" : " Display Rocket Count";

                trackTarget = IsChinese ? "搜索目标" : "Search Target";

                groupedFire = IsChinese ? "同组依次发射" : "Grouped Launch";

                groupFireRate = IsChinese ? "同组发射间隔" : "Goruped Launch Rate";

                autoGrabberRelease = IsChinese ? "自动释放钩爪" : "Auto Grabber Release";

                searchMode = IsChinese ? "搜索模式" : "Search Mode";

                defaultAuto = IsChinese ? "默认自动搜索" : "Default " + Environment.NewLine + "Auto Search";

                defaultManual = IsChinese ? "默认手动搜索" : "Default " + Environment.NewLine + " Manual Search";

                zoomControlMode = IsChinese ? "变焦控制" : "Zoom Contorol";

                mouseWheelZoomControl = IsChinese ? "鼠标滚轮变焦" : "Zoom" + Environment.NewLine + "Mouse Wheel";

                keyboardZoomControl = IsChinese ? "按键变焦" : "Zoom" + Environment.NewLine + "Keyboard";

                prediction = IsChinese ? "预测" : "Prediction";

                impactFuze = IsChinese ? "碰炸" : "Impact Fuze";

                proximityFuze = IsChinese ? "近炸" : "Proximity Fuze";

                noSmoke = IsChinese ? "无烟" : "No Smoke";

                highExplo = IsChinese ? "高爆" : "High-Explosive";

                searchAngle = IsChinese ? "搜索角度" : "Search Angle";

                closeRange = IsChinese ? "近炸距离" : "Proximity" + Environment.NewLine + "Range";

                closeAngle = IsChinese ? "近炸角度" : "Proximity" + Environment.NewLine + "Angle";

                torqueOnRocket = IsChinese ? "扭转力度" : "Turning" + Environment.NewLine + "Torque";

                rocketStability = IsChinese ? "发射后气动" : "Aerodynamics" + Environment.NewLine + "After Launch";

                guideDelay = IsChinese ? "追踪延迟" : "Guide Delay";

                lockTarget = IsChinese ? "锁定目标" : "Lock Target";

                switchGuideMode = IsChinese ? "自动/手动切换" : "Switch" + Environment.NewLine + "Auto/Manual";

                recordTarget = IsChinese ? "记录目标" : "Save Target";

                firstPersonSmooth = IsChinese ? "第一人称" + Environment.NewLine + "平滑" : "FP Smooth";

                zoomIn = IsChinese ? "增加焦距" : "Zoom In";

                zoomOut = IsChinese ? "减小焦距" : "Zoom Out";

                zoomSpeed = IsChinese ? "变焦速度" : "Zoom Speed";

                pauseTracking = IsChinese ? "暂停/恢复追踪" : "Pause/Resume" + Environment.NewLine + "Tracking";

                //CV Joint
                cvJoint = IsChinese ? "万向节" : "Universal Joint";

                //Cannon
                fireInterval = IsChinese ? "发射间隔" : "Fire Interval";

                randomDelay = IsChinese ? "随机延迟" : "Random Delay";

                recoil = IsChinese ? "后坐力" : "Recoil";

                customBullet = IsChinese ? "自定子弹" : "Custom Cannonball";

                inheritSize = IsChinese ? "继承尺寸" : "Inherit Size";

                bulletMass = IsChinese ? "炮弹质量" : "Cannonball Mass";

                bulletDrag = IsChinese ? "炮弹阻力" : "Cannonball Drag";

                trail = IsChinese ? "显示尾迹" : "Trail";

                trailLength = IsChinese ? "尾迹长度" : "Trail Length";

                trailColor = IsChinese ? "尾迹颜色" : "Trail Color";

                //Decoupler
                explodeForce = IsChinese ? "爆炸力" : "Exploding" + Environment.NewLine + "Force";

                explodeTorque = IsChinese ? "爆炸扭矩" : "Exploding" + Environment.NewLine + "Torque";


                //Grip Pad & Piston & Slider & Suspension
                hardness = IsChinese ? "硬度" : "Hardness";

                friction = IsChinese ? "摩擦力" : "Friction";

                bounciness = IsChinese ? "弹力" : "Bounciness";

                softWood = IsChinese ? "朽木" : "Soft Wood";

                midSoftWood = IsChinese ? "桦木" : "Median-Soft Wood";

                hardWood = IsChinese ? "梨木" : "Hard Wood";

                veryHardWood = IsChinese ? "檀木" : "Very Hard Wood";

                lowCarbonSteel = IsChinese ? "低碳钢" : "Low Carbon Steel";

                midCarbonSteel = IsChinese ? "中碳钢" : "Mid Carbon Steel";

                highCarbonSteel = IsChinese ? "高碳钢" : "High Carbon Steel";

                limit = IsChinese ? "限制" : "Limit";

                extend = IsChinese ? "伸出" : "Extend";

                retract = IsChinese ? "收缩" : "Retract";

                hydraulicMode = IsChinese ? "液压模式" : "Hydraulic Mode ";

                feedSpeed = IsChinese ? "进给速度" : "Feed Speed";

                extendLimit = IsChinese ? "伸出限制" : "Extension" + Environment.NewLine + "Limit";

                retractLimit = IsChinese ? "收缩限制" : "Retraction" + Environment.NewLine + "Limit";

                //Small Wheel
                rotatingSpeed = IsChinese ? "旋转速度" : "Rotating" + Environment.NewLine + "Speed";
                customCollider = IsChinese ? "自定碰撞" : "Custom Collider";
                showCollider = IsChinese ? "显示碰撞" : "Show Collider";

                //Spring 
                drag = IsChinese ? "阻力" : "Drag";

                //Steering
                returnToCenter = IsChinese ? "自动回正" : "ReturnToCenter";
                near = IsChinese ? "就近" : "Near";

                //FlameThrower
                thrustForce = IsChinese ? "推力" : "Thrust Force";
                flameColor = IsChinese ? "火焰颜色" : "Flame Color";

                //WaterCannon
                boiling = IsChinese ? "沸腾" : "Boiling";
            }
        }

    }
}