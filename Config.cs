using System.ComponentModel;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using PlayerRoles;
using System.IO;
using System;

namespace SCP5K
{
    public class Config : IConfig
    {
        [Description("是否启用插件")]
        public bool IsEnabled { get; set; } = true;

        [Description("是否调试模式")]
        public bool Debug { get; set; } = false;

        [Description("Inf Ammo / 无限子弹")]
        public bool InfAmmo { get; set; } = true;


        [Description("是否在回合开始时选择良子")]
        public bool EnableSpecialDClass { get; set; } = true;

        [Description("D-9341 初始物品")]
        public List<ItemType> D9341InitialItems { get; set; } = new List<ItemType>
        {
            ItemType.Coin,
            ItemType.Flashlight,
            ItemType.KeycardJanitor
        };

        [Description("存档提示信息")]
        public string SaveHint { get; set; } = "已存档! 当前位置和物品已保存";

        [Description("读档提示信息")]
        public string LoadHint { get; set; } = "已读档! 回到上次存档点";

        [Description("无存档提示信息")]
        public string NoSaveHint { get; set; } = "没有可用的存档!";

        [Description("最大存档数量")]
        public int MaxSaves { get; set; } = 3;

        [Description("D-9341 角色类型")]
        public RoleTypeId D9341RoleType { get; set; } = RoleTypeId.ClassD;

        [Description("是否在回合开始时自动选择D9341")]
        public bool AutoSelectD9341 { get; set; } = true;

        [Description("Omega核弹音乐文件路径")]
        public string OmegaMusicPath { get; set; } = "C:/Path/To/Your/OmegaMusic.mp3";

        [Description("奇术打击音乐文件路径")]
        public string ArcaneMusicPath { get; set; } = "C:/Path/To/Your/ArcaneStrikeMusic.ogg";

        [Description("奇术打击倒计时时长")]
        public float ArcaneCountdownDuration { get; set; } = 30f;

        [Description("奇术打击模型生成位置X坐标")]
        public float ArcaneSchematicX { get; set; } = 0f;

        [Description("奇术打击模型生成位置Y坐标")]
        public float ArcaneSchematicY { get; set; } = 0f;

        [Description("奇术打击模型生成位置Z坐标")]
        public float ArcaneSchematicZ { get; set; } = 0f;

        [Description("GOC奇术打击小组入场音乐文件路径")]
        public string GOCSpawnMusicPath { get; set; } = "C:/Path/To/Your/GOCSpawnMusic.ogg";

        [Description("GOC奇术打击小组启动奇术打击音乐文件路径")]
        public string GOCMusicPath { get; set; } = "C:/Path/To/Your/GOCMusic.ogg";

        [Description("GOC触发奇术打击倒计时时长（秒）")]
        public float GOCCountdownDuration { get; set; } = 120f;

        [Description("GOC原理图生成位置X坐标")]
        public float GOCSchematicX { get; set; } = 0f;

        [Description("GOC原理图生成位置Y坐标")]
        public float GOCSchematicY { get; set; } = 0f;

        [Description("GOC原理图生成位置Z坐标")]
        public float GOCSchematicZ { get; set; } = 0f;

        [Description("GOC原理图名称")]
        public string GOCSchematicName { get; set; } = "入场动画";

        [Description("是否在回合开始时选择运动员")]
        public bool EnableAthlete { get; set; } = true;

        [Description("Nu-7原理图生成位置X坐标")]
        public float Nu7SchematicX { get; set; } = 0f;

        [Description("Nu-7原理图生成位置Y坐标")]
        public float Nu7SchematicY { get; set; } = 0f;

        [Description("Nu-7原理图生成位置Z坐标")]
        public float Nu7SchematicZ { get; set; } = 0f;

        // ★ 修改这部分为A连和B连的单独音乐路径
        [Description("Nu-7-A连原理图音乐文件路径")]
        public string Nu7ASpawnMusicPath { get; set; } = "C:/Path/To/Your/Nu7ASpawnMusic.ogg";

        [Description("Nu-7-B连原理图音乐文件路径")]
        public string Nu7BSpawnMusicPath { get; set; } = "C:/Path/To/Your/Nu7BSpawnMusic.ogg";

        [Description("是否启用自定义刷新管理器")]
        public bool EnableCustomSpawnManager { get; set; } = true;

        [Description("自定义刷新间隔（秒）")]
        public float CustomSpawnInterval { get; set; } = 200f;

        [Description("观察者检测间隔（秒）")]
        public float SpawnCheckInterval { get; set; } = 2f;

        [Description("可用的刷新阵容列表")]
        public List<string> AvailableSquads { get; set; } = new List<string>
        {
            "NU7",
            "GOC"
        };

        [Description("是否禁用所有原版刷新逻辑")]
        public bool DisableVanillaRespawns { get; set; } = true;

        [Description("是否启用SSS设置（服务器特定设置）")]
        public bool EnableSSS { get; set; } = true;

        // GOC打击小组配置
        [Description("GOCTeam原理图生成位置X坐标")]
        public float GOCTeamSchematicX { get; set; } = 0f;

        [Description("GOCTeam原理图生成位置Y坐标")]
        public float GOCTeamSchematicY { get; set; } = 0f;

        [Description("GOCTeam原理图生成位置Z坐标")]
        public float GOCTeamSchematicZ { get; set; } = 0f;

        [Description("GOCTeam原理图音乐文件路径")]
        public string GOCTeamSpawnMusicPath { get; set; } = "C:/Path/To/Your/GOCTeamSpawnMusic.ogg";

        [Description("GOCTeam原理图名称")]
        public string GOCTeamSchematicName { get; set; } = "GOCTeamRc";

        // MVP系统配置
        [Description("是否启用MVP系统")]
        public bool IsEnableMVP { get; set; } = true;

        [Description("MVP JSON数据库文件的完整路径")]
        public string MVPConfigFilePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EXILED",
            "Configs",
            "SCP5K",
            "mvp_database.json"
        );

        // RA命令权限配置
        [Description("5K RA命令所需的权限")]
        public string RoleAssignPermission { get; set; } = "5k.setrole";

        [Description("允许使用5K RA命令的角色列表")]
        public List<string> AllowedRolesForRoleAssign { get; set; } = new List<string>
        {
            "owner",
            "admin",
            "moderator"
        };
    }
}