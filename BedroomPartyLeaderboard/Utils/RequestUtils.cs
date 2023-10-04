using BeatSaberMarkupLanguage;
using BedroomPartyLeaderboard.UI.Leaderboard;
using IPA.Utilities.Async;
using Newtonsoft.Json;
using SiraUtil.Logging;
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
#pragma warning disable CS4014
        [Inject] private readonly LeaderboardData _leaderboardData;
        [Inject] private readonly LeaderboardView _leaderboardView;
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly UIUtils _uiUtils;
        [Inject] private readonly AuthenticationManager _authenticationManager;
        [Inject] private readonly SiraLog _log;

        public async Task GetLeaderboardData((string, int, string) balls, int page, Action<(bool, List<LeaderboardData.LeaderboardEntry>, int)> callback)
        {
            using HttpClient httpClient = new();
            try
            {
                string requestString = getLBDownloadJSON(balls, page, _leaderboardView.sortMethod);

                _log.Info("Getting Leaderboard Data");
                HttpResponseMessage response = await httpClient.GetAsync(requestString);

                int scorecount = 0;
                int totalPages = 0;
                List<LeaderboardData.LeaderboardEntry> data = new();

                if (!response.IsSuccessStatusCode)
                {
                    _log.Info("No Scores Found (Response code not 200)");
                    callback((false, data, 0));
                    return;
                }
                _log.Info("Got Leaderboard Data");
                string jsonResponse = await response.Content.ReadAsStringAsync();
                LeaderboardData.BPLeaderboard leaderboardData = JsonConvert.DeserializeObject<LeaderboardData.BPLeaderboard>(jsonResponse);
                _log.Info(jsonResponse);
                scorecount = leaderboardData.scoreCount;
                totalPages = Mathf.CeilToInt((float)scorecount / 10);
                data = leaderboardData.scores;

                callback((true, data, totalPages));
                return;
            }
            catch (HttpRequestException e)
            {
                _log.Error("EXCEPTION: " + e.ToString());
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

        public void SetBeatMapData(string mapId, string uploadJson)
        {
            _leaderboardView.hasClickedOffResultsScreen = false;
            UnityMainThreadTaskScheduler.Factory.StartNew(() => UploadLeaderboardData(mapId, uploadJson));
        }

        internal bool isUploading = false;

        public event Action<bool, string> UploadCompleted;
        public event Action UploadFailed;

        private async Task UploadLeaderboardData(string mapId, string json)
        {
            _log.Info("Uploading Score");
            if (DateTime.Now.Millisecond > _authenticationManager._localPlayerInfo.sessionExpiry) return;
            _leaderboardView.hasClickedOffResultsScreen = false;
            using HttpClient httpClient = new();
            int x = 0;
            isUploading = true;
            while (x < 3)
            {
                _log.Info("Uploading Score Attempt: " + x);
                try
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _authenticationManager._localPlayerInfo.tempKey);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    _log.Info("Posting...");
                    HttpResponseMessage response = await httpClient.PostAsync(Constants.LEADERBOARD_UPLOAD_END_POINT(mapId), content);
                    _log.Info("Posted");
                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        _log.Info("Score already exists");
                        UploadCompleted?.Invoke(response.IsSuccessStatusCode, $"<color={Constants.badToast}>You have a better score already...</color>");
                        isUploading = false;
                        break;
                    }

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        _log.Info("Score uploaded");
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        UploadCompleted?.Invoke(response.IsSuccessStatusCode, $"<color={Constants.goodToast}>Successfully uploaded score!</color>");
                        isUploading = false;
                        break;
                    }
                }
                catch (HttpRequestException e)
                {
                    _log.Error("EXCEPTION: " + e.ToString());
                    UploadFailed?.Invoke();
                }
                x++;
            }


            if (x == 2)
            {
                UploadFailed?.Invoke();
                await Task.Delay(3000);
            }
        }

        internal async Task HandleLBUpload()
        {
            _log.Info("Handling Upload UI");
            Action<bool, string> uploadCompletedCallback = (isSuccessful, message) =>
            {
                if (isSuccessful)
                {
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast(message, true, false, 5500));
                }
                else
                {
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast(message, true, false, 7500));
                }
            };


            UploadCompleted += uploadCompletedCallback;

            await Constants.WaitUntil(() => _leaderboardView.hasClickedOffResultsScreen);

            if (isUploading)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("Uploading Score...", true, true, 0));
                try
                {
                    await Constants.WaitUntil(() => !isUploading, timeout: 60000);
                }
                catch (TimeoutException)
                {
                    _log.Error("Failed to upload score (TIMEOUT)");
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast($"<color={Constants.badToast}>Failed to upload... (TIMEOUT)</color>", true, false, 7500));
                }
                finally
                {
                    UploadCompleted -= uploadCompletedCallback;
                }
            }
            await Constants.WaitUntil(() => _leaderboardView.isActivated);
            _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
            UploadCompleted -= uploadCompletedCallback;
            _log.Info("Successfully handled upload UI");
        }


        internal async Task HandleLBAuth()
        {
            _log.Info("Handling Auth UI");
            if (!_authenticationManager.IsAuthed)
            {

                UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("Authenticating...", true, true, 0));
                try
                {
                    await Constants.WaitUntil(() => _authenticationManager.IsAuthed, timeout: 60000);
                }
                catch (TimeoutException)
                {
                    _log.Error("Failed to auth (TIMEOUT)");
                    _leaderboardView.SetErrorState(true, "Failed to Auth");
                    UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast("", false, false, 0));
                }
            }

            UnityMainThreadTaskScheduler.Factory.StartNew(() => _uiUtils.SetToast($"<color={Constants.goodToast}>Successfully signed in!</color>", true, false, 10000));
            _panelView.playerUsername.text = _authenticationManager._localPlayerInfo.username;

            _panelView.playerAvatarLoading.gameObject.SetActive(false);

            UnityMainThreadTaskScheduler.Factory.StartNew(() => _leaderboardView.SetSeasonList(1));
            UnityMainThreadTaskScheduler.Factory.StartNew(() => Task.Run(() => _uiUtils.assignStaff()));

            await Constants.WaitUntil(() => _leaderboardView.currentDifficultyBeatmap != null);
            _leaderboardView.OnLeaderboardSet(_leaderboardView.currentDifficultyBeatmap);
            SharedCoroutineStarter.instance.StartCoroutine(UIUtils.GetSpriteAvatar($"{Constants.USER_URL_API(_authenticationManager._localPlayerInfo.userID)}/avatar", (Sprite a, string b) => _panelView.playerAvatar.sprite = a, (string a, string b) => _panelView.playerAvatar.sprite = Utilities.FindSpriteInAssembly("BedroomPartyLeaderboard.Images.Player.png"), new CancellationToken()));
            _log.Info("Successfully handled auth UI");
            return;
        }


    }
}
