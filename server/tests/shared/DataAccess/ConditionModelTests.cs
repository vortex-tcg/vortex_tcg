using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class ConditionModelTests
    {
        [Fact]
        public void Condition_And_Type_Relationships()
        {
            DateTime now = DateTime.UtcNow;
            ConditionType type = new ConditionType
            {
                Id = Guid.NewGuid(),
                Label = "Trigger",
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Conditions = new List<Condition>()
            };

            Condition condition = new Condition
            {
                Id = Guid.NewGuid(),
                Label = "IfSomething",
                ConditionDescription = "desc",
                ConditionTypeId = type.Id,
                ConditionType = type,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                StartEffects = new List<Effect>(),
                EndEffects = new List<Effect>()
            };

            type.Conditions.Add(condition);

            Condition stored = Assert.Single(type.Conditions);
            Assert.Equal(condition.Id, stored.Id);
            Assert.Same(type, stored.ConditionType);
            Assert.Equal(type.Id, stored.ConditionTypeId);
        }
    }
}
