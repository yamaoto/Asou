using Microsoft.EntityFrameworkCore;

namespace Asou.EfCore;

public class DbContextResolver
{
    private readonly DbContext _dbContext;

    public DbContextResolver(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public DbContext GetContext()
    {
        return _dbContext;
    }
}
