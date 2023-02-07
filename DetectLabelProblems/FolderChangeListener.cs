using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectLabelProblems
{
    public class FolderChangeListener : BackgroundWorker
    {
        private string m_lFolderName;
        public string FolderName {
            get { return m_lFolderName; }
            set
            {
                m_lFolderName = value;
                LoadSubfolderList();
            }
                
        }
        public List<string> Subfolders { get; set; }
        public bool Stop { get; set; }

        public FolderChangeListener()
        {
            Stop = false;
        }


        public void LoadSubfolderList()
        {
            Subfolders = GetDateTimeFolderList();            
        }

        
        private List<string> GetDateTimeFolderList()
        {
            List<string> lFolders = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(m_lFolderName);
            foreach(DirectoryInfo dDate in dir.GetDirectories())
                foreach(DirectoryInfo dTime in dDate.GetDirectories())
                    lFolders.Add(dTime.FullName);
            return lFolders;
        }

        public void Run(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("Listener started");
            while (!Stop)
            {
                Debug.WriteLine("Running checking new folder");
                List<string> lFolders = GetDateTimeFolderList();
                foreach(string sFolder in lFolders)
                {
                    if(!Subfolders.Contains(sFolder))
                    {
                        Debug.WriteLine("Folder detector found new folder: " + sFolder);

                        Subfolders.Add(sFolder);
                        ReportProgress(1);
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
