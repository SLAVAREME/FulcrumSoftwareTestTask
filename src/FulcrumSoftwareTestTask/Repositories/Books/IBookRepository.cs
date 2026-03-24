using FulcrumSoftwareTestTask.Areas.Books;

namespace FulcrumSoftwareTestTask.Repositories.Books
{
    public interface IBookRepository
    {
        Task<IReadOnlyList<Book>> LoadAsync(string filePath, CancellationToken ct = default);

        Task SaveAsync(string filePath, IEnumerable<Book> books, CancellationToken ct = default);
    }
}
