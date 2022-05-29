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

    }

    public interface ILanguage
    {
        //Game
        string ModSettings { get; }
        string UnifiedFriction { get; }

        //Enhancement Block
        string Enhancement { get; }
        string AdditionalFunction { get; }
        string BuildSurface { get; }

        //Rocket & Camera
        string DisplayWarning { get; }
        string MarkTarget { get; }
        string DisplayRocketCount { get; }
        string RemainingRockets { get; }
        string TrackTarget { get; }
        string GroupedFire { get; }
        string GroupFireRate { get; }
        string AutoRelease { get; }
        string AsRadar { get; }
        string SearchMode { get; }
        string DefaultAuto { get; }
        string DefaultManual { get; }
        string DefaultPassive { get; }
        List<string> RadarType { get; }

        List<string> SettingType { get; }

        string ZoomControlMode { get; }
        string MouseWheelZoomControl { get; }

        string KeyboardZoomControl { get; }

        string Prediction { get; }

        string ProjectileSpeed { get; }

        string ShowProjectileInterception { get; }

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

        string ManualOverride { get; }

        string RecordTarget { get; }

        string FirstPersonSmooth { get; }

        string ZoomIn { get; }

        string ZoomOut { get; }

        string ZoomSpeed { get; }

        string PauseTracking { get; }

        string SinglePlayerTeam { get; }

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
        string Collision { get; }
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

        string ChangeSpeed { get; }
        string AddSpeed { get; }
        string ReduceSpeed { get; }

        //Armor
        string ChangeChannel { get; }
        string WidthPixel { get; }
        string HeightPixel { get; }
        string Switch { get; }
        List<string> NullChannelList { get; }

        //ArmorRound
        string Play { get; }
        string Stop { get; }
        string Mute { get; }
        string Next { get; }
        string Last { get; }
        string Loop { get; }
        string OneShot { get; }
        string ReleaseToPause { get; }
        string ReleaseToStop { get; }
        string OnCollision { get; }
        string Volume { get; }
        string Pitch { get; }
        string Distance { get; }
        string Doppler { get; }
        string SpatialBlend { get; }

        //Balloon
        string Effected { get; }
        string DragTogether { get; }
    }

    public class Chinese : ILanguage
    {
        //Game
        public string ModSettings { get; } = "扩展模组设置";
        public string UnifiedFriction { get; } = " 统一摩擦";

        //Enhancement Block
        public string Enhancement { get; } = "进阶属性";
        public string AdditionalFunction { get; } = " 增强性能";
        public string BuildSurface { get; } = " 显示蒙皮块的碰撞和质量滑条";

        //Rocket & Camera
        public string DisplayWarning { get; } = " 第一人称下显示火箭警告";

        public string ShowRadar { get; } = " 显示雷达";

        public string AsRadar { get; } = "作为雷达";

        public string MarkTarget { get; } = " 标记火箭目标及着弹点";

        public string DisplayRocketCount { get; } = " 显示剩余火箭量";

        public string RemainingRockets { get; } = " 残余火箭";

        public string TrackTarget { get; } = "搜索目标";

        public string GroupedFire { get; } = "同组依次发射";

        public string GroupFireRate { get; } = "同组发射间隔";

        public string AutoRelease { get; } = "自动分离";

        public string SearchMode { get; } = "搜索模式";

        public string DefaultAuto { get; } = "默认自动搜索";

        public string DefaultManual { get; } = "默认手动搜索";

        public string DefaultPassive { get; } = "被动接受目标";

        public List<string> RadarType { get; } = new List<string> { "主动雷达", "被动雷达" };

        public List<string> SettingType { get; } = new List<string> { "火箭设置", "雷达设置" };

        public string ZoomControlMode { get; } = "变焦控制";

        public string MouseWheelZoomControl { get; } = "鼠标滚轮变焦";

        public string KeyboardZoomControl { get; } = "按键变焦";

        public string Prediction { get; } = "预测";

        public string ProjectileSpeed { get; } = "炮弹速度";

        public string ShowProjectileInterception { get; } = "显示炮弹落点";

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

        public string ManualOverride { get; } = "手动覆盖" + Environment.NewLine + "目标开关";

        public string RecordTarget { get; } = "记录目标";

        public string FirstPersonSmooth { get; } = "第一人称" + Environment.NewLine + "平滑";

        public string ZoomIn { get; } = "增加焦距";

        public string ZoomOut { get; } = "减小焦距";

        public string ZoomSpeed { get; } = "变焦速度";

        public string PauseTracking { get; } = "暂停/恢复追踪";

        public string SinglePlayerTeam { get; } = "单人模式队伍";

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
        public string Collision { get; } = "碰撞";

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

        public string ChangeSpeed { get; } = "改变速度";
        public string AddSpeed { get; } = "加速";
        public string ReduceSpeed { get; } = "减速";

        //Armor
        public string ChangeChannel { get; } = "更换频道";
        public string WidthPixel { get; } = "宽度像素";
        public string HeightPixel { get; } = "高度像素";
        public string Switch { get; } = "开关";
        public List<string> NullChannelList { get; } = new List<string> { "无信号" };

        //ArmorRound
        public string Play { get; } = "播放";
        public string Stop { get; } = "停止";
        public string Mute { get; } = "静音";
        public string Next { get; } = "下一个";
        public string Last { get; } = "上一个";
        public string Loop { get; } = "循环";
        public string OneShot { get; } = "单次播放";
        public string ReleaseToPause { get; } = "放开暂停";
        public string ReleaseToStop { get; } = "放开停止";
        public string OnCollision { get; } = "碰撞时";
        public string Volume { get; } = "音量";
        public string Pitch { get; } = "音调";
        public string Distance { get; } = "传播距离";
        public string Doppler { get; } = "多普勒效应";
        public string SpatialBlend { get; } = "空间衰减";

        //Balloon
        public string Effected { get; } = "使能开关";
        public string DragTogether { get; } = "同时关闭阻力";
    }

    public class English : ILanguage
    {
        //Game
        public string ModSettings { get; } = "Enhancement Mod";
        public string UnifiedFriction { get; } = " Unified Friction";

        //Enhancement Block
        public string Enhancement { get; } = "Enhancement";
        public string AdditionalFunction { get; } = " Enhance More";
        public string BuildSurface { get; } = " Show BuildSurface's Collision and Mass Slider";

        //Rocket & Camera
        public string DisplayWarning { get; } = " Rocket Warning in First Person Camera";

        public string ShowRadar { get; } = " Display Radar";

        public string AsRadar { get; } = "As Radar";

        public string MarkTarget { get; } = " Display Rocket Target & Projectile Interception Point";

        public string DisplayRocketCount { get; } = " Display Rocket Count";

        public string RemainingRockets { get; } = " Rocket Count";

        public string TrackTarget { get; } = "Search Target";

        public string GroupedFire { get; } = "Grouped Launch";

        public string GroupFireRate { get; } = "Goruped Launch Rate";

        public string AutoRelease { get; } = "Auto Eject";

        public string SearchMode { get; } = "Search Mode";

        public string DefaultAuto { get; } = "Default " + Environment.NewLine + "Auto Search";

        public string DefaultManual { get; } = "Default " + Environment.NewLine + " Manual Search";

        public string DefaultPassive { get; } = "Receive Target" + Environment.NewLine + "From Detector";

        public List<string> RadarType { get; } = new List<string> { "Active Radar", "Passive Radar" };

        public List<string> SettingType { get; } = new List<string> { "Rocket Setting", "Radar Setting" };

        public string ZoomControlMode { get; } = "Zoom Contorol";

        public string MouseWheelZoomControl { get; } = "Zoom" + Environment.NewLine + "Mouse Wheel";

        public string KeyboardZoomControl { get; } = "Zoom" + Environment.NewLine + "Keyboard";

        public string Prediction { get; } = "Prediction";

        public string ProjectileSpeed { get; } = "Projectile Speed";

        public string ShowProjectileInterception { get; } = "Show Projectile" + Environment.NewLine + "Interception Point";

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

        public string ManualOverride { get; } = "Manual Override" + Environment.NewLine + "Target Switch";

        public string RecordTarget { get; } = "Save Target";

        public string FirstPersonSmooth { get; } = "FP Smooth";

        public string ZoomIn { get; } = "Zoom In";

        public string ZoomOut { get; } = "Zoom Out";

        public string ZoomSpeed { get; } = "Zoom Speed";

        public string PauseTracking { get; } = "Pause/Resume" + Environment.NewLine + "Tracking";

        public string SinglePlayerTeam { get; } = "Single Player" + Environment.NewLine + "Team";

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
        public string Collision { get; } = "Collision";
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

        public string ChangeSpeed { get; } = "Change Speed";
        public string AddSpeed { get; } = "Add Speed";

        public string ReduceSpeed { get; } = "Reduce Speed";

        //Armor
        public string ChangeChannel { get; } = "Change Channel";
        public string WidthPixel { get; } = "Width Pixel";
        public string HeightPixel { get; } = "Height Pixel";
        public string Switch { get; } = "Switch";
        public List<string> NullChannelList { get; } = new List<string> { "No Channel" };

        //ArmorRound
        public string Play { get; } = "Play";
        public string Stop { get; } = "Stop";
        public string Mute { get; } = "Mute";
        public string Next { get; } = "Next";
        public string Last { get; } = "Last";
        public string Loop { get; } = "Loop";
        public string OneShot { get; } = "OneShot";
        public string ReleaseToPause { get; } = "Release" + Environment.NewLine + "To Pause";
        public string ReleaseToStop { get; } = "Release" + Environment.NewLine + "To Stop";
        public string OnCollision { get; } = "On Collision";
        public string Volume { get; } = "Volume";
        public string Pitch { get; } = "Pitch";
        public string Distance { get; } = "Distance";
        public string Doppler { get; } = "Doppler";
        public string SpatialBlend { get; } = "Spatial Blend";
        //Balloon
        public string Effected { get; } = "Enable Switch";
        public string DragTogether { get; } = "Drag Together";
    }

    public class Japanese : ILanguage
    {
        //Game
        public string ModSettings { get; } = "ブロック エンハンスメントMod";
        public string UnifiedFriction { get; } = " 摩擦を統一する";

        //Enhancement Block
        public string Enhancement { get; } = "機能拡張";
        public string AdditionalFunction { get; } = " さらに拡張する";
        public string BuildSurface { get; } = " ショーけんちくひょうめん 衝突と質量スライダ";

        //Rocket & Camera
        public string DisplayWarning { get; } = " 一人称カメラでロケット警告表示";

        public string ShowRadar { get; } = " レーダー表示";

        public string AsRadar { get; } = "As Radar";

        public string MarkTarget { get; } = " ロケットのターゲット枠表示";

        public string DisplayRocketCount { get; } = " ロケットの残数表示";

        public string RemainingRockets { get; } = " ロケット残弾数";

        public string TrackTarget { get; } = "ロックオン";

        public string GroupedFire { get; } = "同一キーで個別発射";

        public string GroupFireRate { get; } = "発射間隔";

        public string AutoRelease { get; } = "自動解放";

        public string SearchMode { get; } = "ロックオンモード";

        public string DefaultAuto { get; } = "デフォルト " + Environment.NewLine + "自動ロックオン";

        public string DefaultManual { get; } = "デフォルト " + Environment.NewLine + " 手動ロックオン";

        public string DefaultPassive { get; } = "Receive Target" + Environment.NewLine + "From Detector";

        public List<string> RadarType { get; } = new List<string> { "能動レーダー", "受動レーダー" };

        public List<string> SettingType { get; } = new List<string> { "ロケット設定", "レーダー設定" };

        public string ZoomControlMode { get; } = "Zoom Contorol";

        public string MouseWheelZoomControl { get; } = "ズーム" + Environment.NewLine + "マウスホイール";

        public string KeyboardZoomControl { get; } = "ズーム" + Environment.NewLine + "キーボード";

        public string Prediction { get; } = "予測誘導";

        public string ProjectileSpeed { get; } = "弾速";

        public string ShowProjectileInterception { get; } = "迎撃点を示す";

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

        public string ManualOverride { get; } = "オーバーライド" + Environment.NewLine + "目標スイッチ";

        public string RecordTarget { get; } = "ターゲット記憶";

        public string FirstPersonSmooth { get; } = "一人称時スムース";

        public string ZoomIn { get; } = "ズームイン";

        public string ZoomOut { get; } = "ズームアウト";

        public string ZoomSpeed { get; } = "ズームスピード";

        public string PauseTracking { get; } = "注視の" + Environment.NewLine + "停止/再開";

        public string SinglePlayerTeam { get; } = "Single Player" + Environment.NewLine + "Team";

        //CV Joint
        public string CvJoint { get; } = "ﾕﾆﾊﾞｰｻﾙｼﾞｮｲﾝﾄ";

        //Cannon
        public string FireInterval { get; } = "反応速度";

        public string RandomDelay { get; } = "ランダム遅延";

        public string Recoil { get; } = "反動";

        public string CustomBullet { get; } = "砲弾のカスタム";

        public string InheritSize { get; } = "スケーリング反映";

        public string BulletMass { get; } = "砲弾の重さ";

        public string BulletDrag { get; } = "砲弾の抗力";

        public string BulletDelayCollision { get; } = "衝突の遅延";

        public string Trail { get; } = "軌跡";

        public string TrailLength { get; } = "軌跡の長さ";

        public string TrailColor { get; } = "軌跡の色";

        //Decoupler
        public string ExplodeForce { get; } = "切り離し時の" + Environment.NewLine + "初速";

        public string ExplodeTorque { get; } = "切り離し時の" + Environment.NewLine + "ひねり";


        //Grip Pad & Piston & Slider & Suspension
        public List<string> MetalHardness { get; } = new List<string> { "軟質", "中硬", "硬質" };
        public List<string> WoodenHardness { get; } = new List<string> { "軟質", "中硬", "硬質", "超硬質" };

        //public  string hardness {get;}= "硬さ";

        public string Friction { get; } = "摩擦";
        public string Bounciness { get; } = "弾性";
        public string Collision { get; } = "衝突";

        //public  string softWood {get;}= "軟質";

        //public  string midSoftWood {get;}= "中硬";

        //public  string hardWood {get;}= "硬質";

        //public  string veryHardWood {get;}= "超硬質";

        //public  string lowCarbonSteel {get;}= "軟質";

        //public  string midCarbonSteel {get;}= "中硬";

        //public  string highCarbonSteel {get;}= "超硬質";

        public string Damper { get; } = "バネの重さ";

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

        public string ChangeSpeed { get; } = "変化速度";

        public string AddSpeed { get; } = "速度を加える";
        public string ReduceSpeed { get; } = "速度を落とす";

        //Armor
        public string ChangeChannel { get; } = "チャンネル変更";
        public string WidthPixel { get; } = "幅ピクセル";
        public string HeightPixel { get; } = "高さピクセル";
        public string Switch { get; } = "スイッチ";
        public List<string> NullChannelList { get; } = new List<string> { "チャンネルなし" };

        //ArmorRound
        public string Play { get; } = "遊び";
        public string Stop { get; } = "ストップ";
        public string Mute { get; } = "ミュート";
        public string Next { get; } = "次の方";
        public string Last { get; } = "前の方";
        public string Loop { get; } = "ループ";
        public string OneShot { get; } = "ワンショット";
        public string ReleaseToPause { get; } = "休止する";
        public string ReleaseToStop { get; } = "停止する";
        public string OnCollision { get; } = "衝突に関して";
        public string Volume { get; } = "体積";
        public string Pitch { get; } = "ピッチ";
        public string Distance { get; } = "ディスタンス";
        public string Doppler { get; } = "ドップラー";
        public string SpatialBlend { get; } = "空間ブレンド";

        //Balloon
        public string Effected { get; } = "イネーブルスイッチ";
        public string DragTogether { get; } = "一緒にドラッグ";
    }
}