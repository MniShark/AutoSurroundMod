using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace AutoSurroundMod
{
    public class ModConfig
    {
        public SButton TriggerKey { get; set; } = SButton.MouseMiddle;
        public int IntervalMs { get; set; } = 200;
        public bool EnableMachineHarvest { get; set; } = true;
        public bool EnableMachineFill { get; set; } = true;
        public bool EnableFertilize { get; set; } = true;
    }

    public class ModEntry : Mod
    {
        private ModConfig Config;
        private bool _isRolling = false;
        private float _timer = 0f;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            TryRegisterWithGMCM();
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Config.TriggerKey && !_isRolling && Context.IsWorldReady && Game1.activeClickableMenu == null)
            {
                _isRolling = true;
                _timer = 0f;
                DoSurroundAction();
            }
        }

        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == Config.TriggerKey)
            {
                _isRolling = false;
                _timer = 0f;
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_isRolling || !Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            _timer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
            if (_timer >= Config.IntervalMs)
            {
                _timer = 0f;
                DoSurroundAction();
            }
        }

        private void DoSurroundAction()
        {
            Farmer player = Game1.player;
            GameLocation location = Game1.currentLocation;
            Vector2 playerTile = player.Tile;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2 tile = playerTile + new Vector2(dx, dy);
                    if (!location.isTileOnMap(tile)) continue;
                    ProcessTile(location, tile, player);
                }
            }
        }

        private void ProcessTile(GameLocation location, Vector2 tile, Farmer player)
        {
            // 机器
            if (location.objects.TryGetValue(tile, out StardewValley.Object obj))
            {
                if (obj.bigCraftable.Value)
                {
                    if (Config.EnableMachineHarvest && obj.checkForAction(player, false))
                        return;

                    if (Config.EnableMachineFill && player.CurrentItem != null)
                        obj.performObjectDropInAction(player.CurrentItem, false, player);
                }
                return;
            }

            // 施肥
            if (Config.EnableFertilize)
            {
                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature tf) && tf is HoeDirt dirt)
                {
                    Item held = player.CurrentItem;
                    if (held != null && IsFertilizer(held))
                    {
                        if (!dirt.HasFertilizer())
                        {
                            dirt.fertilizer.Value = held.ItemId;
                            player.reduceActiveItemByOne();
                        }
                    }
                }
            }
        }

        private bool IsFertilizer(Item item)
        {
            if (item.Category == StardewValley.Object.fertilizerCategory)
                return true;
            string id = item.ItemId;
            return id == "(O)465" || id == "(O)466" || id == "(O)919" || id == "(O)920" || id == "(O)921";
        }

        private void TryRegisterWithGMCM()
        {
            try
            {
                dynamic gmcm = Helper.ModRegistry.GetApi("spacechase0.GenericModConfigMenu");
                if (gmcm == null)
                {
                    Monitor.Log("GMCM not found.", LogLevel.Info);
                    return;
                }

                gmcm.Register(
                    ModManifest,
                    (Action)(() => Config = new ModConfig()),
                    (Action)(() => Helper.WriteConfig(Config)),
                    false
                );

                // 每个选项独立 try-catch，避免一个失败导致全部中断
                // 1. 触发按键
                try
                {
                    gmcm.AddKeybind(
                        mod: ModManifest,
                        name: (Func<string>)(() => Helper.Translation.Get("Config.TriggerKey.Name")),
                        tooltip: (Func<string>)(() => Helper.Translation.Get("Config.TriggerKey.Desc")),
                        getValue: (Func<SButton>)(() => Config.TriggerKey),
                        setValue: (Action<SButton>)(value => Config.TriggerKey = value)
                    );
                    Monitor.Log("Added TriggerKey option.", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to add TriggerKey: {ex.Message}", LogLevel.Warn);
                }

                // 2. 间隔时间
                try
                {
                    gmcm.AddNumberOption(
                        mod: ModManifest,
                        getValue: (Func<int>)(() => Config.IntervalMs),
                        setValue: (Action<int>)(value => Config.IntervalMs = value),
                        name: (Func<string>)(() => Helper.Translation.Get("Config.IntervalMs.Name")),
                        tooltip: (Func<string>)(() => Helper.Translation.Get("Config.IntervalMs.Desc"))
                    );
                    Monitor.Log("Added IntervalMs option.", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to add IntervalMs: {ex.Message}", LogLevel.Warn);
                }

                // 3. 启用机器收获
                try
                {
                    gmcm.AddBoolOption(
                        mod: ModManifest,
                        name: (Func<string>)(() => Helper.Translation.Get("Config.EnableMachineHarvest.Name")),
                        tooltip: (Func<string>)(() => Helper.Translation.Get("Config.EnableMachineHarvest.Desc")),
                        getValue: (Func<bool>)(() => Config.EnableMachineHarvest),
                        setValue: (Action<bool>)(value => Config.EnableMachineHarvest = value)
                    );
                    Monitor.Log("Added EnableMachineHarvest option.", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to add EnableMachineHarvest: {ex.Message}", LogLevel.Warn);
                }

                // 4. 启用机器填充
                try
                {
                    gmcm.AddBoolOption(
                        mod: ModManifest,
                        name: (Func<string>)(() => Helper.Translation.Get("Config.EnableMachineFill.Name")),
                        tooltip: (Func<string>)(() => Helper.Translation.Get("Config.EnableMachineFill.Desc")),
                        getValue: (Func<bool>)(() => Config.EnableMachineFill),
                        setValue: (Action<bool>)(value => Config.EnableMachineFill = value)
                    );
                    Monitor.Log("Added EnableMachineFill option.", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to add EnableMachineFill: {ex.Message}", LogLevel.Warn);
                }

                // 5. 启用施肥
                try
                {
                    gmcm.AddBoolOption(
                        mod: ModManifest,
                        name: (Func<string>)(() => Helper.Translation.Get("Config.EnableFertilize.Name")),
                        tooltip: (Func<string>)(() => Helper.Translation.Get("Config.EnableFertilize.Desc")),
                        getValue: (Func<bool>)(() => Config.EnableFertilize),
                        setValue: (Action<bool>)(value => Config.EnableFertilize = value)
                    );
                    Monitor.Log("Added EnableFertilize option.", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed to add EnableFertilize: {ex.Message}", LogLevel.Warn);
                }

                Monitor.Log("GMCM registration process completed.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"GMCM registration failed completely: {ex.Message}", LogLevel.Warn);
            }
        }
    }
}