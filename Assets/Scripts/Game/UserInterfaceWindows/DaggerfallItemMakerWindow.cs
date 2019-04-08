// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2019 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Hazelnut, Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
//
// Notes:
//

using System;
using UnityEngine;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.Items;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class DaggerfallItemMakerWindow : DaggerfallPopupWindow
    {
        #region UI Rects

        Rect weaponsAndArmorRect = new Rect(175, 6, 81, 9);
        Rect magicItemsRect = new Rect(175, 15, 81, 9);
        Rect clothingAndMiscRect = new Rect(175, 24, 81, 9);
        Rect ingredientsRect = new Rect(175, 33, 81, 9);

        Rect powersButtonRect = new Rect(8, 183, 77, 10);
        Rect sideeffectsButtonRect = new Rect(106, 183, 77, 10);
        Rect exitButtonRect = new Rect(202, 176, 39, 22);

        Rect enchantButtonRect = new Rect(200, 115, 43, 15);
        Rect selectedItemRect = new Rect(196, 68, 50, 37);

        Rect itemListScrollerRect = new Rect(253, 49, 60, 148);
        Rect itemListPanelRect = new Rect(10, 0, 50, 148);
        readonly Rect[] itemButtonRects = new Rect[]
        {
            new Rect(0, 0, 50, 37),
            new Rect(0, 37, 50, 37),
            new Rect(0, 74, 50, 37),
            new Rect(0, 111, 50, 37)
        };

        //Vector2 firstPowerLabelPos = new Vector2(10, 60);
        //Vector2 firstSideEffectLabelPos = new Vector2(108, 60);
        //const int labelsPerSide = 8;
        //const int secondaryLabelXIndent = 10;
        //const int secondaryLabelYIncrement = 5;
        //const int nextLabelIncrement = 10;

        #endregion

        #region UI Controls

        TextLabel nameLabel = new TextLabel();
        TextLabel goldLabel = new TextLabel();
        TextLabel costLabel = new TextLabel();
        TextLabel enchantLabel = new TextLabel();

        //TextLabel[] powersListLabels = new TextLabel[labelsPerSide * 2];
        //TextLabel[] sideEffectsListLabels = new TextLabel[labelsPerSide * 2];

        Button weaponsAndArmorButton;
        Button magicItemsButton;
        Button clothingAndMiscButton;
        Button ingredientsButton;

        Button powersButton;
        Button sideeffectsButton;
        Button exitButton;

        Button enchantButton;
        Button selectedItemButton;
        Panel selectedItemPanel;

        ItemListScroller itemsListScroller;

        List<IEntityEffect> enchantmentTemplates;
        EnchantmentSettings[] enchantmentSettings;

        EnchantmentList powersList;
        EnchantmentList sideEffectsList;

        bool selectingPowers;
        DaggerfallListPickerWindow enchantmentPrimaryPicker;
        DaggerfallListPickerWindow enchantmentSecondaryPicker;

        #endregion

        #region UI Textures

        Texture2D baseTexture;
        Texture2D goldTexture;

        Texture2D weaponsAndArmorNotSelected;
        Texture2D magicItemsNotSelected;
        Texture2D clothingAndMiscNotSelected;
        Texture2D ingredientsNotSelected;
        Texture2D weaponsAndArmorSelected;
        Texture2D magicItemsSelected;
        Texture2D clothingAndMiscSelected;
        Texture2D ingredientsSelected;

        #endregion

        #region Fields

        const MagicCraftingStations thisMagicStation = MagicCraftingStations.ItemMaker;

        const string baseTextureName = "ITEM00I0.IMG";
        const string goldTextureName = "ITEM01I0.IMG";
        const int alternateAlphaIndex = 12;
        const int maxEnchantSlots = 10;

        PlayerEntity playerEntity;
        DaggerfallInventoryWindow.TabPages selectedTabPage = DaggerfallInventoryWindow.TabPages.WeaponsAndArmor;
        List<DaggerfallUnityItem> itemsFiltered = new List<DaggerfallUnityItem>();
        DaggerfallUnityItem selectedItem;

        List<EnchantmentSettings> itemPowers = new List<EnchantmentSettings>();
        List<EnchantmentSettings> itemSideEffects = new List<EnchantmentSettings>();

        //int powersScrollPos = 0;
        //int sideEffectsScrollPos = 0;

        #endregion

        #region Properties

        PlayerEntity PlayerEntity {
            get { return (playerEntity != null) ? playerEntity : playerEntity = GameManager.Instance.PlayerEntity; }
        }

        #endregion

        #region Constructors

        public DaggerfallItemMakerWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
            : base(uiManager, previous)
        {
        }

        #endregion

        #region Setup Methods

        protected override void Setup()
        {
            // Load textures
            LoadTextures();

            // Always dim background
            ParentPanel.BackgroundColor = ScreenDimColor;

            // Setup native panel background
            NativePanel.BackgroundColor = new Color(0, 0, 0, 0.60f);
            NativePanel.BackgroundTexture = baseTexture;

            // Setup UI
            SetupLabels();
            SetupButtons();
            SetupListBoxes();
            SetupPickers();
            SetupItemListScrollers();

            SelectTabPage(selectedTabPage);
        }

        public override void OnPush()
        {
            if (!IsSetup)
                return;

            Refresh();
        }

        public override void OnPop()
        {
        }

        void Refresh()
        {
            // Update labels
            nameLabel.Text = (selectedItem != null) ? selectedItem.shortName : "";
            goldLabel.Text = PlayerEntity.GetGoldAmount().ToString();
            costLabel.Text = (selectedItem != null) ? "8132" : "";
            enchantLabel.Text = (selectedItem != null) ? "0 / 60" : "";

            // Add appropriate items to filtered list
            itemsFiltered.Clear();
            ItemCollection playerItems = PlayerEntity.Items;
            for (int i = 0; i < playerItems.Count; i++)
                AddFilteredItem(playerItems.GetItem(i));

            itemsListScroller.Items = itemsFiltered;

            if (selectedItem != null) {
                ImageData image = DaggerfallUnity.Instance.ItemHelper.GetInventoryImage(selectedItem);
                selectedItemPanel.BackgroundTexture = image.texture;
                selectedItemPanel.Size = new Vector2(image.texture.width, image.texture.height);
            } else {
                selectedItemPanel.BackgroundTexture = null;
            }
        }

        #endregion

        #region Private Methods

        void LoadTextures()
        {
            baseTexture = ImageReader.GetTexture(baseTextureName, 0, 0, true, alternateAlphaIndex);
            goldTexture = ImageReader.GetTexture(goldTextureName);
            DFSize baseSize = new DFSize(320, 200);
            DFSize goldSize = new DFSize(81, 36);

            // Cut out tab page not selected button textures
            weaponsAndArmorNotSelected = ImageReader.GetSubTexture(baseTexture, weaponsAndArmorRect, baseSize);
            magicItemsNotSelected = ImageReader.GetSubTexture(baseTexture, magicItemsRect, baseSize);
            clothingAndMiscNotSelected = ImageReader.GetSubTexture(baseTexture, clothingAndMiscRect, baseSize);
            ingredientsNotSelected = ImageReader.GetSubTexture(baseTexture, ingredientsRect, baseSize);

            // Cut out tab page selected button textures
            weaponsAndArmorSelected = ImageReader.GetSubTexture(goldTexture, new Rect(0, 0, 81, 9), goldSize);
            magicItemsSelected = ImageReader.GetSubTexture(goldTexture, new Rect(0, 9, 81, 9), goldSize);
            clothingAndMiscSelected = ImageReader.GetSubTexture(goldTexture, new Rect(0, 18, 81, 9), goldSize);
            ingredientsSelected = ImageReader.GetSubTexture(goldTexture, new Rect(0, 27, 81, 9), goldSize);
        }

        void SetupLabels()
        {
            nameLabel = DaggerfallUI.AddDefaultShadowedTextLabel(new Vector2(52, 3), NativePanel);
            goldLabel = DaggerfallUI.AddDefaultShadowedTextLabel(new Vector2(71, 15), NativePanel);
            costLabel = DaggerfallUI.AddDefaultShadowedTextLabel(new Vector2(64, 27), NativePanel);
            enchantLabel = DaggerfallUI.AddDefaultShadowedTextLabel(new Vector2(98, 39), NativePanel);

            //Vector2 powerLabelPos = firstPowerLabelPos;
            //Vector2 sideEffectLabelPos = firstSideEffectLabelPos;
            //for(int i = 0; i < labelsPerSide * 2; i += 2)
            //{
            //    powersListLabels[i] = DaggerfallUI.AddTextLabel(DaggerfallUI.SmallFont, powerLabelPos, "Cast when used:", NativePanel);
            //    // TODO: Only display secondary when present
            //    powerLabelPos.x += secondaryLabelXIndent;
            //    powerLabelPos.y += secondaryLabelYIncrement;
            //    powersListLabels[i + 1] = DaggerfallUI.AddTextLabel(DaggerfallUI.SmallFont, powerLabelPos, "Levitate", NativePanel);
            //    powerLabelPos.x = firstPowerLabelPos.x;
            //    powerLabelPos.y += nextLabelIncrement;

            //    sideEffectsListLabels[i] = DaggerfallUI.AddTextLabel(DaggerfallUI.SmallFont, sideEffectLabelPos, "Soul bound", NativePanel);
            //    // TODO: Only display secondary when present
            //    sideEffectLabelPos.x += secondaryLabelXIndent;
            //    sideEffectLabelPos.y += secondaryLabelYIncrement;
            //    sideEffectsListLabels[i + 1] = DaggerfallUI.AddTextLabel(DaggerfallUI.SmallFont, sideEffectLabelPos, "Dragonling", NativePanel);
            //    sideEffectLabelPos.x = firstSideEffectLabelPos.x;
            //    sideEffectLabelPos.y += nextLabelIncrement;
            //}
        }

        void SetupButtons()
        {
            // Tab page buttons
            weaponsAndArmorButton = DaggerfallUI.AddButton(weaponsAndArmorRect, NativePanel);
            weaponsAndArmorButton.OnMouseClick += WeaponsAndArmor_OnMouseClick;
            magicItemsButton = DaggerfallUI.AddButton(magicItemsRect, NativePanel);
            magicItemsButton.OnMouseClick += MagicItems_OnMouseClick;
            clothingAndMiscButton = DaggerfallUI.AddButton(clothingAndMiscRect, NativePanel);
            clothingAndMiscButton.OnMouseClick += ClothingAndMisc_OnMouseClick;
            ingredientsButton = DaggerfallUI.AddButton(ingredientsRect, NativePanel);
            ingredientsButton.OnMouseClick += Ingredients_OnMouseClick;

            // Add powers & side-effects buttons
            Button powersButton = DaggerfallUI.AddButton(powersButtonRect, NativePanel);
            powersButton.OnMouseClick += PowersButton_OnMouseClick;
            Button sideeffectsButton = DaggerfallUI.AddButton(sideeffectsButtonRect, NativePanel);
            sideeffectsButton.OnMouseClick += SideeffectsButton_OnMouseClick;

            // Exit button
            exitButton = DaggerfallUI.AddButton(exitButtonRect, NativePanel);
            exitButton.OnMouseClick += ExitButton_OnMouseClick;

            Button enchantButton = DaggerfallUI.AddButton(enchantButtonRect, NativePanel);
            enchantButton.OnMouseClick += EnchantButton_OnMouseClick;

            // Selected item button
            selectedItemButton = DaggerfallUI.AddButton(selectedItemRect, NativePanel);
            selectedItemButton.SetMargins(Margins.All, 2);
            selectedItemButton.OnMouseClick += SelectedItemButton_OnMouseClick;
            // Selected item icon image panel
            selectedItemPanel = DaggerfallUI.AddPanel(selectedItemButton, AutoSizeModes.ScaleToFit);
            selectedItemPanel.HorizontalAlignment = HorizontalAlignment.Center;
            selectedItemPanel.VerticalAlignment = VerticalAlignment.Middle;
            selectedItemPanel.MaxAutoScale = 1f;
        }

        void SetupListBoxes()
        {
            powersList = new EnchantmentList();
            powersList.Position = new Vector2(10, 58);
            powersList.Size = new Vector2(75, 120);

            // Add test items
            powersList.AddItem("Cast when used:", "Levitate");
            powersList.AddItem("Cast when used:", "Light");
            powersList.AddItem("Cast when used:", "Invisibility");
            powersList.AddItem("Cast when used:", "Wizard's Fire");
            powersList.AddItem("Cast when used:", "Shock");
            powersList.AddItem("Increased weight allowance", "25% Additional");
            powersList.AddItem("Cast when used:", "Free Action");
            powersList.AddItem("Cast when used:", "Open");
            powersList.AddItem("Cast when used:", "Levitate");
            powersList.AddItem("Cast when used:", "Levitate");
            //for (int i = 0; i < 10; i++)
            //{
            //    powersList.AddItem(new EnchantmentSettings());
            //}

            NativePanel.Components.Add(powersList);

            //powersListBox = new ListBox();
            //powersListBox.Position = new Vector2(10, 60);
            //powersListBox.Size = new Vector2(75, 118);
            //powersListBox.VerticalScrollMode = ListBox.VerticalScrollModes.EntryWise;
            //powersListBox.HorizontalScrollMode = ListBox.HorizontalScrollModes.CharWise;
            //powersListBox.Font = DaggerfallUI.SmallFont;
            //powersListBox.BackgroundColor = new Color32(0, 0, 0, 200);
            //powersListBox.ShadowPosition = Vector2.zero;
            //powersListBox.EnabledHorizontalScroll = true;
            //powersListBox.WrapTextItems = true;
            //powersListBox.WrapWords = true;

            //// Add test items
            //for (int i = 0; i < 6; i++)
            //{
            //    powersListBox.AddItem("Cast when used: Levitate");
            //}

            //NativePanel.Components.Add(powersListBox);
        }

        void SetupPickers()
        {
            // Use a picker for power/side-effect primary selection
            enchantmentPrimaryPicker = new DaggerfallListPickerWindow(uiManager, this, DaggerfallUI.SmallFont, 12);
            enchantmentPrimaryPicker.ListBox.OnUseSelectedItem += EnchantmentPrimaryPicker_OnUseSelectedItem;

            // Use another picker for power/side-effect secondary selection
            enchantmentSecondaryPicker = new DaggerfallListPickerWindow(uiManager, this, DaggerfallUI.SmallFont, 12);
            enchantmentSecondaryPicker.ListBox.OnUseSelectedItem += EnchantmentSecondaryPicker_OnUseSelectedItem;
        }

        void SetupItemListScrollers()
        {
            itemsListScroller = new ItemListScroller(4, 1, itemListPanelRect, itemButtonRects, new TextLabel(), defaultToolTip)
            {
                Position = new Vector2(itemListScrollerRect.x, itemListScrollerRect.y),
                Size = new Vector2(itemListScrollerRect.width, itemListScrollerRect.height),
            };
            NativePanel.Components.Add(itemsListScroller);
            itemsListScroller.OnItemClick += ItemListScroller_OnItemClick;
        }

        void SelectTabPage(DaggerfallInventoryWindow.TabPages tabPage)
        {
            // Select new tab page
            selectedTabPage = tabPage;

            // Set all buttons to appropriate state
            weaponsAndArmorButton.BackgroundTexture = (tabPage == DaggerfallInventoryWindow.TabPages.WeaponsAndArmor) ? weaponsAndArmorSelected : weaponsAndArmorNotSelected;
            magicItemsButton.BackgroundTexture = (tabPage == DaggerfallInventoryWindow.TabPages.MagicItems) ? magicItemsSelected : magicItemsNotSelected;
            clothingAndMiscButton.BackgroundTexture = (tabPage == DaggerfallInventoryWindow.TabPages.ClothingAndMisc) ? clothingAndMiscSelected : clothingAndMiscNotSelected;
            ingredientsButton.BackgroundTexture = (tabPage == DaggerfallInventoryWindow.TabPages.Ingredients) ? ingredientsSelected : ingredientsNotSelected;

            // Update items
            Refresh();
        }

        // Add item to filtered items based on selected tab
        void AddFilteredItem(DaggerfallUnityItem item)
        {
            if (item == selectedItem)
                return;

            bool isWeaponOrArmor = (item.ItemGroup == ItemGroups.Weapons || item.ItemGroup == ItemGroups.Armor);

            if (selectedTabPage == DaggerfallInventoryWindow.TabPages.WeaponsAndArmor)
            {   // Weapons and armor
                if (isWeaponOrArmor && !item.IsEnchanted)
                    itemsFiltered.Add(item);
            }
            else if (selectedTabPage == DaggerfallInventoryWindow.TabPages.MagicItems)
            {   // Enchanted items
                // TODO: seems completely pointless, is there any use case for this?
                if (item.IsEnchanted)
                    itemsFiltered.Add(item);
            }
            else if (selectedTabPage == DaggerfallInventoryWindow.TabPages.Ingredients)
            {   // Ingredients
                if (item.IsIngredient && !item.IsEnchanted)
                    itemsFiltered.Add(item);
            }
            else if (selectedTabPage == DaggerfallInventoryWindow.TabPages.ClothingAndMisc)
            {   // Everything else
                // TODO, filter only enchantable items...
                if (!isWeaponOrArmor && !item.IsEnchanted && !item.IsIngredient && !item.IsOfTemplate((int) MiscItems.Spellbook))
                    itemsFiltered.Add(item);
            }
        }

        #endregion

        #region List Management

        void AddEnchantmentSettings(EnchantmentSettings settings)
        {
            if (selectingPowers)
                itemPowers.Add(settings);
            else
                itemSideEffects.Add(settings);
        }

        #endregion

        #region Event Handlers

        private void ItemListScroller_OnItemClick(DaggerfallUnityItem item)
        {
            selectedItem = item;
            Refresh();
        }

        private void SelectedItemButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            selectedItem = null;
            Refresh();
        }

        private void PowersButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // NOTE: Just working on populating lists for now

            // TODO: Must have an item selected to be enchanted

            // TODO: Check for max enchantments and display "You cannot enchant this item with any more powers."

            enchantmentPrimaryPicker.ListBox.ClearItems();
            selectingPowers = true;

            // Populate item effects list from suitable templates
            // TODO: Rework this so only effects that return enchantment settings are added
            enchantmentTemplates = GameManager.Instance.EntityEffectBroker.GetEnchantmentEffectTemplates();
            foreach(IEntityEffect effect in enchantmentTemplates)
            {
                enchantmentPrimaryPicker.ListBox.AddItem(effect.Properties.GroupName);
            }

            // Show effect group picker
            uiManager.PushWindow(enchantmentPrimaryPicker);
        }

        private void SideeffectsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // TODO: Check for max enchantments and display "No further side-effects may be enchanted in this item."

            enchantmentPrimaryPicker.ListBox.ClearItems();
            selectingPowers = false;

            Debug.Log("Add side-effects");
        }

        private void EnchantButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            Debug.Log("Enchant item!");
        }

        private void WeaponsAndArmor_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTabPage(DaggerfallInventoryWindow.TabPages.WeaponsAndArmor);
        }

        private void MagicItems_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTabPage(DaggerfallInventoryWindow.TabPages.MagicItems);
        }

        private void ClothingAndMisc_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTabPage(DaggerfallInventoryWindow.TabPages.ClothingAndMisc);
        }

        private void Ingredients_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SelectTabPage(DaggerfallInventoryWindow.TabPages.Ingredients);
        }

        private void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
        }

        #endregion

        #region Effect Picker Events

        private void EnchantmentPrimaryPicker_OnUseSelectedItem()
    {
            // Clear existing
            enchantmentSecondaryPicker.ListBox.ClearItems();

            // Get enchantment settings from selected enchantment templates
            IEntityEffect template = enchantmentTemplates[enchantmentPrimaryPicker.ListBox.SelectedIndex];
            enchantmentSettings = template.GetEnchantmentSettings();
            if (enchantmentSettings == null | enchantmentSettings.Length == 0)
            {
                Debug.LogErrorFormat("Enchantment '{0}' returned no settings from GetEnchantmentSettings()", template.Key);
                enchantmentPrimaryPicker.CloseWindow();
                return;
            }

            // Just add single setting if there are no secondary types
            if (enchantmentSettings.Length == 1)
            {
                AddEnchantmentSettings(enchantmentSettings[0]);
                enchantmentPrimaryPicker.CloseWindow();
                return;
            }

            // User must choose from available settings
            foreach (EnchantmentSettings enchantment in enchantmentSettings)
            {
                enchantmentSecondaryPicker.ListBox.AddItem(enchantment.SecondaryDisplayName);
            }
            enchantmentSecondaryPicker.ListBox.SelectedIndex = 0;

            // Show enchantment secondary picker
            uiManager.PushWindow(enchantmentSecondaryPicker);
        }

        private void EnchantmentSecondaryPicker_OnUseSelectedItem()
        {
            // TODO: Check for overflow from automatic enchantments and display "no room in item..."

            // Add selected enchantment settings to powers/side-effects
            AddEnchantmentSettings(enchantmentSettings[enchantmentSecondaryPicker.ListBox.SelectedIndex]);

            // Close effect pickers
            enchantmentPrimaryPicker.CloseWindow();
            enchantmentSecondaryPicker.CloseWindow();
        }

        #endregion
    }
}