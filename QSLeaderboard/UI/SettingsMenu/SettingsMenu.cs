using BeatSaberMarkupLanguage.Attributes;
using BSMLSettings = BeatSaberMarkupLanguage.Settings.BSMLSettings;

namespace QSLeaderboard.UI.SettingsMenu
{
    internal class SettingsMenu : PersistentSingleton<SettingsMenu>
    {
        private string Username = "", Password = "";

        [UIValue("UsernameValue")]
        private string UsernameValue
        {
            get => Username;
            set => Username = value;
        }

        [UIValue("PasswordValue")]
        private string PasswordValue
        {
            get => Password;
            set => Password = value;
        }

        [UIAction("LoginAction")]
        private void Login()
        {
            // Add login function here;
        }

        internal void Init()
        {
            BSMLSettings.instance.AddSettingsMenu("QSLeaderboard", "QSLeaderboard.UI.SettingsMenu.Settings.bsml", this);
        }
    }
}
