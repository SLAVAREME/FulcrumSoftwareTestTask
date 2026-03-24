using FulcrumSoftwareTestTask.Repositories.Books;

namespace FulcrumSoftwareTestTask.Areas.Books.Services
{
    public class BookService : IBookService
    {
        private readonly List<Book> _books = new();
        private readonly IBookRepository _repository;
        private readonly ReaderWriterLockSlim _lock = new();

        public BookService(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task ExportToXmlAsync(string filePath, CancellationToken ct = default)
        {
            IReadOnlyList<Book> snapshot;

            _lock.EnterReadLock();

            try
            {
                snapshot = _books.ToList().AsReadOnly();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            await _repository.SaveAsync(filePath, snapshot, ct);
        }

        public async Task<IReadOnlyList<Book>> ImportFromXmlAsync(string filePath, CancellationToken ct = default)
        {
            var books = await _repository.LoadAsync(filePath, ct);

            _lock.EnterWriteLock();

            try
            {
                _books.AddRange(books);

                return _books.ToList().AsReadOnly();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Task<Book> AddAsync(Book book, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(book);

            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Title is required", nameof(book));
            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Author is required", nameof(book));
            if (book.Pages <= 0)
                throw new ArgumentException("Pages must be positive", nameof(book));

            _lock.EnterWriteLock();

            try
            {
                _books.Add(book);
                return Task.FromResult(book);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IReadOnlyList<Book> GetAll()
        {
            return _books.AsReadOnly();
        }

        public Task<IReadOnlyList<Book>> SearchByPartTitleAsync(string partTitle, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(partTitle))
                throw new ArgumentException("Search title is required", nameof(partTitle));

            _lock.EnterReadLock();

            try
            {
                return Task.FromResult<IReadOnlyList<Book>>(
                    _books
                        .Where(b => b.Title.Contains(
                            partTitle,
                            StringComparison.OrdinalIgnoreCase))
                        .ToList()
                        .AsReadOnly()
                );
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Task<IReadOnlyList<Book>> GetSortedAsync(CancellationToken ct = default)
        {
            _lock.EnterReadLock();

            try
            {
                return Task.FromResult<IReadOnlyList<Book>>(
                    _books
                        .OrderBy(b => b.Author, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(b => b.Title, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                        .AsReadOnly()
                );
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
