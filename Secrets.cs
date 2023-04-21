﻿using System.Text.Json;
namespace ConsoleApp1;

public static class Secrets
{
    static List<string> _keys = new();
    public static string ApiKey
    {
        get
        {
            _keys = ReadKeysFromFile();
            if (_keys.Count >= 2)
                return _keys[0];
            else
                return "";
        }
    }
    public static string ApiSecret
    {
        get
        {
            if (_keys.Count >= 2)
                return _keys[1];
            else
                return "";
        }
    }
    static List<string> ReadKeysFromFile()
    {
        List<string> keys = new();
        string path = "D:\\Projects\\Common\\Secrets\\BinanceApiKey.txt";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var kks = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if(kks == null)
            {
                throw new Exception("File with Api Keys is bad");
            }
            foreach ( var key in kks.Keys )
                keys.Add(kks[key]);
        }
        else
            throw new Exception("File with Api Keys is not found");

        return keys;
    }
}
