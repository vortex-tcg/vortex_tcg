#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.Collection.Controllers;
using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Api.Collection.Services;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace VortexTCG.Tests.Api.Collection.Controllers
{
    public class CollectionControllerTest
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static CollectionController CreateController(VortexDbContext db)
        {
            CollectionProvider provider = new CollectionProvider(db);
            CollectionService service = new CollectionService(provider);
            return new CollectionController(service);
        }
    }
}
