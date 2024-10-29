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
        apiClient.LoginButton(userResponse);
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
        string data = dataInputField.text;

        var dataDict = new Dictionary<string, object> { { "data", data } };
        apiClient.InsertDataButton(table, dataDict, tableResponse);
    }

    private void OnUpdateButtonClick()
    {
        string table = tableDropdown.options[tableDropdown.value].text;
        string column = columnDropdown.options[columnDropdown.value].text;
        string value = valueInputField.text;
        string data = dataInputField.text;

        var dataDict = new Dictionary<string, object> { { "data", data } };
        apiClient.UpdateDataButton(table, dataDict, column, value, tableResponse);
    }
}
