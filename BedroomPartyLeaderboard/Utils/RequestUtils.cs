using BeatSaberMarkupLanguage;
using BedroomPartyLeaderboard.UI.Leaderboard;
using IPA.Utilities.Async;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BedroomPartyLeaderboard.Utils
{
    internal class RequestUtils
    {
        [Inject] private readonly LeaderboardData _leaderboardData;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly UIUtils _uiUtils;
        [Inject] private readonly AuthenticationManager _authenticationManager;

        public async Task GetLeaderboardData((string, int, string) balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int)> callback)
        {
            using HttpClient httpClient = new();
            try
            {
                string requestString = getLBDownloadJSON(balls, page, _leaderboardView.sortMethod);

                HttpResponseMessage response = await httpClient.GetAsync(requestString);

                int scorecount = 0;
                int totalPages = 0;
                List<LeaderboardData.LeaderboardEntry> data = new();

                if (!response.IsSuccessStatusCode)
                {
                    callback((false, data, 0));
                    return;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                LeaderboardData.BPLeaderboard leaderboardData = JsonConvert.DeserializeObject<LeaderboardData.BPLeaderboard>(jsonResponse);
                scorecount = leaderboardData.scoreCount;
                totalPages = Mathf.CeilToInt((float)scorecount / 10);
                data = leaderboardData.scores;

                callback((true, data, totalPages));
                return;
            }
            catch (HttpRequestException e)
            {
                Plugin.Log.Error("EXCEPTION: " + e.ToString());
                callback((false, null, 0));
            }
        }

        private string getLBDownloadJSON((string, int, string) balls, int page, string sort)
        {
            string Data = $"{Constants.LEADERBOARD_DOWNLOAD_END_POINT(balls.Item1)}?char={balls.Item3}&diff={balls.Item2}&sort={sort}&limit=10&page={page}&id={_authenticationManager._localPlayerInfo.userID}";
            return Data;
        }

        public void GetBeatMapData((string, int, string) balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int)> callback)
        {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => GetLeaderboardData(balls, page, callback));
        }

        public void SetBeatMapData(string uploadJson, Action<bool> callback)
        {
            _leaderboardView.hasClickedOffResultsScreen = false;
            UnityMainThreadTaskScheduler.Factory.StartNew(() => UploadLeaderboardData(uploadJson, callback));
        }


        internal bool isUploading = false;
        private async Task UploadLeaderboardData(string balls, Action<bool> callback)
        {

            if (DateTime.Now.Millisecond > _authenticationManager._localPlayerInfo.sessionExpiry) return;
            _leaderboardView.hasClickedOffResultsScreen = false;
            using HttpClient httpClient = new();
            int x = 0;
            isUploading = true;
            while (x < 3)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _authenticationManager._localPlayerInfo.tempKey);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    HttpContent content = new StringContent(balls, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(Constants.LEADERBOARD_UPLOAD_END_POINT(balls), content);

                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        callback(response.IsSuccessStatusCode);
                        isUploading = false;
                        break;
                    }

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        callback(response.IsSuccessStatusCode);
                        isUploading = false;
                        break;
                    }
                }
                catch (HttpRequestException e)
                {
                    Plugin.Log.Error("EXCEPTION: " + e.ToString());
                    callback(false);
                }
                x++;
            }
            if (x == 2)
            {
                callback(false);
                await Task.Delay(3000);
            }
        }

        internal async Task HandleLBAuth()
        {
            if (!_authenticationManager.IsAuthed)
            {

                UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("Authenticating...", true, true, 0));
                try
                {
                    await Constants.WaitUntil(() => _authenticationManager.IsAuthed, timeout: 60000);
                }
                catch (TimeoutException)
                {
                    _leaderboardView.SetErrorState(true, "Failed to Auth");
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("", false, false, 0));
                }
            }

            UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("<color=green>Successfully signed in!</color>", true, false, 10000));
            _panelView.playerUsername.text = _authenticationManager._localPlayerInfo.username;

            SharedCoroutineStarter.instance.StartCoroutine(UIUtils.GetSpriteAvatar($"{Constants.USER_URL_API(_authenticationManager._localPlayerInfo.userID)}/avatar", (Sprite a, string b) => _panelView.playerAvatar.sprite = a, (string a, string b) => _panelView.playerAvatar.sprite = Utilities.FindSpriteInAssembly("BedroomPartyLeaderboard.Images.Player.png"), new CancellationToken()));
            _panelView.playerAvatarLoading.gameObject.SetActive(false);

            UnityMainThreadTaskScheduler.Factory.StartNew(() => _leaderboardView.SetSeasonList(1));
            UnityMainThreadTaskScheduler.Factory.StartNew(() => Task.Run(() => _uiUtils.assignStaff()));

            await Constants.WaitUntil(() => _leaderboardView.currentDifficultyBeatmap != null);
            _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
            return;
        }


        internal async Task HandleLBUpload()
        {
            if (isUploading)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("Uploading Score...", true, true, 0));
                try
                {
                    await Constants.WaitUntil(() => !isUploading, timeout: 60000);
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("<color=green>Successfully uploaded score!</color>", true, false, 5500));
                }
                catch (TimeoutException)
                {
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("<color=red>Failed to upload...</color>", true, false, 7500));
                }
            }
            await Constants.WaitUntil(() => _leaderboardView.hasClickedOffResultsScreen);
            await Task.Delay(100);
            _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
            return;
        }
    }
}
