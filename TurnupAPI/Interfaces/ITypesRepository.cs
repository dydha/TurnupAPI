using System.Collections.Generic;
using System.Threading.Tasks;
using TurnupAPI.Models;

namespace TurnupAPI.Interfaces
{
    /// <summary>
    /// Interface pour la gestion des opérations CRUD (Create, Read, Update, Delete) sur les types de musique.
    /// </summary>
    public interface ITypesRepository
    {
        /// <summary>
        /// Ajoute un type de musique.
        /// </summary>
        /// <param name="type">L'objet Types à ajouter.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task AddAsync(Types type);

        /// <summary>
        /// Récupère un type de musique en fonction de son ID.
        /// </summary>
        /// <param name="id">L'ID du type de musique à récupérer.</param>
        /// <returns>Une tâche asynchrone qui renvoie l'objet Types correspondant à l'ID spécifié.</returns>
        Task<Types> GetAsync(int id);

        /// <summary>
        /// Supprime un type de musique en fonction de son ID.
        /// </summary>
        /// <param name="id">L'ID du type de musique à supprimer.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Met à jour les informations d'un type de musique.
        /// </summary>
        /// <param name="type">L'objet Types contenant les informations mises à jour.</param>
        /// <returns>Une tâche asynchrone.</returns>
        Task UpdateAsync(Types type);

        /// <summary>
        /// Récupère tous les types de musique disponibles.
        /// </summary>
        /// <returns>Une tâche asynchrone qui renvoie une liste de tous les types de musique.</returns>
        Task<List<Types>> GetAllAsync();
    }
}
