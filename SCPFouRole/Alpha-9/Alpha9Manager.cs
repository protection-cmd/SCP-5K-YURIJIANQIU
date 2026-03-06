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
        public static bool IsRoleChanging = false; 

        public static Player Player105;
        public static Player Player076;

        public static Dictionary<Player, float> Cooldowns105_Skill1 = new Dictionary<Player, float>();
        public static Dictionary<Player, float> Cooldowns105_Skill2 = new Dictionary<Player, float>();
        public static Dictionary<Player, float> Cooldowns105_Skill4 = new Dictionary<Player, float>();

        public static Dictionary<Player, float> Cooldowns076_Skill1 = new Dictionary<Player, float>();
        public static Dictionary<Player, float> Cooldowns076_Skill2 = new Dictionary<Player, float>();

        // 105 的坐标记录
        public static List<Vector3> CameraRecords105 = new List<Vector3>();
        public static int CameraUses = 0;
        public static bool Skill3Used = false;
        public static bool Skill076_3Used = false;
        public static bool IsDawnOfOracleActive = false;
        private static bool _076Skill1Active = false;

        private static CoroutineHandle _voteCoroutine;
        private static CoroutineHandle _076RegenCoroutine;
        private static CoroutineHandle _076HumeCoroutine;

        // 静态角色实例，供 Plugin.cs 注册使用
        public static SCP105 Role105 = new SCP105();
        public static SCP076 Role076 = new SCP076();
        public static A9CombatAgent RoleAgent = new A9CombatAgent();
        public static A9Soldier RoleSoldier = new A9Soldier();

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

            Role105.AddRole(Player105);
            Role076.AddRole(Player076);

            for (int i = 2; i < spawnCount; i++)
            {
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
                IsRoleChanging = true; 

                foreach (var p in A9TeamMembers.ToList())
                {
                    p.Broadcast(5, "<b>投票结果：忠诚！向顽抗者降下刑罚！！！</b>");

                    var items = p.Items.Select(x => x.Type).ToList();
                    var ammo = p.Ammo.ToDictionary(x => x.Key, x => x.Value);
                    float health = p.Health;
                    int maxHealth = (int)p.MaxHealth;
                    float hume = p.HumeShield;

                    bool was105 = (p == Player105);
                    bool was076 = (p == Player076);
                    bool wasAgent = RoleAgent.Check(p);
                    bool wasSoldier = RoleSoldier.Check(p);

                    if (was105) p.Role.Set(RoleTypeId.NtfCaptain, RoleSpawnFlags.None);
                    else if (was076) p.Role.Set(RoleTypeId.NtfSergeant, RoleSpawnFlags.None);
                    else p.Role.Set(RoleTypeId.NtfPrivate, RoleSpawnFlags.None);

                    Timing.CallDelayed(0.1f, () =>
                    {
                        if (p == null || !p.IsAlive) return;

                        p.ClearInventory();
                        foreach (var item in items) p.AddItem(item);
                        foreach (var kvp in ammo) p.AddAmmo((AmmoType)kvp.Key, kvp.Value);
                        p.AddItem(ItemType.Radio);

                        p.MaxHealth = maxHealth;
                        p.Health = health;
                        p.HumeShield = hume;

                        // 恢复特有 buff
                        if (was105) p.EnableEffect(EffectType.Slowness, 20);
                        if (wasAgent) p.EnableEffect(EffectType.DamageReduction, 100);
                        if (wasSoldier) p.EnableEffect(EffectType.DamageReduction, 50);
                    });
                }

                Timing.CallDelayed(0.2f, () => IsRoleChanging = false);
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
            if (ev.Player == Player076 && Player105 != null && Player105.IsAlive)
            {
                var message = "<color=red><b>SCP-076-2已死亡\n在挚友的尸体面前，你的想法是何呢？\n去复仇吧，速度大幅提升！</b></color>";
                HSMShowhint.HsmShowHint(Player105, message, 750, 0, 5f, "挚友的尸体");
                Player105.DisableEffect(EffectType.Slowness);
                Player105.EnableEffect(EffectType.MovementBoost, 100, 0f);
            }
            else if (ev.Player == Player105 && Player076 != null && Player076.IsAlive)
            {
                var message = "<color=red><b>SCP-105已死亡\n在挚友的尸体面前，你的想法是何呢？\n去复仇吧，速度大幅提升！</b></color>";
                HSMShowhint.HsmShowHint(Player076, message, 750, 0, 5f, "挚友的尸体");
                Player076.EnableEffect(EffectType.MovementBoost, 50, 0f);
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

        private static IEnumerator<float> HumeShield076Routine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(10f);
                if (Player076 != null && Player076.IsAlive)
                {
                    Player076.HumeShield = 50;
                }
            }
        }

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


        // ==========================================
        // SCP-105 技能
        // ==========================================
        public static void Execute105Skill1(Player player)
        {
            if (CameraUses >= 3)
            {
                var message1 = "<color=red><b>你已经使用过3次SCP-105-B技能了！</b></color>";
                HSMShowhint.HsmShowHint(player, message1, 750, 0, 5f, "SCP-105-B");
                return;
            }
            if (!CheckCooldown(player, Cooldowns105_Skill1, 60f, out float rem))
            {
                var message2 = $"<color=red><b>SCP-105-B冷却中，剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message2, 750, 0, 5f, "SCP-105-B冷却");
                return;
            }

            player.AddItem(ItemType.SCP1344);
            CameraUses++;
            SetCooldown(player, Cooldowns105_Skill1);
            var message3 = "<color=#00FFFF>已发放SCP-105-B，可使用它记录当前位置！(半径5米圆)</color>";
            HSMShowhint.HsmShowHint(player, message3, 750, 0, 5f, "SCP-105-B发放");
        }

        private static void OnUsed1344(UsingItemEventArgs ev)
        {
            if (ev.Player == Player105 && ev.Item.Type == ItemType.SCP1344)
            {
                ev.IsAllowed = false;
                ev.Player.RemoveItem(ev.Item);
                CameraRecords105.Add(ev.Player.Position);
                var message = $"<color=green><b>成功记录坐标！当前记录点数量: {CameraRecords105.Count}</b></color>";
                HSMShowhint.HsmShowHint(ev.Player, message, 750, 0, 5f, "成功记录坐标");
            }
        }

        public static void Execute105Skill2(Player player)
        {
            if (!CheckCooldown(player, Cooldowns105_Skill2, 90f, out float rem))
            {
                var message = $"<color=red><b>镜面攻击冷却中，剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message, 750, 0, 5f, "技能二冷却中");
                return;
            }

            if (CameraRecords105.Count == 0)
            {
                var message = "<color=red><b>没有记录任何坐标点，无法召唤！</b></color>";
                HSMShowhint.HsmShowHint(player, message, 750, 0, 5f, "没有记录任何坐标点");
                return;
            }

            SetCooldown(player, Cooldowns105_Skill2);
            var message2 = "<color=yellow><b>镜面攻击：在记录点召唤追踪 Bot！</b></color>";
            HSMShowhint.HsmShowHint(player, message2, 750, 0, 5f, "召唤追踪 Bot");

            foreach (var oldBot in A9Bots.ToList())
            {
                if (oldBot != null)
                {
                    oldBot.ClearInventory(); 
                    oldBot.Destroy();
                }
            }
            A9Bots.Clear();

            foreach (var pos in CameraRecords105)
            {
                Npc bot = Npc.Spawn("SCP-105-Bot", RoleTypeId.Tutorial, pos);
                bot.Health = 1000;
                bot.MaxHealth = 1000;

                A9Bots.Add(bot);
                bot.GameObject.AddComponent<Alpha9BotAI>().Init(pos, 5f);
            }
        }

        public static void Execute105Skill3(Player player)
        {
            if (Skill3Used)
            {
                var message = "<color=red><b>你已经使用过这个技能了！</b></color>";
                HSMShowhint.HsmShowHint(player, message, 750, 0, 5f, "使用过");
                return;
            }
            Skill3Used = true;

            bool hasRecords = CameraRecords105.Count > 0;

            List<Vector3> targetPositions = hasRecords
                ? new List<Vector3>(CameraRecords105)
                : new List<Vector3> { player.Position };

            var message2 = "<color=red><b>出手，出手，再出手！</b></color>";
            HSMShowhint.HsmShowHint(player, message2, 750, 0, 5f, "出手");

            foreach (var centerPos in targetPositions)
            {
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

            if (!hasRecords)
            {
                ExplosiveGrenade selfBomb = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                selfBomb.FuseTime = 0.1f;
                selfBomb.SpawnActive(player.Position, player);
                player.Kill("自爆牺牲！");
            }
            else
            {
                var message3 = "<color=green><b>远程打击已完成！</b></color>";
                HSMShowhint.HsmShowHint(player, message3, 750, 0, 5f, "远程打击成功");
            }

            Skill3Used = false;
        }

        public static void Execute105Skill4(Player player)
        {

            if (!CheckCooldown(player, Cooldowns105_Skill4, 120f, out float rem))
            {
                var message1 = $"<color=red><b>在那神谕的黎明冷却中\n剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message1, 750, 0, 5f, "在那神谕的黎明冷却");
                return;
            }

            SetCooldown(player, Cooldowns105_Skill4);
            IsDawnOfOracleActive = true;
            var message = "<color=orange><b>狂欢，在那神谕的黎明！\n召唤物、076与自身增伤100% 持续10秒！</b></color>";
            HSMShowhint.HsmShowHint(player, message, 750, 0, 10f, "在那神谕的黎明");

            Timing.CallDelayed(10f, () =>
            {
                IsDawnOfOracleActive = false;
                if (player != null)
                {
                    var message2 = "<color=orange><b>神谕的黎明已过\n增伤效果消失！</b></color>";
                    HSMShowhint.HsmShowHint(player, message2, 750, 0, 5f, "神谕的黎明已过");
                }
            });
        }

        // ==========================================
        // SCP-076-2 技能
        // ==========================================
        public static void Execute076Skill1(Player player)
        {
            if (!CheckCooldown(player, Cooldowns076_Skill1, 35f, out float rem))
            {
                var message1 = $"<color=red><b>忘却往昔，燃尽此身冷却中\n剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message1, 750, 0, 5f, "忘却往昔");
                return;
            }

            SetCooldown(player, Cooldowns076_Skill1);
            player.MaxHealth -= 30;
            if (player.Health > player.MaxHealth) player.Health = player.MaxHealth;
            if (player.MaxHealth <= 0)
            {
                player.Kill("战至最后一刻，自刎归天！");
                return;
            }

            _076Skill1Active = true;
            player.EnableEffect(EffectType.MovementBoost, 40, 10f);
            var message = "<color=red><b>忘却往昔，燃尽此身！\n1509伤害提升至150，移速增加！持续10秒！</b></color>";
            HSMShowhint.HsmShowHint(player, message, 750, 0, 5f, "燃尽此身");

            Timing.CallDelayed(10f, () =>
            {
                _076Skill1Active = false;
            });
        }

        public static void Execute076Skill2(Player player)
        {
            if (!CheckCooldown(player, Cooldowns076_Skill2, 60f, out float rem))
            {
                var message = $"<color=red><b>徒有残躯，何谓未来冷却中\n剩余 {rem:F1} 秒</b></color>";
                HSMShowhint.HsmShowHint(player, message, 750, 0, 5f, "徒有残躯");
                return;
            }

            SetCooldown(player, Cooldowns076_Skill2);
            player.MaxHealth -= 20;
            if (player.Health > player.MaxHealth) player.Health = player.MaxHealth;
            if (player.MaxHealth <= 0)
            {
                player.Kill("站至最后一刻，自刎归天！");
                return;
            }

            player.EnableEffect(EffectType.DamageReduction, 180, 10f);
            var message2 = "<color=yellow><b>徒有残躯，何谓未来！\n获得90%伤害抗性10秒！</b></color>";
            HSMShowhint.HsmShowHint(player, message2, 750, 0, 5f, "何谓未来");
        }

        public static void Execute076Skill3(Player player)
        {
            if (Skill076_3Used)
            {
                var message = "<color=red><b>你已经使用过这个技能了！</b></color>";
                HSMShowhint.HsmShowHint(player, message, 750, 0, 5f, "使用过");
                return;
            }
            if (Player105 == null || !Player105.IsAlive)
            {
                var message = "<color=red><b>SCP-105已死亡\n无法使用这个技能！</b></color>";
                HSMShowhint.HsmShowHint(player, message, 750, 0, 5f, "SCP-105死亡");
                return;
            }

            Skill076_3Used = true;
            Player105.EnableEffect(EffectType.MovementBoost, 30, 10f);
            var message1 = "<color=yellow><b>想象一朵来自远方的玫瑰！\n已为 105 提供额外加速！</b></color>";
            HSMShowhint.HsmShowHint(player, message1, 750, 0, 5f, "想象一朵来自远方的玫瑰");
            var message2 = "<color=yellow><b>想象一朵来自远方的玫瑰！\n获得了来自 076 的加速支援！</b></color>";
            HSMShowhint.HsmShowHint(Player105, message2, 750, 0, 5f, "想象一朵来自远方的玫瑰");
        }


        private static void OnAlpha9Hurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null)
            {
                return;
            }


            if (ev.Attacker.Nickname == "SCP-105-Bot" && ev.DamageHandler.Type == DamageType.AK)
            {
                ev.Amount = 15f;
            }


            bool is1509Damage = ev.DamageHandler.Type == DamageType.Scp1509;
            if (is1509Damage)
            {
                if (ev.Attacker == Player076 && _076Skill1Active)
                {
                    ev.Amount = 150f;
                }
            }


            if (IsDawnOfOracleActive && (ev.Attacker == Player105 || ev.Attacker == Player076 || ev.Attacker.Nickname == "SCP-105-Bot"))
            {
                ev.Amount *= 2f;
            }
        }

        private static void On076HoldSCP1509(ChangingItemEventArgs ev)
        {
            if (ev.Player == Player076 && ev.Item.Type == ItemType.SCP1509)
            {
                ev.Player.HumeShield = 50;
            }
        }

        public static void ResetA9()
        {
            A9TeamMembers.Clear();
            VoteRecords.Clear();
            CameraRecords105.Clear();


            Cooldowns105_Skill1.Clear();
            Cooldowns105_Skill2.Clear();
            Cooldowns105_Skill4.Clear();
            Cooldowns076_Skill1.Clear();
            Cooldowns076_Skill2.Clear();

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
        }

        public static void RegisterEvents()
        {
            ResetA9();

            _076RegenCoroutine = Timing.RunCoroutine(Regen076Routine());
            _076HumeCoroutine = Timing.RunCoroutine(HumeShield076Routine());

            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.UsingItem += OnUsed1344;
            Exiled.Events.Handlers.Player.Hurting += OnAlpha9Hurting;
            Exiled.Events.Handlers.Player.ChangingItem += On076HoldSCP1509;
        }

        public static void UnregisterEvents()
        {
            ResetA9();

            if (_076RegenCoroutine.IsRunning) Timing.KillCoroutines(_076RegenCoroutine);
            if (_076HumeCoroutine.IsRunning) Timing.KillCoroutines(_076HumeCoroutine);

            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.UsingItem -= OnUsed1344;
            Exiled.Events.Handlers.Player.Hurting -= OnAlpha9Hurting;
            Exiled.Events.Handlers.Player.ChangingItem -= On076HoldSCP1509;
        }
    }


    public class Alpha9BotAI : MonoBehaviour
    {
        private Npc _bot;
        private Vector3 _centerPos;
        private float _radius;
        private bool _isShooting = false; 

        public void Init(Vector3 center, float radius)
        {
            _bot = Npc.Get(gameObject);
            _centerPos = center;
            _radius = radius;
            _bot.EnableEffect(EffectType.Ensnared, 255);

            MEC.Timing.CallDelayed(0.5f, () =>
            {
                if (_bot != null && _bot.IsAlive)
                {
                    _bot.ClearInventory();
                    _bot.AddItem(ItemType.GunAK);
                    _bot.CurrentItem = _bot.Items.FirstOrDefault(x => x.Type == ItemType.GunAK);
                }
            });


            InvokeRepeating(nameof(AutoAttack), 1.5f, 0.3f);
        }

        private void AutoAttack()
        {
            if (_bot == null || _bot.GameObject == null)
            {
                CancelInvoke();
                return;
            }

            if (!_bot.IsAlive)
            {
                CancelInvoke();
                Alpha9Manager.A9Bots.Remove(_bot);
                _bot.Destroy();
                return;
            }

            Player target = Player.List.FirstOrDefault(p =>
                p != _bot &&
                p.IsAlive &&
                Vector3.Distance(p.Position, _centerPos) <= _radius &&
                !Alpha9Manager.A9TeamMembers.Contains(p));

            if (target != null)
            {

                Vector3 targetPos = target.Position + Vector3.up * 0.7f;
                Vector3 direction = (targetPos - _bot.CameraTransform.position).normalized;

                if (direction != Vector3.zero)
                {
                    _bot.Rotation = Quaternion.LookRotation(direction);
                }

                if (_bot.CurrentItem != null && _bot.CurrentItem is Firearm firearm)
                {

                    if (firearm.MagazineAmmo < firearm.MaxMagazineAmmo)
                    {
                        firearm.MagazineAmmo = firearm.MaxMagazineAmmo;
                    }


                    if (!_isShooting)
                    {
                        string cmd = $"/dummy action {_bot.Id} {firearm.Type}_(ANY) Shoot->Hold";
                        Server.ExecuteCommand(cmd);
                        _isShooting = true;
                    }
                }
                else
                {
                    target.Hurt(_bot, 15f, DamageType.Firearm);
                }
            }
            else
            {
                if (_isShooting && _bot.CurrentItem != null && _bot.CurrentItem is Firearm firearm)
                {
                    string cmd = $"/dummy action {_bot.Id} {firearm.Type}_(ANY) Shoot->Release";
                    Server.ExecuteCommand(cmd);
                    _isShooting = false;
                }
            }
        }
    }
}