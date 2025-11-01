using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Class.DTOs;
using VortexTCG.Class.Controllers;
using VortexTCG.Class.Providers;
using VortexTCG.Common.DTO;
using System.Numerics;


namespace VortexTCG.Class.Services
{
    public class ClassService {
        private readonly VortexDbContext _db;
        private readonly IConfiguration _configuration;

        private readonly ClassProvider _provider;

        public ClassService(VortexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _provider = new ClassProvider(_db);

        }

        private bool CheckBodyForm(ClassCreateDTO data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Label))
                return true;

            return false;
        }

         public async Task<ResultDTO<ClassDTO>> CreateClass(ClassCreateDTO data)
        {
            if (CheckBodyForm(data))
            {
                return new ResultDTO<ClassDTO>
                {
                    success = false,
                    statusCode = 400,
                    message = "Label requis"
                };
            }
            VortexTCG.DataAccess.Models.Class newClass = null;
            try
            {
                newClass = new VortexTCG.DataAccess.Models.Class
                {
                    Id = Guid.NewGuid(),
                    Label = data.Label
                };

                _db.Class.Add(newClass);
                await _db.SaveChangesAsync();
            }
            catch
            {

                return new ResultDTO<ClassDTO>
                {
                    success = false,
                    statusCode = 500,
                    message = "Erreur serveur lors de la création de la classe."
                };
            }
         
            return new ResultDTO<ClassDTO>
            {
                success = true,
                statusCode = 200,
                data = new ClassDTO
                {
                    Id = newClass.Id,
                    Label = newClass.Label 
                }
            };
        }

    }
        
}
