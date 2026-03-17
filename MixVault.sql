-- Maak de database schoon aan
DROP SCHEMA IF EXISTS MixVault;
CREATE SCHEMA IF NOT EXISTS MixVault;
USE MixVault;

-- 1. User tabel (Nu met profielfoto)
CREATE TABLE IF NOT EXISTS User (
  UserID int auto_increment not null, 
  DisplayName mediumtext NULL,
  Email mediumtext NULL,
  Password mediumtext NULL,
  ProfilePictureURL varchar(255) NULL, -- Link naar de profielfoto
  UserCreatedAt DATETIME NULL,
  PRIMARY KEY (UserID)
);
  
-- 2. Track tabel (Nu met mp3-bestand, artwork en uploader)
CREATE TABLE IF NOT EXISTS Track (
  TrackID int auto_increment not null, 
  Title mediumtext NULL,
  DurationMs double NULL,
  TrackFileURL varchar(255) NULL,      -- Link naar het .mp3 of .wav bestand
  TrackGenre mediumtext NULL,
  TrackTags mediumtext NULL,
  TrackArtworkURL varchar(255) NULL,   -- Link naar de albumhoes
  TrackUploadedAt DATETIME NULL,
  fkUser int not null,                 -- De uploader van de track
  PRIMARY KEY (TrackID),
  FOREIGN KEY (fkUser) REFERENCES User(UserID)
);
  
-- 3. Playlist tabel (Nu met artwork en maker)
CREATE TABLE IF NOT EXISTS Playlist (
  PlaylistID int auto_increment not null, 
  PlaylistName mediumtext NULL,
  PlaylistDescription mediumtext NULL,
  PlaylistGenre mediumtext Null,
  PlaylistTags mediumtext Null,
  PlaylistArtworkURL varchar(255) NULL, -- Link naar de playlist afbeelding
  PlaylistCreatedAt DATETIME NULL,
  fkUser int not null,                  -- De maker van de playlist
  PRIMARY KEY (PlaylistID),
  FOREIGN KEY (fkUser) REFERENCES User(UserID)
);
  
-- 4. Koppeltabel: UserLikesTrack
CREATE TABLE IF NOT EXISTS UserLikesTrack (
    UserLikesTrackID int auto_increment not null primary key,
    fkUser int not null,
    fkTrack int not null,
    FOREIGN KEY (fkUser) REFERENCES User(UserID),
    FOREIGN KEY (fkTrack) REFERENCES Track(TrackID)
);

-- 5. Koppeltabel: UserLikesPlaylist
CREATE TABLE IF NOT EXISTS UserLikesPlaylist (
    UserLikesPlaylistID int auto_increment not null primary key,
    fkUser int not null,
    fkPlaylist int not null,
    FOREIGN KEY (fkUser) REFERENCES User(UserID),
    FOREIGN KEY (fkPlaylist) REFERENCES Playlist(PlaylistID)
);

-- 6. Koppeltabel: PlaylistHasTracks
CREATE TABLE IF NOT EXISTS PlaylistHasTracks (
    PlaylistHasTracksID int auto_increment not null primary key,
    Position int not null,
    fkTrack int not null,
    fkPlaylist int not null,
    FOREIGN KEY (fkTrack) REFERENCES Track(TrackID),
    FOREIGN KEY (fkPlaylist) REFERENCES Playlist(PlaylistID)
);