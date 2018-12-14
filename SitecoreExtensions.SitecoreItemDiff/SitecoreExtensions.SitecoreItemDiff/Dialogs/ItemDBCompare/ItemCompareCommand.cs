using System;
using System.Collections.Generic;
using System.Linq; 
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using SitecoreExtensions.SitecoreItemDiff.Dialogs.ItemDBCompare.Gatherers;

namespace SitecoreExtensions.SitecoreItemDiff.Dialogs.ItemDBCompare
{
    public class ItemCompareCommand : Command
    {
        public ID commandContextItemId;
        public override void Execute(CommandContext commandContext)
        {
            var commandContextItemId = commandContext.Items.First().ID;
            SheerResponse.CheckModified(false);
            SheerResponse.ShowModalDialog(GetDialogUrl(commandContext),"900","600");
        }
 
        private static string GetDialogUrl(CommandContext commandContext)
        {            
            return GetDialogUrl(GetItem(commandContext).ID);
        }
 
        private static string GetDialogUrl(ID id)
        {
            Assert.ArgumentCondition(!ID.IsNullOrEmpty(id), "id", "ID must be set!");
            UrlString urlString = new UrlString(Sitecore.UIUtil.GetUri("control:ItemDiff"));
            urlString.Append("id", id.ToString());
            return urlString.ToString();
        }
 
        public override CommandState QueryState(CommandContext commandContext)
        {
            var databasesGatherer = ItemInDatabasesGatherer.CreateNewItemInDatabasesGatherer(GetItem(commandContext).ID);
 
            if (databasesGatherer.DatabaseGather().Count() > 1)
            {
                return CommandState.Enabled;
            }
 
            return CommandState.Disabled;
        }
 
        private static Item GetItem(CommandContext commandContext)
        {
            Assert.ArgumentNotNull(commandContext, "commandContext");
            Assert.ArgumentNotNull(commandContext.Items, "commandContext.Items");
            return commandContext.Items.FirstOrDefault();
        }
    }
}