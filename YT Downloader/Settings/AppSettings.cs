using System;
using System.IO;
using System.Text.Json;

namespace YT_Downloader.Settings
{
    public class AppSettings
    {
        public string Theme { get; set; }
        public string DefaultDownloadsPath { get; set; }
        public bool AlwaysAskWhereSave { get; set; }

        private readonly string configFilePath;

        public AppSettings()
        {
            // Define o caminho do arquivo de configuração
            configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YT Downloader", "config.json");
        }

        // Salva as configurações no arquivo
        public void SaveNewSettings()
        {
            try
            {
                // Garante que o diretório exista antes de salvar
                Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));

                // Serializa as configurações em JSON e salva no arquivo
                var jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFilePath, jsonString);
            }
            catch (Exception ex)
            {
                // Log de erro simplificado; considere usar um logger mais robusto
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        // Carrega as configurações do arquivo
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    var jsonString = File.ReadAllText(configFilePath);
                    var loadedSettings = JsonSerializer.Deserialize<AppSettings>(jsonString);

                    if (loadedSettings != null)
                    {
                        // Atualiza as propriedades com as configurações carregadas
                        Theme = loadedSettings.Theme;
                        DefaultDownloadsPath = loadedSettings.DefaultDownloadsPath;
                        AlwaysAskWhereSave = loadedSettings.AlwaysAskWhereSave;
                    }
                }
                else
                {
                    // Configuração inicial padrão
                    SetDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                // Log de erro simplificado; considere usar um logger mais robusto
                Console.WriteLine($"Error loading settings: {ex.Message}");

                // Aplica uma configuração padrão caso ocorra algum erro
                SetDefaultSettings();
            }
        }

        // Aplica Configuração Padrão
        public void SetDefaultSettings()
        {
            Theme = "Default";
            DefaultDownloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            AlwaysAskWhereSave = true;
        }
    }
}
