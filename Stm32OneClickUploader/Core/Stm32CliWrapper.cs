using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stm32OneClickUploader.Core
{
    public class Stm32CliWrapper
    {
        private readonly string _cliPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLI_Tools", "bin", "openocd.exe");
        private readonly string _scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CLI_Tools", "scripts");

        public async Task<UploadResult> UploadAsync(string filePath, Action<string> onLogReceived)
        {
            var result = new UploadResult { Success = false };

            if (!File.Exists(filePath))
            {
                result.Message = "Hata: Yüklenecek HEX/ELF dosyası bulunamadı!";
                return result;
            }

            if (!File.Exists(_cliPath))
            {
                result.Message = $"Hata: OpenOCD motoru bulunamadı!\n{_cliPath}";
                return result;
            }

            string safePath = filePath.Replace("\\", "/");
            string openOcdScriptsPath = _scriptsPath.Replace("\\", "/");

            
            onLogReceived?.Invoke("[BİLGİ] Hedef çip kimliği analiz ediliyor (Recon)...");

            string reconArgs = $"-s \"{openOcdScriptsPath}\" -f interface/stlink.cfg -f target/stm32f1x.cfg -c init -c exit";

            string reconOutput = await RunProcessAsync(reconArgs, null);

            Match match = Regex.Match(reconOutput, @"UNEXPECTED idcode: (0x[0-9a-fA-F]+)");

            string targetId = "0x1ba01477";
            bool isClone = false;

            if (match.Success)
            {
                targetId = match.Groups[1].Value;
                isClone = true;
                onLogReceived?.Invoke($"[UYARI] Klon çip tespit edildi! Sahte Kimlik: {targetId}");
            }
            else
            {
                onLogReceived?.Invoke("[BİLGİ] Orijinal ID tespit edildi veya çip itiraz etmedi.");
            }

            
            onLogReceived?.Invoke($"[BİLGİ] Bypass payload'ı hazırlanıyor (ID: {targetId})...");

           
            string exploitArgs = $"-s \"{openOcdScriptsPath}\" -f interface/stlink.cfg -c \"set CPUTAPID {targetId}\" -f target/stm32f1x.cfg -c \"program \\\"{safePath}\\\" verify reset exit\"";

            onLogReceived?.Invoke($"[KOMUT] openocd.exe {exploitArgs}\n");

            
            string finalOutput = await RunProcessAsync(exploitArgs, onLogReceived);

            
            if (finalOutput.Contains("** Verified OK **") || finalOutput.Contains("** Programming Finished **"))
            {
                result.Success = true;
                result.Message = isClone
                    ? $"Klon güvenlik duvarı aşıldı (Bypass ID: {targetId})! Yazılım zorla yüklendi."
                    : "Yazılım başarıyla yüklendi ve doğrulandı.";
            }
            else if (finalOutput.Contains("Error: open failed"))
            {
                result.Message = "ST-LINK bulunamadı. USB veya Pin bağlantılarını kontrol edin.";
            }
            else
            {
                result.Message = "Yükleme başarısız. Logları inceleyin.";
            }

            return result;
        }
       
        public async Task<UploadResult> EraseAsync(Action<string> onLogReceived)
        {
            var result = new UploadResult { Success = false };

            if (!File.Exists(_cliPath))
            {
                result.Message = $"Hata: OpenOCD motoru bulunamadı!\n{_cliPath}";
                return result;
            }

            string openOcdScriptsPath = _scriptsPath.Replace("\\", "/");

            onLogReceived?.Invoke("[BİLGİ] Silme işlemi için hedef analiz ediliyor (Recon)...");
            string reconArgs = $"-s \"{openOcdScriptsPath}\" -f interface/stlink.cfg -f target/stm32f1x.cfg -c init -c exit";
            string reconOutput = await RunProcessAsync(reconArgs, null);

            Match match = Regex.Match(reconOutput, @"UNEXPECTED idcode: (0x[0-9a-fA-F]+)");
            string targetId = match.Success ? match.Groups[1].Value : "0x1ba01477";

            onLogReceived?.Invoke($"[UYARI] Çip hafızası TAMAMEN siliniyor (ID: {targetId})...");

            string eraseArgs = $"-s \"{openOcdScriptsPath}\" -f interface/stlink.cfg -c \"set CPUTAPID {targetId}\" -f target/stm32f1x.cfg -c init -c \"reset halt\" -c \"stm32f1x mass_erase 0\" -c reset -c exit";

            onLogReceived?.Invoke($"[KOMUT] openocd.exe {eraseArgs}\n");
            string finalOutput = await RunProcessAsync(eraseArgs, onLogReceived);

            if (finalOutput.Contains("mass erase complete"))
            {
                result.Success = true;
                result.Message = "Çip hafızası başarıyla temizlendi (Format Atıldı).";
            }
            else if (finalOutput.Contains("Error: open failed"))
            {
                result.Message = "ST-LINK bulunamadı. Bağlantıları kontrol edin.";
            }
            else
            {
                result.Message = "Silme işlemi başarısız. Logları inceleyin.";
            }

            return result;
        }

        private async Task<string> RunProcessAsync(string arguments, Action<string> onLogReceived)
        {
            string fullOutput = string.Empty;

            var startInfo = new ProcessStartInfo
            {
                FileName = _cliPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using (var process = new Process { StartInfo = startInfo })
                {
                    DataReceivedEventHandler logHandler = (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            fullOutput += e.Data + "\n";
                            onLogReceived?.Invoke(e.Data);
                        }
                    };

                    process.OutputDataReceived += logHandler;
                    process.ErrorDataReceived += logHandler;

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await Task.Run(() => process.WaitForExit());
                }
            }
            catch (Exception ex)
            {
                fullOutput += $"SYSTEM_ERROR: {ex.Message}";
                onLogReceived?.Invoke($"[SİSTEM HATASI] {ex.Message}");
            }

            return fullOutput;
        }
    }
}