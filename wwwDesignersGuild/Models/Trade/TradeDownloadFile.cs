using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Trade
{
    public class TradeDownloadFileManagerSettings
    {
        public string RootFolder { get; set; }

        public TradeDownloadFileManagerSettings(string rootFolder)
        {
            RootFolder = rootFolder;
        }

        public static DevExpress.Web.Mvc.FileManagerSettings CreateFileManagerDownloadSettings()
        {
            var settings = new DevExpress.Web.Mvc.FileManagerSettings();

            settings.SettingsEditing.AllowDownload = true;

            settings.Name = "FileManager";

            settings.CallbackRouteValues = new { Controller = "downloads", Action = "pricelists-brochures-grid" };

            settings.ClientSideEvents.BeginCallback = @"function (s, e) { 
                e.customArgs['objMaster'] = getMasterDefinition();
                e.customArgs['countryId'] = getCountryId();
                e.customArgs['languageId'] = getLanguageId();
            }";

            settings.DownloadRouteValues = new { Controller = "downloads", Action = "pricelists-brochures-grid-download-file" };

            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);

            settings.Settings.InitialFolder = "1) Pricelists";
            settings.Settings.ThumbnailFolder = "~\\Thumb\\";

            settings.SettingsFileList.ThumbnailsViewSettings.ThumbnailWidth = 100;
            settings.SettingsFileList.ThumbnailsViewSettings.ThumbnailHeight = 100;

            settings.SettingsToolbar.ShowPath = false;

            settings.Settings.AllowedFileExtensions = new string[] { ".pdf", ".xls", ".xlsx", ".jpg", ".jpeg", "png" };

            settings.SettingsEditing.AllowCreate = false;
            settings.SettingsEditing.AllowDelete = false;
            settings.SettingsEditing.AllowMove = false;
            settings.SettingsEditing.AllowRename = false;

            settings.SettingsEditing.AllowDownload = true;
            settings.SettingsUpload.Enabled = false;
            settings.Settings.EnableMultiSelect = true;

            settings.Styles.FolderContainer.Width = System.Web.UI.WebControls.Unit.Pixel(300);

            settings.SettingsFileList.DetailsViewSettings.Columns.Add(FileInfoType.Thumbnail, " ");
            settings.SettingsFileList.DetailsViewSettings.Columns.Add(FileInfoType.FileName, "Name");

            settings.SettingsToolbar.Items.Add(new FileManagerToolbarDownloadButton() { Text = "Download" });

            settings.SettingsFileList.View = FileListView.Details;

            return settings;
        }
    }

    public class TradeDownloadFileSystemProvider : FileSystemProviderBase
    {
        public TradeDownloadFileSystemProvider(string rootFolder) : base(rootFolder)
        {

        }

        public override IEnumerable<FileManagerFile> GetFiles(FileManagerFolder folder)
        {
            return Directory.GetFiles(folder.FullName, "*.*", SearchOption.TopDirectoryOnly).Select(p =>
                new FileManagerFile(this, Path.GetFileName(p)));
        }

        public override IEnumerable<FileManagerFolder> GetFolders(FileManagerFolder parentFolder)
        {
            return Directory.EnumerateDirectories(parentFolder?.FullName ?? "", "*.*", SearchOption.AllDirectories).Where(p => !p.Contains("internal")).Select(p =>
                new FileManagerFolder(this, Path.GetFileName(p)));
        }
        public override bool Exists(FileManagerFile file)
        {
            return Directory.Exists(file.FullName);
        }
        public override bool Exists(FileManagerFolder folder)
        {
            return Directory.Exists(folder.FullName);
        }
        public override System.IO.Stream ReadFile(FileManagerFile file)
        {
            return new MemoryStream(File.ReadAllBytes(file.FullName));
        }
    }
}