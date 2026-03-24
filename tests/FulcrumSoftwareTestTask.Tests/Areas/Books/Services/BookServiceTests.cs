using FulcrumSoftwareTestTask.Areas.Books;
using FulcrumSoftwareTestTask.Areas.Books.Services;
using FulcrumSoftwareTestTask.Repositories.Books;
using Moq;


namespace FulcrumSoftwareTestTask.Tests.Areas.Books.Services
{
    public class BookServiceTests
    {
        private Mock<IBookRepository> _bookRepositoryMoq;
        private BookService _bookService;

        [SetUp]
        public void SetUp()
        {
            _bookRepositoryMoq = new Mock<IBookRepository>();
            _bookService = new BookService(_bookRepositoryMoq.Object);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void AddAsync_InvalidInput_ThrowsArgumentException(int caseNumber)
        {
            Book? book = caseNumber switch
            {
                1 => null,
                2 => new Book { Title = "", Author = "Frank Herbert", Pages = 412 },
                3 => new Book { Title = "Dune", Author = "", Pages = 412 },
                4 => new Book { Title = "Dune", Author = "Frank Herbert", Pages = 0 },
                _ => throw new ArgumentOutOfRangeException(nameof(caseNumber))
            };

            Assert.That(
                async () => await _bookService.AddAsync(book!),
                Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public async Task AddAsync_ValidBook_BookAppearsInCollection()
        {
            var book = new Book { Title = "Dune", Author = "Frank Herbert", Pages = 412 };

            await _bookService.AddAsync(book);

            var result = _bookService.GetAll();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo(book.Title));
            Assert.That(result[0].Author, Is.EqualTo(book.Author));
            Assert.That(result[0].Pages, Is.EqualTo(book.Pages));
        }

        [TestCase("")]
        [TestCase(" ")]
        public async Task SearchByPartTitleAsync_InvalidPartTitle_ThrowsArgumentException(string partTitle)
        {
            var book = new Book { Title = "Dune", Author = "Frank Herbert", Pages = 412 };

            await _bookService.AddAsync(book);

            Assert.That(
                async () => await _bookService.SearchByPartTitleAsync(partTitle),
                Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public async Task SearchByPartTitleAsync_ValidPartTitle_ReturnOneMatchingBook()
        {
            var books = new List<Book>
            {
                new() { Title = "Dune",  Author = "Frank Herbert", Pages = 412 },
                new() { Title = "1984",  Author = "George Orwell", Pages = 328 }
            };

            _bookRepositoryMoq.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(books);

            await _bookService.ImportFromXmlAsync("TestFilePath");

            var result = await _bookService.SearchByPartTitleAsync("une");

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Dune"));
        }

        [Test]
        public async Task SearchByPartTitleAsync_ValidPartTitle_ReturnsMatchingBooks()
        {
            var books = new List<Book>
            {
                new() { Title = "Dune",  Author = "Frank Herbert", Pages = 412 },
                new() { Title = "Dune2",  Author = "Frank Herbert", Pages = 500 },
                new() { Title = "1984",  Author = "George Orwell", Pages = 328 }
            };

            _bookRepositoryMoq.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(books);

            await _bookService.ImportFromXmlAsync("TestFilePath");

            var result = await _bookService.SearchByPartTitleAsync("une");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo("Dune"));
            Assert.That(result[1].Title, Is.EqualTo("Dune2"));
        }

        [Test]
        public async Task SearchByPartTitleAsync_ValidPartTitleButListEmpty_ReturnsEmptyList()
        {
            var result = await _bookService.SearchByPartTitleAsync("une");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetSortedAsync_NotEmptyList_ReturnsSortedBooks()
        {
            var books = new List<Book>
            {
                new() { Title = "Dune",  Author = "Frank Herbert", Pages = 412 },
                new() { Title = "1984",  Author = "George Orwell", Pages = 328 },
                new() { Title = "Dune2",  Author = "Frank Herbert", Pages = 500 },
            };

            _bookRepositoryMoq.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(books);

            await _bookService.ImportFromXmlAsync("TestFilePath");

            var result = await _bookService.GetSortedAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Author, Is.EqualTo("Frank Herbert"));
            Assert.That(result[2].Author, Is.EqualTo("George Orwell"));
            Assert.That(result[0].Title, Is.EqualTo("Dune"));
            Assert.That(result[1].Title, Is.EqualTo("Dune2"));
            Assert.That(result[2].Title, Is.EqualTo("1984"));
        }

        [Test]
        public async Task GetSortedAsync_NotEmptyList_ReturnsEmptyList()
        {
            var result = await _bookService.GetSortedAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task ExportToXmlAsync_EmptyCollection_SavesEmptyList()
        {
            await _bookService.ExportToXmlAsync("TestFilePath");

            _bookRepositoryMoq.Verify(
                x => x.SaveAsync(
                    "TestFilePath",
                    It.Is<IEnumerable<Book>>(b => !b.Any()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ExportToXmlAsync_NonEmptyCollection_SavesCorrectBooks()
        {
            var books = new List<Book>
            {
                new() { Title = "Dune",  Author = "Frank Herbert", Pages = 412 },
                new() { Title = "1984",  Author = "George Orwell", Pages = 328 }
            };

            _bookRepositoryMoq.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(books);

            await _bookService.ImportFromXmlAsync("TestFilePath");

            await _bookService.ExportToXmlAsync("ExportFilePath");

            _bookRepositoryMoq.Verify(
                x => x.SaveAsync(
                    "ExportFilePath",
                    It.Is<IEnumerable<Book>>(b => b.Count() == 2),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ExportToXmlAsync_RepositoryThrows_ExceptionPropagates()
        {
            _bookRepositoryMoq
                .Setup(x => x.SaveAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Book>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Disk error"));

            Assert.That(
                async () => await _bookService.ExportToXmlAsync("TestFilePath"),
                Throws.InstanceOf<IOException>());
        }

        [Test]
        public async Task ImportFromXmlAsync_ValidFilePath_ReturnsBooksFromRepository()
        {
            var books = new List<Book>
            {
                new() { Title = "Dune",  Author = "Frank Herbert", Pages = 412 },
                new() { Title = "1984",  Author = "George Orwell", Pages = 328 }
            };

            _bookRepositoryMoq.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(books);

            var result = await _bookService.ImportFromXmlAsync("TestFilePath");

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo("Dune"));
            Assert.That(result[1].Title, Is.EqualTo("1984"));
        }

        [Test]
        public async Task ImportFromXmlAsync_EmptyFile_ReturnsEmptyList()
        {
            _bookRepositoryMoq.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Book>());

            var result = await _bookService.ImportFromXmlAsync("TestFilePath");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task ImportFromXmlAsync_CalledTwice_BooksAccumulate()
        {
            var firstBatch = new List<Book>
            {
                new() { Title = "Dune",  Author = "Frank Herbert", Pages = 412 }
            };
            var secondBatch = new List<Book>
            {
                new() { Title = "1984",  Author = "George Orwell", Pages = 328 }
            };

            _bookRepositoryMoq.SetupSequence(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(firstBatch)
                .ReturnsAsync(secondBatch);

            await _bookService.ImportFromXmlAsync("TestFilePath1");
            var result = await _bookService.ImportFromXmlAsync("TestFilePath2");

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task ImportFromXmlAsync_RepositoryThrows_ExceptionPropagatesAndCollectionUnchanged()
        {
            _bookRepositoryMoq.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new FileNotFoundException("File not found"));

            Assert.That(
                async () => await _bookService.ImportFromXmlAsync("TestFilePath"),
                Throws.InstanceOf<FileNotFoundException>());

            Assert.That(_bookService.GetAll(), Is.Empty);
        }
    }
}
