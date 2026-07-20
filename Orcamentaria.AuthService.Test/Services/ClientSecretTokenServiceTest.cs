using FluentAssertions;
using Orcamentaria.AuthService.Test.Fixtures;
using Orcamentaria.Lib.Domain.Exceptions;
using Xunit;

namespace Orcamentaria.AuthService.Test.Services
{
    [Collection(nameof(ServiceCollection))]
    public class ClientSecretTokenServiceTest
    {
        private readonly ServiceFixture _fixture;
        private readonly Application.Services.ClientSecretTokenService _service;

        public ClientSecretTokenServiceTest(ServiceFixture fixture)
        {
            _fixture = fixture;
            _service = new Application.Services.ClientSecretTokenService();
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
        public void Generate_WhenCalled_ReturnsStringWithoutInvalidBase64UrlCharacters()
        {
            var data = _fixture.CreateEntity(1);

            var response = _service.Generate(data);

            response.Should().NotContain("+");
            response.Should().NotContain("/");
            response.Should().NotContain("=");
        }

        [Xunit.Theory]
        [InlineData(1, 10)]
        public void Generate_WhenCalledWithDifferentIds_ReturnsSecretsWithLengthCoherentWithIdSize(long smallerId, long biggerId)
        {
            var smallerData = _fixture.CreateEntity(smallerId);
            var biggerData = _fixture.CreateEntity(biggerId);

            var smallerResponse = _service.Generate(smallerData);
            var biggerResponse = _service.Generate(biggerData);

            var expectedSmallerLength = Convert.ToBase64String(new byte[32 + smallerId]).TrimEnd('=').Length;
            var expectedBiggerLength = Convert.ToBase64String(new byte[32 + biggerId]).TrimEnd('=').Length;

            smallerResponse.Length.Should().Be(expectedSmallerLength);
            biggerResponse.Length.Should().Be(expectedBiggerLength);
            biggerResponse.Length.Should().BeGreaterThan(smallerResponse.Length);
        }

        [Fact]
        public void Generate_WhenCalledTwiceWithSameService_ReturnsDifferentValues()
        {
            var data = _fixture.CreateEntity(1);

            var firstResponse = _service.Generate(data);
            var secondResponse = _service.Generate(data);

            firstResponse.Should().NotBe(secondResponse);
        }

        [Fact]
        public void Generate_WhenIdIsSufficientlyNegative_ThrowsUnexpectedException()
        {
            var data = _fixture.CreateEntity(-100);

            Action act = () => _service.Generate(data);

            act.Should().Throw<UnexpectedException>();
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
