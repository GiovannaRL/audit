using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class AssetByRoomItem
    {
        public AssetByRoomInventory asset_information { get; set; }
        public List<AssetByRoomLocation> rooms { get; set; }

        public AssetByRoomItem()
        {
            this.rooms = new List<AssetByRoomLocation>();
            this.asset_information = new AssetByRoomInventory();
        }

        public AssetByRoomItem(AssetByRoomInventory asset_information, List<AssetByRoomLocation> rooms)
        {
            this.rooms = rooms;
            this.asset_information = asset_information;
        }
    }
}
