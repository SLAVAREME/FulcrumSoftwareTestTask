using FulcrumSoftwareTestTask.Areas.Books;
using System.Xml.Serialization;

namespace FulcrumSoftwareTestTask.Repositories.Books
{
    public class BookRepository : IBookRepository
    {
        public async Task<IReadOnlyList<Book>> LoadAsync(string filePath, CancellationToken ct = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("XML file not found", filePath);

            var content = await File.ReadAllTextAsync(filePath, ct);

            var serializer = new XmlSerializer(typeof(List<Book>));
            using var reader = new StringReader(content);

            var books = (List<Book>?)serializer.Deserialize(reader)
                        ?? new List<Book>();

            return books.AsReadOnly();
        }

        public async Task SaveAsync(string filePath, IEnumerable<Book> books, CancellationToken ct = default)
        {
            var serializer = new XmlSerializer(typeof(List<Book>));
            using var writer = new StringWriter();
            serializer.Serialize(writer, books.ToList());

            await File.WriteAllTextAsync(filePath, writer.ToString(), ct);
        }
    }
}
