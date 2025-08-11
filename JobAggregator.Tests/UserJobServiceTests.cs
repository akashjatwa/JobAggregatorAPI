using System;
using System.Security.Claims;
using System.Threading.Tasks;
using JobAggregator.Application.DTOs;
using JobAggregator.Infrastructure.Data;
using JobAggregator.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JobAggregator.Tests
{
    public class UserJobServiceTests
    {
        [Fact]
        public async Task SaveAsync_UsesUserIdFromHttpContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var userId = Guid.NewGuid();
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "Test"));

            var accessor = new HttpContextAccessor { HttpContext = httpContext };
            var service = new UserJobService(context, accessor);

            var dto = new JobDto("source", "ext", "title", "company", "location", "url", "desc", null, null, null, null);

            var result = await service.SaveAsync(dto);

            var saved = await context.UserJobs.FirstAsync();
            Assert.Equal(userId, saved.UserId);
            Assert.Equal(result.Id, saved.Id);
        }
    }
}

