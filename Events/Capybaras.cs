using AdminToys;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCP5K.Events
{
    internal class Capybalas
    {
        private List<Capybara> spawnedCapybaras = new List<Capybara>();

        public void SubscribeEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RestartingRound += OnRestartingRound;
        }

        public void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RestartingRound -= OnRestartingRound;
        }

        private void OnRoundStarted()
        {
            SpawnCapybarasAtFixedPositions();
        }

        

        private void OnRestartingRound()
        {
            CleanupCapybaras();
        }

        private void SpawnCapybarasAtFixedPositions()
        {
            // 定义四个固定位置
            Vector3[] positions = new Vector3[]
            {
                new Vector3(30.010f, 291.548f, -23.854f),
                new Vector3(29.700f, 291.548f, -23.854f),
                new Vector3(29.400f, 291.548f, -23.854f),
                new Vector3(29.100f, 291.548f, -23.854f)
            };

            foreach (Vector3 position in positions)
            {
                try
                {
                    // 使用Prefab实例化CapybaraToy
                    AdminToys.CapybaraToy toyBase = UnityEngine.Object.Instantiate(Capybara.Prefab);

                    // 获取Exiled包装类
                    Capybara capybara = Capybara.Get(toyBase) as Capybara;

                    if (capybara != null)
                    {
                        // 设置位置、旋转和缩放
                        capybara.Position = position;
                        capybara.Rotation = Quaternion.Euler(0f, 180f, 0f);
                        capybara.Scale = Vector3.one; // 默认缩放

                        // 设置碰撞属性
                        capybara.Collidable = true;

                        // 保持静态以优化性能并防止意外移动
                        capybara.IsStatic = true;

                        // 生成卡皮巴拉
                        capybara.Spawn();

                        // 添加到已生成列表
                        spawnedCapybaras.Add(capybara);

                        Log.Debug($"已生成卡皮巴拉在位置: {position}");
                    }
                    else
                    {
                        Log.Error("无法创建Capybara实例，获取的包装类为null");
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"生成卡皮巴拉时出错: {e}");
                }
            }

            Log.Info($"总共生成了 {spawnedCapybaras.Count} 个卡皮巴拉");
        }

        private void CleanupCapybaras()
        {
            foreach (var capybara in spawnedCapybaras)
            {
                if (capybara != null)
                {
                    try
                    {
                        capybara.UnSpawn();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"销毁卡皮巴拉时出错: {e}");
                    }
                }
            }
            spawnedCapybaras.Clear();
            Log.Debug("已清理所有卡皮巴拉");

            
        }
    }
}