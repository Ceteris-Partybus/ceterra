using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

/// Helper for validating codes against HTTP API
public class InviteCodeValidator : MonoBehaviour {
    private const string API_URL = "https://api.okolyt.com/check-code";
    
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

    public static IEnumerator ValidateCode(string code, Action<CodeResponse> onSuccess, Action<string> onError) {
        if (string.IsNullOrEmpty(code)) {
            onError?.Invoke("Code cannot be empty");
            yield break;
        }

        CodeRequest request = new CodeRequest { code = code };
        string jsonPayload = JsonUtility.ToJson(request);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(API_URL, jsonPayload, "application/json")) {
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success) {
                try {
                    string responseText = webRequest.downloadHandler.text;
                    CodeResponse response = JsonUtility.FromJson<CodeResponse>(responseText);
                    
                    if (response != null && response.success) {
                        onSuccess?.Invoke(response);
                    } else {
                        onError?.Invoke(response?.message ?? "Code validation failed");
                    }
                } catch (Exception e) {
                    onError?.Invoke($"Failed to parse response: {e.Message}");
                }
            } else {
                onError?.Invoke($"Request failed: {webRequest.error} (Status: {webRequest.responseCode})");
            }
        }
    }
}
