using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerCommon;
using xPlannerCommon.Models;
using xPlannerAPI.Controllers;

namespace xPlannerAPI.Tests
{

    public class CreateProjectOptions
    {
        public short DomainId { get; set;} = 5;
        public string ProjectDescription { get; set;} = "Test Project";
        public string AddedByUser { get; set;} = "unittests@audaxware.com";
        public string ProjectComment { get; set;} = "Project used for Unit tests only";
    }

    [TestClass]
    public class BaseTests
    {
        CreateProjectOptions _createProjectOptions;

        List<project> _createdProjects = new List<project>();
        project _lastCreatedProject;
        audaxwareEntities _dbContext = new audaxwareEntities();

        protected const short AudaxWareDomainId = 1;
        protected const short MillCreekDomainId = 24;

        public BaseTests(CreateProjectOptions createProjectOptions = null)
        {
            if (createProjectOptions == null) {
                _createProjectOptions = new CreateProjectOptions();
                // Limits to 40 characters so we do not exceed the database
                // limits
                var testClassName = this.GetType().Name;
                testClassName = testClassName.Substring(0, System.Math.Min(40, testClassName.Length));
                _createProjectOptions.ProjectDescription = $"{testClassName} - Tests";
                _createProjectOptions.AddedByUser = $"{testClassName}@test.com";
                _createProjectOptions.ProjectComment = $"Comment for tests {testClassName}";
            }
            else
                _createProjectOptions = createProjectOptions;
        }

        protected audaxwareEntities CurrentDbContext
        {
            get { return _dbContext; }
        }

        protected void ResetDbContext()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
                _dbContext = null;
            }
            _dbContext = new audaxwareEntities();
        }

        protected CreateProjectOptions CreateProjectOptions
        {
            get { return _createProjectOptions; }
        }

        protected short LastCreatedProjectDomainId {
            get {
                return _lastCreatedProject == null ? (short)-1 : _lastCreatedProject.domain_id;
            }
        }

        protected int LastCreatedProjectId
        {
            get {
                return _lastCreatedProject == null ? -1 : _lastCreatedProject.project_id;
            }
        }

        protected int? ContextDomainId { get; set; }


        void ContextEvent(object sender, ContextEventArgs args)
        {
            args.DomainId = ContextDomainId ?? (LastCreatedProjectDomainId > 0
                    ? LastCreatedProjectDomainId : CreateProjectOptions.DomainId);
            args.ShowAudaxwareInfo = true;
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            _dbContext.Database.CommandTimeout = 2000000;
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent += ContextEvent;
            // Clears all projects based on the added user to ensure
            // we have a clear database
            var projects = _dbContext.projects.Where(x => x.added_by == CreateProjectOptions.AddedByUser);
            foreach(var p in projects)
            {
                ClearProject(p.project_id, p.project_id);
            }
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent -= ContextEvent;

            _lastCreatedProject = null;

            _dbContext.Dispose();
        }



        protected void CreateProject(CreateProjectOptions createProjectOptions = null)
        {
            if (createProjectOptions == null)
            {
                createProjectOptions = _createProjectOptions;
            }

            var newProject = new project
            {
                domain_id = createProjectOptions.DomainId,
                project_description = createProjectOptions.ProjectDescription,
                comment = createProjectOptions.ProjectComment,
                added_by = createProjectOptions.AddedByUser
            };

            using (var tableRepository = new TableRepository<project>())
            {
                tableRepository.Add(newProject);
                _dbContext.SaveChanges();
                _lastCreatedProject = newProject;
                _createdProjects.Add(_lastCreatedProject);
            }
        }

        protected void ImportProject(string filePath, CreateProjectOptions createProjectOptions = null) {
            if (createProjectOptions == null) {
                createProjectOptions = _createProjectOptions;
            }
            CreateProject(createProjectOptions);
            ImportProject(filePath, _lastCreatedProject.domain_id,
                    _lastCreatedProject.project_id,
                    createProjectOptions.AddedByUser);
        }

        protected void ImportProject(string filePath, int domainId, int projectId, string addedByUser)
        {

            using (var repository = new AssetsInventoryImporterRepository())
            {
                var file = EmbeddedResourceHelper.GetTempFilePathFromResource(filePath);
                var analysisResult = repository.Analyze(domainId,
                        projectId,
                        file,
                        xPlannerAPI.Interfaces.ImportColumnsFormat.AudaxWare);
                Assert.AreEqual(analysisResult.First().Status, ImportAnalysisResultStatus.Ok, "Error to analyze imported data");
                var invalid = analysisResult.First().Items.Where(x =>
                        x.Status != ImportItemStatus.New).ToArray();

                Assert.AreEqual(0, invalid.Length, "Import analysis failed for some items");

                var result = repository.Import(analysisResult.First().Items.ToArray(),
                    domainId, projectId, addedByUser, null);

                Assert.AreEqual(ImportAnalysisResultStatus.Ok, result.Status, "Error to import data");
            }
        }

        protected project_room CreateRoom(string name, string number)
        {
            var r = new project_room();
            r.drawing_room_name = name;
            r.drawing_room_number = number;
            _dbContext.Entry(r).State = EntityState.Added;
            _dbContext.SaveChanges();
            return r;
        }

        protected T CreateControler<T>() where T : AudaxWareController, new()
        {
            var controller = new T();
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            controller.EnableTestMode = true;
            return controller;
        }
        protected void ClearProjects(bool deleteProject = true)
        {
            foreach( var p in _createdProjects) {
                ClearProject(p.domain_id, p.project_id, deleteProject);
            }
        }

        protected void ClearProject(int domainId, int projectId, bool deleteProject = true)
        {
            var inventories = _dbContext.project_room_inventory.Where(x => x.domain_id == domainId && x.project_id == projectId).ToList();
            var rooms = _dbContext.project_room.Where(x => x.domain_id == domainId && x.project_id == projectId).ToList();
            var phases = _dbContext.project_phase.Where(x => x.domain_id == domainId && x.project_id == projectId).ToList();
            var departments = _dbContext.project_department.Where(x => x.domain_id == domainId && x.project_id == projectId).ToList();

            _dbContext.project_room_inventory.RemoveRange(inventories);
            _dbContext.project_room.RemoveRange(rooms);
            _dbContext.project_department.RemoveRange(departments);
            _dbContext.project_phase.RemoveRange(phases);
            _dbContext.SaveChanges();

            if (deleteProject)
            {
                var projectToDelete = _dbContext.projects.FirstOrDefault(x => x.domain_id == domainId && x.project_id == projectId);
                if (projectToDelete != null)
                {
                    _dbContext.projects.Remove(projectToDelete);
                    _dbContext.SaveChanges();
                }
            }
        }

        protected void UpdateInventory(project_room_inventory inventory)
        {
            _dbContext.Entry(inventory).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }


        protected cost_center CreateCostCenter(cost_center costCenter)
        {
            using (var tableRepository = new TableRepository<cost_center>())
            {
                tableRepository.Add(costCenter);
                return costCenter;
            }
        }

        protected IQueryable<project_room_inventory> GetLastCreatedProjectInventory()
        {
            return _dbContext.project_room_inventory.Where(inventory =>
                    inventory.project_id == _lastCreatedProject.project_id);
        }
    }
}
