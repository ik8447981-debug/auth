using System.Collections.Generic;

namespace LicensePlatform.Application.Interfaces
{
    public interface ISystemSettingService
    {
        string GetSetting(string key);
        bool SetSetting(string key, string value);
        Dictionary<string, string> GetAllSettings();
    }
}
