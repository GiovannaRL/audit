using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerAPI.Models;

namespace xPlannerAPI.Interfaces
{
    interface IAssetTreeViewRepository : IDisposable
    {
        List<TreeViewItem> GetAll(int domainId);
    }
}
