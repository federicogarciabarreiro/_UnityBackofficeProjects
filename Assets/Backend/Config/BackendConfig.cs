using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BackendConfig", menuName = "Config/BackendConfig")]
public class BackendConfig : ScriptableObject
{
    public string baseUrl;

    public List<TableInfo> tables = new List<TableInfo>();
    public List<EndpointInfo> endpoints = new List<EndpointInfo>();

    #region Validate Functions

    public string GetEndpointPath(string key)
    {
        var endpoint = endpoints.Find(e => e.key == key);
        return endpoint.path;
    }

    public bool IsValidTable(string tableName)
    {
        return tables.Exists(table => table.tableName == tableName);
    }

    public bool IsValidColumn(string tableName, string columnName)
    {
        var table = tables.Find(t => t.tableName == tableName);
        return table.columns.Contains(columnName);
    }

    #endregion

    #region Structs

    [System.Serializable]
    public struct TableInfo
    {
        public string tableName;
        public List<string> columns;
    }

    [System.Serializable]
    public struct EndpointInfo
    {
        public string key;
        public string path;
    }

    #endregion
}
