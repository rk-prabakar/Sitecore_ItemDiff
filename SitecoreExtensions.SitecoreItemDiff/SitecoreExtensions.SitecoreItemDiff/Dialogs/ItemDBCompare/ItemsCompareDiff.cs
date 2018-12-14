using System;
using System.Collections.Generic;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Text.Diff.View;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using SitecoreExtensions.SitecoreItemDiff.Dialogs.ItemDBCompare.Gatherers;
using Sitecore.Collections;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using System.Web.UI;
using Sitecore.Text.Diff;
using System.Web.UI.WebControls;
using Sitecore.Data.Fields;

namespace SitecoreExtensions.SitecoreItemDiff.Dialogs.ItemDBCompare
{
    /// <summary>
    /// 
    /// </summary>
    public class ItemsCompareDiff : BaseForm
    {
        private const string IDKey = "id";
        private const string OneColumnViewRegistry = "OneColumnView";
        private const string TwoColumnViewRegistry = "TwoColumnView";
        private const string ViewRegistryKey = "/Current_User/ItemDBDiff/View";

        protected Sitecore.Web.UI.HtmlControls.Button Cancel;
        protected GridPanel Grid;
        protected Sitecore.Web.UI.HtmlControls.Button OK;
        protected Combobox DatabaseMasterDropdown;
        protected Combobox DatabaseWebDropdown;
        protected Combobox LanguageDropDown;
        protected Combobox MasterVersionDropdown;
        protected Combobox WebVersionDropdown;

        private ID _ID;
        private ID ID
        {
            get
            {
                if (ID.IsNullOrEmpty(_ID))
                {
                    _ID = GetID();
                }

                return _ID;
            }
        }

        private Database _masterDatabase;
        private Database MasterDatabase
        {
            get
            {
                if (_masterDatabase == null)
                {
                    _masterDatabase = GetmasterDatabase();
                }

                return _masterDatabase;
            }
        }

        private Database _webDatabase;
        private Database WebDatabase
        {
            get
            {
                if (_webDatabase == null)
                {
                    _webDatabase = GetWebDatabase();
                }

                return _webDatabase;
            }
        }

        private ID GetID()
        {
            return MainUtil.GetID(GetGetQueryStringProperty(IDKey, IDKey), ID.Null);
        }

        private Database GetmasterDatabase()
        {
            //return GetDatabase("master");
            return GetDatabase(string.IsNullOrEmpty(DatabaseMasterDropdown.SelectedItem.Value) ? "master" : DatabaseMasterDropdown.SelectedItem.Value);
        }

        private Database GetWebDatabase()
        {
            //return GetDatabase("web");
            return GetDatabase(string.IsNullOrEmpty(DatabaseWebDropdown.SelectedItem.Value) ? "web" : DatabaseWebDropdown.SelectedItem.Value);
        }

        private static Database GetDatabase(string databaseName)
        {
            if (!string.IsNullOrEmpty(databaseName))
            {
                return Factory.GetDatabase(databaseName);
            }

            return null;
        }

        private static string GetGetQueryStringProperty(string serverPropertyKey, string queryStringName, string defaultValue = null)
        {
            return GetQueryString(queryStringName, defaultValue);
        }

        private static string GetQueryString(string name, string defaultValue = null)
        {
            Assert.ArgumentNotNullOrEmpty(name, "name");
            if (!string.IsNullOrEmpty(defaultValue))
            {
                return WebUtil.GetQueryString(name, defaultValue);
            }

            return WebUtil.GetQueryString(name);
        }

        private void Compare()
        {
            CompareItems(GetGridDiffView(), Grid, GetMasterItem(), GetWebItem());
        }

        /// <summary>
        /// Compare the master item and web item
        /// </summary>
        /// <param name="diffView"></param>
        /// <param name="gridPanel"></param>
        /// <param name="masterItem"></param>
        /// <param name="webItem"></param>
        private void CompareItems(DiffView diffView, GridPanel gridPanel, Item masterItem, Item webItem)
        {
            Assert.ArgumentNotNull(diffView, "diffView");
            Assert.ArgumentNotNull(gridPanel, "gridPanel");
            Assert.ArgumentNotNull(masterItem, "itemOne");
            Assert.ArgumentNotNull(webItem, "itemTwo");
            //diffView.Compare(gridPanel, masterItem, webItem, string.Empty);
            if (IsOneColumnSelected())
            { ItemCompareView.CompareTwoColumnView(gridPanel, masterItem, webItem, string.Empty); }
            else
            { ItemCompareView.CompareTwoColumnView(gridPanel, masterItem, webItem, string.Empty); }            
        }

        private static DiffView GetGridDiffView()
        {
            if (IsOneColumnSelected())
            {
                return new OneColumnDiffView();
            }

            return new TwoCoumnsDiffView();
        }

        private Item GetMasterItem()
        {
            Assert.IsNotNull(MasterDatabase, "DatabaseOne must be set!");
            return MasterDatabase.GetItem(ID, LanguageManager.GetLanguage(LanguageDropDown.SelectedItem.Value), Sitecore.Data.Version.Latest);
        }
        private Item GetWebItem()
        {
            Assert.IsNotNull(WebDatabase, "DatabaseTwo must be set!");
            return WebDatabase.GetItem(ID, LanguageManager.GetLanguage(LanguageDropDown.SelectedItem.Value), Sitecore.Data.Version.Latest);
        }
        #region Page events
        private static void OnCancel(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            Context.ClientPage.ClientResponse.CloseWindow();
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);

            OK.OnClick += new EventHandler(OnOK);
            Cancel.OnClick += new EventHandler(OnCancel);

            DatabaseMasterDropdown.OnChange += new EventHandler(OnUpdate);
            DatabaseWebDropdown.OnChange += new EventHandler(OnUpdate);
            LanguageDropDown.OnChange += new EventHandler(OnUpdate);
        }

        private static void OnOK(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            Context.ClientPage.ClientResponse.CloseWindow();
        }

        protected override void OnPreRender(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnPreRender(e);

            PopuplateLanguageDropdown(LanguageDropDown);
            if (!Context.ClientPage.IsEvent)
            {
                PopuplateDatabaseDropdowns();
                //PopuplateItemVersionsDropdowns(); -To-do
                Compare();
                UpdateButtons();
            }
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            Refresh();
        }

        private void Refresh()
        {
            Grid.Controls.Clear();
            PopuplateLanguageDropdown(LanguageDropDown);
            Compare();
            Context.ClientPage.ClientResponse.SetOuterHtml("Grid", Grid);
        }
        #endregion

        #region Populate dropdowns value
        private void PopuplateDatabaseDropdowns()
        {
            IDatabasesGatherer IDatabasesGatherer = ItemInDatabasesGatherer.CreateNewItemInDatabasesGatherer(ID);
            PopuplateDatabaseDropdowns(IDatabasesGatherer.DatabaseGather());
        }

        private void PopuplateDatabaseDropdowns(IEnumerable<Database> databases)
        {
            //By default load the master and web database in the dropdown
            PopuplateDatabaseDropdown(DatabaseMasterDropdown, databases, Factory.GetDatabase("master"));
            PopuplateDatabaseDropdown(DatabaseWebDropdown, databases, Factory.GetDatabase("web"));
        }

        private static void PopuplateDatabaseDropdown(Combobox databaseDropdown, IEnumerable<Database> databases, Database selectedDatabase)
        {
            Assert.ArgumentNotNull(databaseDropdown, "databaseDropdown");
            Assert.ArgumentNotNull(databases, "databases");

            foreach (Database database in databases)
            {
                //Ignore core database from the dropdown
                if (database.Name.ToLower() == "core")
                    continue;

                databaseDropdown.Controls.Add
                (
                    new Sitecore.Web.UI.HtmlControls.ListItem
                    {
                        ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("ListItem"),
                        Header = database.Name,
                        Value = database.Name,
                        Selected = string.Equals(database.Name, selectedDatabase.Name)
                    }
                );
            }
        }

        #region To-do Include version compare
        //Load numbered versions in the dropdown for master and web items


        //private void PopuplateItemVersionsDropdowns()
        //{
        //    PopuplateItemVersionsDropdown(MasterVersionDropdown, GetMasterItem());
        //    PopuplateItemVersionsDropdown(WebVersionDropdown, GetWebItem());
        //}

        //private static void PopuplateItemVersionsDropdown(Combobox itemVersionsDropdown, Item itemVersions)
        //{
        //    foreach (var version in itemVersions.Versions.GetVersions())
        //    {
        //        itemVersionsDropdown.Controls.Add(
        //            new ListItem {
        //                ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("ListItem"),
        //                Header = version.Version.Number.ToString(),
        //                Value = version.Version.Number.ToString(),
        //                Selected = string.Equals(version.Name, itemVersions.Name)
        //            }
        //            );
        //    } 
        //}
        #endregion

        private static void PopuplateLanguageDropdown(Combobox languageDropdown)
        {
            LanguageCollection languageVersions = ItemManager.GetContentLanguages(GetDatabase("master").GetItem(MainUtil.GetID(GetGetQueryStringProperty(IDKey, IDKey), ID.Null)));
            foreach (var language in languageVersions)
            {
                var itm = GetDatabase("master").GetItem(MainUtil.GetID(GetGetQueryStringProperty(IDKey, IDKey), ID.Null), language);
                if (itm.Versions.Count > 0)
                {
                    languageDropdown.Controls.Add
                (
                    new Sitecore.Web.UI.HtmlControls.ListItem
                    {
                        ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("ListItem"),
                        Header = language.Title + " - " + language.Name, //To-do - remove obsolete reference
                        Value = language.Name,
                        Selected = string.Equals(language.Name, Context.Language)
                    }
                );
                }
            }
        }

        #endregion
        
        #region Toolbox button
        private static void UpdateButtons()
        {
            bool isOneColumnSelected = IsOneColumnSelected();
            SetToolButtonDown("OneColumn", isOneColumnSelected);
            SetToolButtonDown("TwoColumn", !isOneColumnSelected);
        }
        protected void ShowOneColumn()
        {
            SetRegistryString(ViewRegistryKey, OneColumnViewRegistry);
            UpdateButtons();
            Refresh();
        }
        protected void ShowTwoColumns()
        {
            SetRegistryString(ViewRegistryKey, TwoColumnViewRegistry);
            UpdateButtons();
            Refresh();
        }

        private static bool IsOneColumnSelected()
        {
            return string.Equals(GetRegistryString(ViewRegistryKey, OneColumnViewRegistry), OneColumnViewRegistry);
        }

        private static void SetToolButtonDown(string controlID, bool isDown)
        {
            Assert.ArgumentNotNullOrEmpty(controlID, "controlID");
            Toolbutton toolbutton = FindClientPageControl<Toolbutton>(controlID);
            toolbutton.Down = isDown;
        }

        private static T FindClientPageControl<T>(string controlID) where T : System.Web.UI.Control
        {
            Assert.ArgumentNotNullOrEmpty(controlID, "controlID");
            T control = Context.ClientPage.FindControl(controlID) as T;
            Assert.IsNotNull(control, typeof(T));
            return control;
        }

        private static string GetRegistryString(string key, string defaultValue = null)
        {
            Assert.ArgumentNotNullOrEmpty(key, "key");

            if (!string.IsNullOrEmpty(defaultValue))
            {
                return Registry.GetString(key, defaultValue);
            }

            return Registry.GetString(key);
        }

        private static void SetRegistryString(string key, string value)
        {
            Assert.ArgumentNotNullOrEmpty(key, "key");
            Registry.SetString(key, value);
        }
        #endregion
    }
}