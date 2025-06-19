using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;


namespace xPlannerAPI.Tests
{
    public abstract class BaseTestsProjectDefault : BaseTests
    {
        protected BaseTestsProjectDefault(CreateProjectOptions createProjectOptions = null): base(createProjectOptions)
        {
        }


        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            ImportProject("xPlannerAPI.Tests.Repositories.TestData.ProjectDefault.xlsx");
        }
    }
}
