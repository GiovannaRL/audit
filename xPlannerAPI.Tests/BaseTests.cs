using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xPlannerAPI.Services;
using xPlannerAPI.Models;
using xPlannerAPI.Tests;
using xPlannerCommon;
using xPlannerCommon.Models;

namespace xPlannerAPI.Tests
{
    [TestClass]
    public class BaseTests
    {
        protected int _projectId;
        domain _domain;
        const short _domain_id = 30;
        const string _projectDescription = "AssetInventoryImporterRespositoryTests-F0635B72-447F-4CE5-AB9F-BEF20105FDC2";
        const string _testUser = "unittests@audaxware.com";
        protected audaxwareEntities _db;

        public project_room CreateRoom(string name, string number)
        {
            var r = new project_room();
            r.drawing_room_name = name;
            r.drawing_room_number = number;
            _db.Entry(r).State = EntityState.Added;
            _db.SaveChanges();
            return r;
        }

        public short DomainId
        {
            get
            {
                Assert.IsNotNull(_domain);
                return _domain_id;
            }

        }

        public T CreateControler<T>() where T : ApiController, new()
        {
            var controller =  new T();
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            return controller;
        }

        public project_room_inventory AddInventoryAsset(project_room room, string code, int qty, int budget)
        {
            var a = new project_room_inventory();
            //TODO
            return a;
        }

        void ContextEvent(object sender, ContextEventArgs args)
        {
            if (_domain != null)
            {
                args.DomainId = _domain.domain_id;
                args.ShowAudaxwareInfo = true;
            }
        }

        void ClearAssets()
        {
            ClearProject(true);
        }

        void ClearProject()
        {
            ClearProject(false);
        }

        void ClearProject(bool assetsOnly)
        {
            _db.cost_center.RemoveRange(_db.cost_center.Where(x => x.domain_id == DomainId));
            _db.documents_associations.RemoveRange(_db.documents_associations.Where(x => x.project_domain_id == DomainId || x.asset_domain_id == DomainId));
            _db.inventory_documents.RemoveRange(_db.inventory_documents.Where(x => x.inventory_domain_id == DomainId));
            _db.project_room_inventory.RemoveRange(_db.project_room_inventory.Where(x => x.domain_id == DomainId));
            _db.assets.RemoveRange(_db.assets.Where(x => x.domain_id == DomainId));
            _db.assets_category.RemoveRange(_db.assets_category.Where(x => x.domain_id == DomainId));
            _db.assets_subcategory.RemoveRange(_db.assets_subcategory.Where(x => x.domain_id == DomainId));
            _db.project_room.RemoveRange(_db.project_room.Where(x => x.domain_id == DomainId));
            _db.project_department.RemoveRange(_db.project_department.Where(x => x.domain_id == DomainId));
            _db.project_phase.RemoveRange(_db.project_phase.Where(x => x.domain_id == DomainId));
            _db.manufacturers.RemoveRange(_db.manufacturers.Where(x => x.domain_id == DomainId));
            _db.jsns.RemoveRange(_db.jsns.Where(x => x.domain_id == DomainId));
            _db.projects.RemoveRange(_db.projects.Where(x => x.domain_id == DomainId));
            _db.SaveChanges();
        }

        [TestInitialize]
        public void Initialize()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent += ContextEvent;
            _db = new audaxwareEntities();
            _db.Database.CommandTimeout = 2000000;
            _domain = _db.domains.Where(x => x.domain_id == _domain_id).FirstOrDefault();
            ClearProject();
            using (var tableRepo = new  TableRepository<project>())
            {
                var p = new project();
                p.project_description = _projectDescription;
                p.comment = "Project used for Unit tests only";
                p.domain_id = DomainId;
                p = tableRepo.Add(p);
                _projectId = p.project_id;
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            xPlannerCommon.SessionConnectionInterceptor.ContextEvent -= ContextEvent;
            ClearProject();
            _db.Dispose();
        }
    }
}
