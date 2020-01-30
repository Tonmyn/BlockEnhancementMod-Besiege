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

        public Action<string> OnLanguageChanged;

        private string currentLanguageName;
        private string lastLanguageName = "English";

        public ILanguage CurrentLanguage { get; private set; } = new English();
        Dictionary<string, ILanguage> Dic_Language = new Dictionary<string, ILanguage>
    {
        { "简体中文",new Chinese()},
        { "English",new English()},
            { "日本語",new Japanese()},
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
        string ModSettings { get; }
        string UnifiedFriction { get; }

        //Enhancement Block
        string Enhancement { get; }
        string AdditionalFunction { get; }

        //Rocket & Camera
        string DisplayWarning { get; }
        string MarkTarget { get; }
        string DisplayRocketCount { get; }
        string RemainingRockets { get; }
        string TrackTarget { get; }
        string GroupedFire { get; }
        string GroupFireRate { get; }
        string AutoGrabberRelease { get; }
        string SearchMode { get; }
        string DefaultAuto { get; }
        string DefaultManual { get; }
        string ZoomControlMode { get; }
        string MouseWheelZoomControl { get; }

        string KeyboardZoomControl { get; }

        string Prediction { get; }

        string ImpactFuze { get; }

        string ProximityFuze { get; }

        string NoSmoke { get; }

        string HighExplo { get; }

        string SearchAngle { get; }

        string CloseRange { get; }

        string ShowRadar { get; }

        string TorqueOnRocket { get; }

        string RocketStability { get; }

        string GuideDelay { get; }

        string LockTarget { get; }

        string SwitchGuideMode { get; }

        string RecordTarget { get; }

        string FirstPersonSmooth { get; }

        string ZoomIn { get; }

        string ZoomOut { get; }

        string ZoomSpeed { get; }

        string PauseTracking { get; }

        //CV Joint
        string CvJoint { get; }

        //Cannon
        string FireInterval { get; }

        string RandomDelay { get; }

        string Recoil { get; }

        string CustomBullet { get; }

        string InheritSize { get; }

        string BulletMass { get; }

        string BulletDrag { get; }

        string BulletDelayCollision { get; }

        string Trail { get; }

        string TrailLength { get; }

        string TrailColor { get; }

        //Decoupler
        string ExplodeForce { get; }

        string ExplodeTorque { get; }


        //Grip Pad & Piston & Slider & Suspension

        List<string> MetalHardness { get; }
        List<string> WoodenHardness { get; }
        //string hardness { get; }

        string Friction { get; }

        string Bounciness { get; }

        //string softWood { get; }

        //string midSoftWood { get; }

        //string hardWood { get; }

        //string veryHardWood { get; }

        //string lowCarbonSteel { get; }

        //string midCarbonSteel { get; }

        //string highCarbonSteel { get; }

        string Damper { get; }

        string Limit { get; }

        string Extend { get; }

        string Retract { get; }

        string HydraulicMode { get; }

        string FeedSpeed { get; }

        string ExtendLimit { get; }

        string RetractLimit { get; }

        //Small Wheel
        string RotatingSpeed { get; }
        string CustomCollider { get; }
        string ShowCollider { get; }

        //Spring 
        string Drag { get; }

        //Steering
        string ReturnToCenter { get; }
        string Near { get; }

        //FlameThrower
        string ThrustForce { get; }
        string FlameColor { get; }

        //WaterCannon
        string Boiling { get; }

        //Propeller
        string Enabled { get; }
        string EnabledOnAwake { get; }
        string ToggleMode { get; }
        string LiftIndicator { get; }
    }

    public class Chinese : ILanguage
    {
        //Game
        public string ModSettings { get; } = "扩展模组设置";
        public string UnifiedFriction { get; } = " 统一摩擦";

        //Enhancement Block
        public string Enhancement { get; } = "进阶属性";
        public string AdditionalFunction { get; } = " 增强性能";

        //Rocket & Camera
        public string DisplayWarning { get; } = " 第一人称下显示火箭警告";

        public string ShowRadar { get; } = " 显示雷达";

        public string MarkTarget { get; } = " 标记火箭目标";

        public string DisplayRocketCount { get; } = " 显示剩余火箭量";

        public string RemainingRockets { get; } = " 残余火箭";

        public string TrackTarget { get; } = "搜索目标";

        public string GroupedFire { get; } = "同组依次发射";

        public string GroupFireRate { get; } = "同组发射间隔";

        public string AutoGrabberRelease { get; } = "自动释放钩爪";

        public string SearchMode { get; } = "搜索模式";

        public string DefaultAuto { get; } = "默认自动搜索";

        public string DefaultManual { get; } = "默认手动搜索";

        public string ZoomControlMode { get; } = "变焦控制";

        public string MouseWheelZoomControl { get; } = "鼠标滚轮变焦";

        public string KeyboardZoomControl { get; } = "按键变焦";

        public string Prediction { get; } = "预测";

        public string ImpactFuze { get; } = "碰炸";

        public string ProximityFuze { get; } = "近炸";

        public string NoSmoke { get; } = "无烟";

        public string HighExplo { get; } = "高爆";

        public string SearchAngle { get; } = "搜索角度";

        public string CloseRange { get; } = "近炸距离";

        public string TorqueOnRocket { get; } = "扭转力度";

        public string RocketStability { get; } = "发射后气动";

        public string GuideDelay { get; } = "追踪延迟";

        public string LockTarget { get; } = "锁定目标";

        public string SwitchGuideMode { get; } = "自动/手动切换";

        public string RecordTarget { get; } = "记录目标";

        public string FirstPersonSmooth { get; } = "第一人称" + Environment.NewLine + "平滑";

        public string ZoomIn { get; } = "增加焦距";

        public string ZoomOut { get; } = "减小焦距";

        public string ZoomSpeed { get; } = "变焦速度";

        public string PauseTracking { get; } = "暂停/恢复追踪";

        //CV Joint
        public string CvJoint { get; } = "万向节";

        //Cannon
        public string FireInterval { get; } = "发射间隔";

        public string RandomDelay { get; } = "随机延迟";

        public string Recoil { get; } = "后坐力";

        public string CustomBullet { get; } = "自定子弹";

        public string InheritSize { get; } = "继承尺寸";

        public string BulletMass { get; } = "炮弹质量";

        public string BulletDrag { get; } = "炮弹阻力";

        public string BulletDelayCollision { get; } = "碰撞延时";

        public string Trail { get; } = "显示尾迹";

        public string TrailLength { get; } = "尾迹长度";

        public string TrailColor { get; } = "尾迹颜色";

        //Decoupler
        public string ExplodeForce { get; } = "爆炸力";

        public string ExplodeTorque { get; } = "爆炸扭矩";


        //Grip Pad & Piston & Slider & Suspension
        public List<string> MetalHardness { get; } = new List<string> { "低碳钢", "中碳钢", "高碳钢" };
        public List<string> WoodenHardness { get; } = new List<string> { "朽木", "桦木", "梨木", "檀木" };

        //public  string hardness {get;}= "硬度" ;

        public string Friction { get; } = "摩擦力";

        public string Bounciness { get; } = "弹力";

        //public  string softWood {get;}= "朽木" ;

        //public  string midSoftWood {get;}= "桦木" ;

        //public  string hardWood {get;}= "梨木" ;

        //public  string veryHardWood {get;}= "檀木" ;

        //public  string lowCarbonSteel {get;}= "低碳钢" ;

        //public  string midCarbonSteel {get;}= "中碳钢" ;

        //public  string highCarbonSteel {get;}= "高碳钢" ;

        public string Damper { get; } = "阻尼";

        public string Limit { get; } = "限制";

        public string Extend { get; } = "伸出";

        public string Retract { get; } = "收缩";

        public string HydraulicMode { get; } = "液压模式";

        public string FeedSpeed { get; } = "进给速度";

        public string ExtendLimit { get; } = "伸出限制";

        public string RetractLimit { get; } = "收缩限制";

        //Small Wheel
        public string RotatingSpeed { get; } = "旋转速度";
        public string CustomCollider { get; } = "自定碰撞";
        public string ShowCollider { get; } = "显示碰撞";

        //Spring 
        public string Drag { get; } = "阻力";

        //Steering
        public string ReturnToCenter { get; } = "自动回正";
        public string Near { get; } = "就近";

        //FlameThrower
        public string ThrustForce { get; } = "推力";
        public string FlameColor { get; } = "火焰颜色";

        //WaterCannon
        public string Boiling { get; } = "沸腾";

        //Propeller
        public string Enabled { get; } = "气动开关";
        public string EnabledOnAwake { get; } = "初始生效";
        public string ToggleMode { get; } = "持续生效模式";
        public string LiftIndicator { get; } = "升力指示";
    }

    public class English : ILanguage
    {
        //Game
        public string ModSettings { get; } = "Enhancement Mod";
        public string UnifiedFriction { get; } = " Unified Friction";

        //Enhancement Block
        public string Enhancement { get; } = "Enhancement";
        public string AdditionalFunction { get; } = " Enhance More";

        //Rocket & Camera
        public string DisplayWarning { get; } = " Rocket Warning in First Person Camera";

        public string ShowRadar { get; } = " Display Radar";

        public string MarkTarget { get; } = " Mark Rocket Target";

        public string DisplayRocketCount { get; } = " Display Rocket Count";

        public string RemainingRockets { get; } = " Rocket Count";

        public string TrackTarget { get; } = "Search Target";

        public string GroupedFire { get; } = "Grouped Launch";

        public string GroupFireRate { get; } = "Goruped Launch Rate";

        public string AutoGrabberRelease { get; } = "Auto Grabber Release";

        public string SearchMode { get; } = "Search Mode";

        public string DefaultAuto { get; } = "Default " + Environment.NewLine + "Auto Search";

        public string DefaultManual { get; } = "Default " + Environment.NewLine + " Manual Search";

        public string ZoomControlMode { get; } = "Zoom Contorol";

        public string MouseWheelZoomControl { get; } = "Zoom" + Environment.NewLine + "Mouse Wheel";

        public string KeyboardZoomControl { get; } = "Zoom" + Environment.NewLine + "Keyboard";

        public string Prediction { get; } = "Prediction";

        public string ImpactFuze { get; } = "Impact Fuze";

        public string ProximityFuze { get; } = "Proximity Fuze";

        public string NoSmoke { get; } = "No Smoke";

        public string HighExplo { get; } = "High-Explosive";

        public string SearchAngle { get; } = "Search Angle";

        public string CloseRange { get; } = "Proximity" + Environment.NewLine + "Range";

        public string TorqueOnRocket { get; } = "Turning" + Environment.NewLine + "Torque";

        public string RocketStability { get; } = "Aerodynamics" + Environment.NewLine + "After Launch";

        public string GuideDelay { get; } = "Guide Delay";

        public string LockTarget { get; } = "Lock Target";

        public string SwitchGuideMode { get; } = "Switch" + Environment.NewLine + "Auto/Manual";

        public string RecordTarget { get; } = "Save Target";

        public string FirstPersonSmooth { get; } = "FP Smooth";

        public string ZoomIn { get; } = "Zoom In";

        public string ZoomOut { get; } = "Zoom Out";

        public string ZoomSpeed { get; } = "Zoom Speed";

        public string PauseTracking { get; } = "Pause/Resume" + Environment.NewLine + "Tracking";

        //CV Joint
        public string CvJoint { get; } = "Universal Joint";

        //Cannon
        public string FireInterval { get; } = "Fire Interval";

        public string RandomDelay { get; } = "Random Delay";

        public string Recoil { get; } = "Recoil";

        public string CustomBullet { get; } = "Custom Cannonball";

        public string InheritSize { get; } = "Inherit Size";

        public string BulletMass { get; } = "Cannonball Mass";

        public string BulletDrag { get; } = "Cannonball Drag";

        public string BulletDelayCollision { get; } = "Delay Collision";

        public string Trail { get; } = "Trail";

        public string TrailLength { get; } = "Trail Length";

        public string TrailColor { get; } = "Trail Color";

        //Decoupler
        public string ExplodeForce { get; } = "Exploding" + Environment.NewLine + "Force";

        public string ExplodeTorque { get; } = "Exploding" + Environment.NewLine + "Torque";


        //Grip Pad & Piston & Slider & Suspension
        public List<string> MetalHardness { get; } = new List<string> { "Low Carbon Steel", "Mid Carbon Steel", "High Carbon Steel" };
        public List<string> WoodenHardness { get; } = new List<string> { "Soft Wood", "Median-Soft Wood", "Hard Wood", "Very Hard Wood" };

        //public  string hardness {get;}= "Hardness";

        public string Friction { get; } = "Friction";

        public string Bounciness { get; } = "Bounciness";

        //public  string softWood {get;}= "Soft Wood";

        //public  string midSoftWood {get;}= "Median-Soft Wood";

        //public  string hardWood {get;}= "Hard Wood";

        //public  string veryHardWood {get;}= "Very Hard Wood";

        //public  string lowCarbonSteel {get;}= "Low Carbon Steel";

        //public  string midCarbonSteel {get;}= "Mid Carbon Steel";

        //public  string highCarbonSteel {get;}= "High Carbon Steel";

        public string Damper { get; } = "Damper";

        public string Limit { get; } = "Limit";

        public string Extend { get; } = "Extend";

        public string Retract { get; } = "Retract";

        public string HydraulicMode { get; } = "Hydraulic Mode ";

        public string FeedSpeed { get; } = "Feed Speed";

        public string ExtendLimit { get; } = "Extension" + Environment.NewLine + "Limit";

        public string RetractLimit { get; } = "Retraction" + Environment.NewLine + "Limit";

        //Small Wheel
        public string RotatingSpeed { get; } = "Rotating" + Environment.NewLine + "Speed";
        public string CustomCollider { get; } = "Custom Collider";
        public string ShowCollider { get; } = "Show Collider";

        //Spring 
        public string Drag { get; } = "Drag";

        //Steering
        public string ReturnToCenter { get; } = "ReturnToCenter";
        public string Near { get; } = "Near";

        //FlameThrower
        public string ThrustForce { get; } = "Thrust Force";
        public string FlameColor { get; } = "Flame Color";

        //WaterCannon
        public string Boiling { get; } = "Boiling";

        //Propeller
        public string Enabled { get; } = "Switch";
        public string EnabledOnAwake { get; } = "Enabled On Awake";
        public string ToggleMode { get; } = "Toggle Mode";
        public string LiftIndicator { get; } = "Lift Indicator";
    }

    public class Japanese : ILanguage
    {
        //Game
        public string ModSettings { get; } = "ブロック エンハンスメントMod";
        public string UnifiedFriction { get; } = " 摩擦を統一する";

        //Enhancement Block
        public string Enhancement { get; } = "機能拡張";
        public string AdditionalFunction { get; } = " さらに拡張する";

        //Rocket & Camera
        public string DisplayWarning { get; } = " 一人称カメラでロケット警告表示";

        public string ShowRadar { get; } = " レーダー表示";

        public string MarkTarget { get; } = " ロケットのターゲット枠表示";

        public string DisplayRocketCount { get; } = " ロケットの残数表示";

        public string RemainingRockets { get; } = " ロケット残弾数";

        public string TrackTarget { get; } = "ターゲットを注視";

        public string GroupedFire { get; } = "同一キーで個別発射";

        public string GroupFireRate { get; } = "発射間隔";

        public string AutoGrabberRelease { get; } = "グラバー動作対応";

        public string SearchMode { get; } = "ロックオン";

        public string DefaultAuto { get; } = "デフォルト " + Environment.NewLine + "自動ロックオン";

        public string DefaultManual { get; } = "デフォルト " + Environment.NewLine + " 手動ロックオン";

        public string ZoomControlMode { get; } = "Zoom Contorol";

        public string MouseWheelZoomControl { get; } = "ズーム" + Environment.NewLine + "マウスホイール";

        public string KeyboardZoomControl { get; } = "ズーム" + Environment.NewLine + "キーボード";

        public string Prediction { get; } = "予測誘導";

        public string ImpactFuze { get; } = "衝撃起爆";

        public string ProximityFuze { get; } = "近接起爆";

        public string NoSmoke { get; } = "煙なし";

        public string HighExplo { get; } = "ボムの爆発";

        public string SearchAngle { get; } = "索敵角度";

        public string CloseRange { get; } = "起爆" + Environment.NewLine + "距離";

        public string TorqueOnRocket { get; } = "索敵時の" + Environment.NewLine + "旋回トルク";

        public string RocketStability { get; } = "発射後に" + Environment.NewLine + "空力ブロック化";

        public string GuideDelay { get; } = "誘導遅延";

        public string LockTarget { get; } = "手動ロックオン";

        public string SwitchGuideMode { get; } = "自動/手動" + Environment.NewLine + "切り換え";

        public string RecordTarget { get; } = "ターゲット記憶";

        public string FirstPersonSmooth { get; } = "一人称時スムース";

        public string ZoomIn { get; } = "ズームイン";

        public string ZoomOut { get; } = "ズームアウト";

        public string ZoomSpeed { get; } = "ズームスピード";

        public string PauseTracking { get; } = "注視の" + Environment.NewLine + "停止/再開";

        //CV Joint
        public string CvJoint { get; } = "ユニバーサルジョイント";

        //Cannon
        public string FireInterval { get; } = "反応速度";

        public string RandomDelay { get; } = "ランダム遅延";

        public string Recoil { get; } = "反動";

        public string CustomBullet { get; } = "砲弾のカスタム";

        public string InheritSize { get; } = "本体スケール";

        public string BulletMass { get; } = "砲弾の重さ";

        public string BulletDrag { get; } = "砲弾の抗力";

        public string BulletDelayCollision { get; } = "衝突の遅延";

        public string Trail { get; } = "軌跡";

        public string TrailLength { get; } = "軌跡の長さ";

        public string TrailColor { get; } = "軌跡の色";

        //Decoupler
        public string ExplodeForce { get; } = "切り離し時の" + Environment.NewLine + "初速";

        public string ExplodeTorque { get; } = "切り離し時の" + Environment.NewLine + "トルク";


        //Grip Pad & Piston & Slider & Suspension
        public List<string> MetalHardness { get; } = new List<string> { "軟質鋼", "中硬鋼", "硬質鋼" };
        public List<string> WoodenHardness { get; } = new List<string> { "軟質木材", "中硬木材", "硬質木材", "超硬質木材" };

        //public  string hardness {get;}= "硬さ";

        public string Friction { get; } = "摩擦";

        public string Bounciness { get; } = "弾性";

        //public  string softWood {get;}= "軟質木材";

        //public  string midSoftWood {get;}= "中硬木材";

        //public  string hardWood {get;}= "硬質木材";

        //public  string veryHardWood {get;}= "超硬質木材";

        //public  string lowCarbonSteel {get;}= "軟質鋼";

        //public  string midCarbonSteel {get;}= "中硬鋼";

        //public  string highCarbonSteel {get;}= "超硬質鋼";

        public string Damper { get; } = "バネの反発";

        public string Limit { get; } = "距離";

        public string Extend { get; } = "伸長";

        public string Retract { get; } = "収縮";

        public string HydraulicMode { get; } = "油圧モード ";

        public string FeedSpeed { get; } = "油圧の強さ";

        public string ExtendLimit { get; } = "伸長" + Environment.NewLine + "距離";

        public string RetractLimit { get; } = "収縮" + Environment.NewLine + "距離";

        //Small Wheel
        public string RotatingSpeed { get; } = "軸の" + Environment.NewLine + "回転速度";
        public string CustomCollider { get; } = "カスタムコライダー";
        public string ShowCollider { get; } = "コライダー表示";

        //Spring 
        public string Drag { get; } = "抗力";

        //Steering
        public string ReturnToCenter { get; } = "自動で戻る";
        public string Near { get; } = "最短距離";

        //FlameThrower
        public string ThrustForce { get; } = "推進力";
        public string FlameColor { get; } = "炎の色";

        //WaterCannon
        public string Boiling { get; } = "常に加熱";

        //Propeller
        public string Enabled { get; } = "有効/無効化";
        public string EnabledOnAwake { get; } = "開始時に有効化";
        public string ToggleMode { get; } = "トグルモード";
        public string LiftIndicator { get; } = "揚力方向表示";
    }
}