-- STAP 1: Gebruikers (Users) toevoegen
-- We maken 3 gebruikers aan. Ze krijgen automatisch UserID 1, 2 en 3.
INSERT INTO User (DisplayName, Email, Password, UserCreatedAt) 
VALUES 
('DJ_Spark', 'spark@email.com', 'geheim123', NOW()),
('MusicLover99', 'lover@email.com', 'wachtwoord!', NOW()),
('BeatsBySam', 'sam@muziek.nl', 'sam123', NOW());

-- STAP 2: Nummers (Tracks) toevoegen
-- Let op de fkUser kolom aan het einde! Dit is de uploader.
-- DJ_Spark (UserID 1) uploadt 2 nummers, BeatsBySam (UserID 3) uploadt er 1.
INSERT INTO Track (Title, DurationMs, TrackGenre, TrackTags, TrackUploadedAt, fkUser) 
VALUES 
('Summer Vibes', 180000, 'House', 'summer, upbeat, party', NOW(), 1),
('Chill Lo-Fi', 210000, 'Lo-Fi', 'chill, relax, study', NOW(), 1),
('Bass Drop', 195000, 'EDM', 'electronic, bass, dance', NOW(), 3);

-- STAP 3: Afspeellijsten (Playlists) toevoegen
-- Ook hier gebruiken we fkUser om aan te geven wie de maker is.
-- MusicLover99 (UserID 2) maakt twee afspeellijsten aan.
INSERT INTO Playlist (PlaylistName, PlaylistDescription, PlaylistGenre, PlaylistTags, PlaylistCreatedAt, fkUser)
VALUES 
('Zomer Hits', 'De beste nummers voor op het strand', 'House', 'zomer, party', NOW(), 2),
('Studie Muziek', 'Rustige achtergrondmuziek', 'Lo-Fi', 'focus, relax', NOW(), 2);

-- STAP 4: Relaties leggen (Koppeltabellen vullen)

-- 4a. Nummers aan een afspeellijst toevoegen (PlaylistHasTracks)
-- We zetten 'Summer Vibes' (TrackID 1) op positie 1 in 'Zomer Hits' (PlaylistID 1)
-- En 'Bass Drop' (TrackID 3) op positie 2 in dezelfde afspeellijst
INSERT INTO PlaylistHasTracks (Position, fkTrack, fkPlaylist)
VALUES 
(1, 1, 1),
(2, 3, 1);

-- 4b. Gebruikers liken nummers (UserLikesTrack)
-- 'MusicLover99' (UserID 2) liket het nummer 'Chill Lo-Fi' (TrackID 2)
INSERT INTO UserLikesTrack (fkUser, fkTrack)
VALUES 
(2, 2);

-- 4c. Gebruikers liken afspeellijsten (UserLikesPlaylist)
-- 'BeatsBySam' (UserID 3) liket de afspeellijst 'Zomer Hits' (PlaylistID 1)
INSERT INTO UserLikesPlaylist (fkUser, fkPlaylist)
VALUES 
(3, 1);