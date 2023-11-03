using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TurnupAPI.Models;

namespace TurnupAPI.Areas.Identity.Data;

// Add profile data for application users by adding properties to the Users class
public class Users : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Gender { get; set; } = null!;
    public DateTime  Birthdate { get; set; }
    public byte[]? Picture { get; set; }
    public bool IsDarkTheme { get; set; } = false;
    //-------------RELATION--------------------------
    public List<UserListennedTrack>? UserListennedTracks { get; set; }
    public List<UserFavoriteTrack>? UserFavoriteTracks { get; set; }
    public List<UserFavoriteArtist>? UserFavoriteArtists { get; set; }
    public List<UserFavoritePlaylist>? UserFavoritePlaylists { get; set; }
    public List<Playlist>? Playlists { get; set; }
}

