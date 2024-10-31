using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ApiCanvasManager : MonoBehaviour
{
    [Header("Config")]
    public ApiClient apiClient;

    [Header("Authentication Public References")]
    public TMP_InputField nameInputField;
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;

    public Button loginButton;
    public Button registerButton;

    public TextMeshProUGUI userResponse;

    [Header("Database Public References")]

    public Button selectButton;
    public Button insertButton;
    public Button updateButton;

    public TMP_Dropdown tableDropdown;
    public TMP_Dropdown columnDropdown;
    public TMP_InputField valueInputField;

    public Transform viewContent;
    public GameObject textInputPrefab;
    public GameObject buttonPrefab;

    public TextMeshProUGUI tableResponse;

    [Header("Score Public References")]
    public TextMeshProUGUI resultText;
    public Button scoreButton;
    
    private Dictionary<string, TMP_InputField> inputFields = new Dictionary<string, TMP_InputField>();

    [Header("Events")]
    public UnityEvent startEvent;

    private void Start()
    {
        if (apiClient == null || apiClient.backendConfig == null)
        {
            Debug.LogError("ApiClient o BackendConfig no asignado en la escena.");
            return;
        }

        AssignEvents();
        InitDropdownValues();

        startEvent.Invoke();
    }

    #region Generative Functions

    private void InitDropdownValues()
    {
        PopulateTableDropdown();
        UpdateColumnDropdown();
    }

    private List<string> GetColumnsForTable(string tableName)
    {
        if (apiClient.backendConfig == null)
        {
            Debug.LogError("BackendConfig no asignado en ApiClient.");
            return null;
        }

        var tableInfo = apiClient.backendConfig.tables.Find(t => t.tableName == tableName);
        return tableInfo.columns;
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

    private void PopulateScrollView(List<string> columns)
    {
        foreach (Transform child in viewContent)
        {
            Destroy(child.gameObject);
        }
        inputFields.Clear();

        foreach (string column in columns)
        {
            GameObject instance = Instantiate(textInputPrefab, viewContent);

            TMP_Text label = instance.transform.Find("Label").GetComponent<TMP_Text>();
            label.text = column;

            TMP_InputField inputField = instance.transform.Find("InputField").GetComponent<TMP_InputField>();
            inputFields[column] = inputField;

            if (column.Contains("_id", StringComparison.OrdinalIgnoreCase))
            {
                GameObject buttonGo = Instantiate(buttonPrefab, instance.transform);
                buttonGo.GetComponentInChildren<TMP_Text>().text = "Generate";
                Button button = buttonGo.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    inputField.text = apiClient.GetRandomUUID();
                });
            }

            if (column.Contains("_at", StringComparison.OrdinalIgnoreCase))
            {
                GameObject buttonGo = Instantiate(buttonPrefab, instance.transform);
                buttonGo.GetComponentInChildren<TMP_Text>().text = "Generate";
                Button button = buttonGo.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    inputField.text = apiClient.GetCurrentDateTime();
                });
            }

            if (column.Contains("session_id", StringComparison.OrdinalIgnoreCase))
            {
                instance.GetComponentInChildren<TMP_InputField>().SetTextWithoutNotify(apiClient.sessionUUID);
            }
        }
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

    #endregion

    #region Button Events

    private void AssignEvents()
    {
        loginButton.onClick.AddListener(OnLoginButtonClick);
        registerButton.onClick.AddListener(OnRegisterButtonClick);
        selectButton.onClick.AddListener(OnSelectButtonClick);
        insertButton.onClick.AddListener(OnInsertButtonClick);
        updateButton.onClick.AddListener(OnUpdateButtonClick);
        scoreButton.onClick.AddListener(OnGetTopScoresButtonClick);

        tableDropdown.onValueChanged.AddListener(delegate { UpdateColumnDropdown(); });
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
        string value = valueInputField.text;

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

    private void OnGetTopScoresButtonClick()
    {
        string gameName = valueInputField.text;
        int limit = 10;
        apiClient.GetTopScoresButton(gameName, limit, resultText);
    }



    #endregion
}