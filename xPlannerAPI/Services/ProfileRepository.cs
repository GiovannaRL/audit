using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerAPI.Models;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class ProfileRepository : IDisposable
    {
        private readonly audaxwareEntities _db;
        private bool disposed = false;

        public ProfileRepository()
        {
            _db = new audaxwareEntities();
        }

        public void Update(short domainId, int projectId, string oldProfile, bool oldDetailedBudget, IEnumerable<inventory_options> options, bool newDetailedBudget = false) {

            using (var priRep = new TableRepository<project_room_inventory>())
            {
                var inventories = priRep.GetAll(new [] { "domain_id", "project_id" },
                    new [] { domainId, projectId }, null).Where(pri => pri.detailed_budget == oldDetailedBudget && (oldDetailedBudget ? $"{pri.asset_profile} - {pri.asset_profile_budget}" == oldProfile : pri.asset_profile == oldProfile));

                if (!inventories.Any())
                    return;

                var inventories_id = string.Join(";", inventories.Select(i => i.inventory_id));

                _db.upd_detailed_budget_profile(inventories_id, newDetailedBudget);

                foreach (var option in options)
                {
                    _db.update_inventories_option(option.domain_id, inventories_id, option.option_id, option.quantity, option.unit_price);
                }

                _db.delete_assets_options(inventories_id, string.Join(";", options.Select(op => op.option_id)));
            }

        }

        public IEnumerable<inventory_options> GetOptions(int profileId)
        {
            var pro = _db.profiles.Find(profileId);

            if (pro != null)
            {
                var pri = _db.project_room_inventory.FirstOrDefault(pr => pr.asset_profile == pro.profile1);

                if (pri != null)
                {

                    var items = this._db.inventory_options.Include("assets_options")
                        .Where(io => io.inventory_id == pri.inventory_id).ToList();

                    items.ForEach(delegate (inventory_options item) {
                        item.unit_price = 0;
                    });

                    return items;
                }
            }

            return null;
        }

        public IEnumerable<get_global_profiles_not_project_Result> GetGloblal(short projectDomainId, int projectId, short assetDomainId, int asset_id)
        {
            return _db.get_global_profiles_not_project(projectDomainId, projectId, assetDomainId, asset_id).ToList();
        }

        public IEnumerable<get_project_profiles_Result> GetProjectProfiles(short projectDomainId, int projectId, short assetDomainId, int asset_id)
        {
            return _db.get_project_profiles(projectDomainId, projectId, assetDomainId, asset_id).ToList();
        }

        public void EditAllAssets(short projectDomainId, int projectId, ProfileEditAll data)
        {
            _db.edit_assets_profile(projectDomainId, projectId, data.old_profile, data.old_profile_budget, data.old_detailed_budget, data.inventory_id_new_profile);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}