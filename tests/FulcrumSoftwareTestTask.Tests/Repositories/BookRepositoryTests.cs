using FulcrumSoftwareTestTask.Areas.Books;
using FulcrumSoftwareTestTask.Repositories.Books;

namespace FulcrumSoftwareTestTask.Tests.Repositories
{
    [TestFixture]
    public class BookRepositoryTests
    {
        private BookRepository _repository;
        private string _tempFilePath;

        [SetUp]
        public void SetUp()
        {
            _repository = new BookRepository();
            _tempFilePath = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }

        [Test]
        public void LoadAsync_FileNotFound_ThrowsFileNotFoundException()
        {
            var nonExistentPath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid()}.xml");

            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await _repository.LoadAsync(nonExistentPath));
        }

        [Test]
        public async Task LoadAsync_ValidXml_ReturnsCorrectBooks()
        {

            var books = new List<Book>
            {
                new() { Title = "Dune",  Author = "Frank Herbert", Pages = 412 },
                new() { Title = "1984",  Author = "George Orwell", Pages = 328 }
            };

            await _repository.SaveAsync(_tempFilePath, books);

            var result = await _repository.LoadAsync(_tempFilePath);

            Assert.That(result.Count, Is.EqualTo(2));

            for (var i = 0; i < books.Count; i++)
            {
                Assert.That(result[i].Title, Is.EqualTo(books[i].Title));
                Assert.That(result[i].Author, Is.EqualTo(books[i].Author));
                Assert.That(result[i].Pages, Is.EqualTo(books[i].Pages));
            }
        }

        [Test]
        public async Task LoadAsync_EmptyXml_ReturnsEmptyCollection()
        {
            await _repository.SaveAsync(_tempFilePath, new List<Book>());

            var result = await _repository.LoadAsync(_tempFilePath);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void LoadAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            var ct = new CancellationTokenSource();
            ct.Cancel();

            Assert.That(
                async () => await _repository.LoadAsync(_tempFilePath, ct.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public async Task SaveAsync_ValidBooks_FileIsCreatedWithContent()
        {
            var books = new List<Book>
            {
                new() { Title = "Dune", Author = "Frank Herbert", Pages = 412 }
            };

            await _repository.SaveAsync(_tempFilePath, books);

            Assert.That(File.Exists(_tempFilePath), Is.True);
            Assert.That(new FileInfo(_tempFilePath).Length, Is.GreaterThan(0));
        }

        [Test]
        public async Task SaveAsync_ThenLoad_ReturnsOriginalBooks()
        {
            var books = new List<Book>
            {
                new() { Title = "Dune",  Author = "Frank Herbert", Pages = 412 },
                new() { Title = "1984",  Author = "George Orwell", Pages = 328 }
            };

            await _repository.SaveAsync(_tempFilePath, books);

            var result = await _repository.LoadAsync(_tempFilePath);

            Assert.That(result.Count, Is.EqualTo(books.Count));

            for (var i = 0; i < books.Count; i++)
            {
                Assert.That(result[i].Title, Is.EqualTo(books[i].Title));
                Assert.That(result[i].Author, Is.EqualTo(books[i].Author));
                Assert.That(result[i].Pages, Is.EqualTo(books[i].Pages));
            }
        }

        [Test]
        public async Task SaveAsync_EmptyList_CanBeLoadedBack()
        {
            await _repository.SaveAsync(_tempFilePath, new List<Book>());

            var result = await _repository.LoadAsync(_tempFilePath);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SaveAsync_CalledTwice_OverwritesPreviousContent()
        {
            var firstBatch = new List<Book>
            {
                new() { Title = "Dune", Author = "Frank Herbert", Pages = 412 }
            };

            var secondBatch = new List<Book>
            {
                new() { Title = "1984",            Author = "George Orwell", Pages = 328 },
                new() { Title = "Brave New World", Author = "Aldous Huxley",  Pages = 311 }
            };

            await _repository.SaveAsync(_tempFilePath, firstBatch);
            await _repository.SaveAsync(_tempFilePath, secondBatch);

            var result = await _repository.LoadAsync(_tempFilePath);

            Assert.That(result.Count, Is.EqualTo(2));

            for (var i = 0; i < secondBatch.Count; i++)
            {
                Assert.That(result[i].Title, Is.EqualTo(secondBatch[i].Title));
                Assert.That(result[i].Author, Is.EqualTo(secondBatch[i].Author));
                Assert.That(result[i].Pages, Is.EqualTo(secondBatch[i].Pages));
            }
        }

        [Test]
        public void SaveAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            var books = new List<Book>
            {
                new() { Title = "Dune", Author = "Frank Herbert", Pages = 412 }
            };

            var ct = new CancellationTokenSource();

            ct.Cancel();

            Assert.That(
                async () => await _repository.SaveAsync(_tempFilePath, books, ct.Token),
                Throws.InstanceOf<OperationCanceledException>());
        }
    }
}

