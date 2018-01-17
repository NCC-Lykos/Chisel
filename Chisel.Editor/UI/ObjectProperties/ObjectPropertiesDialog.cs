using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chisel.Common.Mediator;
using Chisel.DataStructures.GameData;
using Chisel.DataStructures.MapObjects;
using Chisel.Editor.Actions;
using Chisel.Editor.Actions.MapObjects.Entities;
using Chisel.Editor.Actions.Visgroups;
using Chisel.Editor.UI.ObjectProperties.SmartEdit;
using Chisel.QuickForms;
using Chisel.Settings.Models;

namespace Chisel.Editor.UI.ObjectProperties
{
    public partial class ObjectPropertiesDialog : Form, IMediatorListener
    {
        private static int _numOpen = 0;
        public static bool IsShowing { get { return _numOpen > 0; } }

        private List<TableValue> _values;

        private readonly Dictionary<VariableType, SmartEditControl> _smartEditControls;
        private readonly SmartEditControl _dumbEditControl;
        public List<MapObject> Objects { get; set; }
        private bool _changingClass;
        private string _prevClass;
        private Documents.Document Document { get; set; }
        public bool FollowSelection { get; set; }

        public bool AllowClassChange
        {
            set
            {
                CancelClassChangeButton.Enabled
                    = ConfirmClassChangeButton.Enabled
                      = Class.Enabled
                        = value; // It's like art or something!
            }
        }

        private bool _populating;

        private bool GBSPMultiple = false;
        private bool GBSPMultipleCustom = false;

        private bool DiffGBSPSolid;
        private bool DiffGBSPWindow;
        private bool DiffGBSPClip;
        private bool DiffGBSPHint;
        private bool DiffGBSPEmpty;
        private bool DiffGBSPWavy;
        private bool DiffGBSPDetail;
        private bool DiffGBSPArea;
        private bool DiffGBSPFlocking;
        private bool DiffGBSPSheet;
        
        public ObjectPropertiesDialog(Documents.Document document)
        {
            Document = document;
            InitializeComponent();
            Objects = new List<MapObject>();
            _smartEditControls = new Dictionary<VariableType, SmartEditControl>();

            _dumbEditControl = new DumbEditControl { Document = Document };
            _dumbEditControl.ValueChanged += PropertyValueChanged;
            _dumbEditControl.NameChanged += PropertyNameChanged;

            RegisterSmartEditControls();

            FollowSelection = true;
        }

        private void RegisterSmartEditControls()
        {
            var types = typeof(SmartEditControl).Assembly.GetTypes()
                .Where(x => typeof(SmartEditControl).IsAssignableFrom(x))
                .Where(x => x != typeof(SmartEditControl))
                .Where(x => x.GetCustomAttributes(typeof(SmartEditAttribute), false).Any());
            foreach (var type in types)
            {
                var attrs = type.GetCustomAttributes(typeof(SmartEditAttribute), false);
                foreach (SmartEditAttribute attr in attrs)
                {
                    var inst = (SmartEditControl)Activator.CreateInstance(type);

                    inst.Document = Document;
                    inst.ValueChanged += PropertyValueChanged;
                    inst.Dock = DockStyle.Fill;

                    _smartEditControls.Add(attr.VariableType, inst);
                }
            }
        }

        private void Apply()
        {
            string actionText = null;
            var ac = new ActionCollection();


            // Check if it's actually editing keyvalues
            if (_values != null)
            {
                var editAction = GetEditEntityDataAction();
                if (editAction != null)
                {
                    // The entity change is more important to show
                    actionText = "Edit entity data";
                    ac.Add(editAction);
                }
            }

            var visgroupAction = GetUpdateVisgroupsAction();
            if (visgroupAction != null)
            {
                // Visgroup change shows if entity data not changed
                if (actionText == null) actionText = "Edit object visgroups";
                ac.Add(visgroupAction);
            }

            if (!ac.IsEmpty())
            {
                // Run if either action shows changes
                Document.PerformAction(actionText, ac);
            }

            
            Dictionary<string, UInt32> fCustom = Objects[0].Parent.MetaData.Get<Dictionary<string, UInt32>>("CustomBrushFlags");
            var keys = fCustom.Keys.ToArray();
            GBSPMultipleCustom = false;
            for (int x = 0; x < Objects.Count; x++)
            {
                if (CustomFlags.GetItemCheckState(x) == CheckState.Indeterminate) GBSPMultipleCustom = true;
            }
            
            if (Objects.All(x => x is Solid) && !GBSPMultiple && !GBSPMultipleCustom)
            {
                UInt32 s = 0;

                    if (chkSolid.Checked)    s |= (UInt32)SolidFlags.solid;
                    if (chkWindow.Checked)   s |= (UInt32)SolidFlags.window;
                    if (chkClip.Checked)     s |= (UInt32)SolidFlags.clip;
                    if (chkHint.Checked)     s |= (UInt32)SolidFlags.hint;
                    if (chkEmpty.Checked)    s |= (UInt32)SolidFlags.empty;

                    if (chkWavy.Checked)     s |= (UInt32)SolidFlags.wavy;
                    if (chkDetail.Checked)   s |= (UInt32)SolidFlags.detail;
                    if (chkArea.Checked)     s |= (UInt32)SolidFlags.area;
                    if (chkFlocking.Checked) s |= (UInt32)SolidFlags.flocking;
                    if (chkSheet.Checked)    s |= (UInt32)SolidFlags.sheet;

                    //Custom Flags
                    for (int x = 0; x < Objects.Count; x++)
                    {
                        for (int i = 0; i < fCustom.Count; i++)
                        {
                            if (CustomFlags.GetItemCheckState(i) == CheckState.Checked) s |= fCustom[keys[i]];
                        }
                        ((Solid)Objects[x]).Flags = s;
                        ((Solid)Objects[x]).SetHighlights();
                    }
            }
            else
            {
                MessageBox.Show("Indeterminate checkboxes found, No changes saved.", "Indeterminate",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            Class.BackColor = Color.White;
        }

        private EditEntityData GetEditEntityDataAction()
        {
            var ents = Objects.Where(x => x is Entity || x is World).ToList();
            if (!ents.Any()) return null;
            var action = new EditEntityData();

            foreach (var entity in ents)
            {
                var entityData = entity.GetEntityData().Clone();
                var changed = false;
                // Updated class
                if (Class.BackColor == Color.LightGreen)
                {
                    entityData.Name = Class.Text;
                    changed = true;
                }

                // Remove nonexistant properties
                var nonExistant = entityData.Properties.Where(x => _values.All(y => y.OriginalKey != x.Key));
                if (nonExistant.Any())
                {
                    changed = true;
                    entityData.Properties.RemoveAll(x => _values.All(y => y.OriginalKey != x.Key));
                }

                // Set updated/new properties
                foreach (var ent in _values.Where(x => x.IsModified || (x.IsAdded && !x.IsRemoved)))
                {
                    entityData.SetPropertyValue(ent.OriginalKey, ent.Value);
                    if (!String.IsNullOrWhiteSpace(ent.NewKey) && ent.NewKey != ent.OriginalKey)
                    {
                        var prop = entityData.Properties.FirstOrDefault(x => String.Equals(x.Key, ent.OriginalKey, StringComparison.InvariantCultureIgnoreCase));
                        if (prop != null && !entityData.Properties.Any(x => String.Equals(x.Key, ent.NewKey, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            prop.Key = ent.NewKey;
                        }
                    }
                    changed = true;
                }

                foreach (var ent in _values.Where(x => x.IsRemoved && !x.IsAdded))
                {
                    entityData.Properties.RemoveAll(x => x.Key == ent.OriginalKey);
                    changed = true;
                }

                // Set flags
                var flags = Enumerable.Range(0, FlagsTable.Items.Count).Select(x => FlagsTable.GetItemCheckState(x)).ToList();
                var entClass = Document.GameData.Classes.FirstOrDefault(x => x.Name == entityData.Name);
                var spawnFlags = entClass == null
                                     ? null
                                     : entClass.Properties.FirstOrDefault(x => x.Name == "spawnflags");
                var opts = spawnFlags == null ? null : spawnFlags.Options.OrderBy(x => int.Parse(x.Key)).ToList();
                if (opts != null && flags.Count == opts.Count)
                {
                    var beforeFlags = entityData.Flags;
                    for (var i = 0; i < flags.Count; i++)
                    {
                        var val = int.Parse(opts[i].Key);
                        if (flags[i] == CheckState.Unchecked) entityData.Flags &= ~val; // Switch the flag off if unchecked
                        else if (flags[i] == CheckState.Checked) entityData.Flags |= val; // Switch it on if checked
                        // No change if indeterminate
                    }
                    if (entityData.Flags != beforeFlags) changed = true;
                }

                if (changed) action.AddEntity(entity, entityData);
            }

            return action.IsEmpty() ? null : action;
        }

        private IAction GetUpdateVisgroupsAction()
        {
            var states = VisgroupPanel.GetAllCheckStates();
            var add = states.Where(x => x.Value == CheckState.Checked).Select(x => x.Key).ToList();
            var rem = states.Where(x => x.Value == CheckState.Unchecked).Select(x => x.Key).ToList();
            // If all the objects are in the add groups and none are in the remove groups, nothing needs to be changed
            if (Objects.All(x => add.All(y => x.IsInVisgroup(y, false)) && !rem.Any(y => x.IsInVisgroup(y, false)))) return null;
            return new EditObjectVisgroups(Objects, add, rem);
        }

        public void Notify(string message, object data)
        {
            if (message == EditorMediator.SelectionChanged.ToString()
                || message == EditorMediator.SelectionTypeChanged.ToString())
            {
                UpdateObjects();
            }

            if (message == EditorMediator.EntityDataChanged.ToString())
            {
                RefreshData();
            }

            if (message == EditorMediator.VisgroupsChanged.ToString())
            {
                UpdateVisgroups(true);
            }
        }

        public void SetObjects(IEnumerable<MapObject> objects)
        {
            Objects.Clear();
            Objects.AddRange(objects);
            RefreshData();
        }

        private void UpdateObjects()
        {
            if (!FollowSelection)
            {
                UpdateKeyValues();
                UpdateVisgroups(false);
                return;
            }
            Objects.Clear();
            if (!Document.Selection.InFaceSelection)
            {
                Objects.AddRange(Document.Selection.GetSelectedParents());
            }
            RefreshData();
        }

        private void EditVisgroupsClicked(object sender, EventArgs e)
        {
            Mediator.Publish(EditorMediator.VisgroupShowEditor);
        }

        private void UpdateVisgroups(bool retainCheckStates)
        {
            _populating = true;

            var visgroups = Document.Map.Visgroups.Select(x => x.Clone()).ToList();

            Action<Visgroup> setVisible = null;
            setVisible = x =>
                             {
                                 x.Visible = false;
                                 x.Children.ForEach(y => setVisible(y));
                             };
            visgroups.ForEach(x => setVisible(x));

            Dictionary<int, CheckState> states;

            if (retainCheckStates)
            {
                states = VisgroupPanel.GetAllCheckStates();
            }
            else
            {
                states = Objects.SelectMany(x => x.Visgroups)
                    .GroupBy(x => x)
                    .Select(x => new { ID = x.Key, Count = x.Count() })
                    .Where(g => g.Count > 0)
                    .ToDictionary(g => g.ID, g => g.Count == Objects.Count
                                                      ? CheckState.Checked
                                                      : CheckState.Indeterminate);
            }

            VisgroupPanel.Update(visgroups);

            foreach (var kv in states)
            {
                VisgroupPanel.SetCheckState(kv.Key, kv.Value);
            }

            VisgroupPanel.ExpandAllNodes();

            _populating = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            _numOpen += 1;
            UpdateObjects();

            Mediator.Subscribe(EditorMediator.SelectionChanged, this);
            Mediator.Subscribe(EditorMediator.SelectionTypeChanged, this);

            Mediator.Subscribe(EditorMediator.EntityDataChanged, this);
            Mediator.Subscribe(EditorMediator.VisgroupsChanged, this);
        }

        protected override void OnClosed(EventArgs e)
        {
            _numOpen -= 1;
            Mediator.UnsubscribeAll(this);
            base.OnClosed(e);
        }

        private void RefreshSolid()
        {
            //TODO(SVK): Clean up
            if (!Objects.All(x => x is Solid)) return;
            
            GBSPMultiple = GBSPMultipleCustom = false;
            

            Dictionary<string, UInt32> fCustom = Objects[0].Parent.MetaData.Get<Dictionary<string, UInt32>>("CustomBrushFlags");
            var keys = fCustom.Keys.ToArray();
            if (!Tabs.TabPages.Contains(SolidTab))
            {
                Tabs.TabPages.Insert(0, SolidTab);
                Tabs.SelectedIndex = 0;
                for (int x = 0; x < fCustom.Count; x++) CustomFlags.Items.Add(keys[x], CheckState.Unchecked);
            }

            _populating = true;
            UInt32 f;
            
            for (int x = 0; x < Objects.Count; x++)
            {
                f = ((Solid)Objects[x]).Flags;
                if(x == 0)
                {
                    //((s & (UInt32)SolidFlags.clip) != 0)
                    chkSolid.Checked    = ((f & (UInt32)SolidFlags.solid) != 0);
                    chkWindow.Checked   = ((f & (UInt32)SolidFlags.window) != 0);
                    chkClip.Checked     = ((f & (UInt32)SolidFlags.clip) != 0);
                    chkHint.Checked     = ((f & (UInt32)SolidFlags.hint) != 0);
                    chkEmpty.Checked    = ((f & (UInt32)SolidFlags.empty) != 0);

                    chkWavy.Checked     = ((f & (UInt32)SolidFlags.wavy) != 0);
                    chkDetail.Checked   = ((f & (UInt32)SolidFlags.detail) != 0);
                    chkArea.Checked     = ((f & (UInt32)SolidFlags.area) != 0);
                    chkFlocking.Checked = ((f & (UInt32)SolidFlags.flocking) != 0);
                    chkSheet.Checked    = ((f & (UInt32)SolidFlags.sheet) != 0);

                    for (int i = 0; i < fCustom.Count; i++)
                    {
                        if (((UInt32)f & fCustom[keys[i]]) != 0) CustomFlags.SetItemCheckState(i, CheckState.Checked);
                    }
                }
                else
                {
                    if (chkSolid.Checked  != ((f & (UInt32)SolidFlags.solid)  != 0)) DiffGBSPSolid = true;
                    if (chkWindow.Checked != ((f & (UInt32)SolidFlags.window) != 0)) DiffGBSPWindow = true;
                    if (chkClip.Checked   != ((f & (UInt32)SolidFlags.clip)   != 0)) DiffGBSPClip = true;
                    if (chkHint.Checked   != ((f & (UInt32)SolidFlags.hint)   != 0)) DiffGBSPHint = true;
                    if (chkEmpty.Checked  != ((f & (UInt32)SolidFlags.empty)  != 0)) DiffGBSPEmpty = true;

                    if (chkWavy.Checked     != ((f & (UInt32)SolidFlags.wavy) != 0)) DiffGBSPWavy = true;
                    if (chkDetail.Checked   != ((f & (UInt32)SolidFlags.detail) != 0)) DiffGBSPDetail = true;
                    if (chkArea.Checked     != ((f & (UInt32)SolidFlags.area) != 0)) DiffGBSPArea = true;
                    if (chkFlocking.Checked != ((f & (UInt32)SolidFlags.flocking) != 0)) DiffGBSPFlocking = true;
                    if (chkSheet.Checked    != ((f & (UInt32)SolidFlags.sheet) != 0)) DiffGBSPSheet = true;
                    for (int i = 0; i < fCustom.Count; i++)
                    {
                        if (!(CustomFlags.GetItemCheckState(i) == CheckState.Indeterminate))
                        {
                            bool c = (CustomFlags.GetItemCheckState(i) == CheckState.Checked) ? true : false;
                            if ((((UInt32)f & fCustom[keys[i]]) != 0) != c)
                            {
                                CustomFlags.SetItemCheckState(i, CheckState.Indeterminate);
                                GBSPMultipleCustom = true;
                            }
                        }
                    }
                }
            }


            if (DiffGBSPSolid || DiffGBSPWindow || DiffGBSPClip || DiffGBSPHint || DiffGBSPEmpty || GBSPMultipleCustom)
            {
                GBSPMultiple = true;
                chkSolid.Checked = chkWindow.Checked = chkClip.Checked = chkHint.Checked = chkEmpty.Checked = false;
                chkWavy.Checked = chkDetail.Checked = chkArea.Checked = chkFlocking.Checked = chkSheet.Checked = false;
                grpGBSPType.Enabled = grpGBSPSubType.Enabled = CustomFlags.Enabled = false;

                if (DiffGBSPSolid) chkSolid.CheckState = CheckState.Indeterminate;
                if (DiffGBSPWindow) chkWindow.CheckState = CheckState.Indeterminate;
                if (DiffGBSPClip) chkClip.CheckState = CheckState.Indeterminate;
                if (DiffGBSPHint) chkHint.CheckState = CheckState.Indeterminate;
                if (DiffGBSPEmpty) chkEmpty.CheckState = CheckState.Indeterminate;
                
                if (DiffGBSPWavy) chkWavy.CheckState = CheckState.Indeterminate;
                if (DiffGBSPDetail) chkDetail.CheckState = CheckState.Indeterminate;
                if (DiffGBSPArea) chkArea.CheckState = CheckState.Indeterminate;
                if (DiffGBSPFlocking) chkFlocking.CheckState = CheckState.Indeterminate;
                if (DiffGBSPSheet) chkSheet.CheckState = CheckState.Indeterminate;
                _populating = false;
            }
            else
            {
                grpGBSPType.Enabled = grpGBSPSubType.Enabled = CustomFlags.Enabled = true;
                _populating = false;
                if      (chkSolid.Checked)  { GBSPTypesChecked(SolidFlags.solid,  false); }
                else if (chkWindow.Checked) { GBSPTypesChecked(SolidFlags.window, false); }
                else if (chkClip.Checked)   { GBSPTypesChecked(SolidFlags.clip,   false); }
                else if (chkHint.Checked)   { GBSPTypesChecked(SolidFlags.hint,   false); }
                else if (chkEmpty.Checked)  { GBSPTypesChecked(SolidFlags.empty,  false); }
            }
            
        }

        private void RefreshData()
        {
            if (!Objects.Any())
            {
                Tabs.TabPages.Clear();
                return;
            }
            while (CustomFlags.Items.Count > 0) CustomFlags.Items.RemoveAt(0);

            UpdateVisgroups(false);

            var beforeTabs = Tabs.TabPages.OfType<TabPage>().ToArray();

            if (!Tabs.TabPages.Contains(VisgroupTab)) Tabs.TabPages.Add(VisgroupTab);

            if (!Objects.All(x => x is Solid)) Tabs.TabPages.Remove(SolidTab);

            if (!Objects.All(x => x is Entity || x is World))
            {
                Tabs.TabPages.Remove(ClassInfoTab);
                Tabs.TabPages.Remove(InputsTab);
                Tabs.TabPages.Remove(OutputsTab);
                Tabs.TabPages.Remove(FlagsTab);
                
                RefreshSolid();
                return;
            }

            if (!Tabs.TabPages.Contains(ClassInfoTab)) Tabs.TabPages.Insert(0, ClassInfoTab);
            if (!Tabs.TabPages.Contains(FlagsTab)) Tabs.TabPages.Insert(Tabs.TabPages.Count - 1, FlagsTab);

            if (Document.Game.Engine == Engine.Goldsource)
            {
                // Goldsource
                Tabs.TabPages.Remove(InputsTab);
                Tabs.TabPages.Remove(OutputsTab);
            }
            else
            {
                // Source
                if (!Tabs.TabPages.Contains(InputsTab)) Tabs.TabPages.Insert(1, InputsTab);
                if (!Tabs.TabPages.Contains(OutputsTab)) Tabs.TabPages.Insert(2, OutputsTab);
            }

            var afterTabs = Tabs.TabPages.OfType<TabPage>().ToArray();

            // If the tabs changed, we want to reset to the first tab
            if (beforeTabs.Length != afterTabs.Length || beforeTabs.Except(afterTabs).Any())
            {
                Tabs.SelectedIndex = 0;
            }

            _populating = true;
            Class.Items.Clear();
            var allowWorldspawn = Objects.Any(x => x is World);
            Class.Items.AddRange(Document.GameData.Classes
                                     .Where(x => x.ClassType != ClassType.Base && (allowWorldspawn || x.Name != "worldspawn"))
                                     .Select(x => x.Name).OrderBy(x => x.ToLower()).OfType<object>().ToArray());
            if (!Objects.Any()) return;
            var classes = Objects.Where(x => x is Entity || x is World).Select(x => x.GetEntityData().Name.ToLower()).Distinct().ToList();
            var cls = classes.Count > 1 ? "" : classes[0];
            if (classes.Count > 1)
            {
                Class.Text = @"<multiple types> - " + String.Join(", ", classes);
                SmartEditButton.Checked = SmartEditButton.Enabled = false;
            }
            else
            {
                var idx = Class.Items.IndexOf(cls);
                if (idx >= 0)
                {
                    Class.SelectedIndex = idx;
                    SmartEditButton.Checked = SmartEditButton.Enabled = true;
                }
                else
                {
                    Class.Text = cls;
                    SmartEditButton.Checked = SmartEditButton.Enabled = false;
                }
            }
            _values = TableValue.Create(Document.GameData, cls, Objects.Where(x => x is Entity || x is World).SelectMany(x => x.GetEntityData().Properties).Where(x => x.Key != "spawnflags").ToList());
            _prevClass = cls;
            PopulateFlags(cls, Objects.Where(x => x is Entity || x is World).Select(x => x.GetEntityData().Flags).ToList());
            _populating = false;

            UpdateKeyValues();

        }

        private void PopulateFlags(string className, List<int> flags)
        {
            FlagsTable.Items.Clear();
            var cls = Document.GameData.Classes.FirstOrDefault(x => x.Name.ToLower() == className);
            if (cls == null) return;
            var flagsProp = cls.Properties.FirstOrDefault(x => x.Name == "spawnflags");
            if (flagsProp == null) return;
            foreach (var option in flagsProp.Options.OrderBy(x => int.Parse(x.Key)))
            {
                var key = int.Parse(option.Key);
                var numChecked = flags.Count(x => (x & key) > 0);
                FlagsTable.Items.Add(option.Description, numChecked == flags.Count ? CheckState.Checked : (numChecked == 0 ? CheckState.Unchecked : CheckState.Indeterminate));
            }
        }

        private void UpdateKeyValues()
        {
            _populating = true;

            var smartEdit = SmartEditButton.Checked;
            var selectedIndex = KeyValuesList.SelectedIndices.Count == 0 ? -1 : KeyValuesList.SelectedIndices[0];
            KeyValuesList.Items.Clear();
            foreach (var tv in _values)
            {
                var dt = smartEdit ? tv.DisplayText(Document.GameData) : tv.OriginalKey;
                var dv = smartEdit ? tv.DisplayValue(Document.GameData) : tv.Value;
                KeyValuesList.Items.Add(new ListViewItem(dt) { Tag = tv.OriginalKey, BackColor = tv.GetColour() }).SubItems.Add(dv);
            }

            Angles.Enabled = false;
            var angleVal = _values.FirstOrDefault(x => x.OriginalKey == "angles");
            if (angleVal != null)
            {
                Angles.Enabled = !_changingClass;
                Angles.SetAnglePropertyString(angleVal.Value);
            }

            if (selectedIndex >= 0 && KeyValuesList.Items.Count > selectedIndex) KeyValuesList.SelectedIndices.Add(selectedIndex);
            else KeyValuesListSelectedIndexChanged(null, null);

            _populating = false;
        }

        private void SmartEditToggled(object sender, EventArgs e)
        {
            if (_populating) return;
            UpdateKeyValues();
            KeyValuesListSelectedIndexChanged(null, null);
        }

        #region Class Change

        private void StartClassChange(object sender, EventArgs e)
        {
            if (_populating) return;
            KeyValuesList.SelectedItems.Clear();
            _changingClass = true;
            Class.BackColor = Color.LightBlue;

            var className = Class.Text;
            if (_values.All(x => x.Class == null || x.Class == className))
            {
                CancelClassChange(null, null);
                return;
            }

            var cls = Document.GameData.Classes.FirstOrDefault(x => x.Name == className);
            var props = cls != null ? cls.Properties : new List<DataStructures.GameData.Property>();

            // Mark the current properties that aren't in the new class as 'removed'
            foreach (var tv in _values)
            {
                var prop = props.FirstOrDefault(x => x.Name == tv.OriginalKey);
                tv.IsRemoved = prop == null;
            }

            // Add the new properties that aren't in the new class as 'added'
            foreach (var prop in props.Where(x => x.Name != "spawnflags" && _values.All(y => y.OriginalKey != x.Name)))
            {
                _values.Add(new TableValue { OriginalKey = prop.Name, NewKey = prop.Name, IsAdded = true, Value = prop.DefaultValue });
            }

            FlagsTable.Enabled = OkButton.Enabled = false;
            ConfirmClassChangeButton.Enabled = CancelClassChangeButton.Enabled = ChangingClassWarning.Visible = true;
            UpdateKeyValues();
        }

        private void ConfirmClassChange(object sender, EventArgs e)
        {
            // Changing class: remove all the 'removed' properties, reset the rest to normal
            var className = Class.Text;
            var cls = Document.GameData.Classes.FirstOrDefault(x => x.Name == className);
            Class.BackColor = Color.LightGreen;
            _values.RemoveAll(x => x.IsRemoved);
            foreach (var tv in _values)
            {
                tv.Class = className;
                tv.IsModified = tv.IsModified || tv.IsAdded;
                tv.IsAdded = false;
            }

            // Update the flags table
            FlagsTable.Items.Clear();
            var flagsProp = cls == null ? null : cls.Properties.FirstOrDefault(x => x.Name == "spawnflags");
            if (flagsProp != null)
            {
                foreach (var option in flagsProp.Options.OrderBy(x => int.Parse(x.Key)))
                {
                    FlagsTable.Items.Add(option.Description, option.On ? CheckState.Checked : CheckState.Unchecked);
                }
            }

            _changingClass = false;
            UpdateKeyValues();
            FlagsTable.Enabled = OkButton.Enabled = true;
            ConfirmClassChangeButton.Enabled = CancelClassChangeButton.Enabled = ChangingClassWarning.Visible = false;
            _prevClass = className;
        }

        private void CancelClassChange(object sender, EventArgs e)
        {
            // Cancelling class change: remove all the 'added' properties, reset the rest to normal
            Class.Text = _prevClass;
            var className = Class.Text;
            var cls = Document.GameData.Classes.FirstOrDefault(x => x.Name == className);
            Class.BackColor = Color.White;
            _values.RemoveAll(x => x.IsAdded);
            foreach (var tv in _values)
            {
                tv.IsRemoved = false;
            }

            _changingClass = false;
            UpdateKeyValues();
            FlagsTable.Enabled = OkButton.Enabled = true;
            ConfirmClassChangeButton.Enabled = CancelClassChangeButton.Enabled = ChangingClassWarning.Visible = false;
        }

        private void KeyValuesListItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (_changingClass && e.Item.Selected) e.Item.Selected = false;
        }

        #endregion

        private void PropertyValueChanged(object sender, string propertyname, string propertyvalue)
        {
            var val = _values.FirstOrDefault(x => x.OriginalKey == propertyname);
            var li = KeyValuesList.Items.OfType<ListViewItem>().FirstOrDefault(x => ((string)x.Tag) == propertyname);
            if (val == null)
            {
                if (li != null) KeyValuesList.Items.Remove(li);
                return;
            }
            val.IsModified = true;
            val.Value = propertyvalue;
            if (li == null)
            {
                var dt = SmartEditButton.Checked ? val.DisplayText(Document.GameData) : val.OriginalKey;
                var dv = SmartEditButton.Checked ? val.DisplayValue(Document.GameData) : val.Value;
                li = new ListViewItem(dt) { Tag = val.OriginalKey, BackColor = val.GetColour() };
                KeyValuesList.Items.Add(li).SubItems.Add(dv);
            }
            else
            {
                li.BackColor = val.GetColour();
                li.SubItems[1].Text = SmartEditButton.Checked ? val.DisplayValue(Document.GameData) : val.Value;
            }
            if (propertyname == "angles" && propertyvalue != Angles.GetAnglePropertyString())
            {
                Angles.SetAnglePropertyString(propertyvalue);
            }
        }

        private void PropertyNameChanged(object sender, string oldName, string newName)
        {
            var val = _values.FirstOrDefault(x => x.OriginalKey == oldName);
            if (val == null)
            {
                return;
            }
            val.IsModified = true;
            val.NewKey = newName;
            var li = KeyValuesList.Items.OfType<ListViewItem>().FirstOrDefault(x => ((string)x.Tag) == oldName);
            if (li != null)
            {
                li.BackColor = val.GetColour();
                li.SubItems[0].Text = SmartEditButton.Checked ? val.DisplayText(Document.GameData) : val.NewKey;
            }
        }

        private void AnglesChanged(object sender, AngleControl.AngleChangedEventArgs e)
        {
            if (_populating) return;
            PropertyValueChanged(sender, "angles", Angles.GetAnglePropertyString());
            if (KeyValuesList.SelectedIndices.Count > 0
                && ((string)KeyValuesList.SelectedItems[0].Tag) == "angles"
                && SmartEditControlPanel.Controls.Count > 0
                && SmartEditControlPanel.Controls[0] is SmartEditControl)
            {
                ((SmartEditControl)SmartEditControlPanel.Controls[0]).SetProperty("angles", "angles", Angles.GetAnglePropertyString(), null);
            }
        }

        private void KeyValuesListSelectedIndexChanged(object sender, EventArgs e)
        {
            HelpTextbox.Text = "";
            CommentsTextbox.Text = "";
            ClearSmartEditControls();
            if (KeyValuesList.SelectedItems.Count == 0 || _changingClass) return;
            var smartEdit = SmartEditButton.Checked;
            var className = Class.Text;
            var selected = KeyValuesList.SelectedItems[0];
            var originalName = (string)selected.Tag;
            var value = selected.SubItems[1].Text;
            var cls = Document.GameData.Classes.FirstOrDefault(x => x.Name == className);
            var prop = _values.FirstOrDefault(x => x.OriginalKey == originalName);
            var gdProp = smartEdit && cls != null && prop != null ? cls.Properties.FirstOrDefault(x => x.Name == prop.NewKey) : null;
            if (gdProp != null)
            {
                HelpTextbox.Text = gdProp.Description;
            }
            AddSmartEditControl(gdProp, originalName, value);
        }

        private void AddPropertyClicked(object sender, EventArgs e)
        {
            if (_changingClass) return;

            using (var qf = new QuickForm("Add Property") { UseShortcutKeys = true }.TextBox("Key").TextBox("Value").OkCancel())
            {
                if (qf.ShowDialog(this) != DialogResult.OK) return;

                var name = qf.String("Key");
                var newName = name;
                var num = 1;
                while (_values.Any(x => String.Equals(x.OriginalKey, newName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    newName = name + "#" + (num++);
                }

                _values.Add(new TableValue
                {
                    Class = Class.Text,
                    OriginalKey = newName,
                    NewKey = newName,
                    Value = qf.String("Value"),
                    IsAdded = true,
                    IsModified = true,
                    IsRemoved = false
                });
                PropertyValueChanged(this, newName, qf.String("Value"));
            }
        }

        private void RemovePropertyClicked(object sender, EventArgs e)
        {
            if (KeyValuesList.SelectedItems.Count == 0 || _changingClass) return;
            var selected = KeyValuesList.SelectedItems[0];
            var propName = (string)selected.Tag;
            var val = _values.FirstOrDefault(x => x.OriginalKey == propName);
            if (val != null)
            {
                if (val.IsAdded)
                {
                    _values.Remove(val);
                }
                else
                {
                    val.IsRemoved = true;
                }
                PropertyValueChanged(this, val.OriginalKey, val.Value);
            }
        }

        private void ClearSmartEditControls()
        {
            foreach (var c in _smartEditControls)
            {
                c.Value.EditingEntityData = null;
            }
            _dumbEditControl.EditingEntityData = null;
            SmartEditControlPanel.Controls.Clear();
        }

        private void AddSmartEditControl(DataStructures.GameData.Property property, string propertyName, string value)
        {
            ClearSmartEditControls();
            var ctrl = _dumbEditControl;
            if (property != null && _smartEditControls.ContainsKey(property.VariableType))
            {
                ctrl = _smartEditControls[property.VariableType];
            }
            var prop = _values.FirstOrDefault(x => x.OriginalKey == propertyName);
            ctrl.EditingEntityData = Objects.Select(x => x.GetEntityData()).Where(x => x != null).ToList();
            ctrl.SetProperty(propertyName, prop == null ? propertyName : prop.NewKey, value, property);
            SmartEditControlPanel.Controls.Add(ctrl);
        }

        private void ApplyButtonClicked(object sender, EventArgs e)
        {
            Apply();
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButtonClicked(object sender, EventArgs e)
        {
            Apply();
            Close();
        }

        private void GBSPSubTypeReset(bool ResetSub)
        {
            if (ResetSub) chkWavy.Checked = chkDetail.Checked = chkArea.Checked = chkFlocking.Checked = chkSheet.Checked = false;
            if (chkWindow.Checked == true) chkDetail.Checked = true;

            GBSPMultiple = false;
            _populating = false;
        }

        private bool GBSPNoneChecked()
        {
            if (chkSolid.Checked == false && chkWindow.Checked == false && chkClip.Checked == false && chkHint.Checked == false && chkEmpty.Checked == false)
            {
                chkSolid.Checked = chkWindow.Checked = chkClip.Checked = chkHint.Checked = chkEmpty.Checked = false;
                return true;
            }
            else return false;
        }
        private void GBSPTypesChecked(SolidFlags s, bool ResetSub)
        {
            if (_populating) return;
            _populating = true;
            
            chkSolid.Checked = chkWindow.Checked = chkClip.Checked = chkHint.Checked = chkEmpty.Checked = false;
            switch (s)
            {
                case SolidFlags.solid:
                    if (GBSPNoneChecked()) chkSolid.Checked = true;
                    chkWavy.Enabled = false;
                    chkDetail.Enabled = chkArea.Enabled = chkFlocking.Enabled = chkSheet.Enabled = true;
                    chkWindow.Checked = chkClip.Checked = chkHint.Checked = chkEmpty.Checked = false;
                    break;
                case SolidFlags.window:
                    if (GBSPNoneChecked()) chkWindow.Checked = true;
                    chkDetail.Enabled = chkArea.Enabled = chkWavy.Enabled = chkSheet.Enabled = false;
                    chkFlocking.Enabled = true;
                    chkSolid.Checked = chkClip.Checked = chkHint.Checked = chkEmpty.Checked = false;
                    break;
                case SolidFlags.clip:
                    if (GBSPNoneChecked()) chkClip.Checked = true;
                    chkDetail.Enabled = chkArea.Enabled = chkWavy.Enabled = chkSheet.Enabled = false;
                    chkFlocking.Enabled = true;
                    chkSolid.Checked = chkWindow.Checked = chkHint.Checked = chkEmpty.Checked = false;
                    break;
                case SolidFlags.hint:
                    if (GBSPNoneChecked()) chkHint.Checked = true;
                    chkDetail.Enabled = chkArea.Enabled = chkWavy.Enabled = chkSheet.Enabled = false;
                    chkFlocking.Enabled = true;
                    chkSolid.Checked = chkWindow.Checked = chkClip.Checked = chkEmpty.Checked = false;
                    break;
                case SolidFlags.empty:
                    if (GBSPNoneChecked()) chkEmpty.Checked = true;
                    chkArea.Enabled = false;
                    chkWavy.Enabled = chkDetail.Enabled = chkFlocking.Enabled = chkSheet.Enabled = true;
                    chkSolid.Checked = chkWindow.Checked = chkClip.Checked = chkHint.Checked = false;
                    break;
            }
            
            GBSPSubTypeReset(ResetSub);
        }

        private void chkSolidClicked(object sender, EventArgs e)
        {
            GBSPTypesChecked(SolidFlags.solid, true);
        }
        private void chkWindowClicked(object sender, EventArgs e)
        {
            GBSPTypesChecked(SolidFlags.window, true);
        }
        private void chkClipClicked(object sender, EventArgs e)
        {
            GBSPTypesChecked(SolidFlags.clip, true);
        }
        private void chkHintClicked(object sender, EventArgs e)
        {
            GBSPTypesChecked(SolidFlags.hint, true);
        }
        private void chkEmptyClicked(object sender, EventArgs e)
        {
            GBSPTypesChecked(SolidFlags.empty, true);
        }
        private void MakeSameButtonClicked(object sender, EventArgs e)
        {
            grpGBSPType.Enabled = grpGBSPSubType.Enabled = CustomFlags.Enabled = true;
            btnMakeSame.Enabled = false;
        }
    }    
}