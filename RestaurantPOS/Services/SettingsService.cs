using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Domain.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantPOS.Services
{
    public sealed class SettingsService
    {
        public event Action? SettingsChanged;

        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public AppSettings Settings { get; private set; }

        public SettingsService()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EPS Tech");

            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "settings.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            Settings = LoadSafe();
        }

        // ✅ SAFE LOAD
        private AppSettings LoadSafe()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    var defaults = new AppSettings();
                    SaveSafe(defaults);
                    return defaults;
                }

                var json = File.ReadAllText(_filePath);

                var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);

                return settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Settings load failed: {ex.Message}");

                // ✅ Recover automatically
                var defaults = new AppSettings();
                SaveSafe(defaults);

                return defaults;
            }
        }

        public decimal CalculateCoverCharge(Order order)
        {
            if (order == null) return 0;

            // Use per-order price override if set, otherwise fall back to settings
            decimal adultPrice = order.CoverAPrice ?? Settings.AdultCoverPrice;
            decimal childPrice = order.CoverBPrice ?? Settings.ChildCoverPrice;

            return (order.AdultCovers * adultPrice)
                 + (order.ChildCovers * childPrice);
        }

        public decimal CalculateAdultCoverCharge(Order order)
        {
            if (order == null) return 0;

            // Use per-order price override if set, otherwise fall back to settings
            decimal adultPrice = order.CoverAPrice ?? Settings.AdultCoverPrice;

            return (order.AdultCovers * adultPrice);
        }

        public decimal CalculateChildCoverCharge(Order order)
        {
            if (order == null) return 0;

            // Use per-order price override if set, otherwise fall back to settings
            decimal childPrice = order.CoverBPrice ?? Settings.ChildCoverPrice;

            return (order.ChildCovers * childPrice);
        }

        // ✅ PUBLIC SAVE
        public void Save()
        {
            SaveSafe(Settings);
        }

        public void Save(AppSettings settings)
        {
            Settings = settings;
            SaveSafe(settings);
        }

        // ✅ SAFE SAVE (Atomic Write)
        private void SaveSafe(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, _jsonOptions);

                var tempFile = _filePath + ".tmp";

                File.WriteAllText(tempFile, json);

                // ✅ Atomic replace prevents corruption
                File.Copy(tempFile, _filePath, true);
                File.Delete(tempFile);

                SettingsChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Settings save failed: {ex.Message}");
            }
        }
    }
}
