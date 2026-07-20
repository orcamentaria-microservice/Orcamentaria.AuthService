using FluentAssertions;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Test.Fixtures;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(ServiceCollection))]
    public class ClientIdTokenServiceTest
    {
        private readonly ServiceFixture _fixture;
        private readonly Application.Services.ClientIdTokenService _service;

        public ClientIdTokenServiceTest(ServiceFixture fixture)
        {
            _fixture = fixture;
            _service = new Application.Services.ClientIdTokenService();
        }

        #region Generate

        [Fact]
        public void Generate_WhenCalled_ReturnsNonEmptyString()
        {
            var data = _fixture.CreateEntity(1);

            var response = _service.Generate(data);

            response.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Generate_WhenCalled_ReturnsValidGuid()
        {
            var data = _fixture.CreateEntity(1);

            var response = _service.Generate(data);

            Guid.TryParse(response, out _).Should().BeTrue();
        }

        [Fact]
        public void Generate_WhenCalledTwiceWithSameService_ReturnsDifferentValues()
        {
            var data = _fixture.CreateEntity(1);

            var firstResponse = _service.Generate(data);
            var secondResponse = _service.Generate(data);

            firstResponse.Should().NotBe(secondResponse);
        }

        #endregion

        #region ValidateAsync

        [Fact]
        public async Task ValidateAsync_WhenCalled_ThrowsNotImplementedException()
        {
            Func<Task> act = async () => await _service.ValidateAsync("token");

            await act.Should().ThrowAsync<NotImplementedException>();
        }

        #endregion
    }
}
