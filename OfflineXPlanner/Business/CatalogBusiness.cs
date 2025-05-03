using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Facade;
using OfflineXPlanner.Utils;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using xPlannerCommon.Models;

namespace OfflineXPlanner.Business
{
    public class CatalogBusiness
    {
        public static DataTable LoadJSNs()
        {
            ICatalogDAO catalogDAO = new CatalogDAO();

            return catalogDAO.GetAllJSN();
        }

        public static jsn LoadJSN(string JSNCode)
        {
            ICatalogDAO catalogDAO = new CatalogDAO();

            return catalogDAO.GetJSN(JSNCode);
        }

        public static bool InsertOrUpdateJSN(JSN jsn)
        {
            ICatalogDAO catalogDAO = new CatalogDAO();

            return catalogDAO.InsertOrUpdateJSN(jsn);
        }

        public static DataTable LoadManufacturer()
        {
            ICatalogDAO catalogDAO = new CatalogDAO();

            return catalogDAO.GetAllManufacturer();
        }

        /* Import all the catalog data from audaxware
         */
        public static async Task importCatalogData() { 

            ProgressBarForm progressBarForm = new ProgressBarForm("Importing manufacturers", 10);
            progressBarForm.Show();

            await importManufacturers(progressBarForm);
            await importJSNs(progressBarForm);

            progressBarForm.Close();
        }

        #region PRIVATE METHODS
        /* Import the manufacturers from audaxware system
         */
        private static async Task importManufacturers(ProgressBarForm progressBarForm)
        {
            ICatalogDAO catalogDAO = new CatalogDAO();

            var manufacturers = ManufacturerFacade.Import();

            if (!ListUtil.isEmptyOrNull(manufacturers))
            {
                List<Manufacturer> manufacturersToAddAtEnd = new List<Manufacturer>();

                progressBarForm.UpdateMaximum(ListUtil.isEmptyOrNull(manufacturers) ? 0 : manufacturers.Count);

                foreach (var mnf in manufacturers)
                {
                    Manufacturer manufacturer = new Manufacturer(mnf.manufacturer_id, mnf.manufacturer_description);

                    ExistsEnum exists = catalogDAO.ManufacturerExists(manufacturer);
                    if (exists == ExistsEnum.None)
                    {
                        catalogDAO.InsertManufacturer(manufacturer);
                    }
                    else if (exists == ExistsEnum.Id)
                    {
                        progressBarForm.UpdateMaximum(progressBarForm.GetMaximum() + 1);
                        /* Let those to add at end otherwise will change 
                         * the manufacturer_id of all the items coming after */
                        manufacturersToAddAtEnd.Add(manufacturer);
                    }
                    progressBarForm.PerformStep();
                }

                /* At the end those which manufacturer_id already exists are added */
                foreach (var mnf in manufacturersToAddAtEnd)
                {
                    mnf.manufacturer_id = catalogDAO.GetMaxManufacturerID() + 1;
                    catalogDAO.InsertManufacturer(mnf);
                    progressBarForm.PerformStep();
                }
            }
        }

        /* Import the JSNs manufacturers from audaxware system
         */
        private static async Task importJSNs(ProgressBarForm progressBarForm)
        {
            ICatalogDAO catalogDAO = new CatalogDAO();
            progressBarForm.UpdateLabel("Importing JSNs");
            var JSNs = JSNFacade.ImportJSNsAsync();
            if (!ListUtil.isEmptyOrNull(JSNs))
            {
                progressBarForm.IncementMaximum(ListUtil.isEmptyOrNull(JSNs) ? 0 : JSNs.Count);

                foreach (var jsn in JSNs)
                {
                    catalogDAO.InsertOrUpdateJSN(new JSN
                    {
                        JSNCode = jsn.jsn_code,
                        JSNNomenclature = jsn.nomenclature,
                        U1 = jsn.utility1,
                        U2 = jsn.utility2,
                        U3 = jsn.utility3,
                        U4 = jsn.utility4,
                        U5 = jsn.utility5,
                        U6 = jsn.utility6
                    });
                    progressBarForm.PerformStep();
                }
            }
        }
        #endregion
    }
}
