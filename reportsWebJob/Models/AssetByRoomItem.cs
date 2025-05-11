using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class AssetByRoomItem
    {
        public AssetByRoomAsset asset_information { get; set; }
        public List<AssetByRoomRoom> rooms { get; set; }

        public AssetByRoomItem()
        {
            this.rooms = new List<AssetByRoomRoom>();
            this.asset_information = new AssetByRoomAsset();
        }

        public AssetByRoomItem(AssetByRoomAsset asset_information, List<AssetByRoomRoom> rooms)
        {
            this.rooms = rooms;
            this.asset_information = asset_information;
        }
    }
}
