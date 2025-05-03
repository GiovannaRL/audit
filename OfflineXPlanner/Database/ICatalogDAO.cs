using OfflineXPlanner.Domain;
using OfflineXPlanner.Domain.Enums;
using System.Collections.Generic;
using System.Data;
using xPlannerCommon.Models;

namespace OfflineXPlanner.Database
{
    public interface ICatalogDAO
    {
        DataTable GetAllJSN();
        List<JSN> GetAllJSNAslist();
        jsn GetJSN(string JSNCode);
        DataTable GetAllManufacturer();
        bool InsertManufacturer(Manufacturer manufacturer);
        ExistsEnum ManufacturerExists(Manufacturer manufacturer);
        int GetMaxManufacturerID();
        bool InsertOrUpdateJSN(JSN jsn);
        bool InsertJSN(JSN jsn);
    }
}
