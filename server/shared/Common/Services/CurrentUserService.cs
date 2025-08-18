using Microsoft.AspNetCore.Http;

namespace VortexTCG.Common.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _ctx;

        public CurrentUserService(IHttpContextAccessor ctx)
        {
            _ctx = ctx;
        }

        public string GetCurrentUsername()
        {
            return _ctx.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}
