using Mtx.CosmosDbServices.Entities;

namespace Mtx.CosmosDbServicesTests
{
    public class DocumentIdTests
    {
		[Theory]
		[InlineData("")]
		[InlineData(null)]
		public void DoesNotAcceptEmpty(string? value)
        {
            Assert.Throws<ArgumentException>(() => new DocumentId(value));
        }

        [Fact]
        public void ThrowsWhenSourceIsNull()
        {
			Assert.Throws<ArgumentException>(() => DocumentId.From<int?>(null));
		}

		[Fact]
		public void StoresTheId()
		{
			var expected = "12";
			var sut = new DocumentId("12");
			Assert.Equal(expected, sut.Id);
		}

		[Theory]
        [InlineData(1)]
        [InlineData(7922816251426433759354395033.0)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(10L)]
		public void WorksWithPrimitives<T>(T expected)
		{
			var actual =  DocumentId.From<T>(expected);
            Assert.Equal(expected.ToString(), actual);
		}

		[Fact]
		public void WorksWithGuid()
		{
            var expected = Guid.NewGuid();
			var actual = DocumentId.From(expected);
			Assert.Equal(expected.ToString(), actual);
		}
	}


}