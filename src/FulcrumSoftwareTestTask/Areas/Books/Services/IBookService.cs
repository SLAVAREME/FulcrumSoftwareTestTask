namespace FulcrumSoftwareTestTask.Areas.Books.Services
{
    public interface IBookService
    {
        Task<IReadOnlyList<Book>> ImportFromXmlAsync(string filePath, CancellationToken cancellationToken = default);

        Task ExportToXmlAsync(string filePath, CancellationToken ct = default);

        Task<Book> AddAsync(Book book, CancellationToken ct = default);

        Task<IReadOnlyList<Book>> GetSortedAsync(CancellationToken ct = default);

        Task<IReadOnlyList<Book>> SearchByPartTitleAsync(string partTitle, CancellationToken ct = default);

        IReadOnlyList<Book> GetAll();
    }
}
