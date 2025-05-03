using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Facade.Domain;
using OfflineXPlanner.Security;
using OfflineXPlanner.Extensions;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using xPlannerCommon.Models;
using System.Linq;

namespace OfflineXPlanner.Facade
{
    public class ProjectFacade
    {
        private static HttpClient client = new HttpClient();
        private static readonly string projectsEndpoint =
            $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.projects}/";
        private static readonly string departmentsEndpoint =
           $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.departments}/";
        private static readonly string roomsEndpoint =
           $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.rooms}/";
        private static readonly string inventoryEndpoint =
           $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.assetsInventory}/";
        private static readonly string costCenterEndpoint =
            $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.costCenters}/";

        public static List<project> ImportProjects()
        {
            // Verify if there is a logged token
            if (!SecurityUtil.IsLogged())
            {
                //TODO: throw exception
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);
            var res = client.Get($"{projectsEndpoint}{RestAPIActions.All}/{AudaxwareRestApiInfo.loggedDomain.domain_id}");
            if (res.IsSuccessStatusCode)
            {
                return res.Content.ReadAs<List<project>>().Where(x => x.status == "I").ToList();
            }

            //TODO: throw exception
            return null;
        }

        public static ImportProjectDataResponse ImportData(Project project)
        {
            // Verify if there is a logged token
            if (SecurityUtil.IsLogged())
            {
                ImportProjectDataResponse response = new ImportProjectDataResponse();
                response.project = project;

                IDepartmentDAO departmentDAO = new DepartmentDAO();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);

                // get cost centers 
                var res = client.Get($"{costCenterEndpoint}{RestAPIActions.All}/{AudaxwareRestApiInfo.loggedDomain.domain_id}/{project.project_id}");
                if (res.IsSuccessStatusCode)
                {
                    response.costCenters = res.Content.ReadAs<List<cost_center>>();
                }

                // get departments
                res = client.Get($"{departmentsEndpoint}{RestAPIActions.All}/{AudaxwareRestApiInfo.loggedDomain.domain_id}/{project.project_id}");
                if (res.IsSuccessStatusCode)
                {
                    response.departments = res.Content.ReadAs<List<project_department>>();

                    // get rooms
                    res = client.Get($"{roomsEndpoint}{RestAPIActions.All}/{AudaxwareRestApiInfo.loggedDomain.domain_id}/{project.project_id}");
                    if (res.IsSuccessStatusCode)
                    {
                        response.rooms = res.Content.ReadAs<List<project_room>>();
                    }

                    // Get inventory
                    res = client.Get($"{inventoryEndpoint}{RestAPIActions.AllInventories}/{AudaxwareRestApiInfo.loggedDomain.domain_id}/{project.project_id}");
                    if (res.IsSuccessStatusCode)
                    {
                        response.assets = res.Content.ReadAs<List<asset_inventory>>();
                    }
                }
                return response;
            }

            //TODO: throw exception
            return null;
        }
    }
}
