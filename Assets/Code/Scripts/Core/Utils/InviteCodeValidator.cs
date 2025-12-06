using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

/// Helper for validating codes against HTTP API
public class InviteCodeValidator : Singleton<InviteCodeValidator> {
    protected override bool ShouldPersistAcrossScenes => true;

    public static readonly Dictionary<string, string> API_SERVERS = new Dictionary<string, string> {
        { "DE/NÜRNBERG01", "https://ceterra-api.fklk.dev" },
        { "DE/NÜRNBERG02", "https://api.okolyt.com" },
        { "DE/KARLSRUHE01", "https://ceterra-api.ossenbeck.dev" }
    };

    public static readonly Dictionary<string, string> GAME_SERVERS = new Dictionary<string, string> {
        { "DE/NÜRNBERG01", "ceterra.fklk.dev" },
        { "DE/NÜRNBERG02", "unity.okolyt.com" },
        { "DE/KARLSRUHE01", "ceterra.ossenbeck.dev" }
    };

    [Serializable]
    public class ServerStatus {
        public string Key;
        public string ApiUrl;
        public string GameServerAddress;
        public bool IsAvailable;
        public int Latency;
    }

    public List<ServerStatus> AvailableServers = new List<ServerStatus>();
    public bool IsScanning { get; private set; }
    public Action OnServerListUpdated;

    private void Start() {
        RefreshServers();
    }

    public void RefreshServers() {
        StartCoroutine(CheckServers());
    }

    private IEnumerator CheckServers() {
        IsScanning = true;
        AvailableServers.Clear();

        foreach (var kvp in API_SERVERS) {
            string key = kvp.Key;
            string apiUrl = kvp.Value;
            string gameServer = GAME_SERVERS.ContainsKey(key) ? GAME_SERVERS[key] : null;

            // Check API
            bool apiAvailable = false;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl + "/health")) {
                webRequest.timeout = 2;
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success) {
                    apiAvailable = true;
                }
            }

            if (apiAvailable) {
                int latency = -1;
                if (!string.IsNullOrEmpty(gameServer)) {
                    string ipAddress = gameServer;

                    // Resolve DNS in background
                    var dnsTask = System.Threading.Tasks.Task.Run(() => {
                        try {
                            var addresses = Dns.GetHostAddresses(gameServer);
                            if (addresses.Length > 0) {
                                return addresses[0].ToString();
                            }
                        }
                        catch (Exception e) {
                            Debug.LogWarning($"DNS resolution failed for {gameServer}: {e.Message}");
                        }
                        return null;
                    });

                    float dnsTimer = 0f;
                    while (!dnsTask.IsCompleted && dnsTimer < 2.0f) {
                        dnsTimer += Time.deltaTime;
                        yield return null;
                    }

                    if (dnsTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion && !string.IsNullOrEmpty(dnsTask.Result)) {
                        ipAddress = dnsTask.Result;
                    }

                    Ping ping = null;
                    try {
                        ping = new Ping(ipAddress);
                    }
                    catch (Exception e) {
                        Debug.LogWarning($"Ping creation failed for {ipAddress} ({gameServer}): {e.Message}");
                    }

                    if (ping != null) {
                        float timeout = 2.0f;
                        float timer = 0f;
                        while (!ping.isDone && timer < timeout) {
                            timer += Time.deltaTime;
                            yield return null;
                        }
                        if (ping.isDone) {
                            if (ping.time >= 0) {
                                latency = ping.time;
                            }
                            else {
                                Debug.LogWarning($"Ping finished but time is invalid for {gameServer}: {ping.time}");
                            }
                        }
                        else {
                            Debug.LogWarning($"Ping timed out for {gameServer}");
                        }
                        ping.DestroyPing();
                    }
                }

                AvailableServers.Add(new ServerStatus {
                    Key = key,
                    ApiUrl = apiUrl,
                    GameServerAddress = gameServer,
                    IsAvailable = true,
                    Latency = latency
                });
            }
        }
        IsScanning = false;
        OnServerListUpdated?.Invoke();
    }

    [Serializable]
    public class CodeRequest {
        public string code;
    }

    [Serializable]
    public class CodeResponse {
        public bool success;
        public string message;
        public string domain;
        public int port;
    }

    public void ValidateCode(string code, string serverKey, Action<CodeResponse> onSuccess, Action<string> onError) {
        StartCoroutine(ValidateCodeRoutine(code, serverKey, onSuccess, onError));
    }

    private IEnumerator ValidateCodeRoutine(string code, string serverKey, Action<CodeResponse> onSuccess, Action<string> onError) {
        if (string.IsNullOrEmpty(code)) {
            onError?.Invoke("Code cannot be empty");
            yield break;
        }

        if (!API_SERVERS.ContainsKey(serverKey)) {
            onError?.Invoke("Invalid server selected");
            yield break;
        }

        string apiUrl = API_SERVERS[serverKey] + "/check-code";

        CodeRequest request = new CodeRequest { code = code };
        string jsonPayload = JsonUtility.ToJson(request);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(apiUrl, jsonPayload, "application/json")) {
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success) {
                try {
                    string responseText = webRequest.downloadHandler.text;
                    CodeResponse response = JsonUtility.FromJson<CodeResponse>(responseText);

                    if (response != null && response.success) {
                        onSuccess?.Invoke(response);
                    }
                    else {
                        onError?.Invoke(response?.message ?? "Code validation failed");
                    }
                }
                catch (Exception e) {
                    onError?.Invoke($"Failed to parse response: {e.Message}");
                }
            }
            else {
                onError?.Invoke($"Request failed: {webRequest.error} (Status: {webRequest.responseCode})");
            }
        }
    }
}
