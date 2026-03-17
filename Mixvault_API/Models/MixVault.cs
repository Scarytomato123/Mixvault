using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Mixvault_API.Models;

public partial class MixVault : DbContext
{
    public MixVault()
    {
    }

    public MixVault(DbContextOptions<MixVault> options)
        : base(options)
    {
    }

    public virtual DbSet<Playlist> Playlists { get; set; }

    public virtual DbSet<Playlisthastrack> Playlisthastracks { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userlikesplaylist> Userlikesplaylists { get; set; }

    public virtual DbSet<Userlikestrack> Userlikestracks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=MixVault;user=root;password=1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.PlaylistId).HasName("PRIMARY");

            entity.ToTable("playlist");

            entity.HasIndex(e => e.FkUser, "fkUser");

            entity.Property(e => e.PlaylistId).HasColumnName("PlaylistID");
            entity.Property(e => e.FkUser).HasColumnName("fkUser");
            entity.Property(e => e.PlaylistArtworkUrl)
                .HasMaxLength(255)
                .HasColumnName("PlaylistArtworkURL");
            entity.Property(e => e.PlaylistCreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PlaylistDescription).HasColumnType("mediumtext");
            entity.Property(e => e.PlaylistGenre).HasColumnType("mediumtext");
            entity.Property(e => e.PlaylistName).HasColumnType("mediumtext");
            entity.Property(e => e.PlaylistTags).HasColumnType("mediumtext");

            entity.HasOne(d => d.FkUserNavigation).WithMany(p => p.Playlists)
                .HasForeignKey(d => d.FkUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playlist_ibfk_1");
        });

        modelBuilder.Entity<Playlisthastrack>(entity =>
        {
            entity.HasKey(e => e.PlaylistHasTracksId).HasName("PRIMARY");

            entity.ToTable("playlisthastracks");

            entity.HasIndex(e => e.FkPlaylist, "fkPlaylist");

            entity.HasIndex(e => e.FkTrack, "fkTrack");

            entity.Property(e => e.PlaylistHasTracksId).HasColumnName("PlaylistHasTracksID");
            entity.Property(e => e.FkPlaylist).HasColumnName("fkPlaylist");
            entity.Property(e => e.FkTrack).HasColumnName("fkTrack");

            entity.HasOne(d => d.FkPlaylistNavigation).WithMany(p => p.Playlisthastracks)
                .HasForeignKey(d => d.FkPlaylist)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playlisthastracks_ibfk_2");

            entity.HasOne(d => d.FkTrackNavigation).WithMany(p => p.Playlisthastracks)
                .HasForeignKey(d => d.FkTrack)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playlisthastracks_ibfk_1");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.TrackId).HasName("PRIMARY");

            entity.ToTable("track");

            entity.HasIndex(e => e.FkUser, "fkUser");

            entity.Property(e => e.TrackId).HasColumnName("TrackID");
            entity.Property(e => e.FkUser).HasColumnName("fkUser");
            entity.Property(e => e.Title).HasColumnType("mediumtext");
            entity.Property(e => e.TrackArtworkUrl)
                .HasMaxLength(255)
                .HasColumnName("TrackArtworkURL");
            entity.Property(e => e.TrackFileUrl)
                .HasMaxLength(255)
                .HasColumnName("TrackFileURL");
            entity.Property(e => e.TrackGenre).HasColumnType("mediumtext");
            entity.Property(e => e.TrackTags).HasColumnType("mediumtext");
            entity.Property(e => e.TrackUploadedAt).HasColumnType("datetime");

            entity.HasOne(d => d.FkUserNavigation).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.FkUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("track_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.DisplayName).HasColumnType("mediumtext");
            entity.Property(e => e.Email).HasColumnType("mediumtext");
            entity.Property(e => e.Password).HasColumnType("mediumtext");
            entity.Property(e => e.ProfilePictureUrl)
                .HasMaxLength(255)
                .HasColumnName("ProfilePictureURL");
            entity.Property(e => e.UserCreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Userlikesplaylist>(entity =>
        {
            entity.HasKey(e => e.UserLikesPlaylistId).HasName("PRIMARY");

            entity.ToTable("userlikesplaylist");

            entity.HasIndex(e => e.FkPlaylist, "fkPlaylist");

            entity.HasIndex(e => e.FkUser, "fkUser");

            entity.Property(e => e.UserLikesPlaylistId).HasColumnName("UserLikesPlaylistID");
            entity.Property(e => e.FkPlaylist).HasColumnName("fkPlaylist");
            entity.Property(e => e.FkUser).HasColumnName("fkUser");

            entity.HasOne(d => d.FkPlaylistNavigation).WithMany(p => p.Userlikesplaylists)
                .HasForeignKey(d => d.FkPlaylist)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userlikesplaylist_ibfk_2");

            entity.HasOne(d => d.FkUserNavigation).WithMany(p => p.Userlikesplaylists)
                .HasForeignKey(d => d.FkUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userlikesplaylist_ibfk_1");
        });

        modelBuilder.Entity<Userlikestrack>(entity =>
        {
            entity.HasKey(e => e.UserLikesTrackId).HasName("PRIMARY");

            entity.ToTable("userlikestrack");

            entity.HasIndex(e => e.FkTrack, "fkTrack");

            entity.HasIndex(e => e.FkUser, "fkUser");

            entity.Property(e => e.UserLikesTrackId).HasColumnName("UserLikesTrackID");
            entity.Property(e => e.FkTrack).HasColumnName("fkTrack");
            entity.Property(e => e.FkUser).HasColumnName("fkUser");

            entity.HasOne(d => d.FkTrackNavigation).WithMany(p => p.Userlikestracks)
                .HasForeignKey(d => d.FkTrack)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userlikestrack_ibfk_2");

            entity.HasOne(d => d.FkUserNavigation).WithMany(p => p.Userlikestracks)
                .HasForeignKey(d => d.FkUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userlikestrack_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
