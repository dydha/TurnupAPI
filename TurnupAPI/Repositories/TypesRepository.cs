using Microsoft.EntityFrameworkCore;
using TurnupAPI.Data;
using TurnupAPI.Exceptions;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Repositories
{
    /// <summary>
    /// Implémentation du repository pour la gestion des types.
    /// </summary>
    public class TypesRepository : ITypesRepository
    {
        private readonly TurnupContext _context;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="TypesRepository"/>.
        /// </summary>
        /// <param name="context">Le contexte de la base de données.</param>
        public TypesRepository(TurnupContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ajoute un type à la base de données.
        /// </summary>
        /// <param name="type">Le type à ajouter.</param>
        /// <returns>Une tâche asynchrone.</returns>
        public async Task AddAsync(Types type)
        {
            _context.Types.Add(type);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Supprime un type de la base de données par son ID.
        /// </summary>
        /// <param name="id">L'ID du type à supprimer.</param>
        /// <returns>Une tâche asynchrone.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            bool result = false;
            var types = await GetAsync(id);
            if(types is not null)
            {
                _context.Types.Remove(types);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Récupère un type de la base de données par son ID.
        /// </summary>
        /// <param name="id">L'ID du type à récupérer.</param>
        /// <returns>Le type récupéré.</returns>
        public async Task<Types?> GetAsync(int id)
        {
            var types = await _context.Types.FirstOrDefaultAsync(t => t.Id == id);
            return types;
        }

        /// <summary>
        /// Récupère tous les types de la base de données.
        /// </summary>
        /// <returns>Une liste de tous les types.</returns>
        public async Task<IEnumerable<Types>> GetAllAsync()
        {
            var typesList = await _context.Types.AsNoTracking().ToListAsync();
            return typesList is not null && typesList.Any() ? typesList : Enumerable.Empty<Types>();
        }

        /// <summary>
        /// Met à jour un type dans la base de données.
        /// </summary>
        /// <param name="type">Le type à mettre à jour.</param>
        /// <returns>Une tâche asynchrone.</returns>
        public async Task<bool> UpdateAsync(Types type)
        {
            bool result = false;
            var existingTypes = await _context.Types.FirstOrDefaultAsync(t => t.Id == type.Id);
            if (existingTypes is not null)
            {
                _context.Types.Update(type);
                await _context.SaveChangesAsync();
                result = true;
            }
            return result;
        }
    }
}
