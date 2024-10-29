using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApiCanvasManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public Button loginButton;
    public Button registerButton;
    public TextMeshProUGUI userResponse;

    public TMP_Dropdown tableDropdown;
    public TMP_Dropdown columnDropdown;
    public TMP_InputField valueInputField;
    public TMP_InputField dataInputField;
    public Button selectButton;
    public Button insertButton;
    public Button updateButton;
    public TextMeshProUGUI tableResponse;

    public ApiClient apiClient;

    private void Start()
    {
        if (apiClient == null || apiClient.config == null)
        {
            Debug.LogError("ApiClient o BackendConfig no asignado en la escena.");
            return;
        }

        loginButton.onClick.AddListener(OnLoginButtonClick);
        registerButton.onClick.AddListener(OnRegisterButtonClick);
        selectButton.onClick.AddListener(OnSelectButtonClick);
        insertButton.onClick.AddListener(OnInsertButtonClick);
        updateButton.onClick.AddListener(OnUpdateButtonClick);

        tableDropdown.onValueChanged.AddListener(delegate { UpdateColumnDropdown(); });

        PopulateTableDropdown();
        UpdateColumnDropdown();
    }

    private void PopulateTableDropdown()
    {
        tableDropdown.ClearOptions();
        List<string> tableNames = new List<string>
        {
            "actions", "games", "games_sessions", "scores", "sessions",
            "sessions_actions", "sessions_scores", "users", "users_sessions"
        };
        tableDropdown.AddOptions(tableNames);
    }

    private List<string> GetColumnsForTable(string tableName)
    {
        if (apiClient.config == null)
        {
            Debug.LogError("BackendConfig no asignado en ApiClient.");
            return null;
        }

        var tableInfo = apiClient.config.tables.Find(t => t.tableName == tableName);
        return tableInfo.columns;
    }

    private void UpdateColumnDropdown()
    {
        columnDropdown.ClearOptions();
        string selectedTable = tableDropdown.options[tableDropdown.value].text;

        List<string> columns = GetColumnsForTable(selectedTable);
        if (columns != null && columns.Count > 0)
        {
            columnDropdown.AddOptions(columns);
        }
        else
        {
            Debug.LogWarning("No se encontraron columnas para la tabla seleccionada.");
        }
    }

    private void OnLoginButtonClick()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        apiClient.LoginButton(username, password, userResponse);
    }

    private void OnRegisterButtonClick()
    {
        string name = nameInputField.text;
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        apiClient.RegisterButton(name, username, password, userResponse);
    }

    private void OnSelectButtonClick()
    {
        string table = tableDropdown.options[tableDropdown.value].text;
        string column = columnDropdown.options[columnDropdown.value].text;
        string value = valueInputField.text;

        apiClient.SelectDataButton(table, column, value, tableResponse);
    }

    private void OnInsertButtonClick()
    {
        string table = tableDropdown.options[tableDropdown.value].text;
        string jsonData = dataInputField.text;

        Dictionary<string, object> dataDict;
        try
        {
            dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
        }
        catch (JsonException ex)
        {
            Debug.LogError("Error al deserializar el JSON: " + ex.Message);
            return;
        }

        apiClient.InsertDataButton(table, dataDict, tableResponse);
    }

    private void OnUpdateButtonClick()
    {
        string table = tableDropdown.options[tableDropdown.value].text;
        string column = columnDropdown.options[columnDropdown.value].text;
        string value = valueInputField.text;
        string jsonData = dataInputField.text;

        Dictionary<string, object> dataDict;
        try
        {
            dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
        }
        catch (JsonException ex)
        {
            Debug.LogError("Error al deserializar el JSON: " + ex.Message);
            return;
        }

        apiClient.UpdateDataButton(table, dataDict, column, value, tableResponse);
    }
}
