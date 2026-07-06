using Document_Management_System.Models;
using Document_Management_System.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace DocumentManagementSystem.Tests.UnitTests
{
    public class JwtServiceTests
    {
        private JwtService GetJwtService()
        {
            var settings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsAVeryStrongSecretKeyForJwtTesting12345" },
                { "Jwt:Issuer", "DocumentManagementSystem" },
                { "Jwt:Audience", "DocumentManagementSystemUsers" }
            };

            IConfiguration configuration =
                new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            return new JwtService(configuration);
        }

        [Fact]
        public void GenerateToken_ShouldReturnToken()
        {
            // Arrange
            var service = GetJwtService();

            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                Role = "Candidate"
            };

            // Act
            var token = service.GenerateToken(user);

            // Assert
            token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GenerateToken_ShouldContainUserIdClaim()
        {
            // Arrange
            var service = GetJwtService();

            var user = new User
            {
                Id = 25,
                Email = "test@test.com",
                Role = "Candidate"
            };

            // Act
            var token = service.GenerateToken(user);

            var jwt =
                new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            // Assert
            jwt.Claims
                .First(x => x.Type == ClaimTypes.NameIdentifier)
                .Value
                .Should()
                .Be("25");
        }

        [Fact]
        public void GenerateToken_ShouldContainEmailClaim()
        {
            // Arrange
            var service = GetJwtService();

            var user = new User
            {
                Id = 1,
                Email = "john@test.com",
                Role = "Candidate"
            };

            // Act
            var token = service.GenerateToken(user);

            var jwt =
                new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            // Assert
            jwt.Claims
                .First(x => x.Type == ClaimTypes.Email)
                .Value
                .Should()
                .Be("john@test.com");
        }

        [Fact]
        public void GenerateToken_ShouldContainRoleClaim()
        {
            // Arrange
            var service = GetJwtService();

            var user = new User
            {
                Id = 1,
                Email = "admin@test.com",
                Role = "Admin"
            };

            // Act
            var token = service.GenerateToken(user);

            var jwt =
                new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            // Assert
            jwt.Claims
                .First(x => x.Type == ClaimTypes.Role)
                .Value
                .Should()
                .Be("Admin");
        }

        [Fact]
        public void GenerateToken_ShouldSetCorrectIssuer()
        {
            // Arrange
            var service = GetJwtService();

            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                Role = "Candidate"
            };

            // Act
            var token = service.GenerateToken(user);

            var jwt =
                new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            // Assert
            jwt.Issuer.Should().Be("DocumentManagementSystem");
        }

        [Fact]
        public void GenerateToken_ShouldSetCorrectAudience()
        {
            // Arrange
            var service = GetJwtService();

            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                Role = "Candidate"
            };

            // Act
            var token = service.GenerateToken(user);

            var jwt =
                new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            // Assert
            jwt.Audiences.Should().Contain("DocumentManagementSystemUsers");
        }

        [Fact]
        public void GenerateToken_ShouldExpireInApproximatelyFourHours()
        {
            // Arrange
            var service = GetJwtService();

            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                Role = "Candidate"
            };

            var before = DateTime.UtcNow;

            // Act
            var token = service.GenerateToken(user);

            var jwt =
                new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            var after = DateTime.UtcNow;

            // Assert
            jwt.ValidTo.Should().BeAfter(before.AddHours(3).ToUniversalTime());
            jwt.ValidTo.Should().BeBefore(after.AddHours(5).ToUniversalTime());
        }
    }
}