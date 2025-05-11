using System;
using System.Linq;

namespace xPlannerCommon.Models
{
    public static class audaxwareEntitiesExtensions
    {

        public static asset duplicate_asset_ex(this audaxwareEntities db, Nullable<short> new_domain_id, Nullable<int> old_asset_id, Nullable<bool> change_inventories, string added_by)
        {
            var retResult = db.duplicate_asset(new_domain_id, old_asset_id, change_inventories, added_by).FirstOrDefault();
            var ret = new asset();
            ConvertObject(retResult, ret);
            return ret;
        }
        public static project_room clone_template_ex(this audaxwareEntities db, Nullable<short> domain_id, Nullable<int> project_id, Nullable<int> phase_id, Nullable<int> department_id, Nullable<int> room_id, Nullable<short> new_domain_id, string description, string added_by, Nullable<short> related_project_domain_id, Nullable<int> related_project_id, Nullable<bool> fromRoom, string room_comment)
        {
            var retResult = db.create_template(domain_id, project_id, phase_id, department_id, room_id, new_domain_id,
                description, added_by, related_project_domain_id, related_project_id, fromRoom, room_comment).FirstOrDefault();
            var ret = new project_room();
            ConvertObject(retResult, ret);
            return ret;
        }

        static void ConvertObject<T1, T2>(T1 src,  T2 target)
        {
            var fields = src.GetType().GetProperties();
            var newType = target.GetType();
            for (int i = 0; i < fields.Count(); ++i){
                var newProp = newType.GetProperty(fields[i].Name);
                if (newProp != null)
                    newProp.SetValue(target, fields[i].GetValue(src));
            }
        }
        public static bool CheckDomainAccess(this audaxwareEntities db, int domainId)
        {
            return true;
        }
    }
}
