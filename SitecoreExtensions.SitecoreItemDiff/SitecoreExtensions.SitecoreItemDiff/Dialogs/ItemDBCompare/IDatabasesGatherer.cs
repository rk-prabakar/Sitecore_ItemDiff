using System.Collections.Generic;

namespace SitecoreExtensions.SitecoreItemDiff.Dialogs.ItemDBCompare
{
    public interface IDatabasesGatherer
    {
        IEnumerable<Sitecore.Data.Database> DatabaseGather();
    }
}