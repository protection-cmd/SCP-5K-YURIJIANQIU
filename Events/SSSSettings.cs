using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using System.Collections.Generic;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace SCP5K.SCPFouRole
{
    public static class SSSSettings
    {
        internal static IEnumerable<SettingBase> _settings;

        // Nu-7 A连
        public const int NU7A_CMDR_SKILL1_ID = 110;
        public const int NU7A_CMDR_SKILL2_ID = 111;
        public const int NU7A_JIFENG_SKILL_ID = 112;

        // Nu-7 B连
        public const int NU7B_CMDR_SKILL1_ID = 113;
        public const int NU7B_CMDR_SKILL2_ID = 114;
        public const int NU7B_TIEXUE_SKILL1_ID = 115;
        public const int NU7B_TIEXUE_SKILL2_ID = 116;

        public const int ATHLETE_ABILITY_KEYBIND_ID = 101;
        public const int GOC_COMMANDER_ABILITY_KEYBIND_ID = 102;
        public const int GOC_HEAVY_ABILITY_KEYBIND_ID = 103;
        public const int GOC_SERGEANT_ABILITY_KEYBIND_ID = 104;
        public const int SCP682_ABHORRENCE_ABILITY_KEYBIND_ID = 105;
        public const int SCP682_PITIFUL_ABILITY_KEYBIND_ID = 106;
        public const int CI_RAZNOV_COIN_ABILITY_KEYBIND_ID = 107;

        // GRU-CI 阵营技能ID
        public const int GRUCI_DEMO_SKILL_ID = 130;
        public const int GRUCI_CMDR_SKILL_ID = 131;
        public const int GRUCI_INV_SKILL1_ID = 132;
        public const int GRUCI_INV_SKILL2_ID = 133;

        // Alpha-9 阵营技能ID
        public const int A9_105_SKILL1_ID = 140;
        public const int A9_105_SKILL2_ID = 141;
        public const int A9_105_SKILL3_ID = 142;
        public const int A9_105_SKILL4_ID = 143;
        public const int A9_076_SKILL1_ID = 144;
        public const int A9_076_SKILL2_ID = 145;
        public const int A9_076_SKILL3_ID = 146;

        [System.Obsolete]
        public static void Register()
        {
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnAbilityKeybindsReceived;

            _settings = new SettingBase[]
            {
                new HeaderSetting("SCP基金会 Nu-7-A连","",false),
                new KeybindSetting(NU7A_CMDR_SKILL1_ID, "指挥官-先发制人", KeyCode.G, hintDescription: "设置锚点20秒后强行返回(CD 60s)"),
                new KeybindSetting(NU7A_CMDR_SKILL2_ID, "指挥官-全军出击", KeyCode.H, hintDescription: "存活队友20%加速持续10秒(CD 60s)"),
                new KeybindSetting(NU7A_JIFENG_SKILL_ID, "疾风-无畏无惧", KeyCode.G, hintDescription: "自身50%减伤+20%加速持续10秒(CD 40s)"),

                new HeaderSetting("SCP基金会 Nu-7-B连","",false),
                new KeybindSetting(NU7B_CMDR_SKILL1_ID, "指挥官-献祭过往", KeyCode.G, hintDescription: "记录坐标3秒后大清算(单次技能)"),
                new KeybindSetting(NU7B_CMDR_SKILL2_ID, "指挥官-画地为牢", KeyCode.H, hintDescription: "禁锢周围非基金会阵营1.5秒(CD 60s)"),
                new KeybindSetting(NU7B_TIEXUE_SKILL1_ID, "铁血-再著诗篇", KeyCode.G, hintDescription: "立刻获得一把囚鸟(CD 120s)"),
                new KeybindSetting(NU7B_TIEXUE_SKILL2_ID, "铁血-冲，冲，冲！", KeyCode.H, hintDescription: "加速并造成2倍伤害持续5秒(CD 60s)"),

                new HeaderSetting("GRU-CI 特遣队","",false),
                new KeybindSetting(GRUCI_DEMO_SKILL_ID, "爆破手-火力充足", KeyCode.G, hintDescription: "获得一个手雷 (CD 35s)"),
                new KeybindSetting(GRUCI_CMDR_SKILL_ID, "指挥官-高斯放电", KeyCode.G, hintDescription: "获得一把电炮 (仅1次)"),
                new KeybindSetting(GRUCI_INV_SKILL1_ID, "考察员-寻找真相", KeyCode.G, hintDescription: "为所有存活队友提升移速 (CD 50s)"),
                new KeybindSetting(GRUCI_INV_SKILL2_ID, "考察员-牵引器", KeyCode.H, hintDescription: "将一名随机敌人拉到面前定身 (仅1次)"),

                new HeaderSetting("轻收容阵营","",false),
                new KeybindSetting(ATHLETE_ABILITY_KEYBIND_ID, "爆发极限", KeyCode.G, hintDescription: "短时间内大幅提升移动速度"),

                new HeaderSetting("全球超自然联盟打击小组","",false),
                new KeybindSetting(GOC_COMMANDER_ABILITY_KEYBIND_ID, "指挥官-静谧行动", KeyCode.G, hintDescription: "获得高额虚化效果持续10秒"),
                new KeybindSetting(GOC_HEAVY_ABILITY_KEYBIND_ID, "凝神静气", KeyCode.G, hintDescription: "获得50%伤害抗性持续5秒"),
                new KeybindSetting(GOC_SERGEANT_ABILITY_KEYBIND_ID, "生命付之一炬", KeyCode.G, hintDescription: "获得护盾但生命上限降至75(一次性)"),

                new HeaderSetting("SCP-682 不灭孽蜥","",false),
                new KeybindSetting(SCP682_ABHORRENCE_ABILITY_KEYBIND_ID, "憎恶", KeyCode.G, hintDescription: "普攻附带心脏骤停20秒"),
                new KeybindSetting(SCP682_PITIFUL_ABILITY_KEYBIND_ID, "可悲", KeyCode.H, hintDescription: "每秒回复10HP持续10秒"),

                new HeaderSetting("混沌分裂者GRU小组","",false),
                new KeybindSetting(CI_RAZNOV_COIN_ABILITY_KEYBIND_ID, "雷泽诺夫-小心脚下", KeyCode.G, hintDescription: "重新获得一个硬币"),

                new HeaderSetting("Alpha-9 最后的希望","",false),
                new KeybindSetting(A9_105_SKILL1_ID, "105-技能一:SCP-105-B", KeyCode.Alpha1, hintDescription: "获得一个特殊SCP-1344记录坐标(CD60s)"),
                new KeybindSetting(A9_105_SKILL2_ID, "105-技能二:镜面攻击", KeyCode.Alpha2, hintDescription: "记录点召唤BOT追踪攻击(CD90s)"),
                new KeybindSetting(A9_105_SKILL3_ID, "105-技能三:出手再出手", KeyCode.Alpha3, hintDescription: "在记录点放炸弹并自爆(单次)"),
                new KeybindSetting(A9_105_SKILL4_ID, "105-技能四:神谕的黎明", KeyCode.Alpha4, hintDescription: "召唤物与076与自身100%增伤(CD120s)"),
                new KeybindSetting(A9_076_SKILL1_ID, "076-技能一:忘却往昔", KeyCode.G, hintDescription: "加移速及伤害，扣30生命上限(CD35s)"),
                new KeybindSetting(A9_076_SKILL2_ID, "076-技能二:徒有残躯", KeyCode.H, hintDescription: "获得90%减伤，扣20生命上限(CD60s)"),
                new KeybindSetting(A9_076_SKILL3_ID, "076-技能三:远方的玫瑰", KeyCode.J, hintDescription: "给予105额外30%移速(单次)")
            };

            SettingBase.Register(_settings);
        }

        public static void Unregister()
        {
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnAbilityKeybindsReceived;
        }

        private static void OnAbilityKeybindsReceived(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is not SSKeybindSetting keybindSetting || !keybindSetting.SyncIsPressed) return;
            if (!Player.TryGet(referenceHub, out Player player)) return;

            switch (keybindSetting.SettingId)
            {
                case NU7A_CMDR_SKILL1_ID: if (Nu7ACommander.Instance.Check(player)) Nu7HammerDown.ExecuteNu7ACmdrSkill1(player); break;
                case NU7A_CMDR_SKILL2_ID: if (Nu7ACommander.Instance.Check(player)) Nu7HammerDown.ExecuteNu7ACmdrSkill2(player); break;
                case NU7A_JIFENG_SKILL_ID: if (Nu7AJiFeng.Instance.Check(player)) Nu7HammerDown.ExecuteNu7AJiFengSkill(player); break;
                case NU7B_CMDR_SKILL1_ID: if (Nu7BCommander.Instance.Check(player)) Nu7HammerDown.ExecuteNu7BCmdrSkill1(player); break;
                case NU7B_CMDR_SKILL2_ID: if (Nu7BCommander.Instance.Check(player)) Nu7HammerDown.ExecuteNu7BCmdrSkill2(player); break;
                case NU7B_TIEXUE_SKILL1_ID: if (Nu7BTieXue.Instance.Check(player)) Nu7HammerDown.ExecuteNu7BTieXueSkill1(player); break;
                case NU7B_TIEXUE_SKILL2_ID: if (Nu7BTieXue.Instance.Check(player)) Nu7HammerDown.ExecuteNu7BTieXueSkill2(player); break;

                case GRUCI_DEMO_SKILL_ID: if (GRUCIDemolitionist.Instance.Check(player)) GRUCIManager.ExecuteDemoSkill(player); break;
                case GRUCI_CMDR_SKILL_ID: if (GRUCICommander.Instance.Check(player)) GRUCIManager.ExecuteCmdrSkill(player); break;
                case GRUCI_INV_SKILL1_ID: if (GRUCIInvestigator.Instance.Check(player)) GRUCIManager.ExecuteInvSkill1(player); break;
                case GRUCI_INV_SKILL2_ID: if (GRUCIInvestigator.Instance.Check(player)) GRUCIManager.ExecuteInvSkill2(player); break;

                case ATHLETE_ABILITY_KEYBIND_ID: if (SCP5K.LCZRole.DDRunning.IsAthlete(player)) SCP5K.LCZRole.DDRunning.ActivateAthleteAbility(player); break;
                case GOC_COMMANDER_ABILITY_KEYBIND_ID: if (GOCTeam.IsCommander(player)) GOCTeam.ExecuteCommanderAbilityFromKeybind(player); break;
                case GOC_HEAVY_ABILITY_KEYBIND_ID: if (GOCTeam.IsHeavy(player)) GOCTeam.ExecuteHeavyAbilityFromKeybind(player); break;
                case GOC_SERGEANT_ABILITY_KEYBIND_ID: if (GOCTeam.IsSergeant(player)) GOCTeam.ExecuteSergeantAbilityFromKeybind(player); break;
                case SCP682_ABHORRENCE_ABILITY_KEYBIND_ID: if (SCP682.IsSCP682(player)) SCP682.ExecuteAbhorrenceAbilityFromKeybind(player); break;
                case SCP682_PITIFUL_ABILITY_KEYBIND_ID: if (SCP682.IsSCP682(player)) SCP682.ExecutePitifulAbilityFromKeybind(player); break;
                case CI_RAZNOV_COIN_ABILITY_KEYBIND_ID: if (CIGRU.IsRaznov(player)) CIGRU.ExecuteRaznovCoinAbilityFromKeybind(player); break;

                
                case A9_105_SKILL1_ID: if (Alpha9Manager.Role105.Check(player)) Alpha9Manager.Execute105Skill1(player); break;
                case A9_105_SKILL2_ID: if (Alpha9Manager.Role105.Check(player)) Alpha9Manager.Execute105Skill2(player); break;
                case A9_105_SKILL3_ID: if (Alpha9Manager.Role105.Check(player)) Alpha9Manager.Execute105Skill3(player); break;
                case A9_105_SKILL4_ID: if (Alpha9Manager.Role105.Check(player)) Alpha9Manager.Execute105Skill4(player); break;
                case A9_076_SKILL1_ID: if (Alpha9Manager.Role076.Check(player)) Alpha9Manager.Execute076Skill1(player); break;
                case A9_076_SKILL2_ID: if (Alpha9Manager.Role076.Check(player)) Alpha9Manager.Execute076Skill2(player); break;
                case A9_076_SKILL3_ID: if (Alpha9Manager.Role076.Check(player)) Alpha9Manager.Execute076Skill3(player); break;
            }
        }
    }
}