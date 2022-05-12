using SampleDynamoBlogAspNetApi.Models;

namespace SampleDynamoBlogAspNetApi.Repositories
{
  public interface IBlogRepository
  {
    Task<List<Blog>> GetAll();
  }
}
