using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MyBox;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [SerializeField] private BackendConfig config;

    [ReadOnly][SerializeField] private string sessionUUID;
    [ReadOnly][SerializeField] private string userUUID;

    private string username;
    private string password;
    private string _name;

    public void SetUsername(string _username) => username = _username;
    public void SetPassword(string _password) => password = _password;
    public void SetName(string n) => _name = n;

    // Métodos públicos para manejar los botones
    public void InsertDataButton(string tableName, Dictionary<string, object> data)
    {
        StartCoroutine(InsertData(tableName, data, response =>
        {
            if (response.success)
                Debug.Log("Datos insertados correctamente.");
            else
                Debug.LogError($"Error al insertar datos: {response.message}");
        }));
    }

    public void UpdateDataButton(string tableName, Dictionary<string, object> data, string column, string value)
    {
        StartCoroutine(UpdateData(tableName, data, column, value, response =>
        {
            if (response.success)
                Debug.Log("Datos actualizados correctamente.");
            else
                Debug.LogError($"Error al actualizar datos: {response.message}");
        }));
    }

    public void SelectDataButton(string tableName, string column, string value)
    {
        StartCoroutine(SelectData(tableName, column, value, response =>
        {
            if (response.success)
            {
                var data = response.data;
                Debug.Log($"Datos seleccionados: {data}");
            }
            else
            {
                Debug.LogError($"Error al seleccionar datos: {response.message}");
            }
        }));
    }

    public void LoginButton()
    {
        StartCoroutine(Login(response =>
        {
            if (response.success)
            {
                Debug.Log("Inicio de sesión exitoso.");
                userUUID = response.data?.ToString();
            }
            else
            {
                Debug.LogError($"Error al iniciar sesión: {response.message}");
            }
        }));
    }

    public void RegisterButton(string name, string username, string password)
    {

        StartCoroutine(Register(name, username, password, response =>
        {
            if (response.success)
            {
                Debug.Log("Registro exitoso.");
                userUUID = response.data?.ToString();
            }
            else
            {
                Debug.LogError($"Error en el registro: {response.message}");
            }
        }));
    }

    // Métodos de corrutina para realizar las solicitudes
    public IEnumerator Login(Action<ResponseData> callback)
    {
        var url = $"{config.baseUrl}{config.GetEndpointPath("login")}";
        var requestData = new Dictionary<string, string>
        {
            { "username", username },
            { "password", password }
        };

        print(url);
        print(username);
        print(password);
        print(_name);

        yield return PostRequest(url, requestData, callback);
    }

    public IEnumerator Register(string name, string username, string password, Action<ResponseData> callback)
    {
        var url = $"{config.baseUrl}{config.GetEndpointPath("register")}";
        var requestData = new Dictionary<string, string>
        {
            { "name", name },
            { "username", username },
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

    public IEnumerator UpdateData(string tableName, Dictionary<string, object> data, string column, string value, Action<ResponseData> callback)
    {
        var url = $"{config.baseUrl} {config.GetEndpointPath("update")}";
        var requestData = new Dictionary<string, object>
        {
            { "table", tableName },
            { "data", data },
            { "column", column },
            { "value", value }
        };

        yield return PostRequest(url, requestData, callback);
    }

    public IEnumerator SelectData(string tableName, string column, string value, Action<ResponseData> callback)
    {
        var url = $"{config.baseUrl}{config.GetEndpointPath("select")}?table={tableName}&column={column}&value={value}";
        yield return GetRequest(url, callback);
    }

    private IEnumerator PostRequest(string url, object jsonData, Action<ResponseData> callback)
    {
        var requestData = JsonUtility.ToJson(jsonData);
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
            Debug.Log($"Respuesta recibida: {request.downloadHandler.text}");
            return JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"Error en la solicitud: {request.error}");
            return new ResponseData { success = false, message = request.error };
        }
    }

    [System.Serializable]
    public class ResponseData
    {
        public bool success;
        public string message;
        public object data;
    }
}
