﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace unBand.CloudHelpers
{
    /// <summary>
    ///     For now none of the properties call NotifyPropertyChanged as they are driven by the UI
    /// </summary>
    [Serializable]
    public class CloudDataExporterSettings : INotifyPropertyChanged
    {
        public CloudDataExporterSettings()
        {
            // set up default
            IncludeRuns = true;
            IncludeSleep = true;
            IncludeSteps = true;
            IncludeWorkouts = true;
            IncludeBiking = true;

            ExportAll = true;
            ExportCount = 100;

            ConvertDateTimeToLocal = true;

            OpenFileAfterExport = true;
        }

        public bool IncludeSteps { get; set; }
        public bool IncludeWorkouts { get; set; }
        public bool IncludeSleep { get; set; }
        public bool IncludeRuns { get; set; }
        public bool IncludeBiking { get; set; }

        /// <summary>
        ///     If true, overrides ExportCount
        /// </summary>
        public bool ExportAll { get; set; }

        public int ExportCount { get; set; }
        public bool ConvertDateTimeToLocal { get; set; }
        public bool OpenFileAfterExport { get; set; }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }));
            }
        }

        #endregion
    }
}