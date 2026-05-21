using System;
using System.IO;

namespace MixVault_FrontEnd.Services
{
    public class AudioState
    {
        public string? CurrentTrackTitle { get; private set; }
        public string? CurrentTrackUrl { get; private set; }
        public bool IsPlaying { get; private set; }
        
        // NIEUW: Een unieke ID voor elke afspeel-actie
        public Guid PlayId { get; private set; } = Guid.NewGuid();

        public event Action? OnTrackChanged;

        public void PlayTrack(string title, string url)
        {
            CurrentTrackTitle = title;
            
            if (url.StartsWith("/"))
            {
                var fileName = Path.GetFileName(url);
                CurrentTrackUrl = $"https://localhost:7240/tracks/stream/{fileName}"; // Check je poort!
            }
            else
            {
                CurrentTrackUrl = url;
            }

            IsPlaying = true;
            
            // NIEUW: Genereer een nieuwe ID. Hierdoor weet Blazor dat het een "nieuwe" actie is
            PlayId = Guid.NewGuid();
            
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnTrackChanged?.Invoke();
    }
}