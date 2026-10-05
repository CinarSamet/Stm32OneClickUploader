using System.IO;

namespace Stm32OneClickUploader.Utils
{
    public static class FileValidator
    {
        public static bool IsValidFirmwareFile(string filePath, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                errorMessage = "Dosya yolu boş olamaz.";
                return false;
            }

            if (!File.Exists(filePath))
            {
                errorMessage = "Belirtilen dosya bulunamadı.";
                return false;
            }

            // 3. Uzantı kontrolü (.hex veya .elf olmalı)
            string extension = Path.GetExtension(filePath).ToLower();
            if (extension != ".hex" && extension != ".elf")
            {
                errorMessage = "Sadece .hex veya .elf uzantılı dosyalar yüklenebilir.";
                return false;
            }

            // İstersen buraya 4. adım olarak "Dosya boyutu 0 byte mı?" 
            // veya "İçinde geçerli bir hex başlığı var mı?" gibi byte seviyesinde 
            // ileri düzey güvenlik kontrolleri (sanity check) de ekleyebilirsin.

            return true;
        }
    }
}