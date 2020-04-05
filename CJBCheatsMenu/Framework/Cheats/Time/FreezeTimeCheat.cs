using System;
using System.Collections.Generic;
using CJB.Common;
using CJBCheatsMenu.Framework.Components;
using CJBCheatsMenu.Framework.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace CJBCheatsMenu.Framework.Cheats.Time
{
    /// <summary>A cheat which stops the passage of time.</summary>
    internal class FreezeTimeCheat : BaseCheat
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the config UI fields to show in the cheats menu.</summary>
        /// <param name="context">The cheat context.</param>
        public override IEnumerable<OptionsElement> GetFields(CheatContext context)
        {
            return new[]
            {
                this.GetField(context, "inside", context.Config.FreezeTimeInside, value => context.Config.FreezeTimeInside = value),
                this.GetField(context, "caves", context.Config.FreezeTimeCaves, value => context.Config.FreezeTimeCaves = value),
                this.GetField(context, "everywhere", context.Config.FreezeTime, value => context.Config.FreezeTime = value)
            };
        }

        /// <summary>Handle the cheat options being loaded or changed.</summary>
        /// <param name="context">The cheat context.</param>
        /// <param name="needsUpdate">Whether the cheat should be notified of game updates.</param>
        /// <param name="needsInput">Whether the cheat should be notified of button presses.</param>
        /// <param name="needsRendering">Whether the cheat should be notified of render ticks.</param>
        public override void OnConfig(CheatContext context, out bool needsInput, out bool needsUpdate, out bool needsRendering)
        {
            needsInput = context.Config.FreezeTimeKey != SButton.None;
            needsUpdate = needsInput || context.Config.FreezeTime || context.Config.FreezeTimeInside || context.Config.FreezeTimeCaves;
            needsRendering = needsUpdate;
        }

        /// <summary>Handle the player pressing a button if <see cref="ICheat.OnSaveLoaded"/> indicated input was needed.</summary>
        /// <param name="context">The cheat context.</param>
        /// <param name="e">The input event arguments.</param>
        public override void OnButtonPressed(CheatContext context, ButtonPressedEventArgs e)
        {
            ModConfig config = context.Config;

            if (e.Button == config.FreezeTimeKey)
                config.FreezeTime = !config.FreezeTime;
        }

        /// <summary>Handle a game update if <see cref="ICheat.OnSaveLoaded"/> indicated updates were needed.</summary>
        /// <param name="context">The cheat context.</param>
        /// <param name="e">The update event arguments.</param>
        public override void OnUpdated(CheatContext context, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (this.ShouldFreezeTime(context.Config, Game1.currentLocation, out bool _))
                Game1.gameTimeInterval = 0;
        }

        /// <summary>Handle the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="context">The cheat context.</param>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public override void OnRendered(CheatContext context, SpriteBatch spriteBatch)
        {
            if (this.ShouldFreezeTime(context.Config, Game1.currentLocation, out bool isCave))
                CommonHelper.DrawTextBox(5, isCave ? 100 : 5, Game1.smallFont, context.Text.Get("messages.time-frozen"));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get an option field.</summary>
        /// <param name="context">The cheat context.</param>
        /// <param name="translationKey">The unique portion of its display text translation key.</param>
        /// <param name="frozen">Whether the time is frozen.</param>
        /// <param name="setValue">Set the value.</param>
        private CheatsOptionsCheckbox GetField(CheatContext context, string translationKey, bool frozen, Action<bool> setValue)
        {
            return new CheatsOptionsCheckbox(
                label: context.Text.Get($"time.freeze-{translationKey}"),
                value: frozen,
                setValue: setValue
            );
        }

        /// <summary>Get whether time should be frozen in the given location.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="location">The location to check.</param>
        /// <param name="isCave">Indicates whether the given location is a cave.</param>
        private bool ShouldFreezeTime(ModConfig config, GameLocation location, out bool isCave)
        {
            isCave = location is MineShaft || location is FarmCave;
            return
                config.FreezeTime
                || (config.FreezeTimeCaves && isCave)
                || (config.FreezeTimeInside && location != null && !location.IsOutdoors && !isCave);
        }
    }
}
