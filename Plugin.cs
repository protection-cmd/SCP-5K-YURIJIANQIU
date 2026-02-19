using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using LabApi.Events.Handlers;
using Respawning;
using SCP5K.Events;
using SCP5K.LCZRole;
using SCP5K.MVPSystem;
using SCP5K.SCPFouRole;
using System;
using System.IO;

namespace SCP5K
{
    public class Plugin : Plugin<Config>
    {
        public const string Package = "YuRiJianQiu-5K";
        public override string Name => "SCP-5K-YuRiJianQiu";
        public override string Author => "聿日箋秋-Protection";
        public override Version Version => new Version(1, 0, 1);
        public override Version RequiredExiledVersion => new Version(9, 10, 2);

        public static Plugin Instance;
        private D9341EventHandler d9341Handler;
        internal AmmoEvents AmmoEvents { get; } = new AmmoEvents();

        private MVPConfigManager mvpConfigManager;
        private MusicPlayer musicPlayer;
        private MvpEvent mvpEvent;
        private MVPSystem.MVPSystem mvpSystem;
        private Capybalas capybalas;
        private GateC gateC;

        [Obsolete]
        public override void OnEnabled()
        {
            Instance = this;

            // 注册角色
            RegisterAllCustomRoles();

            // 注册技能状态清空管家
            SkillCleanupManager.RegisterEvents();

            AmmoEvents.RegEvent();
            EnsureMVPConfigDirectoryExists();

            this.mvpConfigManager = new MVPConfigManager();
            this.mvpConfigManager.LoadMVPConfig();
            this.musicPlayer = new MusicPlayer(mvpConfigManager);
            this.mvpEvent = new MvpEvent(mvpConfigManager);
            this.mvpSystem = new MVPSystem.MVPSystem(musicPlayer);
            this.mvpConfigManager.StartAutoReload();

            this.capybalas = new Capybalas();
            this.capybalas.SubscribeEvents();

            this.gateC = new GateC();
            this.gateC.SubscribeEvents();

            Exiled.Events.Handlers.Player.Verified += this.mvpEvent.Verified;
            Exiled.Events.Handlers.Player.Dying += this.mvpEvent.Dying;
            Exiled.Events.Handlers.Player.Hurting += this.mvpEvent.Hurting;
            Exiled.Events.Handlers.Scp079.GainingExperience += this.mvpEvent.OnGainingExperience;
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.mvpEvent.WaitingForPlayer;
            Exiled.Events.Handlers.Server.RoundEnded += this.mvpSystem.RoundEnded;

            Exiled.Events.Handlers.Server.WaitingForPlayers += this.musicPlayer.WaitingForPlayer;
            Exiled.Events.Handlers.Server.RoundEnded += (ev) => this.musicPlayer.RoundEnded();

            if (Config.DisableVanillaRespawns)
            {
                VanillaSpawnDisabler.DisableVanillaRespawns = Config.DisableVanillaRespawns;
                VanillaSpawnDisabler.Init();
                VanillaSpawnDisabler.RegisterEvents();
            }

            // ★ 移除：DDpig 和 DDRunning 的直接事件注册
            // 它们的逻辑已移交 ClassDSpawnManager 统一管理

            // ★ 新增：D级人员统一刷新管理器
            ClassDSpawnManager.RegisterEvents();

            // D9341 仍需注册事件以处理丢物品、存档等交互逻辑
            d9341Handler = new D9341EventHandler();
            d9341Handler.RegisterEvents();

            Exiled.Events.Handlers.Server.RoundStarted += OmegaWarhead.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded.Subscribe(ev => OmegaWarhead.OnRoundEnd());
            Exiled.Events.Handlers.Server.RoundStarted += CASSIE.OnRoundStarted;

            ArcaneStrike.MusicPath = Config.ArcaneMusicPath;
            ArcaneStrike.CountdownDuration = Config.ArcaneCountdownDuration;
            ArcaneStrike.SchematicPosition = new UnityEngine.Vector3(Config.ArcaneSchematicX, Config.ArcaneSchematicY, Config.ArcaneSchematicZ);

            GOCArcaneStrike.MusicPath = Config.GOCMusicPath;
            GOCArcaneStrike.SpawnMusicPath = Config.GOCSpawnMusicPath;
            GOCArcaneStrike.CountdownDuration = Config.GOCCountdownDuration;
            GOCArcaneStrike.SchematicPosition = new UnityEngine.Vector3(Config.GOCSchematicX, Config.GOCSchematicY, Config.GOCSchematicZ);
            GOCArcaneStrike.SchematicName = Config.GOCSchematicName;
            GOCArcaneStrike.RGMSchematicPosition = new UnityEngine.Vector3(Config.ArcaneSchematicX, Config.ArcaneSchematicY, Config.ArcaneSchematicZ);

            Nu7HammerDown.SchematicPosition = new UnityEngine.Vector3(Config.Nu7SchematicX, Config.Nu7SchematicY, Config.Nu7SchematicZ);
            Nu7HammerDown.SpawnMusicPathA = Config.Nu7ASpawnMusicPath;
            Nu7HammerDown.SpawnMusicPathB = Config.Nu7BSpawnMusicPath;

            GOCTeam.SchematicPosition = new UnityEngine.Vector3(Config.GOCTeamSchematicX, Config.GOCTeamSchematicY, Config.GOCTeamSchematicZ);
            GOCTeam.SchematicName = Config.GOCTeamSchematicName;
            GOCTeam.SpawnMusicPath = Config.GOCTeamSpawnMusicPath;

            GOCArcaneStrike.RegisterEvents();
            Nu7HammerDown.RegisterEvents();
            GOCTeam.RegisterEvents();
            CIGRU.RegisterEvents();
            SCP682.RegisterEvents();
            SCP610.RegisterEvents();

            // 注册 GRU-CI 事件管理器
            GRUCIManager.RegisterEvents();

            if (Config.EnableCustomSpawnManager)
            {
                CustomSpawnManager.Init();
                CustomSpawnManager.SpawnInterval = Config.CustomSpawnInterval;
                CustomSpawnManager.CheckInterval = Config.SpawnCheckInterval;
                CustomSpawnManager.AvailableSquads = Config.AvailableSquads;
                CustomSpawnManager.RegisterEvents();
            }

            if (Config.EnableSSS) SSSSettings.Register();

            Log.Info("SCP-5K 插件已成功启动！");
            base.OnEnabled();
        }

        private void RegisterAllCustomRoles()
        {
            try
            {
                // Nu-7 A连 与 B连
                Nu7ACommander.Instance.Register();
                Nu7AJiFeng.Instance.Register();
                Nu7APrivate.Instance.Register();
                Nu7BCommander.Instance.Register();
                Nu7BTieXue.Instance.Register();
                Nu7BPrivate.Instance.Register();

                // GOC 与 CIGRU
                GOCCommander.Instance.Register();
                GOCHeavy.Instance.Register();
                GOCSergeant.Instance.Register();
                GOCPrivate.Instance.Register();
                GOCArcaneCommander.Instance.Register();
                GOCArcaneSergeant.Instance.Register();
                CICommanderRole.Instance.Register();
                CIRaznovRole.Instance.Register();
                CIHeavyRole.Instance.Register();
                CIRiflemanRole.Instance.Register();

                // GRU-CI 特遣队所有职业注册
                GRUCIHacker.Instance.Register();
                GRUCIBreacher.Instance.Register();
                GRUCIDemolitionist.Instance.Register();
                GRUCICommander.Instance.Register();
                GRUCIInvestigator.Instance.Register();
                GRUCISoldier.Instance.Register();
                GRUCIConscript.Instance.Register();

                // SCP 与 特殊D级
                SCP610MotherRole.Instance.Register();
                SCP610SprayerRole.Instance.Register();
                SCP610ChildRole.Instance.Register();
                SCP682Role.Instance.Register();
                D9341Role.Instance.Register();
                LiangziRole.Instance.Register();
                AthleteRole.Instance.Register();

                Log.Info("所有 CustomRole 角色通过单例模式硬核注册完毕！");
            }
            catch (Exception ex)
            {
                Log.Error($"注册自定义角色时出错: {ex}");
            }
        }

        public override void OnDisabled()
        {
            CustomRole.UnregisterRoles();

            this.mvpConfigManager?.StopAutoReload();
            Exiled.Events.Handlers.Player.Verified -= this.mvpEvent.Verified;
            Exiled.Events.Handlers.Player.Dying -= this.mvpEvent.Dying;
            Exiled.Events.Handlers.Player.Hurting -= this.mvpEvent.Hurting;
            Exiled.Events.Handlers.Scp079.GainingExperience -= this.mvpEvent.OnGainingExperience;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.mvpEvent.WaitingForPlayer;
            Exiled.Events.Handlers.Server.RoundEnded -= this.mvpSystem.RoundEnded;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.musicPlayer.WaitingForPlayer;
            Exiled.Events.Handlers.Server.RoundEnded -= (ev) => this.musicPlayer.RoundEnded();

            this.capybalas?.UnsubscribeEvents();
            this.gateC?.UnsubscribeEvents();
            d9341Handler?.UnregisterEvents();
            AmmoEvents.UnRegEvent();
            SkillCleanupManager.UnregisterEvents();

            Exiled.Events.Handlers.Server.RoundStarted -= OmegaWarhead.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded.Unsubscribe(ev => OmegaWarhead.OnRoundEnd());
            Exiled.Events.Handlers.Server.RoundStarted -= CASSIE.OnRoundStarted;

            ClassDSpawnManager.UnregisterEvents();
            GOCArcaneStrike.UnregisterEvents();
            Nu7HammerDown.UnregisterEvents();
            GOCTeam.UnregisterEvents();
            CIGRU.UnregisterEvents();
            SCP682.UnregisterEvents();
            SCP610.UnregisterEvents();
            GRUCIManager.UnregisterEvents();
            CustomSpawnManager.UnregisterEvents();
            VanillaSpawnDisabler.UnregisterEvents();
            SSSSettings.Unregister();

            this.mvpConfigManager = null;
            this.musicPlayer = null;
            this.mvpEvent = null;
            this.mvpSystem = null;
            this.capybalas = null;
            this.gateC = null;
            d9341Handler = null;
            Instance = null;
            base.OnDisabled();
        }

        [Obsolete]
        public override void OnReloaded()
        {
            base.OnReloaded();
        }

        private void EnsureMVPConfigDirectoryExists()
        {
            try
            {
                string mvpConfigDir = Path.GetDirectoryName(Config.MVPConfigFilePath);
                if (!Directory.Exists(mvpConfigDir)) Directory.CreateDirectory(mvpConfigDir);
            }
            catch (Exception ex) { Log.Error($"创建MVP配置目录时出错: {ex.Message}"); }
        }
    }
}