using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

[Authorize(Roles = "Teacher")]
public class BackupController : Controller
{
    private readonly string _backupFolder;
    private readonly string _connectionString;
    private readonly string _dbName;

    [HttpGet]
    public ActionResult Debug()
    {
        var info = new
        {
            FolderExists = Directory.Exists(_backupFolder),
            FolderPath = _backupFolder,
            SqlpackagePath = FindSqlpackage()
        };
        return Json(info);
    }

    private string FindSqlpackage()
    {
        try
        {
            var p = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "where", // use "which" on Linux
                    Arguments = "sqlpackage",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            p.Start();
            return p.StandardOutput.ReadToEnd();
        }
        catch (Exception ex) { return ex.Message; }
    }

    public BackupController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _connectionString = configuration.GetConnectionString("EaglesConnection");
        _dbName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
        _backupFolder = Path.Combine(env.ContentRootPath, "App_Data", "Backups");
        Directory.CreateDirectory(_backupFolder);
    }

    [HttpGet]
    public ActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public ActionResult GetBackups()
    {
        var files = Directory.GetFiles(_backupFolder, "*.bacpac")
                             .Select(Path.GetFileName)
                             .OrderByDescending(f => f)
                             .ToList();

        return Json(files);
    }

    [HttpPost]
    public ActionResult CreateBackup()
    {
        try
        {
            string fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bacpac";
            string backupPath = Path.Combine(_backupFolder, fileName);

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sqlpackage",
                    Arguments = $"/Action:Export /SourceConnectionString:\"{_connectionString}\" /TargetFile:\"{backupPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit(300000);

            return process.ExitCode == 0
                ? Json(new { success = true, message = $"Backup created: {fileName}" })
                : Json(new { success = false, message = "Backup failed: " + error });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Backup failed: " + ex.Message });
        }
    }

    [HttpPost]
    public ActionResult RestoreBackup(string fileName)
    {
        try
        {
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                return Json(new { success = false, message = "Invalid file name." });

            string backupPath = Path.Combine(_backupFolder, fileName);

            if (!System.IO.File.Exists(backupPath))
                return Json(new { success = false, message = "Backup file not found." });

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sqlpackage",
                    Arguments = $"/Action:Import /TargetConnectionString:\"{_connectionString}\" /SourceFile:\"{backupPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit(300000);

            return process.ExitCode == 0
                ? Json(new { success = true, message = $"Restored from: {fileName}" })
                : Json(new { success = false, message = "Restore failed: " + error });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Restore failed: " + ex.Message });
        }
    }
}