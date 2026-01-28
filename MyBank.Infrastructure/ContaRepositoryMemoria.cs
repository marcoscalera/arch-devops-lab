using Microsoft.EntityFrameworkCore;
using MyBank.Domain;

namespace MyBank.Infrastructure;

public class ContaRepositoryEF : IContaRepository
{
    private readonly AppDbContext _context;

    public ContaRepositoryEF(AppDbContext context)
    {
        _context = context;
    }

    public Conta? GetById(int id)
    {
        return _context.Contas.FirstOrDefault(c => c.Id == id);
    }

    public void Update(Conta conta)
    {
        _context.Contas.Update(conta);
        _context.SaveChanges();
    }
}