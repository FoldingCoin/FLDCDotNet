﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StatsDownload.Logging {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ErrorMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("StatsDownload.Logging.ErrorMessages", typeof(ErrorMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem with the user&apos;s data. The user &apos;{0}&apos; has a bitcoin address length of &apos;{1}&apos; and exceeded the max bitcoin address length. The user should shorten their bitcoin address. You should contact your technical advisor to review the logs and rejected users..
        /// </summary>
        internal static string BitcoinAddressExceedsMaxSize {
            get {
                return ResourceManager.GetString("BitcoinAddressExceedsMaxSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem downloading the file payload. There was a problem connecting to the data store. The data store is unavailable, ensure the data store is available and configured correctly and try again..
        /// </summary>
        internal static string DataStoreUnavailable {
            get {
                return ResourceManager.GetString("DataStoreUnavailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem connecting to the database. The database is unavailable, ensure the database is available and configured correctly and try again..
        /// </summary>
        internal static string DefaultDatabaseUnavailable {
            get {
                return ResourceManager.GetString("DefaultDatabaseUnavailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The database is missing required objects. Add the missing database objects and try again. You should contact your technical advisor to review the logs..
        /// </summary>
        internal static string DefaultMissingRequiredObjects {
            get {
                return ResourceManager.GetString("DefaultMissingRequiredObjects", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an unexpected exception. Check the log for more information..
        /// </summary>
        internal static string DefaultUnexpectedException {
            get {
                return ResourceManager.GetString("DefaultUnexpectedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem with the user&apos;s data. The user &apos;{0}&apos; has a FAH name length of &apos;{1}&apos; and exceeded the max FAH name length. The user should shorten their FAH name. You should contact your technical advisor to review the logs and rejected users..
        /// </summary>
        internal static string FahNameExceedsMaxSize {
            get {
                return ResourceManager.GetString("FahNameExceedsMaxSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem adding the user &apos;{0}&apos; to the database. Contact your technical advisor to review the logs and rejected users..
        /// </summary>
        internal static string FailedAddUserToDatabase {
            get {
                return ResourceManager.GetString("FailedAddUserToDatabase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem parsing a user from the stats file. The user &apos;{0}&apos; failed data parsing. You should contact your technical advisor to review the logs and rejected users..
        /// </summary>
        internal static string FailedParsingUserData {
            get {
                return ResourceManager.GetString("FailedParsingUserData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem uploading the file payload. The file passed validation but {0} lines failed; processing continued after encountering these lines. If this problem occurs again, then you should contact your technical advisor to review the logs and failed users..
        /// </summary>
        internal static string FailedUserDataCount {
            get {
                return ResourceManager.GetString("FailedUserDataCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem downloading the file payload. There was a problem connecting to the database. The database is unavailable, ensure the database is available and configured correctly and try again..
        /// </summary>
        internal static string FileDownloadDatabaseUnavailable {
            get {
                return ResourceManager.GetString("FileDownloadDatabaseUnavailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem decompressing the file payload. The file has been moved to a failed directory for review. If this problem occurs again, then you should contact your technical advisor to review the logs and failed files..
        /// </summary>
        internal static string FileDownloadFailedDecompression {
            get {
                return ResourceManager.GetString("FileDownloadFailedDecompression", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem downloading the file payload. The database is missing required objects. Add the missing database objects and try again. You should contact your technical advisor to review the logs..
        /// </summary>
        internal static string FileDownloadMissingRequiredObjects {
            get {
                return ResourceManager.GetString("FileDownloadMissingRequiredObjects", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem downloading the file payload. The file payload could not be found. Check the download URI configuration and try again. If this problem occurs again, then you should contact your technical advisor to review the logs..
        /// </summary>
        internal static string FileDownloadNotFound {
            get {
                return ResourceManager.GetString("FileDownloadNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem downloading the file payload. There was a timeout when downloading the file payload. If a timeout occurs again, then you can try increasing the configurable download timeout..
        /// </summary>
        internal static string FileDownloadTimedOut {
            get {
                return ResourceManager.GetString("FileDownloadTimedOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem downloading the file payload. There was an unexpected exception. Check the log for more information..
        /// </summary>
        internal static string FileDownloadUnexpectedException {
            get {
                return ResourceManager.GetString("FileDownloadUnexpectedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem with the user&apos;s data. The user &apos;{0}&apos; has a friendly name length of &apos;{1}&apos; and exceeded the max friendly name length. The user should shorten their friendly name. You should contact your technical advisor to review the logs and rejected users..
        /// </summary>
        internal static string FriendlyNameExceedsMaxSize {
            get {
                return ResourceManager.GetString("FriendlyNameExceedsMaxSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem uploading the file payload. The file failed validation; check the logs for more information. If this problem occurs again, then you should contact your technical advisor to review the logs and failed uploads..
        /// </summary>
        internal static string InvalidStatsFile {
            get {
                return ResourceManager.GetString("InvalidStatsFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem downloading the file payload. The file download service was run before the minimum wait time {0} or the configured wait time {1}. Configure to run the service less often or decrease your configured wait time and try again..
        /// </summary>
        internal static string MinimumWaitTimeNotMet {
            get {
                return ResourceManager.GetString("MinimumWaitTimeNotMet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem downloading the file payload. The required settings are invalid; check the logs for more information. Ensure the settings are complete and accurate, then try again..
        /// </summary>
        internal static string RequiredSettingsAreInvalid {
            get {
                return ResourceManager.GetString("RequiredSettingsAreInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem uploading the file payload. There was a problem connecting to the database. The database is unavailable, ensure the database is available and configured correctly and try again..
        /// </summary>
        internal static string StatsUploadDatabaseUnavailable {
            get {
                return ResourceManager.GetString("StatsUploadDatabaseUnavailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem uploading the file payload. The database is missing required objects. Add the missing database objects and try again. You should contact your technical advisor to review the logs..
        /// </summary>
        internal static string StatsUploadMissingRequiredObjects {
            get {
                return ResourceManager.GetString("StatsUploadMissingRequiredObjects", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem uploading the file payload. There was an unexpected database exception and the file has been marked rejected. If this problem occurs again, then you should contact your technical advisor to review the rejections and logs..
        /// </summary>
        internal static string StatsUploadTimeout {
            get {
                return ResourceManager.GetString("StatsUploadTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem uploading the file payload. There was an unexpected exception. Check the log for more information..
        /// </summary>
        internal static string StatsUploadUnexpectedException {
            get {
                return ResourceManager.GetString("StatsUploadUnexpectedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem validating the file. There was an unexpected exception while validating. Check the log for more information..
        /// </summary>
        internal static string UnexpectedValidationException {
            get {
                return ResourceManager.GetString("UnexpectedValidationException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem parsing a user from the stats file. The user &apos;{0}&apos; was in an unexpected format. You should contact your technical advisor to review the logs and rejected users..
        /// </summary>
        internal static string UserDataUnexpectedFormat {
            get {
                return ResourceManager.GetString("UserDataUnexpectedFormat", resourceCulture);
            }
        }
    }
}
