namespace NeoServiceLayer.Services.Storage.Configuration
{
    /// <summary>
    /// Configuration for database backup
    /// </summary>
    public class DatabaseBackupConfiguration
    {
        /// <summary>
        /// Gets or sets the backup interval in hours
        /// </summary>
        public int BackupIntervalHours { get; set; } = 24;

        /// <summary>
        /// Gets or sets the backup directory
        /// </summary>
        public string BackupDirectory { get; set; } = "./Backups";

        /// <summary>
        /// Gets or sets the number of backups to keep
        /// </summary>
        public int BackupsToKeep { get; set; } = 7;

        /// <summary>
        /// Gets or sets whether to upload backups to S3
        /// </summary>
        public bool UploadToS3 { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to delete local backups after uploading to S3
        /// </summary>
        public bool DeleteLocalBackupAfterUpload { get; set; } = false;
    }
}
