using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Autofac.Tests.Integration.SharePoint
{
    [TestFixture]
    public class ContainerDisposalModuleFixture
    {
        [SetUp]
        public void SetUp()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void EndRequestLifetime_is_called_on_all_containerproviders()
        {
            throw new NotImplementedException();
        }
    }
}
