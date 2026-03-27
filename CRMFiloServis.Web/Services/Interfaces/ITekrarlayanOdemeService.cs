using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface ITekrarlayanOdemeService
{
    Task<List<TekrarlayanOdeme>> GetAllAsync();
    Task<TekrarlayanOdeme?> GetByIdAsync(int id);
    Task<TekrarlayanOdeme> CreateAsync(TekrarlayanOdeme odeme);
    Task<TekrarlayanOdeme> UpdateAsync(TekrarlayanOdeme odeme);
    Task DeleteAsync(int id);
    Task<List<TekrarlayanOdeme>> GetAktifOdemelerAsync();
    Task<List<TekrarlayanOdeme>> GetYaklasanOdemelerAsync(int gunSayisi = 7);
}
