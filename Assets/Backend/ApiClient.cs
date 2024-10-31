using MyBox;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using static ApiClient;

public class ApiClient : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public BackendConfig backendConfig;

    [Header("Values")]
    [ReadOnly] public string sessionUUID;
    [ReadOnly] public string userUUID, accessToken, refreshToken;
    [ReadOnly] public string actionId, actionDescription;
    [ReadOnly] public string gameId, gameName;
    [ReadOnly] public string gameSessionId;
    [ReadOnly] public string scoreId, scoreName, scoreValue;
    [ReadOnly] public string userId, userName, userEmail;
    [ReadOnly] public string created_at;

    private void Start()
    {
        InitializeSession();
    }

    public void InitializeSession()
    {
        sessionUUID = GetRandomUUID();
    }

    #region Public Calls

    public void LoginButton(string username, string password, TMP_Text text)
    {
        StartCoroutine(Login(username, password, response =>
        {
            if (response.success && response.message == "Inicio de sesión exitoso")
            {
                Debug.Log("Inicio de sesión exitoso.");
                userUUID = response.data?.ToString();
                text.text = "Inicio de sesión exitoso.";
            }
            else
            {
                try
                {
                    var errorJson = JToken.Parse(response.message);
                    var detailMessage = errorJson["detail"]?.ToString() ?? "Error desconocido.";
                    Debug.LogError(detailMessage);
                    text.text = detailMessage;
                }
                catch (JsonException)
                {
                    Debug.LogError(response.message);
                    text.text = response.message;
                }
            }
        }));
    }

    public void RegisterButton(string name, string username, string password, TMP_Text text)
    {
        StartCoroutine(Register(name, username, password, response =>
        {
            if (response.success)
            {
                Debug.Log("Registro exitoso.");
                userUUID = response.data?.ToString();
                text.text = "Registro exitoso.";
            }
            else
            {
                try
                {
                    var errorJson = JToken.Parse(response.message);
                    var detailMessage = errorJson["detail"]?.ToString() ?? "Error desconocido.";
                    Debug.LogError(detailMessage);
                    text.text = detailMessage;
                }
                catch (JsonException)
                {
                    Debug.LogError(response.message);
                    text.text = response.message;
                }
            }
        }));
    }

    public void InsertDataButton(string tableName, Dictionary<string, object> data, TMP_Text text)
    {
        StartCoroutine(InsertData(tableName, data, response =>
        {
            if (response.success)
            {
                Debug.Log("Datos insertados correctamente.");
                text.text = "Datos insertados correctamente.";
            }
            else
            {
                HandleErrorResponse(response.message, text);
            }
        }));
    }

    public void UpdateDataButton(string tableName, Dictionary<string, object> data, string column, string value, TMP_Text text)
    {
        StartCoroutine(UpdateData(tableName, data, column, value, response =>
        {
            if (response.success)
            {
                Debug.Log("Datos actualizados correctamente.");
                text.text = "Datos actualizados correctamente.";
            }
            else
            {
                HandleErrorResponse(response.message, text);
            }
        }));
    }


    public void SelectDataButton(string tableName, string column, string value, TMP_Text text)
    {
        StartCoroutine(SelectData(tableName, column, value, response =>
        {
            if (response.success)
            {
                var data = response.data as List<Dictionary<string, object>>;

                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        string itemDetails = string.Join(", ", item.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                        Debug.Log($"Datos seleccionados: {itemDetails}");
                    }

                    string displayText = string.Join("\n", data.Select(d =>
                        string.Join(", ", d.Select(kvp => $"{kvp.Key}: {kvp.Value}"))));

                    text.text = displayText;
                }
                else
                {
                    text.text = "No se encontraron datos.";
                    Debug.Log("No se encontraron datos.");
                }
            }
            else
            {
                try
                {
                    var errorJson = JToken.Parse(response.message);
                    var detailMessage = errorJson["detail"]?.ToString() ?? "Error desconocido.";
                    Debug.LogError(detailMessage);
                    text.text = detailMessage;
                }
                catch (JsonException)
                {
                    Debug.LogError(response.message);
                    text.text = response.message;
                }
            }
        }));
    }


    private void HandleErrorResponse(string message, TMP_Text text)
    {
        try
        {
            var errorJson = JToken.Parse(message);
            var detailMessage = errorJson["detail"]?.ToString() ?? "Error desconocido.";
            Debug.LogError(detailMessage);
            text.text = detailMessage;
        }
        catch (JsonException)
        {
            Debug.LogError(message);
            text.text = message;
        }
    }


    #endregion

    #region Coroutines

    public IEnumerator Login(string username, string password, Action<ResponseData> callback)
    {
        var url = $"{backendConfig.baseUrl}{backendConfig.GetEndpointPath("login")}";
        var requestData = new Dictionary<string, string>
        {
            { "email", username },
            { "password", password }
        };

        yield return PostRequest(url, requestData, callback);
    }

    public IEnumerator Register(string name, string username, string password, Action<ResponseData> callback)
    {
        var url = $"{backendConfig.baseUrl}{backendConfig.GetEndpointPath("register")}";
        var requestData = new Dictionary<string, string>
        {
            { "user_name", name },
            { "email", username },
            { "password", password }
        };

        yield return PostRequest(url, requestData, callback);
    }

    public IEnumerator InsertData(string tableName, Dictionary<string, object> data, Action<ResponseData> callback)
    {
        var url = $"{backendConfig.baseUrl}{backendConfig.GetEndpointPath("insert")}";
        var requestData = new Dictionary<string, object>
        {
            { "table", tableName },
            { "data", data }
        };

        yield return PostRequest(url, requestData, callback);
    }

    public IEnumerator SelectData(string tableName, string column, string value, Action<ResponseData> callback)
    {
        var url = $"{backendConfig.baseUrl}{backendConfig.GetEndpointPath("select")}?table={tableName}&column={column}&value={value}";
        yield return GetRequest(url, callback);
    }

    public IEnumerator UpdateData(string tableName, Dictionary<string, object> data, string column, string value, Action<ResponseData> callback)
    {
        var url = $"{backendConfig.baseUrl}{backendConfig.GetEndpointPath("update")}";
        var requestData = new Dictionary<string, object>
        {
            { "table", tableName },
            { "data", data },
            { "column", column },
            { "value", value }
        };

        yield return PutRequest(url, requestData, callback);
    }

    #endregion

    #region Network Requests

    private IEnumerator PutRequest(string url, object jsonData, Action<ResponseData> callback)
    {
        string requestData = JsonConvert.SerializeObject(jsonData);
        print(requestData);
        using var request = new UnityWebRequest(url, "PUT")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestData)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        var response = ParseResponse(request);
        callback?.Invoke(response);
    }

    private IEnumerator PostRequest(string url, object jsonData, Action<ResponseData> callback)
    {
        string requestData = JsonConvert.SerializeObject(jsonData);
        print(requestData);
        using var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestData)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        var response = ParseResponse(request);
        callback?.Invoke(response);
    }

    private IEnumerator GetRequest(string url, Action<ResponseData> callback)
    {
        using var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        var response = ParseResponse(request);
        callback?.Invoke(response);
    }

    #endregion

    #region Parse Functions

    private ResponseData ParseResponse(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            var textData = request.downloadHandler.text;
            Debug.Log($"Respuesta recibida: {textData}");

            try
            {
                var jsonToken = JToken.Parse(textData);
                ResponseData responseData = new ResponseData();

                if (jsonToken is JObject)
                {
                    if (jsonToken["access_token"] != null)
                        return ParseRegisterResponse(jsonToken);

                    if (jsonToken["message"] != null && jsonToken["user"] != null)
                        return ParseLoginResponse(jsonToken);
                }
                else if (jsonToken is JArray)
                {
                    return ParseSelectResponse(jsonToken);
                }

                Debug.LogError("Estructura JSON no reconocida");
                return new ResponseData { success = false, message = "Estructura JSON no reconocida", data = textData };
            }
            catch (JsonException e)
            {
                Debug.LogError($"Error al analizar JSON: {e.Message}");
                return new ResponseData { success = false, message = "Error en la respuesta JSON", data = textData };
            }
        }
        else
        {
            string errorMessage = request.downloadHandler.text;
            Debug.LogError($"Error {request.responseCode}: {errorMessage}");
            return new ResponseData { success = false, message = errorMessage };
        }
    }


    private ResponseData ParseRegisterResponse(JToken jsonToken)
    {
        var registerResponse = jsonToken.ToObject<RegisterResponse>();

        accessToken = registerResponse.access_token;
        refreshToken = registerResponse.refresh_token;

        Debug.Log("Parsed");

        return new ResponseData
        {
            success = true,
            data = registerResponse,
            message = "Registro exitoso"
        };
    }

    private ResponseData ParseLoginResponse(JToken jsonToken)
    {
        var loginResponse = jsonToken.ToObject<LoginResponse>();

        accessToken = loginResponse.user.access_token;
        refreshToken = loginResponse.user.refresh_token;

        Debug.Log("Parsed");

        return new ResponseData
        {
            success = true,
            data = loginResponse.user,
            message = loginResponse.message
        };
    }

    private ResponseData ParseSelectResponse(JToken jsonToken)
    {
        var selectData = jsonToken.ToObject<List<Dictionary<string, object>>>();

        if (selectData != null && selectData.Count > 0)
        {
            foreach (var data in selectData)
            {
                JToken dataToken = JToken.FromObject(data);
                AsignarDatos(dataToken);
            }
        }

        Debug.Log("Parsed");

        return new ResponseData
        {
            success = true,
            data = selectData ?? new List<Dictionary<string, object>>(),
            message = "Consulta exitosa"
        };
    }


    private void AsignarDatos(JToken data)
    {
        foreach (var property in data.Children<JProperty>())
        {
            switch (property.Name)
            {
                case "action_id":
                    actionId = property.Value.ToString();
                    break;
                case "action_description":
                    actionDescription = property.Value.ToString();
                    break;
                case "game_id":
                    gameId = property.Value.ToString();
                    break;
                case "game_name":
                    gameName = property.Value.ToString();
                    break;
                case "game_session_id":
                    gameSessionId = property.Value.ToString();
                    break;
                case "session_id":
                    sessionUUID = property.Value.ToString();
                    break;
                case "score_id":
                    scoreId = property.Value.ToString();
                    break;
                case "score_name":
                    scoreName = property.Value.ToString();
                    break;
                case "score_value":
                    scoreValue = property.Value.ToString();
                    break;
                case "user_id":
                    userId = property.Value.ToString();
                    break;
                case "user_name":
                    userName = property.Value.ToString();
                    break;
                case "user_email":
                    userEmail = property.Value.ToString();
                    break;
                case "created_at":
                    created_at = property.Value.ToString();
                    break;
                default:
                    Debug.LogWarning($"Propiedad desconocida: {property.Name}");
                    break;
            }
        }
    }


    #endregion

    #region Response Models

    public class RegisterResponse
    {
        public string email { get; set; }
        public string user_name { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }

    public class LoginResponse
    {
        public string message { get; set; }
        public User user { get; set; }
    }

    public class User
    {
        public string email { get; set; }
        public string user_name { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }

    public class ResponseData
    {
        public bool success { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }

    public class OperationResponse
    {
        public string table { get; set; }
        public object data { get; set; }
    }

    #endregion

    #region Auxiliar Functions

    public string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public string GetRandomUUID()
    {
        return Guid.NewGuid().ToString();
    }

    #endregion

}

