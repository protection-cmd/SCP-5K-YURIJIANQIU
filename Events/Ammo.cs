using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using System.Linq;

namespace SCP5K
{
    internal class AmmoEvents
    {
        // 护甲类型与子弹上限的映射（包含无护甲情况）
        private readonly Dictionary<ItemType, Dictionary<AmmoType, ushort>> _armorAmmoLimits = new Dictionary<ItemType, Dictionary<AmmoType, ushort>>
        {
            {
                ItemType.None, // 无护甲
                new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato556, 40 },
                    { AmmoType.Ammo12Gauge, 14 },
                    { AmmoType.Nato9, 40 },
                    { AmmoType.Ammo44Cal, 18 },
                    { AmmoType.Nato762, 40 }
                }
            },
            {
                ItemType.ArmorLight, // 轻型护甲
                new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato556, 40 },
                    { AmmoType.Ammo12Gauge, 14 },
                    { AmmoType.Nato9, 70 },
                    { AmmoType.Ammo44Cal, 18 },
                    { AmmoType.Nato762, 40 }
                }
            },
            {
                ItemType.ArmorCombat, // 战术护甲
                new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato556, 120 },
                    { AmmoType.Ammo12Gauge, 54 },
                    { AmmoType.Nato9, 170 },
                    { AmmoType.Ammo44Cal, 48 },
                    { AmmoType.Nato762, 120 }
                }
            },
            {
                ItemType.ArmorHeavy, // 重型护甲
                new Dictionary<AmmoType, ushort>
                {
                    { AmmoType.Nato556, 200 },
                    { AmmoType.Ammo12Gauge, 74 },
                    { AmmoType.Nato9, 210 },
                    { AmmoType.Ammo44Cal, 68 },
                    { AmmoType.Nato762, 200 }
                }
            }
        };

        // 枪械与弹药类型的映射
        private readonly Dictionary<ItemType, AmmoType> _weaponAmmoMap = new Dictionary<ItemType, AmmoType>
        {
            { ItemType.GunE11SR, AmmoType.Nato556 },
            { ItemType.GunFRMG0, AmmoType.Nato556 },
            { ItemType.GunAK, AmmoType.Nato762 },
            { ItemType.GunA7, AmmoType.Nato762 },
            { ItemType.GunLogicer, AmmoType.Nato762 },
            { ItemType.GunCOM15, AmmoType.Nato9 },
            { ItemType.GunCOM18, AmmoType.Nato9 },
            { ItemType.GunCom45, AmmoType.Nato9 },
            { ItemType.GunFSP9, AmmoType.Nato9 },
            { ItemType.GunCrossvec, AmmoType.Nato9 },
            { ItemType.GunRevolver, AmmoType.Ammo44Cal },
            { ItemType.GunShotgun, AmmoType.Ammo12Gauge }
        };

        public void RegEvent()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Player.Dying += OnDying;
            Exiled.Events.Handlers.Player.DroppingAmmo += OnDroppingAmmo;
            Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;
        }

        public void UnRegEvent()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Player.Dying -= OnDying;
            Exiled.Events.Handlers.Player.DroppingAmmo -= OnDroppingAmmo;
            Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;
        }

        private void OnDroppingAmmo(DroppingAmmoEventArgs Args)
        {
            if (Plugin.Instance.Config.InfAmmo)
                Args.IsAllowed = false;
        }

        private void OnDroppingItem(DroppingItemEventArgs Args)
        {
            if (Args.Item.IsAmmo && Plugin.Instance.Config.InfAmmo)
                Args.IsAllowed = false;
        }

        private void OnRoundStart()
        {
            if (!Plugin.Instance.Config.InfAmmo)
                return;
            Timing.KillCoroutines($"{Plugin.Package}:InfAmmo");
            Timing.RunCoroutine(InfAmmo(), $"{Plugin.Package}:InfAmmo");
        }

        private void OnDying(DyingEventArgs ev)
        {
            if (Plugin.Instance.Config.InfAmmo)
                ev.Player.ClearAmmo();
        }

        private IEnumerator<float> InfAmmo()
        {
            while (true)
            {
                foreach (Player player in Player.List.Where(x => x.IsAlive))
                {
                    // 默认使用无护甲配置
                    ItemType armorType = ItemType.None;

                    // 查找玩家身上的护甲物品
                    var armorItem = player.Items.FirstOrDefault(item =>
                        item.Type == ItemType.ArmorLight ||
                        item.Type == ItemType.ArmorCombat ||
                        item.Type == ItemType.ArmorHeavy);

                    if (armorItem != null)
                    {
                        armorType = armorItem.Type;
                    }

                    // 获取当前护甲对应的子弹上限
                    if (!_armorAmmoLimits.TryGetValue(armorType, out var ammoLimits))
                        continue;

                    // 为玩家拥有的每种武器补充弹药
                    foreach (var item in player.Items)
                    {
                        if (!_weaponAmmoMap.TryGetValue(item.Type, out var ammoType))
                            continue;

                        if (ammoLimits.TryGetValue(ammoType, out var ammoLimit))
                        {
                            player.SetAmmo(ammoType, ammoLimit);
                        }
                    }
                }
                yield return Timing.WaitForSeconds(2f);
            }
        }
    }
}