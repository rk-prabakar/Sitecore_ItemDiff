using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Configuration;

namespace SitecoreExtensions.SitecoreItemDiff.Dialogs.ItemDBCompare.Gatherers
{
    public class ItemInDatabasesGatherer : IDatabasesGatherer
    {
        private ID ID { get; set; }

        private ItemInDatabasesGatherer(string id)
            : this(Sitecore.MainUtil.GetID(id))
        {
        }

        private ItemInDatabasesGatherer(ID id)
        {
            SetID(id);
        }

        private void SetID(ID id)
        {
            AssertID(id);
            ID = id;
        }

        public IEnumerable<Sitecore.Data.Database> DatabaseGather()
        {
            return GetAllDatabases().Where(database => DoesDatabaseContainItemByID(database, ID));
        }

        private static IEnumerable<Sitecore.Data.Database> GetAllDatabases()
        {
            return Factory.GetDatabases();
        }

        private bool DoesDatabaseContainItemByID(Sitecore.Data.Database database, ID id)
        {
            return GetItem(database, id) != null;
        }

        private static Item GetItem(Sitecore.Data.Database database, ID id)
        {
            Assert.ArgumentNotNull(database, "database");
            AssertID(id);
            return database.GetItem(id);
        }

        private static void AssertID(ID id)
        {
            Assert.ArgumentCondition(!ID.IsNullOrEmpty(id), "id", "ID must be set!");
        }

        public static IDatabasesGatherer CreateNewItemInDatabasesGatherer(string id)
        {
            return new ItemInDatabasesGatherer(id);
        }

        public static IDatabasesGatherer CreateNewItemInDatabasesGatherer(ID id)
        {
            return new ItemInDatabasesGatherer(id);
        }
    }
}