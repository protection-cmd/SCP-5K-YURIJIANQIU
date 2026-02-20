using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using Respawning.Waves;
using SCP5K.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using static Subtitles.SubtitleCategory;

namespace SCP5K.SCPFouRole
{
    public static class Alpha9Manager
    {
        public static List<Player> A9TeamMembers = new List<Player>();
        public static Dictionary<Player, bool> VoteRecords = new Dictionary<Player, bool>();
        public static bool IsVotingActive = false;
        public static bool HasRebelled = false;

        public static Player Player105;
        public static Player Player076;

        // 技能CD记录
        public static Dictionary<Player, float> Cooldowns105 = new Dictionary<Player, float>();
        public static Dictionary<Player, float> Cooldowns076 = new Dictionary<Player, float>();

        // 105 的坐标记录
        public static List<Vector3> CameraRecords105 = new List<Vector3>();
        public static int CameraUses = 0;
        public static bool Skill3Used = false;
        public static bool Skill076_3Used = false;
        public static bool IsDawnOfOracleActive = false;
        private static bool _076Skill1Active = false;

        private static CoroutineHandle _voteCoroutine;
        private static CoroutineHandle _076RegenCoroutine;

        // 静态角色实例，供 Plugin.cs 注册使用
        public static SCP105 Role105 = new SCP105();
        public static SCP076 Role076 = new SCP076();
        public static A9CombatAgent RoleAgent = new A9CombatAgent();
        public static A9Soldier RoleSoldier = new A9Soldier();

        // 用于 Bot 的列表
        public static List<Npc> A9Bots = new List<Npc>();

        public static bool SpawnAlpha9(List<Player> players)
        {
            var ntfWave = new NtfSpawnWave();
            Respawn.PlayEffect(ntfWave);
            ResetA9();
            if (players.Count < 2) return false;

            
            int spawnCount = Mathf.Min(players.Count, 6);
            List<Player> selected = players.Take(spawnCount).ToList();

            Player105 = selected[0];
            Player076 = selected[1];

            A9TeamMembers.Add(Player105);
            A9TeamMembers.Add(Player076);
            Role105.AddRole(Player105);
            Role076.AddRole(Player076);

            for (int i = 2; i < spawnCount; i++)
            {
                A9TeamMembers.Add(selected[i]);
                if (i == 2)
                    RoleAgent.AddRole(selected[i]);
                else
                    RoleSoldier.AddRole(selected[i]);
            }

            IsVotingActive = true;
            foreach (var p in A9TeamMembers)
            {
                p.Broadcast(15, "<b><color=#00FFFF>你已被选为 Alpha-9 最后的希望 阵营！</color></b>\n请选择是否叛离SCP基金会\n控制台输入 <color=red>.a9 yes</color> 为叛变\n<color=green>.a9 no</color> 为不叛变\n你有 20 秒的时间进行投票！");
            }

            _voteCoroutine = Timing.RunCoroutine(VoteTimer());
            _076RegenCoroutine = Timing.RunCoroutine(Regen076Routine());

            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.UsedItem += OnUsed1344;
            Exiled.Events.Handlers.Player.Hurting += OnAlpha9Hurting;

            return true;
        }

        private static IEnumerator<float> VoteTimer()
        {
            yield return Timing.WaitForSeconds(20f);
            IsVotingActive = false;

            int yesVotes = VoteRecords.Values.Count(v => v == true);
            int noVotes = VoteRecords.Values.Count(v => v == false);

            if (noVotes > A9TeamMembers.Count / 2)
            {
                HasRebelled = false;
                foreach (var p in A9TeamMembers)
                {
                    p.Broadcast(5, "<b>投票结果：忠诚！向顽抗者降下刑罚！！！</b>");

                    if (Role105.Check(p)) p.Role.Set(RoleTypeId.NtfCaptain, RoleSpawnFlags.None);
                    else if (Role076.Check(p)) p.Role.Set(RoleTypeId.NtfSergeant, RoleSpawnFlags.None);
                    else p.Role.Set(RoleTypeId.NtfPrivate, RoleSpawnFlags.None);

                    p.AddItem(ItemType.Radio);
                }
            }
            else
            {
                HasRebelled = true;
                foreach (var p in A9TeamMembers)
                {
                    p.Broadcast(5, "<b>投票结果：叛离！拯救人类，消灭基金会！！！</b>");
                }
            }
        }

        public static void RegisterVote(Player player, bool isYes)
        {
            if (VoteRecords.ContainsKey(player)) VoteRecords[player] = isYes;
            else VoteRecords.Add(player, isYes);
        }

        private static void OnPlayerDied(DiedEventArgs ev)
        {
            // 076死亡，105获得加成
            if (ev.Player == Player076 && Player105 != null && Player105.IsAlive)
            {
                var message = "<color=red><b>SCP-076-2已死亡\n在挚友的尸体面前，你的想法是何呢？\n去复仇吧，速度大幅提升！</b></color>";
                HSMShowhint.HsmShowHint(Player105, message, 600, 0, 5f, "挚友的尸体");
                Player105.DisableEffect(EffectType.Slowness);
                Player105.EnableEffect(EffectType.MovementBoost, 100, 0f); 
            }
            // 105死亡，076获得加成
            else if (ev.Player == Player105 && Player076 != null && Player076.IsAlive)
            {
                var message = "<color=red><b>SCP-105已死亡\n在挚友的尸体面前，你的想法是何呢？\n去复仇吧，速度大幅提升！</b></color>";
                HSMShowhint.HsmShowHint(Player076, message, 600, 0, 5f, "挚友的尸体");
                Player076.EnableEffect(EffectType.MovementBoost, 50, 0f); 
            }

            if (ev.Player == Player076)
            {
                if (_076RegenCoroutine.IsRunning) Timing.KillCoroutines(_076RegenCoroutine);
            }
        }

        private static IEnumerator<float> Regen076Routine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(30f);
                if (Player076 != null && Player076.IsAlive)
                {
                    Player076.MaxHealth += 1;
                    Player076.Health += 1;
                }
            }
        }


        // 技能冷却系统
        private static bool CheckCooldown(Player player, Dictionary<Player, float> cooldownDict, float cooldownTime, out float remaining)
        {
            if (cooldownDict.TryGetValue(player, out float lastTime))
            {
                remaining = cooldownTime - (Time.time - lastTime);
                if (remaining > 0) return false;
            }
            remaining = 0;
            return true;
        }

        private static void SetCooldown(Player player, Dictionary<Player, float> cooldownDict)
        {
            cooldownDict[player] = Time.time;
        }


        // SCP-105 技能
        public static void Execute105Skill1(Player player)
        {
            if (CameraUses >= 3)
            {
                var message1 = "<color=red><b>你已经使用过3次SCP-105-B技能了！</b></color>";
                HSMShowhint.HsmShowHint(player, message1, 600, 0, 5f, "SCP-105-B");
                return;
            }
            if (!CheckCooldown(player, Cooldowns105, 60f, out float rem))
            {
                var message2 = $"<color=red><b>SCP-105-B冷却中，剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message2, 600, 0, 5f, "SCP-105-B冷却");
                return;
            }

            player.AddItem(ItemType.SCP1344);
            CameraUses++;
            SetCooldown(player, Cooldowns105);
            var message3 = "<color=#00FFFF>已发放SCP-105-B，可使用它记录当前位置！(半径5米圆)</color>";
            HSMShowhint.HsmShowHint(player, message3, 600, 0, 5f, "SCP-105-B发放");
        }

        private static void OnUsed1344(UsedItemEventArgs ev)
        {
            if (Role105.Check(ev.Player) && ev.Item.Type == ItemType.SCP1344)
            {
                CameraRecords105.Add(ev.Player.Position);
                var message = $"<color=green><b>成功记录坐标！当前记录点数量: {CameraRecords105.Count}</b></color>";
                HSMShowhint.HsmShowHint(ev.Player, message, 600, 0, 5f, "成功记录坐标");
                ev.Player.RemoveItem(ev.Item); 
            }
        }

        public static void Execute105Skill2(Player player)
        {
            if (!CheckCooldown(player, Cooldowns105, 90f, out float rem))
            {
                var message = $"<color=red><b>镜面攻击冷却中，剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "技能二冷却中");
                return;
            }

            if (CameraRecords105.Count == 0)
            {
                var message = "<color=red><b>没有记录任何坐标点，无法召唤！</b></color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "没有记录任何坐标点");
                return;
            }

            SetCooldown(player, Cooldowns105);
            var message2 = "<color=yellow><b>镜面攻击：在记录点召唤追踪 Bot！</b></color>";
            HSMShowhint.HsmShowHint(player, message2, 600, 0, 5f, "召唤追踪 Bot");

            foreach (var pos in CameraRecords105)
            {
                Npc bot = Npc.Spawn("SCP-105-Bot", RoleTypeId.Tutorial, pos);
                bot.Health = 1000;
                bot.MaxHealth = 1000;
                bot.AddItem(ItemType.GunAK);
                bot.CurrentItem = bot.Items.FirstOrDefault(x => x.Type == ItemType.GunAK);

                A9Bots.Add(bot);
                bot.GameObject.AddComponent<Alpha9BotAI>().Init(pos, 5f);
            }
        }

        public static void Execute105Skill3(Player player)
        {
            if (Skill3Used)
            {
                var message = "<color=red><b>你已经使用过这个技能了！</b></color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "使用过");
                return;
            }
            Skill3Used = true;

            List<Vector3> targetPositions = CameraRecords105.Count > 0
                ? new List<Vector3>(CameraRecords105)
                : new List<Vector3> { player.Position };

            var message2 = "<color=red><b>出手，出手，再出手！</b></color>";
            HSMShowhint.HsmShowHint(player, message2, 600, 0, 5f, "出手");

            foreach (var centerPos in targetPositions)
            {
                // 在半径5米圆内每一米放一个0.1秒引爆的手雷
                for (float x = -5; x <= 5; x += 1f)
                {
                    for (float z = -5; z <= 5; z += 1f)
                    {
                        if (x * x + z * z <= 25)
                        {
                            Vector3 spawnPos = centerPos + new Vector3(x, 0.5f, z);
                            ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                            grenade.FuseTime = 0.1f;
                            grenade.SpawnActive(spawnPos, player);
                        }
                    }
                }
            }

            CameraRecords105.Clear();

            // 105 自爆
            ExplosiveGrenade selfBomb = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
            selfBomb.FuseTime = 0.1f;
            selfBomb.SpawnActive(player.Position, player);
            player.Kill("自爆牺牲！");
        }

        public static void Execute105Skill4(Player player)
        {
            if (!CheckCooldown(player, Cooldowns105, 120f, out float rem))
            {
                player.Broadcast(3, $"技能四冷却中，剩余 {rem:F1} 秒");
                return;
            }

            SetCooldown(player, Cooldowns105);
            IsDawnOfOracleActive = true;
            var message = "<color=orange><b>狂欢，在那神谕的黎明！召唤物、076与自身增伤100% 持续10秒！</b></color>";
            HSMShowhint.HsmShowHint(player, message, 600, 0, 10f, "在那神谕的黎明");

            Timing.CallDelayed(10f, () =>
            {
                IsDawnOfOracleActive = false;
                if (player != null)
                {
                    var message2 = "<color=orange><b>神谕的黎明已过，增伤效果消失！</b></color>";
                    HSMShowhint.HsmShowHint(player, message2, 600, 0, 5f, "神谕的黎明已过");
                }
            });
        }

        // ==========================================
        // SCP-076-2 技能
        // ==========================================
        public static void Execute076Skill1(Player player)
        {
            if (!CheckCooldown(player, Cooldowns076, 35f, out float rem))
            {
                var message1 = $"<color=red><b>忘却往昔，燃尽此身冷却中，剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message1, 600, 0, 5f, "忘却往昔");
                return;
            }

            SetCooldown(player, Cooldowns076);
            player.MaxHealth -= 30;
            if (player.Health > player.MaxHealth) player.Health = player.MaxHealth;
            if (player.MaxHealth <= 0)
            {
                player.Kill("战至最后一刻，自刎归天！");
                return;
            }

            _076Skill1Active = true;
            player.EnableEffect(EffectType.MovementBoost, 40, 10f); // 持续10秒
            var message = "<color=red><b>忘却往昔，燃尽此身！1509伤害提升至150，移速增加！持续10秒！</b></color>";
            HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "燃尽此身");

            Timing.CallDelayed(10f, () =>
            {
                _076Skill1Active = false;
            });
        }

        public static void Execute076Skill2(Player player)
        {
            if (!CheckCooldown(player, Cooldowns076, 60f, out float rem))
            {
                var message = $"<color=red><b>徒有残躯，何谓未来冷却中，剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "徒有残躯");
                return;
            }

            SetCooldown(player, Cooldowns076);
            player.MaxHealth -= 20;
            if (player.Health > player.MaxHealth) player.Health = player.MaxHealth;
            if (player.MaxHealth <= 0)
            {
                player.Kill("站至最后一刻，自刎归天！");
                return;
            }

            // 根据要求：获得90%伤害抗性，即 DamageReduction, 180, 10s
            player.EnableEffect(EffectType.DamageReduction, 180, 10f);
            var message2 = "<color=yellow><b>徒有残躯，何谓未来！获得90%伤害抗性10秒！</b></color>";
            HSMShowhint.HsmShowHint(player, message2, 600, 0, 5f, "何谓未来");
        }

        public static void Execute076Skill3(Player player)
        {
            if (Skill076_3Used)
            {
                var message = "<color=red><b>你已经使用过这个技能了！</b></color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "使用过");
                return;
            }
            if (Player105 == null || !Player105.IsAlive)
            {
                var message = "<color=red><b>SCP-105已死亡，无法使用这个技能！</b></color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "SCP-105死亡");
                return;
            }

            Skill076_3Used = true;
            // 给予105额外30%移速持续10秒
            Player105.EnableEffect(EffectType.MovementBoost, 30, 10f);
            var message1 = "<color=yellow><b>想象一朵来自远方的玫瑰！已为 105 提供额外加速！</b></color>";
            HSMShowhint.HsmShowHint(player, message1, 600, 0, 5f, "想象一朵来自远方的玫瑰");
            var message2 = "<color=yellow><b>想象一朵来自远方的玫瑰！获得了来自 076 的加速支援！</b></color>";
            HSMShowhint.HsmShowHint(Player105, message2, 600, 0, 5f, "想象一朵来自远方的玫瑰");
        }

        // ==========================================
        // 伤害判定与重置
        // ==========================================
        private static void OnAlpha9Hurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null) return;

            // 105 技能四：100% 增伤
            if (IsDawnOfOracleActive && (Role105.Check(ev.Attacker) || Role076.Check(ev.Attacker) || ev.Attacker.Nickname == "SCP-105-Bot"))
            {
                ev.Amount *= 2f;
            }

            // 076 技能一：1509 (代指MicroHID或近战) 伤害变为 150/刀
            if (_076Skill1Active && Role076.Check(ev.Attacker))
            {
                ev.Amount = 150f;
            }

            // Bot AK 伤害最高 15/枪
            if (ev.Attacker.Nickname == "SCP-105-Bot" && ev.DamageHandler.Type == DamageType.Firearm)
            {
                if (ev.Amount > 15f) ev.Amount = 15f;
            }
        }

        public static void ResetA9()
        {
            A9TeamMembers.Clear();
            VoteRecords.Clear();
            CameraRecords105.Clear();
            Cooldowns105.Clear();
            Cooldowns076.Clear();

            foreach (var bot in A9Bots) { if (bot != null) bot.Destroy(); }
            A9Bots.Clear();

            CameraUses = 0;
            Skill3Used = false;
            Skill076_3Used = false;
            IsDawnOfOracleActive = false;
            _076Skill1Active = false;
            IsVotingActive = false;
            HasRebelled = false;
            Player105 = null;
            Player076 = null;

            if (_voteCoroutine.IsRunning) Timing.KillCoroutines(_voteCoroutine);
            if (_076RegenCoroutine.IsRunning) Timing.KillCoroutines(_076RegenCoroutine);

            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.UsedItem -= OnUsed1344;
            Exiled.Events.Handlers.Player.Hurting -= OnAlpha9Hurting;
        }
    }

    // ==========================================
    // 简易 Bot AI 脚本
    // ==========================================
    public class Alpha9BotAI : MonoBehaviour
    {
        private Npc _bot;
        private Vector3 _centerPos;
        private float _radius;

        public void Init(Vector3 center, float radius)
        {
            _bot = Npc.Get(gameObject);
            _centerPos = center;
            _radius = radius;
            InvokeRepeating(nameof(AutoAttack), 1f, 0.5f);
            _bot.EnableEffect(EffectType.Ensnared, 255);
        }

        
        private void AutoAttack()
        {
            if (_bot == null || !_bot.IsAlive)
            {
                CancelInvoke();
                return;
            }

            // 寻找圆内非自己阵营的敌人
            Player target = Player.List.FirstOrDefault(p =>
                p != _bot &&
                p.IsAlive &&
                Vector3.Distance(p.Position, _centerPos) <= _radius &&
                !Alpha9Manager.A9TeamMembers.Contains(p));

            if (target != null)
            {
                Vector3 direction = (target.Position - _bot.Position).normalized;
                if (direction != Vector3.zero)
                {
                    _bot.Rotation = Quaternion.LookRotation(direction);
                }
                // 模拟射击 (Exiled 8 可能有 _bot.Shoot 等封装，若无则使用底层发包或直接给玩家造伤)

                target.Hurt(_bot, 15f, DamageType.Firearm);
            }
        }

    }
}