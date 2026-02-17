using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using System.Collections.Generic;
using UnityEngine;
using UserSettings.ServerSpecific;
using SCP5K.LCZRole;
using SCP5K.SCPFouRole;

namespace SCP5K.SCPFouRole
{
    public static class SSSSettings
    {
        internal static IEnumerable<SettingBase> _settings;

        // 定义SSS设置ID
        public const int NU7_COMMANDER_ABILITY_KEYBIND_ID = 100;
        public const int ATHLETE_ABILITY_KEYBIND_ID = 101;
        public const int GOC_COMMANDER_ABILITY_KEYBIND_ID = 102;
        public const int GOC_HEAVY_ABILITY_KEYBIND_ID = 103;
        public const int GOC_SERGEANT_ABILITY_KEYBIND_ID = 104;

        // 添加SCP-682设置ID
        public const int SCP682_ABHORRENCE_ABILITY_KEYBIND_ID = 105;
        public const int SCP682_PITIFUL_ABILITY_KEYBIND_ID = 106;

        // 添加CI雷泽诺夫设置ID
        public const int CI_RAZNOV_COIN_ABILITY_KEYBIND_ID = 107;

        [System.Obsolete]
        public static void Register()
        {
            // 注册按键绑定事件
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnCommanderAbilityKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnAthleteAbilityKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnGOCCommanderAbilityKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnGOCHeavyAbilityKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnGOCSergeantAbilityKeybind;

            // 注册SCP-682按键绑定事件
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSCP682AbhorrenceKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSCP682PitifulKeybind;

            // 注册CI雷泽诺夫按键绑定事件
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnCIRaznovCoinKeybind;

            _settings =
            [
                new HeaderSetting("SCP基金会 Nu-7-落锤","",false),
                new KeybindSetting(NU7_COMMANDER_ABILITY_KEYBIND_ID, "战术协调能力", KeyCode.G,
                    hintDescription: "按下使用战术协调能力，牺牲队友血量换取伤害抗性"),

                new HeaderSetting("轻收容阵营","",false),
                new KeybindSetting(ATHLETE_ABILITY_KEYBIND_ID, "爆发极限", KeyCode.G,
                    hintDescription: "按下使用爆发极限，短时间内大幅提升移动速度"),

                // 添加GOC设置
                new HeaderSetting("全球超自然联盟打击小组","",false),
                new KeybindSetting(GOC_COMMANDER_ABILITY_KEYBIND_ID, "指挥官-静谧行动", KeyCode.G,
                    hintDescription: "按下使用静谧行动，获得高额虚化效果持续10秒（60秒冷却）"),
                new KeybindSetting(GOC_HEAVY_ABILITY_KEYBIND_ID, "凝神静气", KeyCode.G,
                    hintDescription: "按下使用凝神静气，获得50%伤害抗性持续5秒（30秒冷却）"),
                new KeybindSetting(GOC_SERGEANT_ABILITY_KEYBIND_ID, "生命付之一炬", KeyCode.G,
                    hintDescription: "按下使用生命付之一炬，获得护盾和名刀但生命上限降至75（一次性能力）"),
                
                // 添加SCP-682设置
                new HeaderSetting("SCP-682 不灭孽蜥","",false),
                new KeybindSetting(SCP682_ABHORRENCE_ABILITY_KEYBIND_ID, "憎恶", KeyCode.G,
                    hintDescription: "按下激活憎恶能力，普通攻击附带心脏骤停效果20秒（50秒冷却）"),
                new KeybindSetting(SCP682_PITIFUL_ABILITY_KEYBIND_ID, "可悲", KeyCode.H,
                    hintDescription: "按下激活可悲能力，每秒回复10HP，持续10秒（50秒冷却）"),
                
                // 添加混沌分裂者GRU设置
                new HeaderSetting("混沌分裂者GRU小组","",false),
                new KeybindSetting(CI_RAZNOV_COIN_ABILITY_KEYBIND_ID, "雷泽诺夫-小心脚下", KeyCode.G,
                    hintDescription: "按下重新获得一个硬币（60秒冷却）")
            ];

            SettingBase.Register(_settings);
        }

        public static void Unregister()
        {
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnCommanderAbilityKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnAthleteAbilityKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnGOCCommanderAbilityKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnGOCHeavyAbilityKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnGOCSergeantAbilityKeybind;

            // 注销SCP-682事件
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSCP682AbhorrenceKeybind;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSCP682PitifulKeybind;

            // 注销CI雷泽诺夫事件
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnCIRaznovCoinKeybind;
        }

        private static void OnCommanderAbilityKeybind(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            // 检查是否是Nu-7指挥官能力按键
            if (settingBase is not SSKeybindSetting keybindSetting ||
                keybindSetting.SettingId != NU7_COMMANDER_ABILITY_KEYBIND_ID ||
                !keybindSetting.SyncIsPressed)
                return;

            if (!Player.TryGet(referenceHub, out Player player))
                return;

            // 检查玩家是否是Nu-7指挥官
            if (!Nu7HammerDown.IsNu7Member(player) ||
                !Nu7HammerDown.IsCommander(player))
                return;

            // 触发指挥官能力
            Nu7HammerDown.ExecuteCommanderAbilityFromKeybind(player);
        }

        private static void OnAthleteAbilityKeybind(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            // 检查是否是运动员能力按键
            if (settingBase is not SSKeybindSetting keybindSetting ||
                keybindSetting.SettingId != ATHLETE_ABILITY_KEYBIND_ID ||
                !keybindSetting.SyncIsPressed)
                return;

            if (!Player.TryGet(referenceHub, out Player player))
                return;

            // 检查玩家是否是运动员
            if (!DDRunning.IsAthlete(player))
                return;

            // 触发运动员爆发极限能力
            DDRunning.ActivateAthleteAbility(player);
        }

        // GOC指挥官能力按键绑定处理
        private static void OnGOCCommanderAbilityKeybind(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is not SSKeybindSetting keybindSetting ||
                keybindSetting.SettingId != GOC_COMMANDER_ABILITY_KEYBIND_ID ||
                !keybindSetting.SyncIsPressed)
                return;

            if (!Player.TryGet(referenceHub, out Player player))
                return;

            // 检查玩家是否是GOC指挥官
            if (!GOCTeam.IsGOCMember(player) || !GOCTeam.IsCommander(player))
                return;

            // 触发指挥官能力
            GOCTeam.ExecuteCommanderAbilityFromKeybind(player);
        }

        // GOC重装能力按键绑定处理
        private static void OnGOCHeavyAbilityKeybind(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is not SSKeybindSetting keybindSetting ||
                keybindSetting.SettingId != GOC_HEAVY_ABILITY_KEYBIND_ID ||
                !keybindSetting.SyncIsPressed)
                return;

            if (!Player.TryGet(referenceHub, out Player player))
                return;

            // 检查玩家是否是GOC重装
            if (!GOCTeam.IsGOCMember(player) || !GOCTeam.IsHeavy(player))
                return;

            // 触发重装能力
            GOCTeam.ExecuteHeavyAbilityFromKeybind(player);
        }

        // GOC中士能力按键绑定处理
        private static void OnGOCSergeantAbilityKeybind(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is not SSKeybindSetting keybindSetting ||
                keybindSetting.SettingId != GOC_SERGEANT_ABILITY_KEYBIND_ID ||
                !keybindSetting.SyncIsPressed)
                return;

            if (!Player.TryGet(referenceHub, out Player player))
                return;

            // 检查玩家是否是GOC中士
            if (!GOCTeam.IsGOCMember(player) || !GOCTeam.IsSergeant(player))
                return;

            // 触发中士能力
            GOCTeam.ExecuteSergeantAbilityFromKeybind(player);
        }

        // SCP-682憎恶能力按键绑定处理
        private static void OnSCP682AbhorrenceKeybind(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is not SSKeybindSetting keybindSetting ||
                keybindSetting.SettingId != SCP682_ABHORRENCE_ABILITY_KEYBIND_ID ||
                !keybindSetting.SyncIsPressed)
                return;

            if (!Player.TryGet(referenceHub, out Player player))
                return;

            // 检查玩家是否是SCP-682
            if (!SCP682.IsSCP682(player))
                return;

            // 触发憎恶能力
            SCP682.ExecuteAbhorrenceAbilityFromKeybind(player);
        }

        // SCP-682可悲能力按键绑定处理
        private static void OnSCP682PitifulKeybind(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is not SSKeybindSetting keybindSetting ||
                keybindSetting.SettingId != SCP682_PITIFUL_ABILITY_KEYBIND_ID ||
                !keybindSetting.SyncIsPressed)
                return;

            if (!Player.TryGet(referenceHub, out Player player))
                return;

            // 检查玩家是否是SCP-682
            if (!SCP682.IsSCP682(player))
                return;

            // 触发可悲能力
            SCP682.ExecutePitifulAbilityFromKeybind(player);
        }

        // CI雷泽诺夫硬币能力按键绑定处理
        private static void OnCIRaznovCoinKeybind(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is not SSKeybindSetting keybindSetting ||
                keybindSetting.SettingId != CI_RAZNOV_COIN_ABILITY_KEYBIND_ID ||
                !keybindSetting.SyncIsPressed)
                return;

            if (!Player.TryGet(referenceHub, out Player player))
                return;

            // 检查玩家是否是雷泽诺夫
            if (!CIGRU.IsRaznov(player))
                return;

            // 触发雷泽诺夫的硬币能力
            CIGRU.ExecuteRaznovCoinAbilityFromKeybind(player);
        }
    }
}