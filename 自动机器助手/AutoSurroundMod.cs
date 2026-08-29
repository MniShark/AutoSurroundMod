using System;
using GenericModConfigMenu;
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
            helper.WriteConfig(Config);
			
			Monitor.Log(" 已加载", LogLevel.Info);

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

        private string GetText(string key, string fallback)
        {
            try { return Helper.Translation.Get(key); }
            catch { return fallback; }
        }

        private void TryRegisterWithGMCM()
        {
            try
            {
                IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
                if (gmcm == null) return;

                gmcm.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                gmcm.AddKeybind(
                    mod: ModManifest,
                    name: () => GetText("Config.TriggerKey.Name", "Trigger Key"),
                    tooltip: () => GetText("Config.TriggerKey.Desc", "Hold to repeat actions."),
                    getValue: () => Config.TriggerKey,
                    setValue: value => Config.TriggerKey = value
                );

                gmcm.AddNumberOption(
                    mod: ModManifest,
                    name: () => GetText("Config.IntervalMs.Name", "Interval (ms)"),
                    tooltip: () => GetText("Config.IntervalMs.Desc", "Time between actions."),
                    getValue: () => Config.IntervalMs,
                    setValue: value => Config.IntervalMs = value,
                    min: 50,
                    max: 1000,
                    interval: 10
                );

                gmcm.AddBoolOption(
                    mod: ModManifest,
                    name: () => GetText("Config.EnableMachineHarvest.Name", "Harvest Machines"),
                    tooltip: () => GetText("Config.EnableMachineHarvest.Desc", "Auto harvest finished machines."),
                    getValue: () => Config.EnableMachineHarvest,
                    setValue: value => Config.EnableMachineHarvest = value
                );

                gmcm.AddBoolOption(
                    mod: ModManifest,
                    name: () => GetText("Config.EnableMachineFill.Name", "Fill Machines"),
                    tooltip: () => GetText("Config.EnableMachineFill.Desc", "Auto fill machines with items."),
                    getValue: () => Config.EnableMachineFill,
                    setValue: value => Config.EnableMachineFill = value
                );

                gmcm.AddBoolOption(
                    mod: ModManifest,
                    name: () => GetText("Config.EnableFertilize.Name", "Fertilize Soil"),
                    tooltip: () => GetText("Config.EnableFertilize.Desc", "Auto fertilize unfertilized soil."),
                    getValue: () => Config.EnableFertilize,
                    setValue: value => Config.EnableFertilize = value
                );
            }
            catch (Exception ex)
            {
                Monitor.Log($"GMCM registration failed: {ex.Message}", LogLevel.Warn);
            }
        }
    }
}