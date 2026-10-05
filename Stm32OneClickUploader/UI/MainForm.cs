using System;
using System.Drawing;
using System.Windows.Forms;
using Stm32OneClickUploader.Core;
using Stm32OneClickUploader.Utils; // Dosya kontrol sınıfımız için eklendi

namespace Stm32OneClickUploader.UI
{
    public partial class MainForm : Form
    {
        private readonly Stm32CliWrapper _cliWrapper;

        public MainForm()
        {
            InitializeComponent();
            // Motorumuzu arayüz açılırken çalışmaya hazır hale getiriyoruz
            _cliWrapper = new Stm32CliWrapper();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                // Sadece HEX ve ELF dosyalarına izin veriyoruz
                ofd.Filter = "STM32 Firmware|*.hex;*.elf|HEX Files|*.hex|ELF Files|*.elf";
                ofd.Title = "Yüklenecek Firmware Dosyasını Seçin";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    LogMessage($"[BİLGİ] Dosya seçildi: {ofd.FileName}", Color.Cyan);
                }
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            string filePath = txtFilePath.Text;

            // 1. Yazdığımız profesyonel Validator ile dosyayı denetliyoruz
            if (!FileValidator.IsValidFirmwareFile(filePath, out string errorMsg))
            {
                MessageBox.Show(errorMsg, "Dosya Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. İşlem başlarken arayüzü kilitliyoruz (Spam tıklamayı önlemek için)
            SetUiState(false);
            rtbLogs.Clear();
            lblStatus.Text = "Durum: ST-LINK Aranıyor...";
            lblStatus.ForeColor = Color.Orange;

            LogMessage("--- YÜKLEME İŞLEMİ BAŞLATILDI ---", Color.White);

            // 3. Core sınıfındaki asenkron metodu çağırıp, gelen logları arayüze basıyoruz
            var result = await _cliWrapper.UploadAsync(filePath, log =>
            {
                // CLI'dan gelen her satırı güvenli şekilde UI'a yazdır
                LogMessage(log, Color.LightGray);
            });

            // 4. İşlem bitti, sonuca göre arayüzü güncelliyoruz
            if (result.Success)
            {
                lblStatus.Text = "Durum: Başarılı!";
                lblStatus.ForeColor = Color.Lime;
                LogMessage($"\n[SONUÇ] {result.Message}", Color.Lime);
                MessageBox.Show("Yazılım çipe başarıyla gömüldü ve cihaz resetlendi.", "Flash Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "Durum: HATA!";
                lblStatus.ForeColor = Color.Red;
                LogMessage($"\n[HATA] {result.Message}", Color.Red);
                MessageBox.Show($"Yükleme başarısız oldu:\n{result.Message}\n\nLütfen log panelini inceleyin.", "Flash Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 5. İşlem bittiği için butonların kilidini açıyoruz
            SetUiState(true);
        }

        // --- YARDIMCI METOTLAR ---

        // Thread-Safe Log yazdırma fonksiyonu (Çökmeyi engeller)
        private void LogMessage(string message, Color color)
        {
            if (rtbLogs.InvokeRequired)
            {
                rtbLogs.BeginInvoke(new Action(() => LogMessage(message, color)));
                return;
            }

            rtbLogs.SelectionStart = rtbLogs.TextLength;
            rtbLogs.SelectionLength = 0;
            rtbLogs.SelectionColor = color;
            rtbLogs.AppendText(message + "\n");
            rtbLogs.ScrollToCaret(); // Otomatik olarak en aşağı kaydır
        }

        // Yükleme sırasında butonları deaktif etme
        private void SetUiState(bool isEnabled)
        {
            btnBrowse.Enabled = isEnabled;
            btnUpload.Enabled = isEnabled;
            txtFilePath.Enabled = isEnabled;
        }

        private async void btnErase_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Çipin içindeki tüm yazılım geri döndürülemez şekilde silinecek! Onaylıyor musunuz?", "Tehlikeli İşlem", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialog == DialogResult.No)
                return;

            SetUiState(false);
            btnErase.Enabled = false; // Silme butonunu da kilitle
            rtbLogs.Clear();
            lblStatus.Text = "Durum: Çip Siliniyor...";
            lblStatus.ForeColor = Color.Orange;

            LogMessage("--- SİLME (FORMAT) İŞLEMİ BAŞLATILDI ---", Color.Orange);

            var result = await _cliWrapper.EraseAsync(log =>
            {
                LogMessage(log, Color.LightGray);
            });

            if (result.Success)
            {
                lblStatus.Text = "Durum: Silindi!";
                lblStatus.ForeColor = Color.Cyan;
                LogMessage($"\n[SONUÇ] {result.Message}", Color.Cyan);
                MessageBox.Show(result.Message, "Silme Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "Durum: HATA!";
                lblStatus.ForeColor = Color.Red;
                LogMessage($"\n[HATA] {result.Message}", Color.Red);
                MessageBox.Show(result.Message, "Silme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SetUiState(true);
            btnErase.Enabled = true;
        }
    }
}