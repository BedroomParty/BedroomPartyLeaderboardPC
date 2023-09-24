using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class AuthenticationManager
    {
        [Inject] private readonly PlayerUtils _playerUtils;
        [Inject] private readonly SiraLog _log;
        internal PlayerUtils.PlayerInfo _localPlayerInfo;
        private bool _isAuthed = false;
        internal bool _currentlyAuthing = false;

        public bool IsAuthed => _isAuthed;

        public async Task<bool> AuthenticateAsync()
        {
            _log.Info("Authenticating");
            if (_currentlyAuthing)
            {
                return false;
            }

            try
            {
                _currentlyAuthing = true;

                _localPlayerInfo = await _playerUtils.GetPlayerInfoAsync().Result;

                if (_localPlayerInfo.authKey == null)
                {
                    _log.Error("AUTH KEY NULL");
                    _isAuthed = false;
                    return false;
                }

                using HttpClient httpClient = new();
                int attempt = 0;
                while (attempt < 3)
                {
                    try
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _localPlayerInfo.authKey);
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        string requestBody = _playerUtils.GetLoginString(_localPlayerInfo.userID);
                        HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(Constants.AUTH_END_POINT, content);
                        _isAuthed = response.StatusCode == HttpStatusCode.OK;

                        string responseContent = await response.Content.ReadAsStringAsync();

                        PlayerResponse playerResponse = JsonConvert.DeserializeObject<PlayerResponse>(responseContent);
                        if (playerResponse != null)
                        {
                            _localPlayerInfo.tempKey = playerResponse.sessionKey;
                            _localPlayerInfo.username = playerResponse.username;
                            _localPlayerInfo.userID = playerResponse.gameID;
                            _localPlayerInfo.discordID = playerResponse.discordID;
                            _localPlayerInfo.sessionExpiry = playerResponse.sessionKeyExpires;
                        }
                        else
                        {
                            throw new Exception("Error Authenticating, RESTART GAME.");
                        }
                        if (_isAuthed)
                        {
                            _currentlyAuthing = false;
                            return true;
                        }
                        await Task.Delay(500);
                        attempt++;
                    }
                    catch (HttpRequestException)
                    {
                        _log.Error("Error Authenticating, retrying...");
                        attempt++;
                        await Task.Delay(5000);
                    }
                    attempt++;
                }
                if (attempt < 2)
                {
                    _log.Error("Error Authenticating");
                }
                _currentlyAuthing = false;
                return false;
            }
            catch (Exception e)
            {
                _log.Error(e);
                _currentlyAuthing = false;
                return false;
            }
        }

        public async Task LoginUserAsync()
        {
            try
            {
                if (await AuthenticateAsync())
                {
                    _log.Info("authed");
                }
                else
                {
                    _log.Error("Not authenticated!");
                }
            }
            catch (Exception ex)
            {
                _log.Error("LoginUserAsync failed: " + ex.Message);
            }
        }
    }
}
