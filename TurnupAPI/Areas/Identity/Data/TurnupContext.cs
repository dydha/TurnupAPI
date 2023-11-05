using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TurnupAPI.Areas.Identity.Data;
using TurnupAPI.Models;

namespace TurnupAPI.Data;

public class TurnupContext : IdentityDbContext<Users>
{
    public DbSet<Track> Track { get; set; }
    public DbSet<TrackType> TrackType { get; set; }
    public DbSet<TrackArtist> TrackArtist { get; set; }
    public DbSet<Artist> Artist { get; set; }
    public DbSet<ArtistAlbum> ArtistAlbum { get; set; }
    public DbSet<Album> Album{ get; set; }
    public DbSet<Types> Types{ get; set; }
    public DbSet<Playlist> Playlist{ get; set; }
    public DbSet<PlaylistTrack> PlaylistTrack{ get; set; }
    public DbSet<UserFavoriteArtist> UserFavoriteArtist { get; set; }
    public DbSet<UserFavoriteTrack> UserFavoriteTrack { get; set; }
    public DbSet<UserFavoritePlaylist> UserFavoritePlaylist{ get; set; }
    public DbSet<UserListennedTrack> UserListennedTrack { get; set; }
    public TurnupContext(DbContextOptions<TurnupContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Users>().ToTable(nameof(Users), tableBuilder => tableBuilder.IsTemporal());
       
    }
    
    
}
