namespace MixVault_FrontEnd.Services
{
    public class Userstate
    {
        public int UserID { get; set; }
        public string? DisplayName { get; set; }
        public bool IsLoggedIn { get; set; }

        // 1. Dit event is de 'megafoon' die andere pagina's waarschuwt
        public event Action? OnChange;

        public void LogIn(int userId, string displayName)
        {
            UserID = userId;
            DisplayName = displayName;
            IsLoggedIn = true;
            NotifyStateChanged(); // 2. Stuur het seintje
        }

        public void LogOut()
        {
            UserID = 0;
            DisplayName = null;
            IsLoggedIn = false;
            NotifyStateChanged(); // 2. Stuur het seintje
        }

        // 3. De methode die de megafoon activeert
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}