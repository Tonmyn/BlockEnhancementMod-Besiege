using System;
using Localisation;
using System.Collections.Generic;
using System.Linq;
using Modding.Common;
using Modding;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class LanguageManager: SingleInstance<LanguageManager>
    {
        public override string Name { get; } = "Language Manager";

        public Action<string> OnLanguageChanged;

        private string currentLanguageName;
        private string lastLanguageName = "English";

        public ILanguage CurrentLanguage { get; private set; } = new English();
        Dictionary<string, ILanguage> Dic_Language = new Dictionary<string, ILanguage>
    {
        { "简体中文",new Chinese()},
        { "English",new English()},
    };

        void Awake()
        {
            OnLanguageChanged += ChangLanguage;
        }

        void Update()
        {
            currentLanguageName = LocalisationManager.Instance.currLangName;

            if (!lastLanguageName.Equals(currentLanguageName))
            {
                lastLanguageName = currentLanguageName;

                OnLanguageChanged.Invoke(currentLanguageName);
            }
        }

        void ChangLanguage(string value)
        {
            try
            {
                CurrentLanguage = Dic_Language[value];
            }
            catch
            {
                CurrentLanguage = Dic_Language["English"];
            }
        }

        ////Game
        //public static string modSettings = IsChinese ? "扩展模组设置" : "Enhancement Mod Settings";
        //public static string unifiedFriction = IsChinese ? " 统一摩擦" : " Unified Friction";

        ////Enhancement Block
        //public static string enhancement = IsChinese ? "进阶属性" : "Enhancement";
        //public static string additionalFunction = IsChinese ? " 增强性能" : " Enhance More";

        ////Rocket & Camera
        //public static string displayWarning = IsChinese ? " 第一人称下显示火箭警告" : " Rocket Warning in First Person Camera";

        //public static string markTarget = IsChinese ? " 标记火箭目标" : " Mark Rocket Target";

        //public static string displayRocketCount = IsChinese ? " 显示剩余火箭量" : " Display Rocket Count";

        //public static string remainingRockets = IsChinese ? " 残余火箭" : " Rocket Count";

        //public static string trackTarget = IsChinese ? "搜索目标" : "Search Target";

        //public static string groupedFire = IsChinese ? "同组依次发射" : "Grouped Launch";

        //public static string groupFireRate = IsChinese ? "同组发射间隔" : "Goruped Launch Rate";

        //public static string autoGrabberRelease = IsChinese ? "自动释放钩爪" : "Auto Grabber Release";

        //public static string searchMode = IsChinese ? "搜索模式" : "Search Mode";

        //public static string defaultAuto = IsChinese ? "默认自动搜索" : "Default " + Environment.NewLine + "Auto Search";

        //public static string defaultManual = IsChinese ? "默认手动搜索" : "Default " + Environment.NewLine + " Manual Search";

        //public static string zoomControlMode = IsChinese ? "变焦控制" : "Zoom Contorol";

        //public static string mouseWheelZoomControl = IsChinese ? "鼠标滚轮变焦" : "Zoom" + Environment.NewLine + "Mouse Wheel";

        //public static string keyboardZoomControl = IsChinese ? "按键变焦" : "Zoom" + Environment.NewLine + "Keyboard";

        //public static string prediction = IsChinese ? "预测" : "Prediction";

        //public static string impactFuze = IsChinese ? "碰炸" : "Impact Fuze";

        //public static string proximityFuze = IsChinese ? "近炸" : "Proximity Fuze";

        //public static string noSmoke = IsChinese ? "无烟" : "No Smoke";

        //public static string highExplo = IsChinese ? "高爆" : "High-Explosive";

        //public static string searchAngle = IsChinese ? "搜索角度" : "Search Angle";

        //public static string closeRange = IsChinese ? "近炸距离" : "Proximity" + Environment.NewLine + "Range";

        //public static string closeAngle = IsChinese ? "近炸角度" : "Proximity" + Environment.NewLine + "Angle";

        //public static string torqueOnRocket = IsChinese ? "扭转力度" : "Turning" + Environment.NewLine + "Torque";

        //public static string rocketStability = IsChinese ? "发射后气动" : "Aerodynamics" + Environment.NewLine + "After Launch";

        //public static string guideDelay = IsChinese ? "追踪延迟" : "Guide Delay";

        //public static string lockTarget = IsChinese ? "锁定目标" : "Lock Target";

        //public static string switchGuideMode = IsChinese ? "自动/手动切换" : "Switch" + Environment.NewLine + "Auto/Manual";

        //public static string recordTarget = IsChinese ? "记录目标" : "Save Target";

        //public static string firstPersonSmooth = IsChinese ? "第一人称" + Environment.NewLine + "平滑" : "FP Smooth";

        //public static string zoomIn = IsChinese ? "增加焦距" : "Zoom In";

        //public static string zoomOut = IsChinese ? "减小焦距" : "Zoom Out";

        //public static string zoomSpeed = IsChinese ? "变焦速度" : "Zoom Speed";

        //public static string pauseTracking = IsChinese ? "暂停/恢复追踪" : "Pause/Resume" + Environment.NewLine + "Tracking";

        ////CV Joint
        //public static string cvJoint = IsChinese ? "万向节" : "Universal Joint";

        ////Cannon
        //public static string fireInterval = IsChinese ? "发射间隔" : "Fire Interval";

        //public static string randomDelay = IsChinese ? "随机延迟" : "Random Delay";

        //public static string recoil = IsChinese ? "后坐力" : "Recoil";

        //public static string customBullet = IsChinese ? "自定子弹" : "Custom Cannonball";

        //public static string inheritSize = IsChinese ? "继承尺寸" : "Inherit Size";

        //public static string bulletMass = IsChinese ? "炮弹质量" : "Cannonball Mass";

        //public static string bulletDrag = IsChinese ? "炮弹阻力" : "Cannonball Drag";

        //public static string trail = IsChinese ? "显示尾迹" : "Trail";

        //public static string trailLength = IsChinese ? "尾迹长度" : "Trail Length";

        //public static string trailColor = IsChinese ? "尾迹颜色" : "Trail Color";

        ////Decoupler
        //public static string explodeForce = IsChinese ? "爆炸力" : "Exploding" + Environment.NewLine + "Force";

        //public static string explodeTorque = IsChinese ? "爆炸扭矩" : "Exploding" + Environment.NewLine + "Torque";


        ////Grip Pad & Piston & Slider & Suspension
        //public static string hardness = IsChinese ? "硬度" : "Hardness";

        //public static string friction = IsChinese ? "摩擦力" : "Friction";

        //public static string bounciness = IsChinese ? "弹力" : "Bounciness";

        //public static string softWood = IsChinese ? "朽木" : "Soft Wood";

        //public static string midSoftWood = IsChinese ? "桦木" : "Median-Soft Wood";

        //public static string hardWood = IsChinese ? "梨木" : "Hard Wood";

        //public static string veryHardWood = IsChinese ? "檀木" : "Very Hard Wood";

        //public static string lowCarbonSteel = IsChinese ? "低碳钢" : "Low Carbon Steel";

        //public static string midCarbonSteel = IsChinese ? "中碳钢" : "Mid Carbon Steel";

        //public static string highCarbonSteel = IsChinese ? "高碳钢" : "High Carbon Steel";

        //public static string limit = IsChinese ? "限制" : "Limit";

        //public static string extend = IsChinese ? "伸出" : "Extend";

        //public static string retract = IsChinese ? "收缩" : "Retract";

        //public static string hydraulicMode = IsChinese ? "液压模式" : "Hydraulic Mode ";

        //public static string feedSpeed = IsChinese ? "进给速度" : "Feed Speed";

        //public static string extendLimit = IsChinese ? "伸出限制" : "Extension" + Environment.NewLine + "Limit";

        //public static string retractLimit = IsChinese ? "收缩限制" : "Retraction" + Environment.NewLine + "Limit";

        ////Small Wheel
        //public static string rotatingSpeed = IsChinese ? "旋转速度" : "Rotating" + Environment.NewLine + "Speed";
        //public static string customCollider = IsChinese ? "自定碰撞" : "Custom Collider";
        //public static string showCollider = IsChinese ? "显示碰撞" : "Show Collider";

        ////Spring 
        //public static string drag = IsChinese ? "阻力" : "Drag";

        ////Steering
        //public static string returnToCenter = IsChinese ? "自动回正" : "ReturnToCenter";
        //public static string near = IsChinese ? "就近" : "Near";

        ////FlameThrower
        //public static string thrustForce = IsChinese ? "推力" : "Thrust Force";
        //public static string flameColor = IsChinese ? "火焰颜色" : "Flame Color";

        ////WaterCannon
        //public static string boiling = IsChinese ? "沸腾" : "Boiling";


        //void Update()
        //{
        //    IsChinese = LocalisationManager.Instance.currLangName.Contains("中文");

        //    if ((IsChinese && !wasChinese) || (!IsChinese && wasChinese))
        //    {
        //        wasChinese = IsChinese;

        //        //Game
        //        modSettings = IsChinese ? "扩展模组设置" : "Enhancement Mod Settings";
        //        unifiedFriction = IsChinese ? " 统一摩擦" : " Unified Friction";

        //        //Enhancement Block
        //        enhancement = IsChinese ? "进阶属性" : "Enhancement";
        //        additionalFunction = IsChinese ? " 增强性能" : " Enhance More";

        //        //Rocket & Camera
        //        displayWarning = IsChinese ? " 第一人称下显示火箭警告" : " Rocket Warning in First Person Camera";

        //        markTarget = IsChinese ? " 标记火箭目标" : " Mark Rocket Target";

        //        displayRocketCount = IsChinese ? " 显示剩余火箭量" : " Display Rocket Count";

        //        trackTarget = IsChinese ? "搜索目标" : "Search Target";

        //        groupedFire = IsChinese ? "同组依次发射" : "Grouped Launch";

        //        groupFireRate = IsChinese ? "同组发射间隔" : "Goruped Launch Rate";

        //        autoGrabberRelease = IsChinese ? "自动释放钩爪" : "Auto Grabber Release";

        //        searchMode = IsChinese ? "搜索模式" : "Search Mode";

        //        defaultAuto = IsChinese ? "默认自动搜索" : "Default " + Environment.NewLine + "Auto Search";

        //        defaultManual = IsChinese ? "默认手动搜索" : "Default " + Environment.NewLine + " Manual Search";

        //        zoomControlMode = IsChinese ? "变焦控制" : "Zoom Contorol";

        //        mouseWheelZoomControl = IsChinese ? "鼠标滚轮变焦" : "Zoom" + Environment.NewLine + "Mouse Wheel";

        //        keyboardZoomControl = IsChinese ? "按键变焦" : "Zoom" + Environment.NewLine + "Keyboard";

        //        prediction = IsChinese ? "预测" : "Prediction";

        //        impactFuze = IsChinese ? "碰炸" : "Impact Fuze";

        //        proximityFuze = IsChinese ? "近炸" : "Proximity Fuze";

        //        noSmoke = IsChinese ? "无烟" : "No Smoke";

        //        highExplo = IsChinese ? "高爆" : "High-Explosive";

        //        searchAngle = IsChinese ? "搜索角度" : "Search Angle";

        //        closeRange = IsChinese ? "近炸距离" : "Proximity" + Environment.NewLine + "Range";

        //        closeAngle = IsChinese ? "近炸角度" : "Proximity" + Environment.NewLine + "Angle";

        //        torqueOnRocket = IsChinese ? "扭转力度" : "Turning" + Environment.NewLine + "Torque";

        //        rocketStability = IsChinese ? "发射后气动" : "Aerodynamics" + Environment.NewLine + "After Launch";

        //        guideDelay = IsChinese ? "追踪延迟" : "Guide Delay";

        //        lockTarget = IsChinese ? "锁定目标" : "Lock Target";

        //        switchGuideMode = IsChinese ? "自动/手动切换" : "Switch" + Environment.NewLine + "Auto/Manual";

        //        recordTarget = IsChinese ? "记录目标" : "Save Target";

        //        firstPersonSmooth = IsChinese ? "第一人称" + Environment.NewLine + "平滑" : "FP Smooth";

        //        zoomIn = IsChinese ? "增加焦距" : "Zoom In";

        //        zoomOut = IsChinese ? "减小焦距" : "Zoom Out";

        //        zoomSpeed = IsChinese ? "变焦速度" : "Zoom Speed";

        //        pauseTracking = IsChinese ? "暂停/恢复追踪" : "Pause/Resume" + Environment.NewLine + "Tracking";

        //        //CV Joint
        //        cvJoint = IsChinese ? "万向节" : "Universal Joint";

        //        //Cannon
        //        fireInterval = IsChinese ? "发射间隔" : "Fire Interval";

        //        randomDelay = IsChinese ? "随机延迟" : "Random Delay";

        //        recoil = IsChinese ? "后坐力" : "Recoil";

        //        customBullet = IsChinese ? "自定子弹" : "Custom Cannonball";

        //        inheritSize = IsChinese ? "继承尺寸" : "Inherit Size";

        //        bulletMass = IsChinese ? "炮弹质量" : "Cannonball Mass";

        //        bulletDrag = IsChinese ? "炮弹阻力" : "Cannonball Drag";

        //        trail = IsChinese ? "显示尾迹" : "Trail";

        //        trailLength = IsChinese ? "尾迹长度" : "Trail Length";

        //        trailColor = IsChinese ? "尾迹颜色" : "Trail Color";

        //        //Decoupler
        //        explodeForce = IsChinese ? "爆炸力" : "Exploding" + Environment.NewLine + "Force";

        //        explodeTorque = IsChinese ? "爆炸扭矩" : "Exploding" + Environment.NewLine + "Torque";


        //        //Grip Pad & Piston & Slider & Suspension
        //        hardness = IsChinese ? "硬度" : "Hardness";

        //        friction = IsChinese ? "摩擦力" : "Friction";

        //        bounciness = IsChinese ? "弹力" : "Bounciness";

        //        softWood = IsChinese ? "朽木" : "Soft Wood";

        //        midSoftWood = IsChinese ? "桦木" : "Median-Soft Wood";

        //        hardWood = IsChinese ? "梨木" : "Hard Wood";

        //        veryHardWood = IsChinese ? "檀木" : "Very Hard Wood";

        //        lowCarbonSteel = IsChinese ? "低碳钢" : "Low Carbon Steel";

        //        midCarbonSteel = IsChinese ? "中碳钢" : "Mid Carbon Steel";

        //        highCarbonSteel = IsChinese ? "高碳钢" : "High Carbon Steel";

        //        limit = IsChinese ? "限制" : "Limit";

        //        extend = IsChinese ? "伸出" : "Extend";

        //        retract = IsChinese ? "收缩" : "Retract";

        //        hydraulicMode = IsChinese ? "液压模式" : "Hydraulic Mode ";

        //        feedSpeed = IsChinese ? "进给速度" : "Feed Speed";

        //        extendLimit = IsChinese ? "伸出限制" : "Extension" + Environment.NewLine + "Limit";

        //        retractLimit = IsChinese ? "收缩限制" : "Retraction" + Environment.NewLine + "Limit";

        //        //Small Wheel
        //        rotatingSpeed = IsChinese ? "旋转速度" : "Rotating" + Environment.NewLine + "Speed";
        //        customCollider = IsChinese ? "自定碰撞" : "Custom Collider";
        //        showCollider = IsChinese ? "显示碰撞" : "Show Collider";

        //        //Spring 
        //        drag = IsChinese ? "阻力" : "Drag";

        //        //Steering
        //        returnToCenter = IsChinese ? "自动回正" : "ReturnToCenter";
        //        near = IsChinese ? "就近" : "Near";

        //        //FlameThrower
        //        thrustForce = IsChinese ? "推力" : "Thrust Force";
        //        flameColor = IsChinese ? "火焰颜色" : "Flame Color";

        //        //WaterCannon
        //        boiling = IsChinese ? "沸腾" : "Boiling";
        //    }
        //}

    }

    public interface ILanguage
    {
        //Game
        string modSettings { get; }
        string unifiedFriction { get; }

        //Enhancement Block
        string enhancement { get; }
        string additionalFunction { get; }

        //Rocket & Camera
        string displayWarning { get; }
        string markTarget { get; }
        string displayRocketCount { get; }
        string remainingRockets { get; }
        string trackTarget { get; }
        string groupedFire { get; }
        string groupFireRate { get; }
        string autoGrabberRelease { get; }
        string searchMode { get; }
        string defaultAuto { get; }
        string defaultManual { get; }
        string zoomControlMode { get; }
        string mouseWheelZoomControl { get; }

        string keyboardZoomControl { get; }

        string prediction { get; }

        string impactFuze { get; }

        string proximityFuze { get; }

        string noSmoke { get; }

        string highExplo { get; }

        string searchAngle { get; }

        string closeRange { get; }

        string closeAngle { get; }

        string torqueOnRocket { get; }

        string rocketStability { get; }

        string guideDelay { get; }

        string lockTarget { get; }

        string switchGuideMode { get; }

        string recordTarget { get; }

        string firstPersonSmooth { get; }

        string zoomIn { get; }

        string zoomOut { get; }

        string zoomSpeed { get; }

        string pauseTracking { get; }

        //CV Joint
        string cvJoint { get; }

        //Cannon
        string fireInterval { get; }

        string randomDelay { get; }

        string recoil { get; }

        string customBullet { get; }

        string inheritSize { get; }

        string bulletMass { get; }

        string bulletDrag { get; }

        string trail { get; }

        string trailLength { get; }

        string trailColor { get; }

        //Decoupler
        string explodeForce { get; }

        string explodeTorque { get; }


        //Grip Pad & Piston & Slider & Suspension
        string hardness { get; }

        string friction { get; }

        string bounciness { get; }

        string softWood { get; }

        string midSoftWood { get; }

        string hardWood { get; }

        string veryHardWood { get; }

        string lowCarbonSteel { get; }

        string midCarbonSteel { get; }

        string highCarbonSteel { get; }

        string limit { get; }

        string extend { get; }

        string retract { get; }

        string hydraulicMode { get; }

        string feedSpeed { get; }

        string extendLimit { get; }

        string retractLimit { get; }

        //Small Wheel
        string rotatingSpeed { get; }
        string customCollider { get; }
        string showCollider { get; }

        //Spring 
        string drag { get; }

        //Steering
        string returnToCenter { get; }
        string near { get; }

        //FlameThrower
        string thrustForce { get; }
        string flameColor { get; }

        //WaterCannon
        string boiling { get; }

        //Propeller
        string enabled { get; }
        string enabledOnAwake { get; }
        string toggleMode { get; }

    }

    public class Chinese : ILanguage
    {
        //Game
        public  string modSettings {get;}= "扩展模组设置" ;
        public  string unifiedFriction {get;}= " 统一摩擦" ;

        //Enhancement Block
        public  string enhancement {get;}= "进阶属性" ;
        public  string additionalFunction {get;}= " 增强性能" ;

        //Rocket & Camera
        public  string displayWarning {get;}= " 第一人称下显示火箭警告" ;

        public  string markTarget {get;}= " 标记火箭目标" ;

        public  string displayRocketCount {get;}= " 显示剩余火箭量" ;

        public  string remainingRockets {get;}= " 残余火箭" ;

        public  string trackTarget {get;}= "搜索目标" ;

        public  string groupedFire {get;}= "同组依次发射" ;

        public  string groupFireRate {get;}= "同组发射间隔" ;

        public  string autoGrabberRelease {get;}= "自动释放钩爪" ;

        public  string searchMode {get;}= "搜索模式" ;

        public  string defaultAuto {get;}= "默认自动搜索" ;

        public  string defaultManual {get;}= "默认手动搜索" ;

        public  string zoomControlMode {get;}= "变焦控制" ;

        public  string mouseWheelZoomControl {get;}= "鼠标滚轮变焦" ;

        public  string keyboardZoomControl {get;}= "按键变焦" ;

        public  string prediction {get;}= "预测" ;

        public  string impactFuze {get;}= "碰炸" ;

        public  string proximityFuze {get;}= "近炸" ;

        public  string noSmoke {get;}= "无烟" ;

        public  string highExplo {get;}= "高爆" ;

        public  string searchAngle {get;}= "搜索角度" ;

        public  string closeRange {get;}= "近炸距离" ;

        public  string closeAngle {get;}= "近炸角度" ;

        public  string torqueOnRocket {get;}= "扭转力度" ;

        public  string rocketStability {get;}= "发射后气动" ;

        public  string guideDelay {get;}= "追踪延迟" ;

        public  string lockTarget {get;}= "锁定目标" ;

        public  string switchGuideMode {get;}= "自动/手动切换" ;

        public  string recordTarget {get;}= "记录目标" ;

        public  string firstPersonSmooth {get;}= "第一人称" + Environment.NewLine + "平滑" ;

        public  string zoomIn {get;}= "增加焦距" ;

        public  string zoomOut {get;}= "减小焦距" ;

        public  string zoomSpeed {get;}= "变焦速度" ;

        public  string pauseTracking {get;}= "暂停/恢复追踪" ;

        //CV Joint
        public  string cvJoint {get;}= "万向节" ;

        //Cannon
        public  string fireInterval {get;}= "发射间隔" ;

        public  string randomDelay {get;}= "随机延迟" ;

        public  string recoil {get;}= "后坐力" ;

        public  string customBullet {get;}= "自定子弹" ;

        public  string inheritSize {get;}= "继承尺寸" ;

        public  string bulletMass {get;}= "炮弹质量" ;

        public  string bulletDrag {get;}= "炮弹阻力" ;

        public  string trail {get;}= "显示尾迹" ;

        public  string trailLength {get;}= "尾迹长度" ;

        public  string trailColor {get;}= "尾迹颜色" ;

        //Decoupler
        public  string explodeForce {get;}= "爆炸力" ;

        public  string explodeTorque {get;}= "爆炸扭矩" ;


        //Grip Pad & Piston & Slider & Suspension
        public  string hardness {get;}= "硬度" ;

        public  string friction {get;}= "摩擦力" ;

        public  string bounciness {get;}= "弹力" ;

        public  string softWood {get;}= "朽木" ;

        public  string midSoftWood {get;}= "桦木" ;

        public  string hardWood {get;}= "梨木" ;

        public  string veryHardWood {get;}= "檀木" ;

        public  string lowCarbonSteel {get;}= "低碳钢" ;

        public  string midCarbonSteel {get;}= "中碳钢" ;

        public  string highCarbonSteel {get;}= "高碳钢" ;

        public  string limit {get;}= "限制" ;

        public  string extend {get;}= "伸出" ;

        public  string retract {get;}= "收缩" ;

        public  string hydraulicMode {get;}= "液压模式" ;

        public  string feedSpeed {get;}= "进给速度" ;

        public  string extendLimit {get;}= "伸出限制" ;

        public  string retractLimit {get;}= "收缩限制" ;

        //Small Wheel
        public  string rotatingSpeed {get;}= "旋转速度" ;
        public  string customCollider {get;}= "自定碰撞" ;
        public  string showCollider {get;}= "显示碰撞" ;

        //Spring 
        public  string drag {get;}= "阻力" ;

        //Steering
        public  string returnToCenter {get;}= "自动回正" ;
        public  string near {get;}= "就近" ;

        //FlameThrower
        public  string thrustForce {get;}= "推力" ;
        public  string flameColor {get;}= "火焰颜色" ;

        //WaterCannon
        public  string boiling {get;}= "沸腾" ;

        //Propeller
        public string enabled { get; } = "气动开关";
        public string enabledOnAwake { get; } = "初始生效";
        public string toggleMode { get; } = "持续生效模式";
    }

    public class English : ILanguage
    {
        //Game
        public  string modSettings {get;}= "Enhancement Mod Settings";
        public  string unifiedFriction {get;}= " Unified Friction";

        //Enhancement Block
        public  string enhancement {get;}= "Enhancement";
        public  string additionalFunction {get;}= " Enhance More";

        //Rocket & Camera
        public  string displayWarning {get;}= " Rocket Warning in First Person Camera";

        public  string markTarget {get;}= " Mark Rocket Target";

        public  string displayRocketCount {get;}= " Display Rocket Count";

        public  string remainingRockets {get;}= " Rocket Count";

        public  string trackTarget {get;}= "Search Target";

        public  string groupedFire {get;}= "Grouped Launch";

        public  string groupFireRate {get;}= "Goruped Launch Rate";

        public  string autoGrabberRelease {get;}= "Auto Grabber Release";

        public  string searchMode {get;}= "Search Mode";

        public  string defaultAuto {get;}= "Default " + Environment.NewLine + "Auto Search";

        public  string defaultManual {get;}= "Default " + Environment.NewLine + " Manual Search";

        public  string zoomControlMode {get;}= "Zoom Contorol";

        public  string mouseWheelZoomControl {get;}= "Zoom" + Environment.NewLine + "Mouse Wheel";

        public  string keyboardZoomControl {get;}= "Zoom" + Environment.NewLine + "Keyboard";

        public  string prediction {get;}= "Prediction";

        public  string impactFuze {get;}= "Impact Fuze";

        public  string proximityFuze {get;}= "Proximity Fuze";

        public  string noSmoke {get;}= "No Smoke";

        public  string highExplo {get;}= "High-Explosive";

        public  string searchAngle {get;}= "Search Angle";

        public  string closeRange {get;}= "Proximity" + Environment.NewLine + "Range";

        public  string closeAngle {get;}= "Proximity" + Environment.NewLine + "Angle";

        public  string torqueOnRocket {get;}= "Turning" + Environment.NewLine + "Torque";

        public  string rocketStability {get;}= "Aerodynamics" + Environment.NewLine + "After Launch";

        public  string guideDelay {get;}= "Guide Delay";

        public  string lockTarget {get;}= "Lock Target";

        public  string switchGuideMode {get;}= "Switch" + Environment.NewLine + "Auto/Manual";

        public  string recordTarget {get;}= "Save Target";

        public  string firstPersonSmooth {get;}= "FP Smooth";

        public  string zoomIn {get;}= "Zoom In";

        public  string zoomOut {get;}= "Zoom Out";

        public  string zoomSpeed {get;}= "Zoom Speed";

        public  string pauseTracking {get;}= "Pause/Resume" + Environment.NewLine + "Tracking";

        //CV Joint
        public  string cvJoint {get;}= "Universal Joint";

        //Cannon
        public  string fireInterval {get;}= "Fire Interval";

        public  string randomDelay {get;}= "Random Delay";

        public  string recoil {get;}= "Recoil";

        public  string customBullet {get;}= "Custom Cannonball";

        public  string inheritSize {get;}= "Inherit Size";

        public  string bulletMass {get;}= "Cannonball Mass";

        public  string bulletDrag {get;}= "Cannonball Drag";

        public  string trail {get;}= "Trail";

        public  string trailLength {get;}= "Trail Length";

        public  string trailColor {get;}= "Trail Color";

        //Decoupler
        public  string explodeForce {get;}= "Exploding" + Environment.NewLine + "Force";

        public  string explodeTorque {get;}= "Exploding" + Environment.NewLine + "Torque";


        //Grip Pad & Piston & Slider & Suspension
        public  string hardness {get;}= "Hardness";

        public  string friction {get;}= "Friction";

        public  string bounciness {get;}= "Bounciness";

        public  string softWood {get;}= "Soft Wood";

        public  string midSoftWood {get;}= "Median-Soft Wood";

        public  string hardWood {get;}= "Hard Wood";

        public  string veryHardWood {get;}= "Very Hard Wood";

        public  string lowCarbonSteel {get;}= "Low Carbon Steel";

        public  string midCarbonSteel {get;}= "Mid Carbon Steel";

        public  string highCarbonSteel {get;}= "High Carbon Steel";

        public  string limit {get;}= "Limit";

        public  string extend {get;}= "Extend";

        public  string retract {get;}= "Retract";

        public  string hydraulicMode {get;}= "Hydraulic Mode ";

        public  string feedSpeed {get;}= "Feed Speed";

        public  string extendLimit {get;}= "Extension" + Environment.NewLine + "Limit";

        public  string retractLimit {get;}= "Retraction" + Environment.NewLine + "Limit";

        //Small Wheel
        public  string rotatingSpeed {get;}= "Rotating" + Environment.NewLine + "Speed";
        public  string customCollider {get;}= "Custom Collider";
        public  string showCollider {get;}= "Show Collider";

        //Spring 
        public  string drag {get;}= "Drag";

        //Steering
        public  string returnToCenter {get;}= "ReturnToCenter";
        public  string near {get;}= "Near";

        //FlameThrower
        public  string thrustForce {get;}= "Thrust Force";
        public  string flameColor {get;}= "Flame Color";

        //WaterCannon
        public  string boiling {get;}= "Boiling";

        //Propeller
        public string enabled { get; } = "Switch";
        public string enabledOnAwake { get; } = "Enabled On Awake";
        public string toggleMode { get; } = "Toggle Mode";
    }
}