using FulcrumSoftwareTestTask.Areas.Books.Services;
using FulcrumSoftwareTestTask.Repositories.Books;
using Microsoft.Extensions.DependencyInjection;

namespace FulcrumSoftwareTestTask.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBookServices(this IServiceCollection services)
    {
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddSingleton<IBookService, BookService>();

        return services;
    }

    public static IServiceCollection AddFulcrumSoftwareTestTask(this IServiceCollection services)
    {
        return services.AddBookServices();
    }
}
