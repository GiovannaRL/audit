using System.Collections.Generic;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Models;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsAssets)]
    public class ProfileController : TableGenericController<profile>
    {
        public ProfileController() : base(new [] { "asset_domain_id", "asset_id", "profile_id" }, new [] { "asset_domain_id", "asset_id" }) { }

        [ActionName("AllNotProject")]
        public IEnumerable<get_global_profiles_not_project_Result> GetAll(short id1, int id2, short id3, int id4)
        {
            using (var profRep = new ProfileRepository())
            {
                return profRep.GetGloblal(id1, id2, id3, id4);
            }
        }

        [ActionName("Item")]
        public void Put(short id1, int id2, [FromBody] ProfileData profile)
        {
            using (var rep = new ProfileRepository())
            {
                rep.Update(id1, id2, profile.oldProfile, profile.old_detailed_budget, profile.options, profile.new_detailed_budget);
            }
        }

        [ActionName("AllProject")]
        public IEnumerable<get_project_profiles_Result> Get(short id1, int id2, short id3, int id4)
        {
            using (var rep = new ProfileRepository())
            {
                return rep.GetProjectProfiles(id1, id2, id3, id4);
            }
        }

        [ActionName("options")]
        public IEnumerable<inventory_options> GetOptions(int id1, int id2, int id3)
        {
            using (var rep = new ProfileRepository())
            {
                return rep.GetOptions(id3);
            }
        }

        [ActionName("EditAllAssets")]
        public void Put(short id1, int id2, [FromBody] ProfileEditAll profileData)
        {
            using (var rep = new ProfileRepository())
            {
                rep.EditAllAssets(id1, id2, profileData);
            }
        }
    }
}
