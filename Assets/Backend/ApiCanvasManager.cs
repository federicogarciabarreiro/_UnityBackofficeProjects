using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApiCanvasManager : MonoBehaviour
{
    // Login/Register
    public TMP_InputField nameInputField;
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;

    public Button loginButton;
    public Button registerButton;

    public TextMeshProUGUI userResponse;

    // Database
    public TMP_Dropdown tableDropdown;
    public TMP_Dropdown columnDropdown;

    public GameObject textInputPrefab;
    public Transform scrollViewContent;

    public Button selectButton;
    public Button insertButton;
    public Button updateButton;

    public TextMeshProUGUI tableResponse;

    public ApiClient apiClient;

    private Dictionary<string, TMP_InputField> inputFields = new Dictionary<string, TMP_InputField>();

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
            PopulateScrollView(columns);
        }
        else
        {
            Debug.LogWarning("No se encontraron columnas para la tabla seleccionada.");
        }
    }

    private void PopulateScrollView(List<string> columns)
    {
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
        inputFields.Clear();

        foreach (string column in columns)
        {
            GameObject instance = Instantiate(textInputPrefab, scrollViewContent);

            TMP_Text label = instance.transform.Find("Label").GetComponent<TMP_Text>();
            label.text = column;

            TMP_InputField inputField = instance.transform.Find("InputField").GetComponent<TMP_InputField>();
            inputFields[column] = inputField;
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
        string value = inputFields[column].text;

        apiClient.SelectDataButton(table, column, value, tableResponse);
    }

    private void OnInsertButtonClick()
    {
        string table = tableDropdown.options[tableDropdown.value].text;

        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        foreach (var field in inputFields)
        {
            string inputValue = field.Value.text;
            if (!string.IsNullOrEmpty(inputValue))
            {
                dataDict[field.Key] = inputValue;
            }
        }

        apiClient.InsertDataButton(table, dataDict, tableResponse);
    }

    private void OnUpdateButtonClick()
    {
        string table = tableDropdown.options[tableDropdown.value].text;
        string column = columnDropdown.options[columnDropdown.value].text;
        string value = inputFields[column].text;

        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        foreach (var field in inputFields)
        {
            string inputValue = field.Value.text;
            if (!string.IsNullOrEmpty(inputValue))
            {
                dataDict[field.Key] = inputValue;
            }
        }

        apiClient.UpdateDataButton(table, dataDict, column, value, tableResponse);
    }

}
