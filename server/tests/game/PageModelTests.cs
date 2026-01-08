using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using game.Pages;
using Xunit;

namespace game.Tests
{
    public class PageModelTests
    {
        [Fact]
        public void ErrorModel_OnGet_ShouldSetRequestId()
        {
            ErrorModel model = new ErrorModel(NullLogger<ErrorModel>.Instance);
            DefaultHttpContext context = new DefaultHttpContext();
            context.TraceIdentifier = "trace-1";
            model.PageContext = new PageContext
            {
                HttpContext = context
            };

            model.OnGet();

            Assert.True(model.ShowRequestId);
            Assert.Equal("trace-1", model.RequestId);
        }

        [Fact]
        public void IndexModel_OnGet_ShouldNotThrow()
        {
            IndexModel model = new IndexModel(NullLogger<IndexModel>.Instance);
            model.OnGet();
            Assert.NotNull(model);
        }

        [Fact]
        public void PrivacyModel_OnGet_ShouldNotThrow()
        {
            PrivacyModel model = new PrivacyModel(NullLogger<PrivacyModel>.Instance);
            model.OnGet();
            Assert.NotNull(model);
        }
    }
}
