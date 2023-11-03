using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.DTO;
using TurnupAPI.Forms;
using TurnupAPI.Models;

namespace TurnupAPI.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Users, UserDTO>()
                .ForMember(dest => dest.FullName, opt =>
                {
                    opt.MapFrom(src => string.IsNullOrEmpty(src.FirstName) || string.IsNullOrEmpty(src.LastName)
                        ? string.Empty : GetFormattedFullName(src.FirstName, src.LastName));
                });
            CreateMap<Users, UserDataForm>();
            CreateMap<Artist, ArtistDTO>()
                .ForMember(dest => dest.FansNumber, opt =>
                {
                    opt.MapFrom(src => (src.UserFavoriteArtists != null && src.UserFavoriteArtists.Any() ? src.UserFavoriteArtists.Count : 0));

                })
                 .ForMember(dest => dest.TracksCount, opt =>
                 {
                     opt.MapFrom(src => (src.TrackArtists != null && src.TrackArtists.Any() ? src.TrackArtists.Count : 0));

                 });
            CreateMap<ArtistForm, Artist>()
                .ForMember(dest => dest.Picture, opt =>
                {
                    opt.MapFrom(src => $"picture/{src.Picture}");

                });
            CreateMap<Playlist, PlaylistDTO>()
                 .ForMember(dest => dest.OwnerId, opt =>
                 {
                     opt.MapFrom(src => src.UsersId);

                 })
                .ForMember(dest => dest.OwnerPicture, opt =>
                 {
                     opt.MapFrom(src => src.Users!.Picture);

                 })
                .ForMember(dest => dest.OwnerName, opt =>
                {
                    opt.MapFrom(src => string.IsNullOrEmpty(src.Users!.FirstName) || string.IsNullOrEmpty(src.Users.LastName)
                        ? string.Empty : GetFormattedFullName(src.Users.FirstName, src.Users.LastName));
                });
            CreateMap<Types, TypesDTO>().ReverseMap();
            CreateMap<Track, TrackDTO>().ReverseMap();
        
        }
        /// <summary>
        /// Formate le nom complet en mettant en majuscule la première lettre du prénom et du nom.
        /// </summary>
        /// <param name="firstname">Le prénom de l'utilisateur.</param>
        /// <param name="lastname">Le nom de l'utilisateur.</param>
        /// <returns>Le nom complet formaté.</returns>
        public  static string GetFormattedFullName(string firstname, string lastname)
        {
            string fullname = firstname.Substring(0, 1).ToUpper() + firstname.ToLower().Substring(1, firstname.Length - 1) + " " + lastname.Substring(0, 1).ToUpper() + lastname.ToLower().Substring(1, lastname.Length - 1);
            return fullname;
        }
    }
}
