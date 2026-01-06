using System;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class AuditableEntityTests
    {
        private class TestAuditable : AuditableEntity { }

        [Fact]
        public void Auditable_StoresMetadata()
        {
            DateTime created = DateTime.UtcNow;
            DateTime updated = created.AddMinutes(5);

            TestAuditable entity = new TestAuditable
            {
                CreatedAtUtc = created,
                CreatedBy = "creator",
                UpdatedAtUtc = updated,
                UpdatedBy = "updater"
            };

            Assert.Equal(created, entity.CreatedAtUtc);
            Assert.Equal("creator", entity.CreatedBy);
            Assert.Equal(updated, entity.UpdatedAtUtc);
            Assert.Equal("updater", entity.UpdatedBy);
        }
    }
}
