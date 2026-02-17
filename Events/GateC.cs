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
    internal class GateC
    {
        private List<Text> spawnedtext = new List<Text>();

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
            SpawnTextAtFixedPositions();
        }



        private void OnRestartingRound()
        {
            CleanupText();
        }

        private void SpawnTextAtFixedPositions()
        {
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-41.5f,294,-42.7f)
            };

            foreach (Vector3 position in positions)
            {
                try
                {

                    AdminToys.TextToy toyBase = UnityEngine.Object.Instantiate(Text.Prefab);

                    // 获取Exiled包装类
                    Text text = Text.Get(toyBase) as Text;

                    if (text != null)
                    {
                        // 设置位置、旋转和缩放
                        text.Base.TextFormat = "<color=green>GATE C</color>";
                        text.Position = position;
                        text.Rotation = Quaternion.Euler(0f, 270f, 0f);
                        text.Scale = Vector3.one; // 默认缩放



                        // 保持静态以优化性能并防止意外移动
                        text.IsStatic = true;

                        // 生成
                        text.Spawn();

                        // 添加到已生成列表
                        spawnedtext.Add(text);

                        Log.Debug($"已生成Text在位置: {position}");
                    }
                    else
                    {
                        Log.Error("无法创建Text实例，获取的包装类为null");
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"生成Text时出错: {e}");
                }
            }

            Log.Info($"成功生成text");
        }

        private void CleanupText()
        {
            foreach (var text in spawnedtext)
            {
                if (text != null)
                {
                    try
                    {
                            text.UnSpawn();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"销毁TEXT时出错: {e}");
                    }
                }
            }
            spawnedtext.Clear();
            Log.Debug("已清理TEXT");


        }
    }
}