using Microsoft.AspNetCore.Mvc;
using VortexTCG.Common.DTO;

namespace VortexTCG.Common.Services
{
    public abstract class VortexBaseController : ControllerBase

    {
    protected IActionResult toActionResult<T>(ResultDTO<T> result) =>
        StatusCode(result.statusCode, result);
    }

}

