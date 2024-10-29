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

    public TMP_InputField tableInputField;
    public TMP_InputField columnInputField;
    public TMP_InputField valueInputField;
    public TMP_InputField dataInputField;
    public Button selectButton;
    public Button insertButton;
    public Button updateButton;
    public TextMeshProUGUI tableResponse;

    private ApiClient apiClient;

    private void Start()
    {
        apiClient = FindObjectOfType<ApiClient>();

        if (apiClient == null)
        {
            Debug.LogError("ApiClient no encontrado en la escena.");
            return;
        }

        loginButton.onClick.AddListener(OnLoginButtonClick);
        registerButton.onClick.AddListener(OnRegisterButtonClick);
        selectButton.onClick.AddListener(OnSelectButtonClick);
        insertButton.onClick.AddListener(OnInsertButtonClick);
        updateButton.onClick.AddListener(OnUpdateButtonClick);
    }

    private void OnLoginButtonClick()
    {
        apiClient.LoginButton();
    }

    private void OnRegisterButtonClick()
    {
        string name = nameInputField.text;
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        apiClient.RegisterButton(name, username, password);
    }

    private void OnSelectButtonClick()
    {
        string table = tableInputField.text;
        string column = columnInputField.text;
        string value = valueInputField.text;

        apiClient.SelectDataButton(table, column, value);
    }

    private void OnInsertButtonClick()
    {
        string table = tableInputField.text;
        string data = dataInputField.text;

        var dataDict = new Dictionary<string, object> { { "data", data } };
        apiClient.InsertDataButton(table, dataDict);
    }

    private void OnUpdateButtonClick()
    {
        string table = tableInputField.text;
        string column = columnInputField.text;
        string value = valueInputField.text;
        string data = dataInputField.text;

        var dataDict = new Dictionary<string, object> { { "data", data } };
        apiClient.UpdateDataButton(table, dataDict, column, value);
    }
}
