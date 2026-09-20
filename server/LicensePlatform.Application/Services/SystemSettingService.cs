using System;
using System.Collections.Generic;
using System.Linq;
using LicensePlatform.Application.Interfaces;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Application.Services
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly LicensePlatformDbContext _context;

        public SystemSettingService(LicensePlatformDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string GetSetting(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Setting key cannot be empty.");

            var setting = _context.SystemSettings
                .FirstOrDefault(s => s.SettingKey == key.Trim());

            if (setting == null)
                throw new KeyNotFoundException($"Setting with key '{key}' not found.");

            return setting.SettingValue;
        }

        public bool SetSetting(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Setting key cannot be empty.");

            var setting = _context.SystemSettings
                .FirstOrDefault(s => s.SettingKey == key.Trim());

            if (setting != null)
            {
                setting.SettingValue = value;
                setting.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                setting = new SystemSetting
                {
                    SystemSettingId = Guid.NewGuid(),
                    SettingKey = key.Trim(),
                    SettingValue = value,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.SystemSettings.Add(setting);
            }

            _context.SaveChanges();
            return true;
        }

        public Dictionary<string, string> GetAllSettings()
        {
            return _context.SystemSettings
                .OrderBy(s => s.SettingKey)
                .ToDictionary(s => s.SettingKey, s => s.SettingValue);
        }
    }
}
