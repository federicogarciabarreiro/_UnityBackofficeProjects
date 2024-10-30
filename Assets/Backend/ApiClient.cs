using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyBox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [SerializeField] public BackendConfig config;

    [ReadOnly][SerializeField] private string sessionUUID;
    [ReadOnly][SerializeField] private string userUUID;

    // Valores
    [ReadOnly] public string actionId, actionDescription;
    [ReadOnly] public string gameId, gameName;
    [ReadOnly] public string gameSessionId, sessionId;
    [ReadOnly] public string scoreId, scoreName, scoreValue;
    [ReadOnly] public string userId, userName, userEmail;

    // Métodos públicos para manejar los botones
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
                Debug.LogError($"Error al insertar datos: {response.message}");
                text.text = $"Error al insertar datos: {response.message}";
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
                Debug.LogError($"Error al actualizar datos: {response.message}");
                text.text = $"Error al actualizar datos: {response.message}";
            }
        }));
    }

    public void SelectDataButton(string tableName, string column, string value, TMP_Text text)
    {
        StartCoroutine(SelectData(tableName, column, value, response =>
        {
            if (response.success)
            {
                var data = response.data;
                Debug.Log($"Datos seleccionados: {data}");
                text.text = data.ToString();
            }
            else
            {
                Debug.LogError($"Error al seleccionar datos: {response.message}");
                text.text = $"Error al seleccionar datos: {response.message}";
            }
        }));
    }

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
                Debug.LogError($"Error al iniciar sesión: {response.message}");
                text.text = $"Error al iniciar sesión: {response.message}";
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
                Debug.LogError($"Error en el registro: {response.message}");
                text.text = $"Error en el registro: {response.message}";
            }
        }));
    }

    public IEnumerator Login(string username, string password, Action<ResponseData> callback)
    {
        var url = $"{config.baseUrl}{config.GetEndpointPath("login")}";
        var requestData = new Dictionary<string, string>
        {
            { "email", username },
            { "password", password }
        };

        yield return PostRequest(url, requestData, callback);
    }

    public IEnumerator Register(string name, string username, string password, Action<ResponseData> callback)
    {
        var url = $"{config.baseUrl}{config.GetEndpointPath("register")}";
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
        var url = $"{config.baseUrl}{config.GetEndpointPath("insert")}";
        var requestData = new Dictionary<string, object>
        {
            { "table", tableName },
            { "data", data }
        };

        yield return PostRequest(url, requestData, callback);
    }

    public IEnumerator SelectData(string tableName, string column, string value, Action<ResponseData> callback)
    {
        var url = $"{config.baseUrl}{config.GetEndpointPath("select")}?table={tableName}&column={column}&value={value}";
        yield return GetRequest(url, callback);
    }

    public IEnumerator UpdateData(string tableName, Dictionary<string, object> data, string column, string value, Action<ResponseData> callback)
    {
        var url = $"{config.baseUrl}{config.GetEndpointPath("update")}";
        var requestData = new Dictionary<string, object>
        {
            { "table", tableName },
            { "data", data },
            { "column", column },
            { "value", value }
        };

        yield return PutRequest(url, requestData, callback);
    }

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

                if (jsonToken["access_token"] != null)
                {
                    var registerResponse = jsonToken.ToObject<RegisterResponse>();
                    if (registerResponse != null)
                    {
                        responseData.success = true;
                        responseData.data = registerResponse;
                        responseData.message = "Registro exitoso";
                        return responseData;
                    }
                }

                if (jsonToken["message"] != null && jsonToken["user"] != null)
                {
                    var loginResponse = jsonToken.ToObject<LoginResponse>();
                    if (loginResponse != null)
                    {
                        responseData.success = true;
                        responseData.data = loginResponse.user;
                        responseData.message = loginResponse.message;
                        return responseData;
                    }
                }

                if (jsonToken["table"] != null && jsonToken["data"] != null)
                {
                    var operationResponse = jsonToken.ToObject<OperationResponse>();
                    if (operationResponse != null)
                    {
                        responseData.success = true;
                        responseData.data = operationResponse.data;
                        responseData.message = $"Operación exitosa en la tabla {operationResponse.table}";
                        return responseData;
                    }
                }

                Debug.LogError("Respuesta no válida: la estructura JSON no coincide");
                return new ResponseData { success = false, message = "Respuesta no válida", data = textData };
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


    private void AsignarDatos(JToken data)
    {
        actionId = data["action_id"]?.ToString();
        actionDescription = data["action_description"]?.ToString();
        gameId = data["game_id"]?.ToString();
        gameName = data["game_name"]?.ToString();
        gameSessionId = data["game_session_id"]?.ToString();
        sessionId = data["session_id"]?.ToString();
        scoreId = data["score_id"]?.ToString();
        scoreName = data["score_name"]?.ToString();
        scoreValue = data["score_value"]?.ToString();
        userId = data["user_id"]?.ToString();
        userName = data["user_name"]?.ToString();
        userEmail = data["user_email"]?.ToString();
    }

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


}

