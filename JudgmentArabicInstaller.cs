using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace JudgmentArabicInstaller
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InstallerForm());
        }
    }

    internal sealed class InstallerForm : Form
    {
        private readonly TextBox gameBox = new TextBox();
        private readonly TextBox packageBox = new TextBox();
        private readonly TextBox logBox = new TextBox();
        private readonly Button installButton = new Button();
        private readonly Button restoreButton = new Button();

        private static readonly string[] RequiredFiles = new string[]
        {
            "db.judge.en.par",
            "font.judge/en/meta_ot_cond_book.dds",
            "font.judge/en/meta_ot_cond_book_italic.dds"
        };

        private static readonly HashSet<string> AllowedFiles = new HashSet<string>(
            RequiredFiles.Concat(new string[] { "tarib_judgment.txt", "PATCH_INFO.txt", "SHA256SUMS.txt" }),
            StringComparer.OrdinalIgnoreCase);

        public InstallerForm()
        {
            Text = "مثبّت التعريب العربي - Judgment";
            Width = 760;
            Height = 590;
            MinimumSize = new Size(700, 520);
            StartPosition = FormStartPosition.CenterScreen;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(245, 247, 250);

            var root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(18);
            root.ColumnCount = 1;
            root.RowCount = 6;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            Controls.Add(root);

            var heading = new Label();
            heading.Text = "تثبيت التعريب العربي للعبة Judgment";
            heading.Dock = DockStyle.Fill;
            heading.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            heading.TextAlign = ContentAlignment.MiddleCenter;
            heading.ForeColor = Color.FromArgb(25, 42, 68);
            root.Controls.Add(heading, 0, 0);

            root.Controls.Add(BuildPathRow("مجلد اللعبة", gameBox, "اختيار مجلد اللعبة", BrowseGame), 0, 1);
            root.Controls.Add(BuildPathRow("ملف حزمة التعريب ZIP", packageBox, "اختيار الحزمة", BrowsePackage), 0, 2);

            var buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.FlowDirection = FlowDirection.RightToLeft;
            buttons.WrapContents = false;

            installButton.Text = "تثبيت التعريب";
            installButton.Width = 150;
            installButton.Height = 38;
            installButton.BackColor = Color.FromArgb(25, 105, 70);
            installButton.ForeColor = Color.White;
            installButton.FlatStyle = FlatStyle.Flat;
            installButton.Click += (s, e) => InstallPatch();
            buttons.Controls.Add(installButton);

            restoreButton.Text = "استعادة آخر نسخة احتياطية";
            restoreButton.Width = 205;
            restoreButton.Height = 38;
            restoreButton.Click += (s, e) => RestoreLatestBackup();
            buttons.Controls.Add(restoreButton);
            root.Controls.Add(buttons, 0, 3);

            logBox.Dock = DockStyle.Fill;
            logBox.Multiline = true;
            logBox.ReadOnly = true;
            logBox.ScrollBars = ScrollBars.Vertical;
            logBox.Font = new Font("Consolas", 9.5F);
            logBox.RightToLeft = RightToLeft.Yes;
            logBox.BackColor = Color.White;
            root.Controls.Add(logBox, 0, 4);

            var footer = new Label();
            footer.Dock = DockStyle.Fill;
            footer.Text = "الإصدار 1.0.0  |  ينشئ نسخة احتياطية قبل استبدال الملفات";
            footer.TextAlign = ContentAlignment.MiddleCenter;
            footer.ForeColor = Color.DimGray;
            root.Controls.Add(footer, 0, 5);

            Log("مرحباً. اختر مجلد اللعبة وحزمة التعريب ZIP للمتابعة.");
            Log("تأكد من إغلاق اللعبة قبل التثبيت.");
        }

        private Control BuildPathRow(string caption, TextBox box, string buttonText, EventHandler handler)
        {
            var panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 3;
            panel.RowCount = 2;
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var label = new Label();
            label.Text = caption;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleRight;
            panel.Controls.Add(label, 0, 0);
            panel.SetColumnSpan(label, 3);

            box.Dock = DockStyle.Fill;
            box.ReadOnly = true;
            box.RightToLeft = RightToLeft.No;
            panel.Controls.Add(box, 1, 1);

            var button = new Button();
            button.Text = buttonText;
            button.Dock = DockStyle.Fill;
            button.Click += handler;
            panel.Controls.Add(button, 2, 1);
            return panel;
        }

        private void BrowseGame(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "اختر مجلد اللعبة أو المجلد الذي يحتوي على مجلد Judgment";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    gameBox.Text = dialog.SelectedPath;
                    Log("تم اختيار مجلد اللعبة: " + dialog.SelectedPath);
                }
            }
        }

        private void BrowsePackage(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "اختر حزمة التعريب";
                dialog.Filter = "حزمة التعريب (*.zip)|*.zip";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    packageBox.Text = dialog.FileName;
                    Log("تم اختيار الحزمة: " + dialog.FileName);
                }
            }
        }

        private void InstallPatch()
        {
            string dataDir = ResolveDataDirectory(gameBox.Text);
            if (dataDir == null)
            {
                MessageBox.Show(this, "لم أتمكن من العثور على runtime\\media\\data أو ملف db.judge.en.par داخل المجلد المحدد.", "مجلد اللعبة غير صحيح", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(packageBox.Text))
            {
                MessageBox.Show(this, "اختر ملف حزمة التعريب ZIP أولاً.", "الحزمة غير محددة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Process.GetProcessesByName("Judgment").Length > 0)
            {
                MessageBox.Show(this, "أغلق لعبة Judgment تماماً ثم أعد المحاولة.", "اللعبة قيد التشغيل", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string stage = Path.Combine(Path.GetTempPath(), "JudgmentArabic_" + Guid.NewGuid().ToString("N"));
            string backup = null;
            try
            {
                Directory.CreateDirectory(stage);
                Dictionary<string, byte[]> files = ReadAndVerifyPackage(packageBox.Text);
                foreach (var pair in files)
                {
                    string staged = Path.Combine(stage, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(staged));
                    File.WriteAllBytes(staged, pair.Value);
                }

                string gameRoot = Directory.GetParent(Directory.GetParent(Directory.GetParent(dataDir).FullName).FullName).FullName;
                backup = Path.Combine(gameRoot, "_Arabic_Backups", DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
                Directory.CreateDirectory(backup);

                var backupManifest = new List<string>();
                foreach (var pair in files)
                {
                    string destination = Path.Combine(dataDir, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(destination))
                    {
                        string backupFile = Path.Combine(backup, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                        File.Copy(destination, backupFile, true);
                        backupManifest.Add("EXISTS|" + pair.Key);
                    }
                    else
                    {
                        backupManifest.Add("MISSING|" + pair.Key);
                    }
                }
                File.WriteAllLines(Path.Combine(backup, "backup_manifest.txt"), backupManifest.ToArray(), Encoding.UTF8);

                try
                {
                    foreach (var pair in files)
                    {
                        string destination = Path.Combine(dataDir, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(Path.GetDirectoryName(destination));
                        string staged = Path.Combine(stage, pair.Key.Replace('/', Path.DirectorySeparatorChar));
                        File.Copy(staged, destination, true);
                    }
                }
                catch
                {
                    RestoreFromBackup(dataDir, backup);
                    throw;
                }

                Log("اكتمل تثبيت التعريب بنجاح.");
                Log("النسخة الاحتياطية: " + backup);
                MessageBox.Show(this, "تم تثبيت ملفات التعريب بنجاح.\r\nتم حفظ الملفات الأصلية في:\r\n" + backup, "اكتمل التثبيت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("فشل التثبيت: " + ex.Message);
                MessageBox.Show(this, "تعذر تثبيت الحزمة:\r\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try { if (Directory.Exists(stage)) Directory.Delete(stage, true); } catch { }
            }
        }

        private static string ResolveDataDirectory(string selected)
        {
            if (String.IsNullOrWhiteSpace(selected) || !Directory.Exists(selected)) return null;
            string direct = Path.Combine(selected, "runtime", "media", "data");
            if (File.Exists(Path.Combine(direct, "db.judge.en.par"))) return direct;
            string nested = Path.Combine(selected, "Judgment", "runtime", "media", "data");
            if (File.Exists(Path.Combine(nested, "db.judge.en.par"))) return nested;
            return null;
        }

        private static Dictionary<string, byte[]> ReadAndVerifyPackage(string zipPath)
        {
            var result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    string name = entry.FullName.Replace('\\', '/').TrimStart('/');
                    if (String.IsNullOrEmpty(name) || name.EndsWith("/")) continue;
                    if (name.Contains("..") || name.Contains(":") || !AllowedFiles.Contains(name))
                        throw new InvalidDataException("الحزمة تحتوي على مسار أو ملف غير مسموح: " + name);
                    if (result.ContainsKey(name)) throw new InvalidDataException("ملف مكرر داخل الحزمة: " + name);
                    using (var input = entry.Open())
                    using (var memory = new MemoryStream())
                    {
                        input.CopyTo(memory);
                        result.Add(name, memory.ToArray());
                    }
                }
            }

            foreach (string required in RequiredFiles)
                if (!result.ContainsKey(required))
                    throw new InvalidDataException("الحزمة لا تحتوي على الملف المطلوب: " + required);

            if (!result.ContainsKey("SHA256SUMS.txt"))
                throw new InvalidDataException("ملف التحقق SHA256SUMS.txt غير موجود داخل الحزمة.");

            var checksums = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = new StringReader(Encoding.UTF8.GetString(result["SHA256SUMS.txt"])))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    int split = line.IndexOfAny(new char[] { ' ', '\t' });
                    if (split < 1) throw new InvalidDataException("تنسيق ملف SHA256SUMS.txt غير صحيح.");
                    string hash = line.Substring(0, split).Trim();
                    string file = line.Substring(split).Trim().TrimStart('*');
                    if (!AllowedFiles.Contains(file)) throw new InvalidDataException("اسم ملف غير متوقع في ملف التحقق: " + file);
                    checksums[file] = hash;
                }
            }

            foreach (string required in RequiredFiles)
            {
                if (!checksums.ContainsKey(required)) throw new InvalidDataException("لا يوجد SHA-256 للملف: " + required);
            }

            foreach (var pair in result)
            {
                if (pair.Key.Equals("SHA256SUMS.txt", StringComparison.OrdinalIgnoreCase) ||
                    pair.Key.Equals("PATCH_INFO.txt", StringComparison.OrdinalIgnoreCase)) continue;
                if (!checksums.ContainsKey(pair.Key)) throw new InvalidDataException("لا يوجد SHA-256 للملف: " + pair.Key);
                string actual;
                using (var sha = SHA256.Create())
                    actual = BitConverter.ToString(sha.ComputeHash(pair.Value)).Replace("-", "").ToLowerInvariant();
                if (!actual.Equals(checksums[pair.Key], StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("فشل التحقق من سلامة الملف: " + pair.Key);
            }
            return result.Where(p => !p.Key.Equals("SHA256SUMS.txt", StringComparison.OrdinalIgnoreCase) &&
                                     !p.Key.Equals("PATCH_INFO.txt", StringComparison.OrdinalIgnoreCase))
                         .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);
        }

        private void RestoreLatestBackup()
        {
            string dataDir = ResolveDataDirectory(gameBox.Text);
            if (dataDir == null)
            {
                MessageBox.Show(this, "حدد مجلد اللعبة أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Process.GetProcessesByName("Judgment").Length > 0)
            {
                MessageBox.Show(this, "أغلق اللعبة قبل الاستعادة.", "اللعبة قيد التشغيل", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string gameRoot = Directory.GetParent(Directory.GetParent(Directory.GetParent(dataDir).FullName).FullName).FullName;
            string backupRoot = Path.Combine(gameRoot, "_Arabic_Backups");
            if (!Directory.Exists(backupRoot))
            {
                MessageBox.Show(this, "لم يتم العثور على نسخ احتياطية.", "لا توجد نسخة احتياطية", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var latest = Directory.GetDirectories(backupRoot).OrderByDescending(Directory.GetCreationTimeUtc).FirstOrDefault();
            if (latest == null) return;
            if (MessageBox.Show(this, "استعادة الملفات الأصلية من النسخة التالية؟\r\n" + latest, "تأكيد الاستعادة", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                RestoreFromBackup(dataDir, latest);
                Log("تمت استعادة الملفات من: " + latest);
                MessageBox.Show(this, "تمت استعادة الملفات بنجاح.", "اكتملت الاستعادة", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("فشلت الاستعادة: " + ex.Message);
                MessageBox.Show(this, ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void RestoreFromBackup(string dataDir, string backup)
        {
            string manifest = Path.Combine(backup, "backup_manifest.txt");
            if (File.Exists(manifest))
            {
                foreach (string line in File.ReadAllLines(manifest, Encoding.UTF8))
                {
                    if (String.IsNullOrWhiteSpace(line)) continue;
                    int split = line.IndexOf('|');
                    if (split <= 0) continue;
                    string state = line.Substring(0, split);
                    string relative = line.Substring(split + 1).Replace('/', Path.DirectorySeparatorChar);
                    if (relative.Contains("..") || Path.IsPathRooted(relative))
                        throw new InvalidDataException("مسار غير صالح في النسخة الاحتياطية.");
                    string destination = Path.Combine(dataDir, relative);
                    if (state == "EXISTS")
                    {
                        string saved = Path.Combine(backup, relative);
                        if (!File.Exists(saved)) throw new FileNotFoundException("ملف النسخة الاحتياطية مفقود.", saved);
                        Directory.CreateDirectory(Path.GetDirectoryName(destination));
                        File.Copy(saved, destination, true);
                    }
                    else if (state == "MISSING" && File.Exists(destination))
                    {
                        File.Delete(destination);
                    }
                }
                return;
            }

            foreach (string file in Directory.GetFiles(backup, "*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(backup.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string destination = Path.Combine(dataDir, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(file, destination, true);
            }
        }

        private void Log(string message)
        {
            logBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
        }
    }
}
