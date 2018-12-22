using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StarMoonValley
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
    {
        /* ********** *
         * Properties *
         * ********** */

        private ModData model; // used to manage saved files
        private IModHelper modHelper; // makes helper accessible to all methods in ModEntry
        private readonly Moon moon = new Moon();

        /* ************** *
         * Public Methods *
         * ************** */

        public override void Entry(IModHelper helper)
        {
            modHelper = helper;
            moon.Monitor = Monitor;
            Monitor.Log("Mod loaded!", LogLevel.Trace);
            // tell the moon to do stuff based on alerts received
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            Helper.Events.GameLoop.Saving += OnSaving;
            Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

            // for adding objects to Pierre's shop menu
            Helper.Events.Display.MenuChanged += OnMenuChanged;

            // these are for the lunar calendar overlay
            Helper.Events.Display.Rendering += Events_Rendering;
            Helper.Events.Display.Rendered += Events_Rendered;
        }

        #region replacing xnb's for fish & herbs
        // checks to see if editing can occur
        public bool CanEdit<T>(IAssetInfo asset)
        {
            // change fish spawn
            if (asset.AssetNameEquals("Data/Fish"))
            {
                Monitor.Log("CanEdit() called on Fish.xnb", LogLevel.Trace);
                return true;
            }

            // adds new crops to the game
            if (asset.AssetNameEquals("Data/Crops"))
            {
                Monitor.Log("CanEdit() called on Data/Crops.xnb", LogLevel.Trace);
                return true;
            }
            if (asset.AssetNameEquals("Data/ObjectInformation"))
            {
                Monitor.Log("CanEdit() called on Data/ObjectInformation.xnb", LogLevel.Trace);
                return true;
            }
            if (asset.AssetNameEquals("TileSheets/Crops"))
            {
                Monitor.Log("CanEdit() called on Tilesheets/Crops.xnb", LogLevel.Trace);
                return true;
            }
            if (asset.AssetNameEquals("Maps/SpringObjects"))
            {
                Monitor.Log("CanEdit() called on Maps/SpringObjects.xnb", LogLevel.Trace);
                return true;
            }

            return false;
        }

        // A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            // change fish spawn
            if (asset.AssetNameEquals("Data/Fish"))
            {
                Monitor.Log("Edit() called on Fish.xnb", LogLevel.Trace);
                IDictionary<int, string> fish = asset.AsDictionary<int, string>().Data; // translates the data from IAssetData into a Dictionary named fish
                switch (moon.Phase)
                {
                    /* ++ = 0.69 spawn rate multiplier
                     * +  = 0.57
                     */
                    case 0: // new moon
                        fish[132] = "Bream/35/smooth/12/30/1800 2600/spring summer fall winter/both/684 .35/1/.57/.1/0"; // 0.45 -> 0.57
                        fish[717] = "Crab/trap/.005/684 .45/ocean/1/20"; // 0.1 -> 0.005
                        Monitor.Log("Fish.xnb called during the new moon. 2 fish affected.", LogLevel.Trace);
                        break; 
                    case 1: // waxing moon
                        Monitor.Log("Fish.xnb called during the waxing moon. 0 fish affected.", LogLevel.Trace);
                        break;
                    case 2: // full moon
                        fish[132] = "Bream/35/smooth/12/30/1800 2600/spring summer fall winter/both/684 .35/1/.69/.1/0"; // 0.45 -> 0.69
                        fish[717] = "Crab/trap/.95/684 .45/ocean/1/20"; // 0.1 -> 0.95
                        Monitor.Log("Fish.xnb called during the full moon. 2 fish affected.", LogLevel.Trace);
                        break;
                    case 3: // waning moon
                        Monitor.Log("Fish.xnb called during the waning moon. 0 fish affected.", LogLevel.Trace);
                        break;
                    default: // eldritch moon
                        Monitor.Log($"Fish.xnb called, but phase was {moon.Phase}", LogLevel.Trace);
                        break;
                }
            }

            // adding new crops to the game
            if (asset.AssetNameEquals("Data/Crops"))
            {
                Monitor.Log("Edit() called on Data/Crops.xnb", LogLevel.Trace);
                IDictionary<int, string> crops = asset.AsDictionary<int, string>().Data;

                // add row for each crop, references growing sprites and object sprite
                crops.Add(806, "1 2 2 2 1/spring summer fall/42/805/3/1/false/false/false");
            }

            // adding new objects to the game (crops and seeds)
            if (asset.AssetNameEquals("Data/ObjectInformation"))
            {
                Monitor.Log("Edit() called on Data/ObjectInformation.xnb", LogLevel.Trace);
                IDictionary<int, string> objInfo = asset.AsDictionary<int, string>().Data;

                // add row for each object (keep crops and seeds together, please)
                objInfo.Add(805, "Mugwort/50/10/Basic -75/Mugwort/An aromatic herb./food/0 0 0 0 0 0 0 0 0 0 0/0");
                objInfo.Add(806, "Mugwort Seeds/10/0/Seeds -74/Mugwort Seeds/Mugwort seeds.");
            }

            // loads and replaces crop growth sprites
            if (asset.AssetNameEquals("TileSheets/Crops"))
            {
                Monitor.Log("Edit() called on TileSheets/Crops.xnb", LogLevel.Trace);
                Texture2D oldCrops = asset.AsImage().Data;
                Texture2D newCrops = Helper.Content.Load<Texture2D>("assets/mugwort-crops.png", ContentSource.ModFolder);
                asset.ReplaceWith(newCrops); // swap assets
            }

            // adding icons for new objects
            if (asset.AssetNameEquals("Maps/SpringObjects"))
            {
                Monitor.Log("Edit() called on Maps/SpringObjects.xnb", LogLevel.Trace);
                Texture2D springObj = asset.AsImage().Data;
                Texture2D newObj = Helper.Content.Load<Texture2D>("assets/springobjects.png", ContentSource.ModFolder);
                asset.ReplaceWith(newObj); // swap assets
            }
        } // needs work
        #endregion

        /* *************** *
         * Private Methods *
         * *************** */

        private void OnSaveLoaded(object sender, EventArgs e)
        {
            // read file
            model = Helper.ReadJsonFile<ModData>($"data/{Constants.SaveFolderName}.json");
            if (model != null)
            {
                moon.Cycle = model.Cycle;
                moon.Phase = moon.CalculatePhase(moon.Cycle);
                moon.PhaseName = moon.CalculatePhaseName(moon.Phase);
                moon.FirstCycle = moon.CalculateFirstCycle(moon.Cycle);
                Monitor.Log($"ModData loaded! Current variables are: {moon.Cycle} - {moon.Phase} - {moon.PhaseName} - {moon.HasChanged} - {moon.FirstCycle}", LogLevel.Trace);
            } else
            {
                model = new ModData();
                Monitor.Log("No ModData found. Generating new data.", LogLevel.Trace);

                moon.InitializeCycle();

                // prepares to write back to the file immediately
                model.Cycle = moon.Cycle;
                model.Phase = moon.Phase;
                model.PhaseName = moon.PhaseName;
                model.FirstCycle = moon.FirstCycle;

                Monitor.Log($"ModData generated. Current variables are: {moon.Cycle} - {moon.Phase} - {moon.PhaseName} - {moon.HasChanged} - {moon.FirstCycle}", LogLevel.Trace);

                Helper.WriteJsonFile($"data/{Constants.SaveFolderName}.json", model);
            }

            moon.CalculateCalendar(moon.FirstCycle);
        }

        private void OnTimeChanged(object sender, EventArgs e)
        {
            if (Game1.timeOfDay == 1800)
            {
                moon.IncrementCycle();
                Monitor.Log($"Incremented at 1800: {moon.Cycle} - {moon.Phase} - {moon.PhaseName} - {moon.HasChanged} - {moon.FirstCycle}", LogLevel.Trace);
                modHelper.Content.InvalidateCache("Data/Fish"); // possibly move to IncrementCycle()?
            }
        }

        private void OnSaving(object sender, EventArgs e)
        {
            if (!moon.HasChanged)
            {
                moon.IncrementCycle();
                Monitor.Log($"Forcing cycle change before save, now {moon.Cycle} - {moon.Phase} - {moon.PhaseName} - {moon.HasChanged} - {moon.FirstCycle}", LogLevel.Trace);
            }

            moon.HasChanged = false;

            // write file
            model.Cycle = moon.Cycle;
            model.Phase = moon.Phase;
            model.PhaseName = moon.PhaseName;
            model.FirstCycle = moon.FirstCycle; // possibly encapsulate this in a method too
            Helper.WriteJsonFile($"data/{Constants.SaveFolderName}.json", model);
        }

        private void OnReturnedToTitle(object sender, EventArgs e)
        {
            moon.Cycle = 0;
            moon.Phase = 0;
            moon.PhaseName = "null";
            moon.HasChanged = false;
            moon.FirstCycle = 0;
            Monitor.Log($"Gone to Title Screen. Current variables are: {moon.Cycle} - {moon.Phase} - {moon.PhaseName} - {moon.HasChanged} - {moon.FirstCycle}", LogLevel.Trace);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu)
            {
                IClickableMenu menu = e.NewMenu;
                IList<Item> forSale = Helper.Reflection.GetField<List<Item>>(menu, "forSale").GetValue();
                IDictionary<Item, int[]> itemPriceAndStock = Helper.Reflection.GetField<Dictionary<Item, int[]>>(menu, "itemPriceAndStock").GetValue(); // this gathers the current menu @ Pierre's

                // add items here
                StardewValley.Object item = new StardewValley.Object(Vector2.Zero, 806, int.MaxValue); // calls ObjectInformation.xnb on mugwort seeds
                int price = 50;
                forSale.Add(item);
                itemPriceAndStock.Add(item, new int[] { price, int.MaxValue }); // price, quantity available
            }
        }

        #region Lunar Calendar events
        private void Events_Rendering(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is Billboard)
            {
                #region accessing Billboard
                Billboard menu = (Billboard)Game1.activeClickableMenu;
                FieldInfo calendarField = menu.GetType().GetField("calendarDays", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (calendarField == null)
                {
                    this.Monitor.Log("Could not find field 'calendarDays' in Billboard!", LogLevel.Error);
                    return;
                }

                List<ClickableTextureComponent> calendarDays = (List<ClickableTextureComponent>)calendarField.GetValue(menu);
                IReflectedField<string> privateField = this.Helper.Reflection.GetField<string>(menu, "hoverText");
                string hoverText = privateField.GetValue();
                #endregion

                if (calendarDays != null && !(hoverText.Contains("Moon") || hoverText.Contains("moon")))
                {
                    for (int day = 1; day <= 28; day++)
                    {
                        ClickableTextureComponent component = calendarDays[day - 1]; // 0 - 27

                        if (component.bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
                        {
                            if (moon.LunarCalendar[day] >= 0 && moon.LunarCalendar[day] <= 3)
                            {
                                if (hoverText.Length > 0)
                                    hoverText += "\n";

                                hoverText += $"{moon.CalculatePhaseName(moon.LunarCalendar[day])} moon";
                            }
                            else
                            {
                                hoverText += "";
                                break;
                            }
                        } 
                    }

                    privateField.SetValue(hoverText);
                } 
            } 
        }

        private void Events_Rendered(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is Billboard)
            {
                #region accessing Billboard and sprites
                Billboard menu = (Billboard)Game1.activeClickableMenu;
                FieldInfo calendarField = menu.GetType().GetField("calendarDays", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (calendarField == null)
                {
                    this.Monitor.Log("Could not find field 'calendarDays' in Billboard!", LogLevel.Error);
                    return;
                }
                List<ClickableTextureComponent> calendarDays = (List<ClickableTextureComponent>)calendarField.GetValue(menu);
                if (calendarDays == null) return;
                string hoverText = this.Helper.Reflection.GetField<string>(menu, "hoverText").GetValue();
                SpriteBatch b = Game1.spriteBatch;
                #endregion
                for (int day = 1; day <= 28; day++)
                {
                    ClickableTextureComponent component = calendarDays[day - 1];

                    if (moon.LunarCalendar[day] >= 0 && moon.LunarCalendar[day] <= 3)
                    {
                        // insert loop that changes id based on phase
                        const int id = 339; // id for moon, 339 = full
                        Rectangle source = GameLocation.getSourceRectForObject(id);
                        Vector2 dest = new Vector2(component.bounds.X, component.bounds.Y + 10f * Game1.pixelZoom);
                        b.Draw(Game1.objectSpriteSheet, dest, new Rectangle?(source), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom / 2f, SpriteEffects.None, 1f);
                    }
                }

                IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null);
            }
        }
        #endregion
    }
}