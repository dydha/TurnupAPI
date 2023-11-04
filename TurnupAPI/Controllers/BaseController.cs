using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;

namespace TurnupAPI.Controllers
{
   
    [ApiController]

    public class BaseController : ControllerBase
    {
        protected readonly IUserRepository _userRepository;
        protected readonly IArtistRepository _artistRepository;
        protected readonly ITrackRepository _trackRepository;
        protected readonly TurnupContext _context;
        protected readonly IMapper _mapper;
        public BaseController(
            IUserRepository userRepository,
             IArtistRepository artistRepository,
             ITrackRepository trackRepository,
             TurnupContext context,
             IMapper mapper
            ) 
        {
            _userRepository = userRepository;
            _artistRepository = artistRepository;
            _trackRepository = trackRepository;
            _context = context;
            _mapper = mapper;
        }
        protected string? GetLoggedUserEmail() => User.FindFirstValue(ClaimTypes.Email);
       
        /// <summary>
        /// Récucupère l'utilisateur connecté.
        /// </summary>
        /// <returns>retourne l'utilisateur connecté</returns>
        protected async Task<Users?> GetLoggedUserAsync()
        {
            var email = GetLoggedUserEmail();
            Users? user = null;
            if(!string.IsNullOrEmpty(email)) 
            {
                user = await _userRepository.GetLoggedUserAsync(email);
            }   
            return user;          
        }
        /// <summary>
        /// Récucupère l'id de l'utilisateur connecté.
        /// </summary>
        /// <returns>retourne l'utilisateur connecté</returns>
        protected async Task<string> GetLoggedUserIdAsync()
        {
            var email = GetLoggedUserEmail();
            var userId = string.Empty;
            if (!string.IsNullOrEmpty(email))
            {
                userId = await _userRepository.GetLoggedUserIdAsync(email);
            }
            return userId;

        }
        /// <summary>
        /// Convertit un en List.
        /// </summary>
        protected IEnumerable<ArtistDTO> MapToEnumerableArtistsDTO(IEnumerable<Artist> artists)
        {
            var artistsDTO = artists.Select(a => _mapper.Map<ArtistDTO>(a)).ToList();
            return artistsDTO;
        }
        /// <summary>
        /// Mappe un objet ArtistForm en un objet Artist.
        /// </summary>
        /// <param name="artistForm">Formulaire d'artiste.</param>
        /// <returns>Un objet Artist mappé à partir du formulaire.</returns>
        protected Artist MapToArtist(ArtistForm artistForm)
        {
            return new Artist
            {
                Name = artistForm.Name,
                Description = artistForm.Description,
                Country = artistForm.Country,
                Picture = $@"picture/{artistForm.Picture}"
            };
        }
        /// <summary>
        /// Convertit une List<Playlist> en List<PlaylistDTO>
        /// </summary>
        /// <returns>retourne une List<PlaylistDTO></returns>
        protected IEnumerable<PlaylistDTO> MapToEnumerablePlaylistDTO(IEnumerable<Playlist> playlists)
        {
            var playlistsDTO = playlists.Select(p => _mapper.Map<PlaylistDTO>(p)).ToList();
            return playlistsDTO;
        }
        

        /// <summary>
        /// Convertit une PlaylistForm en Playlist
        /// </summary>
        /// <returns>retourne une Playlist</returns>
        protected Playlist MapToPlaylist(PlaylistForm input, string userId)
        {
            var playlist = new Playlist()
            {
                Name = input.Name,
                IsPrivate = input.IsPrivate,
                UsersId = userId,
            };
            return playlist;
        }
        /// <summary>
        /// Convertit une List<Track> en List<TrackDTO>
        /// </summary>
        /// <returns>retourne une List<TrackDTO></returns>
        protected IEnumerable<TrackDTO> MapToEnumerableTrackDTO(IEnumerable<Track> tracks)
        {
            var tracksDTO = tracks.Select(t => MapToTrackDTO(t)).ToList();

            return tracksDTO;
        }

        /// <summary>
        /// Mappe un objet TrackForm en un objet Track.
        /// </summary>
        /// <param name="input">Formulaire de l'objet  Track.</param>
        /// <returns>Un objet Track mappé à partir du formulaire.</returns>
        protected Track MapToTrack(TrackForm input)
        {
            return new Track()
            {
                Title = input.Title,
                Minutes = input.Minutes,
                Seconds = input.Seconds,
                Source = $@"music/{input.Source}",
            };
        }
        /// <summary>
        /// Convertit une Track en TrackDTO
        /// </summary>
        /// <returns>retourne une TrackDTO</returns>
        protected TrackDTO MapToTrackDTO(Track track)
        {

            var trackDTO = new TrackDTO
            {
                Id = track.Id,
                Title = track.Title,
                Duration = new TimeSpan(0, track.Minutes, track.Seconds),
                Source = track.Source,
                ArtistName = _artistRepository.GetPrincipalArtistNameByTrackId(track.Id),
                ArtistPicture = _artistRepository.GetPrincipalArtistPictureByTrackId(track.Id),
                FeaturingArtists = _artistRepository.GetFeaturingArtistsNamesByTrackId(track.Id),
                ListeningCount = _trackRepository.GetTrackListeningNumber(track.Id),

            };
            return trackDTO;

        }
        /// <summary>
        /// Retourne le cacheKey de la dernière list de musique écoutée par l'utilisateur connecté.
        /// </summary>
        /// <returns>etourne le cacheKey de la dernière list de musique écoutée par l'utilisateur connecté</returns>
        protected string CacheKeyForUserLastPlayingTracks(string userId) => $"lastplayingtracks_{userId}"; //Methode qui génère le cacheKey pour la dernière liste d'écoute  d'un utilisateur.
        protected string CacheKeyForUserFavoriteTracks(string userId) => $"favoritetracks_{userId}"; //Methode qui génère le cacheKey pour les tracks favoris  d'un utilisateur.
        protected string CacheKeyForPopularTracks() => $"populartracks"; //Methode qui génère le cacheKey pour les tracks favoris  d'un utilisateur.
        protected string CacheKeyForDiscoveryTracks() => $"discoverytracks"; //Methode qui génère le cacheKey pour les tracks favoris  d'un utilisateur.
        protected string CacheKeyForNewTracks() => $"newtracks"; //Methode qui génère le cacheKey pour les tracks favoris  d'un utilisateur.
        protected string CacheKeyForTop20Tracks() => $"top20tracks"; //Methode qui génère le cacheKey pour les tracks favoris  d'un utilisateur.
        protected string CacheKeyForTypesTracks(int typesId) => $"tracksfortype_{typesId}"; //Methode qui génère le cacheKey pour les tracks favoris  d'un utilisateur.
        protected string CacheKeyForTypes() => $"types"; //Methode qui génère le cacheKey pour les types.
        protected string CacheKeyForArtistTracks(int artistId) => $"tracksforartist_{artistId}"; //Methode qui génère le cacheKey pour les tracks favoris  d'un utilisateur.
        protected string CacheKeyForArtists() => $"artists"; //Methode qui génère le cacheKey pour les types.

        /// <summary>
        /// Désérialise les données.
        /// </summary>
        /// <returns> Désérialise les données.</returns>
        protected T DeserializeData<T>(byte[] data)
        {
            var jsonString = Encoding.UTF8.GetString(data);
            var deserializedData = JsonConvert.DeserializeObject<T>(jsonString);
            return deserializedData!;

        }
        /// <summary>
        /// Sérialise les données.
        /// </summary>
        /// <returns> Sérialise les données.</returns>
        protected byte[] SerializeData<T>(T data)
        {

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var jsonString = JsonConvert.SerializeObject(data, settings);
            var serializedData = Encoding.UTF8.GetBytes(jsonString);
            return serializedData;
        }
        /// <summary>
        ///Methode d'option pour les données mises en cache.
        /// </summary>
        /// <returns> Methode d'option pour les données mises en DistributedCache.</returns>
        protected DistributedCacheEntryOptions GetCacheOptions()
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            };

            return cacheOptions;
        }

        /// <summary>
        ///Methode d'option pour les données mises en MemoryCache.
        /// </summary>
        /// <returns> Methode d'option pour les données mises en cache.</returns>
        protected MemoryCacheEntryOptions GetMemoryCacheOptions()
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            };

            return cacheOptions;
        }
        protected static bool LoggedUserNotAuthorizedToSeeThisPlaylist(Playlist playlist, string loggedUserId)
        {
            return playlist.IsPrivate && playlist.UsersId != loggedUserId;
        }
    }
   
}
