using Microsoft.EntityFrameworkCore;

namespace Organization.Product.Module.Infrastructure.Persistence.Core;

public sealed class TemplateReadDbContext(
    DbContextOptions<TemplateReadDbContext> options)
    : ReadDbContext<TemplateReadDbContext>(options);
