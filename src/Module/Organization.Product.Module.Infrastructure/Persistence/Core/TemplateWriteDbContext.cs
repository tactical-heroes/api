using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Organization.Product.Module.Infrastructure.Persistence.Core;

public sealed class TemplateWriteDbContext(
    DbContextOptions<TemplateWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<TemplateWriteDbContext>(options, interceptors);
