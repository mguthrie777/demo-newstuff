//Test-Github Binary file detection - Melek
using System;
using System.Drawing;
//using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using SingleInstance;
using SORTspace;
using Microsoft.Win32;
using BrightIdeasSoftware;
using DirectShowLib;
using DirectShowLib.DMO;

//Melek iTextSharp free pdf library
using iTextSharp.text;
using iTextSharp.text.pdf;
//using System.Web.Mail; //Melek
//using System.Net; //Email - Melek
//using System.Net.Mail; //Melek
//using CrystalDecisions.CrystalReports.Engine; //Crystal report  - Melek
//using System.Configuration;
//using DexterLib;
//using System.Collections.Specialized; //Melek
//using System.Text.RegularExpressions;//Melek
//using System.Configuration;//Melek
//using SortableBindingList;

// could be used to give permission to write in the registry
//using System.Security.Permissions;
//[assembly:SecurityPermissionAttribute(SecurityAction.RequestMinimum, Flags = SecurityPermissionFlag.UnmanagedCode)]
//[assembly:RegistryPermissionAttribute(SecurityAction.RequestMinimum, ViewAndModify="HKEY_CURRENT_USER")]
//using System.Reflection; //Melek 

namespace qavod
{
    public enum PlayState
    {
        Stopped,
        Paused,
        Running,
        Init
    };

    internal enum MediaType
    {
        Audio,
        Video
    }

    /// <summary>
    /// Generate and updates a windows form used 
    /// to dynamically run the QA tool on avc clips.
    /// and display the results graphically
    /// </summary>
    /// 
    public partial class GUI : System.Windows.Forms.Form
    {
        /// <summary>
        /// Clean up any resources being used.		/// </summary>
        protected override void Dispose(bool disposing)
        {
            //Melek-Remove Enent Handlers-Memory allocation
            //qatool.RemoveJobEvent -= new qatoolEventH(RemoveJobFromList);
            //qatool.EditJobEvent -= new qatoolEventH(EditJobInList);
            //qatool.AddJobEvent -= new qatoolEventH(AddJobToList);
            //FoldersManager.NewProfileForJobEvent -= new qatoolEventH(EditJobInList);
            //JobManager.RefreshJobEvent -= new qatoolEventH(EditJobInList);
            //JobManager.RefreshCoresEvent -= new qatoolEventH2(updategui);

            ////qatool.NewStreamListEvent         += new qatoolEventH (onNewStreamList);
            //qatool.RemoveSummaryEvent -= new qatoolEventH(RemoveSummaryFromList);
            //qatool.EditSummaryEvent -= new qatoolEventH(EditSummaryInList);
            //qatool.AddSummaryEvent -= new qatoolEventH(AddSummaryToList);

            //Global.NewGuiLogEvent -= new qatoolEventH(GuilogEvent);

            //qatool.JobListButtonsEvent -= new qatoolEventH(updatejoblistbuttons);
            //qatool.UpdateGuiEvent -= new qatoolEventH2(updategui);
            //qaClass.UpdateGuisourceFoldersEvent -= new qatoolEventH2(UpdateFoldersGui);
            //WatchedFolder.UpdateGUIFoldersEvent -= new qatoolEventH2(UpdateFoldersGui);

            // Also save GUI settings on exit
            GUIsettings.SetResultColumns(ResultListView.AllColumns);
            GUIsettings.SetFailedResultColumns(FailedResultListView.AllColumns); //Melek 
            GUIsettings.SetJoblisttColumns(JobListView.AllColumns);
            GUIsettings.ResultListDecisionColour = this.DecisionColoredRow.Checked;
            GUIsettings.FailedResultListDecisionColour = this.FailedDecisionColoredRow.Checked; //Melek
            GUIsettings.ResultListGraphics = this.ownerDrawCheckBox.Checked;
            GUIsettings.FailedResultListGraphics = this.ownerDrawCheckBox.Checked;//Melek
            GUIsettings.InterfacePosition = this.Location;
            // GUIsettings.InterfacePosition = new System.Drawing.Point(0, 0);//Melek
            GUIsettings.InterfaceSize = this.Size;
            //GUIsettings.InterfaceSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size; // Retrieve the screen resolution to display full screen GUI - Melek


            GUIsettings.Save();

            if (File.Exists("GUIsettings.xml")) //Melek - Delete GUIsettings file to resolve the joblist column name issue
            { File.Delete("GUIsettings.xml"); }

            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);

            // monitor thread is permanentely working so we dont need to test if it's running
            qatoolThread.Abort();
            #region kill threads - Melek
            Process[] pArry = Process.GetProcesses();
            foreach (System.Diagnostics.Process p in pArry) //Kill the processes
            {
                try
                {
                    string s = p.ProcessName;

                    if (s.CompareTo("demux") == 0)
                    {
                        p.Kill();
                    }

                    else if (s.CompareTo("ffmpeg") == 0)
                    {
                        p.Kill();
                    }
                    else if (s.CompareTo("MP4Box") == 0)
                    {
                        p.Kill();
                    }
                    else if (s.CompareTo("BTqaMPEG2") == 0)
                    {
                        p.Kill();
                    }
                    else if (s.CompareTo("BTqaAVC") == 0)
                    {
                        p.Kill();
                    }
                    else if (s.CompareTo("7za") == 0)
                    {
                        p.Kill();
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
            try //Melek - Remove the QAcache folder
            {
                string d = qaClass.settings.CacheFolder;
                if (Directory.Exists(d)) Directory.Delete(d, true);
            }
            catch (Exception ex)
            {
                Global.log("Problem deleting QAcache folder.");
            }

            #endregion
            qatool.dispose();

            CleanupHungProcesses();

            Environment.Exit(0);

        }

        private void CleanupHungProcesses()
        {
            /* This code can be uncommented to help prevent leaving hung processes on the system.
             * The main reason for commenting out is because on a developers setup, it could
             * interfere with ffmpeg or btutil processes created outside of v.Cortex
             * 
            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                string pname = p.ProcessName.ToLower();
                if (pname == "ffmpeg" || pname == "mp4box" ||
                     pname == "tonamedpipe" ||
                     pname == "btqaavc" || pname == "btqampeg2")
                {
                    try
                    {
                        p.Kill();
                    }
                    catch (Exception) { }
                }
            }
             */
        }

        //public DataTable jobsTable;

        public string reportsfoldertemp; //Melek

        public bool reportsfolderchangedtemp=false;//Melek
        #region DSHOW objects etc.

        private const int WMGraphNotify = 0x0400 + 13;
        private const int VolumeFull = 0;
        private const int VolumeSilence = -10000;

        // dmo stuff ...
        private IMediaParams m_param = null;
        private DirectShowLib.IGraphBuilder graphBuilder = null;
        private IMediaControl mediaControl = null;
        private IMediaEventEx mediaEventEx = null;
        private IVideoWindow videoWindow = null;
        private IBasicAudio basicAudio = null;
        private IBasicVideo basicVideo = null;
        private IMediaSeeking mediaSeeking = null;
        private IMediaPosition mediaPosition = null;
        private IVideoFrameStep frameStep = null;

        private string filename = string.Empty;
        //private bool isAudioOnly = false;
        //private bool isFullScreen = false;
        private int currentVolume = VolumeFull;
        private PlayState currentState = PlayState.Stopped;
        private double currentPlaybackRate = 1.0;

        private IntPtr hDrain = IntPtr.Zero;
        //private Button LogClearButton;

#if DEBUG
        private DsROTEntry rot = null;
#endif

        #endregion

        #region DLL imported, user32,

        // retreive current user name
        // define the dll as well as the method to use
        //[DllImport("Advapi32.dll", EntryPoint = "GetUserName", ExactSpelling = false, SetLastError = true)]
        //This specifies the exact method we are going to call from within our imported .dll
        //static extern bool GetUserName(
        //    [MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer,
        //    [MarshalAs(UnmanagedType.LPArray)] Int32[] nSize);

        // bring a window to the front
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        // round rectangle forms
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,      // x-coordinate of upper-left corner
            int nTopRect,       // y-coordinate of upper-left corner
            int nRightRect,     // x-coordinate of lower-right corner
            int nBottomRect,    // y-coordinate of lower-right corner
            int nWidthEllipse,  // height of ellipse
            int nHeightEllipse  // width of ellipse
        );
        #endregion

        #region Columns style
        HeaderStateStyle headerStateStyle1 = new HeaderStateStyle(), headerStateStyle2 = new HeaderStateStyle(), headerStateStyle3 = new HeaderStateStyle();
        HeaderFormatStyle headerFormatStyleData = new HeaderFormatStyle();
        #endregion


        // these are used to enforce single instance
        private static Event evnt;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //string dllpath=@"C:\Users\QAvod-1\Desktop\12142010 v.cortex\v.Cortex---Full-Source-Code\bin\Debug\Tools\BTqaMPEG2.exe"; //Melek -learn if the exe's and dlls are x86 or 64-bit
            //GetDllMachineType(dllpath);//Melek

            // this class provides single instance feature
            evnt = new Event();

            // already exists means another instance is running
            if (evnt.EventAlreadyExists())
            {
                // signal the other instance to come to foreground
                evnt.SignalEvent();
                MessageBox.Show("v.Cortex is already running");

            }
            else
            {

                //if (!File.Exists(Environment.SystemDirectory + "\\p.txt")) //Melek - secucrity check
                //  if (!File.Exists("C:\\Windows\\twain_32 .dll.bak")) 
                //{
                //    MessageBox.Show("Unauthorized Access!");
                //    Environment.Exit(0);              
                //}
                // otherwise start normally
                // Check required filters are installed
               // Prerequisites PreReq = new Prerequisites();
                //if (!PreReq.Check())//Melek - disabled
                //{ }   // Environment.Exit(0);//Melek do not exit if the filters did not registered

                GUI f = new GUI();
                // important: access the Handle so .net will create it
                IntPtr dummy = f.Handle;
                f.eventSignalled = new EventSignalledHandler(f.evnt_EventSignalled);
                evnt.SetObject(f);
                evnt.SignalEvent();
                Global gl = new Global();

                BTDecoderWarnings.init();

                Application.EnableVisualStyles(); // gives the current xp style to all form elements whose flatsyle is set to system
                try
                {
                    Application.Run();
                }
                catch (Exception ex)
                {
                    Global.log("Unexpected general exception.\n" + ex);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////MElek\
        //public static MachineType GetDllMachineType(string dllPath)
        //{
        //    //see http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx 
        //    //offset to PE header is always at 0x3C 
        //    //PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00 
        //    //followed by 2-byte machine type field (see document above for enum) 
        //    FileStream fs = new FileStream(dllPath, FileMode.Open);
        //    BinaryReader br = new BinaryReader(fs);
        //    fs.Seek(0x3c, SeekOrigin.Begin);
        //    Int32 peOffset = br.ReadInt32();
        //    fs.Seek(peOffset, SeekOrigin.Begin);
        //    UInt32 peHead = br.ReadUInt32();
        //    if (peHead != 0x00004550) // "PE\0\0", little-endian 
        //        throw new Exception("Can't find PE header");
        //    MachineType machineType = (MachineType)br.ReadUInt16();
        //    br.Close();
        //    fs.Close();
        //    return machineType;
        //}
        //public enum MachineType : ushort
        //{
        //    IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
        //    IMAGE_FILE_MACHINE_AM33 = 0x1d3,
        //    IMAGE_FILE_MACHINE_AMD64 = 0x8664,
        //    IMAGE_FILE_MACHINE_ARM = 0x1c0,
        //    IMAGE_FILE_MACHINE_EBC = 0xebc,
        //    IMAGE_FILE_MACHINE_I386 = 0x14c,
        //    IMAGE_FILE_MACHINE_IA64 = 0x200,
        //    IMAGE_FILE_MACHINE_M32R = 0x9041,
        //    IMAGE_FILE_MACHINE_MIPS16 = 0x266,
        //    IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
        //    IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
        //    IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
        //    IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
        //    IMAGE_FILE_MACHINE_R4000 = 0x166,
        //    IMAGE_FILE_MACHINE_SH3 = 0x1a2,
        //    IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
        //    IMAGE_FILE_MACHINE_SH4 = 0x1a6,
        //    IMAGE_FILE_MACHINE_SH5 = 0x1a8,
        //    IMAGE_FILE_MACHINE_THUMB = 0x1c2,
        //    IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
        //}
        //public static bool? UnmanagedDllIs64Bit(string dllPath)
        //{
        //    switch (GetDllMachineType(dllPath))
        //    {
        //        case MachineType.IMAGE_FILE_MACHINE_AMD64:
        //        case MachineType.IMAGE_FILE_MACHINE_IA64:
        //            return true;
        //        case MachineType.IMAGE_FILE_MACHINE_I386:
        //            return false;
        //        default:
        //            return null;
        //    }
        //}
        //////////////////////////////////////////////////////////////////////////////////////////////

        // this is called by a seperate thread created by the Event class
        // Invoke does the cross thread magic for us
        public void evnt_EventSignalled()
        {
            Show();
            // WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
            // XP will flash the 1st instance instead of bring
            // to foreground, unless the 1st instance is hidden
            // W2K brings to foreground, unless the window is hidden
            // in which case it shows but doesn't highlight title bar
            SetForegroundWindow(Handle);
        }

        // These delegates enable asynchronous calls for updating the gui info.
        delegate void ThreadUpdateCallback(object Sender, EventArgs e);
        delegate void UpdateCallback();

        // classes and threads declaration
        private qaClass qatool;	        // instance of the ScanClass class
        private Thread qatoolThread;	// instance of the thread that is going to monitor streams and result files

        // Result List counters values
        int rejected = 0, passed = 0, borderline = 0;
        long totalstreamduration = 0;

        // Settings		
        int streamsCurrentlyOnServer = 0;         // number of streams currently on server

        // Lists in GUI (using Objectlistview dll)
        private DataListView ResultListView;
        //private FastObjectListView ResultListView;
        private DataListView FailedResultListView; //Melek
        public DataListView JobListView;

        // binding list to automate the list updates
        //public static BindingList<Job> GjobList;
        //public static BindingList<VStream> GsummaryList;
        public static List<VStream> SummaryList;
        //public static BindingList<VStream> SummaryBindingList;

        //public static BrightIdeasSoftware.FastObjectListDataSource SummaryList;


        //ResultListView = new BrightIdeasSoftware.DataListView();
        //ResultListView = new BrightIdeasSoftware.FastObjectListView();
        static bool summaryListIsDirty = false;

        public VStream vs;	// stream to use only for the current stream displayed in detailed view
        public VStream vs1; //Melek - Show selected file's info and make sure its not the one which you are playing 
        public string streampath, rf1, sum;

        //Melek starts
        public bool fileattached = false;
        public bool firsttimemailsent = true;
        public bool GenerateReportResultsViewClicked = false;
        public bool GenerateReportButtonReviewViewClicked = false;
        public string reportname;
        public bool morethanoneattachment = false;
        public bool docopened = false;
        public bool reportdatedisplayed = false;
        public bool defaultbackcolor = false; //To change the control default backcolor to silver - Melek
        //public bool enablefilter = true; //Review Tab- Monitor streams - Enable -Disable filter settings - Melek
        //Melek ends
        bool isTimer = false;

        private ListViewItemComparer _lvwItemComparer; 		        // The StreamListView Sorter
        private ListViewItemComparer _lvwItemComparerBadScenes; 	// The BadSceneListView Sorter
        private ListViewItemComparer.JobListItemComparer _lvwJobItemComparer;   // joblist sorter

        public GuiSettings GUIsettings;

        public void InitColumnsStyle()
        {
            headerStateStyle1.BackColor = Color.FromArgb(64, 64, 64);
            headerStateStyle1.ForeColor = System.Drawing.Color.White;
            headerStateStyle2.BackColor = Color.FromArgb(0, 0, 0);
            headerStateStyle2.ForeColor = System.Drawing.Color.Gainsboro;
            headerStateStyle3.BackColor = Color.FromArgb(128, 128, 128);
            headerStateStyle3.ForeColor = System.Drawing.Color.White;
            headerStateStyle3.FrameColor = System.Drawing.Color.WhiteSmoke;
            headerStateStyle3.FrameWidth = 2F;
            headerFormatStyleData.Normal = headerStateStyle2;
            headerFormatStyleData.Hot = headerStateStyle1;
            headerFormatStyleData.Pressed = headerStateStyle3;
        }


        public GUI()
        {

            InitializeComponent();
            InitColumnsStyle();

            // look for logo in current directory
            //this.logo_webbrowser.Url = new System.Uri(Directory.GetCurrentDirectory() + "\\Tools\\small_bt_logo.gif", System.UriKind.Absolute);

            //Version vrs = new Version(Application.ProductVersion);

            // display a round edged rectangle. doesnt trace/work with borders though.
            //TestRoundRectanglePanel.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, TestRoundRectanglePanel.Width - 10, TestRoundRectanglePanel.Height - 10, 20, 20));
            UpdateMainTitle();

            // The ListViewItemSorter property allows you to specify the object that performs the sorting of items in the ListView.
            // You can use the ListViewItemSorter property in combination with the Sort method to perform custom sorting. 
            _lvwItemComparer = new ListViewItemComparer();

            _lvwItemComparerBadScenes = new ListViewItemComparer();
            BadSceneListView.ListViewItemSorter = _lvwItemComparerBadScenes;

            _lvwJobItemComparer = new ListViewItemComparer.JobListItemComparer();

            //GsummaryList = new BindingList<VStream>();
            SummaryList = new List<VStream>();
            //SummaryBindingList = new BindingList<VStream>();

            //GjobList = new BindingList<Job>();
            padlock = new object();
            jobList = new BindingList<Job>();

            CloseInterfaces();

            #region Threading / events init code. // setup events and the thread scanning the stream folders

            // init the qavod class

            qatool = new qavod.qaClass();

            // Wire up the qaclass event so we can respond to them when they fire
            //qatool.NewJobListEvent            += new qatoolEventH (onNewJobList);
            qatool.RemoveJobEvent += new qatoolEventH(RemoveJobFromList);
            qatool.EditJobEvent += new qatoolEventH(EditJobInList);
            qatool.AddJobEvent += new qatoolEventH(AddJobToList);
            FoldersManager.NewProfileForJobEvent += new qatoolEventH(EditJobInList);
            JobManager.RefreshJobEvent += new qatoolEventH(EditJobInList);
            JobManager.RefreshCoresEvent += new qatoolEventH2(updategui);

            //qatool.NewStreamListEvent         += new qatoolEventH (onNewStreamList);
            qatool.RemoveSummaryEvent += new qatoolEventH(RemoveSummaryFromList);
            qatool.EditSummaryEvent += new qatoolEventH(EditSummaryInList);
            qatool.AddSummaryEvent += new qatoolEventH(AddSummaryToList);

            Global.NewGuiLogEvent += new qatoolEventH(GuilogEvent);

            qatool.JobListButtonsEvent += new qatoolEventH(updatejoblistbuttons);
            qatool.UpdateGuiEvent += new qatoolEventH2(updategui);
            qaClass.UpdateGuisourceFoldersEvent += new qatoolEventH2(UpdateFoldersGui);
            WatchedFolder.UpdateGUIFoldersEvent += new qatoolEventH2(UpdateFoldersGui);

            refreshListTimer.Interval = 1000;
            refreshListTimer.Enabled = true;

            ResultListView = new BrightIdeasSoftware.DataListView();
            //ResultListView = new BrightIdeasSoftware.FastObjectListView();
            FailedResultListView = new BrightIdeasSoftware.DataListView();
            JobListView = new BrightIdeasSoftware.DataListView();

            // load gui settings
            GUIsettings = new GuiSettings();
            GUIsettings = GUIsettings.Load();
            

            // set joblist view bindings
            SetJoblistBinding();

            // set results view bindings
            SetResultListBinding();
            SetFailedResultListBinding(); //Melek

            // Restore GUI settings
            RestoreGUIsettings();


            // Set up and run the qavod thread and start monitoring streams
            qatoolThread = new Thread(new ThreadStart(qatool.MonitorStreams)) { Name = "Monitor Streams" };
            qatoolThread.Start();

            #endregion


        }

        //~GUI() { } //Melek - destructor - Memory allocation

        static object padlock;
        static BindingList<Job> jobList;

        public static List<Job> ReadOnlyJobs
        {
            get
            {
                lock (padlock)
                {
                    return jobList.ToList();
                }
            }
        }

        public static void AddJob(Job job)
        {
            lock (padlock)
            {
                //if(!job.pathdeleted)
                //{
                jobList.Add(job);
                Global.debuglog(string.Format("Add Job: {0}", job.StreamFileName));
                // }

            }
        }

        public static void AddJob(IEnumerable<Job> jobs)
        {
            lock (padlock)
            {
                foreach (var job in jobs)
                {
                    //if (!job.pathdeleted)
                    //     {
                    jobList.Add(job);
                    Global.debuglog(string.Format("Add Job(s): {0}", job.StreamFileName));
                    //}
                }
            }
        }

        public static bool RemoveJob(Job job)
        {
            lock (padlock)
            {
                bool retVal = jobList.Remove(job);
                Global.log("Remove Job " + job.StreamFileName);
                //if (retVal) Global.debuglog(string.Format("RemoveJob: {0}", job.StreamFileName));
                //else Global.debuglog(string.Format("RemoveJob: {0} FAILED", job.StreamFileName));
                return retVal;
            }
        }

        public static void RemoveJobs(IEnumerable<Job> jobs)
        {
            lock (padlock)
            {
                foreach (var job in jobs)
                {
                    bool retVal = jobList.Remove(job);
                    //if (retVal) Global.debuglog(string.Format("RemoveJob: {0}", job.StreamFileName));
                    //else Global.debuglog(string.Format("RemoveJob: {0} FAILED", job.StreamFileName));
                }
            }
        }

        public static void AddVStream(VStream vStream)
        {
            lock (padlock)
            {
             //   Global.log(string.Format("AddVStream: {0}", vStream.StreamFileName));
                SummaryList.Add(vStream);

                //VStreamActionList.Add(new AddRemoveVStreamItem(){ VStream = vStream, IsAdd = true});

                summaryListIsDirty = true;
            }
        }

        public static void RemoveVStream(VStream vStream)
        {
            lock (padlock)
            {
               // Global.log(string.Format("RemoveVStream: {0}", vStream.StreamFileName));
                SummaryList.Remove(vStream);

                //VStreamActionList.Add(new AddRemoveVStreamItem() { VStream = vStream, IsAdd = false });

                summaryListIsDirty = true;
            }
        }

        //internal class AddRemoveVStreamItem
        //{
        //    public VStream VStream { get; set; }
        //    public bool IsAdd { get; set; }
        //}

        //static List<AddRemoveVStreamItem> VStreamActionList = new List<AddRemoveVStreamItem>();

        public static bool UpdateVStream(VStream oldItem, VStream newItem)
        {
            lock (padlock)
            {
               // Global.log(string.Format("UpdateVStream: {0}", oldItem.StreamFileName));
                int index = GUI.SummaryList.IndexOf(oldItem);
                if (index == 1) return false;

                SummaryList[index] = newItem;

                //// Also update the item in the BindingList
                //index = SummaryBindingList.IndexOf(oldItem);
                //SummaryBindingList[index] = newItem;

                return true;
            }
        }

        public static void ClearVStreamList()
        {
            lock (padlock)
            {
                SummaryList.Clear();
                summaryListIsDirty = true;
            }
        }

        //
        // Summary:
        //     Retrieves all the elements that match the conditions defined by the specified
        //     predicate.
        //
        // Parameters:
        //   match:
        //     The System.Predicate<T> delegate that defines the conditions of the elements
        //     to search for.
        //
        // Returns:
        //     A System.Collections.Generic.List<T> containing all the elements that match
        //     the conditions defined by the specified predicate, if found; otherwise, an
        //     empty System.Collections.Generic.List<T>.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     match is null.
        public static IEnumerable<VStream> FindAllVStream(Predicate<VStream> match)
        {
            //try//Melek
            //{
               lock (padlock)
                {
                    return SummaryList.FindAll(match).ToArray();
                }
            //}
            //catch(Exception ex)
            //{
            //   // return SummaryList.FindAll(match).ToArray();
            //} //Melek
        }
       

        //
        // Summary:
        //     Searches for an element that matches the conditions defined by the specified
        //     predicate, and returns the first occurrence within the entire System.Collections.Generic.List<T>.
        //
        // Parameters:
        //   match:
        //     The System.Predicate<T> delegate that defines the conditions of the element
        //     to search for.
        //
        // Returns:
        //     The first element that matches the conditions defined by the specified predicate,
        //     if found; otherwise, the default value for type T.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     match is null.
        public static VStream FindVStream(Predicate<VStream> match)
        {
            lock (padlock)
            {
                return SummaryList.Find(match);
            }
        }

        public void RestoreGUIsettings()
        {
            if (GUIsettings.InterfacePosition != null)
                this.Location = GUIsettings.InterfacePosition;
            //else
            // GUIsettings.InterfacePosition = new System.Drawing.Point(0, 0); //Melek

            if (GUIsettings.InterfaceSize != null)
            {

                // this.Size = GUIsettings.InterfaceSize; //original
                //this.Size = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;//--Melek screensize
                int newheight = (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
                int newwidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                this.Width = newwidth;
                this.Height = newheight;
            }

            // Load columns settings (columns position&order) if available
            ResultListView.AllColumns = GUIsettings.GetResultColumns(ResultListView.AllColumns);
            FailedResultListView.AllColumns = GUIsettings.GetFailedResultColumns(FailedResultListView.AllColumns); //Melek
            JobListView.AllColumns = GUIsettings.GetJobListColumns(JobListView.AllColumns);

            this.DecisionColoredRow.Checked = GUIsettings.ResultListDecisionColour;
            this.FailedDecisionColoredRow.Checked = GUIsettings.FailedResultListDecisionColour; //Melek
            this.ownerDrawCheckBox.Checked = GUIsettings.ResultListGraphics;
            this.FailedownerDrawCheckBox.Checked = GUIsettings.FailedResultListGraphics; //Melek
            
        }


        #region Job/Summary handling

        /// <summary>
        /// Define Result DataListView, columnsm, binding
        /// </summary>
        public void SetResultListBinding()
        {
            ResultListView.MultiSelect = true;//Melek
            //ResultListView.DataBindings.
            ResultListView.SmallImageList = this.imageList1;
            ResultListView.RowHeight = 20;

            ResultListView.Location = new System.Drawing.Point(8, 101);
            ResultListView.Size = new System.Drawing.Size(1250, 585);
            ResultListView.BackColor = System.Drawing.Color.Gainsboro;

            //ResultListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)
            //            | System.Windows.Forms.AnchorStyles.Right)));
            //MonitoringTab.Controls.Add(ResultListView);
            ResultsView_Panel.Controls.Add(ResultListView);
            ResultListView.Dock = DockStyle.Fill;
            ResultListView.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.ResultListView_FormatRow);
            // ResultListView.FormatRow -= new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.ResultListView_FormatRow);//Melek - remove the eventhandler - Memory Allocation
            // handle selection of a stream
            ResultListView.EmptyListMsg = "No result at the moment.";
            ResultListView.AllowColumnReorder = true;

            #region defines all columns of result view

            //OLVColumn RColJobID = new OLVColumn();
            //RColJobID.AspectName = "JobID"; // name of the get parameter to get the value from
            //RColJobID.HeaderFont = null;
            //RColJobID.IsTileViewColumn = true;
            //RColJobID.Text = "Job ID";
            //RColJobID.Width = 60;
            //ResultListView.AllColumns.Add(RColJobID);

            OLVColumn RColStreamName = new OLVColumn();
            RColStreamName.AspectName = "StreamFileName";
            RColStreamName.HeaderFont = null;
            RColStreamName.IsTileViewColumn = true;
            RColStreamName.Text = "Name";
            RColStreamName.Width = 510;
            RColStreamName.ImageGetter = delegate(object row)
            {
                switch (((VStream)row).Final_Decision.Made)
                {
                    case Decision.FAIL:
                        return "lred16.png";
                    case Decision.BORDERLINE:
                        return "lamber16.png";
                    case Decision.PASS:
                        return "lgreen16.png";
                    default:
                        return "lgrey16.png";
                }
            };


            ResultListView.AllColumns.Add(RColStreamName);


            OLVColumn RProfile = new OLVColumn();
            RProfile.AspectName = "QualityProfile";
            RProfile.HeaderFont = null;
            RProfile.IsTileViewColumn = true;
            RProfile.Text = "Quality Profile";
            RProfile.Width = 100;
            ResultListView.AllColumns.Add(RProfile);

            OLVColumn RAvMOSstd = new OLVColumn();
            RAvMOSstd.AspectName = "AvMOSStdDev";
            RAvMOSstd.HeaderFont = null;
            RAvMOSstd.IsTileViewColumn = true;
            RAvMOSstd.Text = "Av.MOS (Std.dev)";
            RAvMOSstd.Width = 80;
            ResultListView.AllColumns.Add(RAvMOSstd);


            OLVColumn RColBadPerc = new OLVColumn();
            RColBadPerc.AspectName = "BadPerc";
            RColBadPerc.HeaderFont = null;
            RColBadPerc.IsTileViewColumn = true;
            RColBadPerc.Text = "Bad %";
            RColBadPerc.TextAlign = HorizontalAlignment.Center;
            RColBadPerc.Width = 50;
            TextBarRenderer BadPercRenderer = new TextBarRenderer();
            BadPercRenderer.UseStandardBar = false;
            BadPercRenderer.BackgroundColor = Colours.pass_backColor;
            BadPercRenderer.GradientStartColor = Colours.borderline_backColor;
            BadPercRenderer.GradientEndColor = System.Drawing.Color.Crimson;
            BadPercRenderer.MaximumValue = 100;
            BadPercRenderer.MinimumValue = 0;
            RColBadPerc.Renderer = BadPercRenderer;
            ResultListView.AllColumns.Add(RColBadPerc);


            OLVColumn RColTroughWarning = new OLVColumn();
            RColTroughWarning.AspectName = "TroughWarning";
            RColTroughWarning.HeaderFont = null;
            RColTroughWarning.IsTileViewColumn = true;
            RColTroughWarning.Text = "Troughs";
            RColTroughWarning.Width = 60;
            ResultListView.AllColumns.Add(RColTroughWarning);


            OLVColumn RColBlankWarning = new OLVColumn();
            RColBlankWarning.AspectName = "BlankWarning";
            RColBlankWarning.HeaderFont = null;
            RColBlankWarning.IsTileViewColumn = true;
            RColBlankWarning.Text = "Blank";
            RColBlankWarning.Width = 50;
            ResultListView.AllColumns.Add(RColBlankWarning);

            //OLVColumn RColFrozenWarning = new OLVColumn();
            //RColFrozenWarning.AspectName = "FrozenWarning";
            //RColFrozenWarning.HeaderFont = null;
            //RColFrozenWarning.IsTileViewColumn = true;
            //RColFrozenWarning.Text = "Froz.";
            //RColFrozenWarning.Width = 50;
            //ResultListView.AllColumns.Add(RColFrozenWarning);


            OLVColumn RColVideo = new OLVColumn();
            RColVideo.AspectName = "Video";
            RColVideo.HeaderFont = null;
            RColVideo.IsTileViewColumn = true;
            RColVideo.Text = "Video";
            RColVideo.Width = 110;
            ResultListView.AllColumns.Add(RColVideo);

            OLVColumn RColProfile = new OLVColumn();
            RColProfile.AspectName = "VideoCodecProfile";
            RColProfile.HeaderFont = null;
            RColProfile.IsTileViewColumn = true;
            RColProfile.Text = "Codec Profile";
            RColProfile.Width = 70;
            //ResultListView.AllColumns.Add(RColProfile);

            OLVColumn RColLabRes = new OLVColumn();
            RColLabRes.AspectName = "ResolutionLabel";
            RColLabRes.HeaderFont = null;
            RColLabRes.IsTileViewColumn = true;
            RColLabRes.Text = "Format";
            RColLabRes.Width = 70;
            ResultListView.AllColumns.Add(RColLabRes);

            OLVColumn RColRes = new OLVColumn();
            RColRes.AspectName = "Resolution";
            RColRes.HeaderFont = null;
            RColRes.IsTileViewColumn = true;
            RColRes.Text = "Resolution";
            RColRes.Width = 70;
            RColRes.IsVisible = true;
            ResultListView.AllColumns.Add(RColRes);

            OLVColumn RColAR = new OLVColumn();
            RColAR.AspectName = "DisplayAspectRatioString";
            RColAR.HeaderFont = null;
            RColAR.IsTileViewColumn = true;
            RColAR.Text = "A.R.";
            RColAR.Width = 50;
            ResultListView.AllColumns.Add(RColAR);

            OLVColumn RColAudio = new OLVColumn();
            RColAudio.AspectName = "AudioSummary";
            RColAudio.HeaderFont = null;
            RColAudio.IsTileViewColumn = true;
            RColAudio.Text = "Audio";
            RColAudio.Width = 130;
            ResultListView.AllColumns.Add(RColAudio);

            //OLVColumn RMOS = new OLVColumn();
            //RMOS.AspectName = "avMOS";
            //RMOS.HeaderFont = null;
            //RMOS.IsTileViewColumn = true;
            //RMOS.Text = "Average MOS";
            //RMOS.Width = 80;
            //PartialMultiImageRenderer mosRenderer = new PartialMultiImageRenderer("star16.png", 5, 1, 5);
            //RMOS.Renderer = mosRenderer;
            //ResultListView.AllColumns.Add(RMOS);


            //OLVColumn RColFieldWarning = new OLVColumn();
            //RColFieldWarning.AspectName = "FieldWarning";
            //RColFieldWarning.HeaderFont = null;
            //RColFieldWarning.IsTileViewColumn = true;
            //RColFieldWarning.Text = "Fields";
            //RColFieldWarning.Width = 50;
            //ResultListView.AllColumns.Add(RColFieldWarning);

            OLVColumn RColDuration = new OLVColumn();
            RColDuration.AspectName = "VideoDurationString";
            RColDuration.HeaderFont = null;
            RColDuration.IsTileViewColumn = true;
            RColDuration.Text = "Duration";
            RColDuration.Width = 60;
            ResultListView.AllColumns.Add(RColDuration);



            OLVColumn RColCompletedDate = new OLVColumn();
            RColCompletedDate.AspectName = "CompletedDate";
            RColCompletedDate.HeaderFont = null;
            RColCompletedDate.IsTileViewColumn = true;
            RColCompletedDate.Text = "Completed";
            RColCompletedDate.Width = 120;
            ResultListView.AllColumns.Add(RColCompletedDate);

            OLVColumn RPath = new BrightIdeasSoftware.OLVColumn();
            RPath.AspectName = "StreamPath";
            RPath.HeaderFont = null;
            RPath.IsTileViewColumn = true;
            RPath.Text = "Path";
            RPath.Width = 230;
            ResultListView.AllColumns.Add(RPath);

            OLVColumn RColComment = new OLVColumn();
            RColComment.AspectName = "Comment";
            RColComment.HeaderFont = null;
            RColComment.IsTileViewColumn = true;
            RColComment.Text = "Comment";
            RColComment.Width = 150;
            ResultListView.AllColumns.Add(RColComment);

            #endregion

            ResultListView.DataSource = SummaryList;

            ResultListView.Name = "ResultDataListView";
            ResultListView.ShowGroups = false;
            ResultListView.GridLines = true;

            ResultListView.FullRowSelect = true;
            ResultListView.DoubleClick += new System.EventHandler(ListViewDataSetSelectedIndexChanged);
            //ResultListView.SelectedIndexChanged += new System.EventHandler(this.ResultsListViewDataSetSelectedIndexChanged); //Melek

            // ResultListView.CellClick +=  new EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.ResultsListViewDataSetSelectedIndexChanged); //Melek

            // ResultListView.DoubleClick -= new System.EventHandler(ListViewDataSetSelectedIndexChanged);//Melek - remove the eventhandler - Memory Allocation
            //ResultListView.HighlightBackgroundColor = System.Drawing.Color.Crimson;
            //ResultListView.HighlightForegroundColor = System.Drawing.Color.White;

            ResultListView.UseCellFormatEvents = true;

            // Hot item settings
            ResultListView.UseHotItem = true;
            ResultListView.UseTranslucentHotItem = true;

            RowBorderDecoration rbd = new RowBorderDecoration();
            rbd.BorderPen = new Pen(Color.Black, 2);
            rbd.FillBrush = null;
            rbd.CornerRounding = 4.0f;
            HotItemStyle hotItemStyle2 = new HotItemStyle();
            hotItemStyle2.Decoration = rbd;

            HotItemStyle hotItemStyle = new HotItemStyle();
            hotItemStyle.ForeColor = Color.AliceBlue;
            //hotItemStyle.BackColor = Color.FromArgb(255, 64, 64, 64);

            #region new column style - Melek

            hotItemStyle.BackColor = Color.Gainsboro;
            hotItemStyle.ForeColor = Color.Black;
            hotItemStyle.Font = new System.Drawing.Font(Font.FontFamily, Font.Size + 2, FontStyle.Regular);
            hotItemStyle.Decoration = rbd;

            #endregion end of  new column style - Melek

            ResultListView.HotItemStyle = hotItemStyle;
            ResultListView.UseFiltering = true;
            ResultListView.CellRightClick += new EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.ResultListView_CellRightClick);
            // ResultListView.CellRightClick -= new EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.ResultListView_CellRightClick); //Melek - remove the eventhandler - Memory Allocation
            // layout
            ResultListView.HeaderFormatStyle = headerFormatStyleData;
            ResultListView.HeaderUsesThemes = false;
            ResultListView.RebuildColumns();

        }

        #region Set Failed Results view - Melek
        public void SetFailedResultListBinding()
        {
            FailedResultListView.MultiSelect = true;//Melek
            FailedResultListView.SmallImageList = this.imageList1;
            FailedResultListView.RowHeight = 20;
            FailedResultListView.Location = new System.Drawing.Point(0, 0);
            FailedResultListView.Scrollable = true;
            FailedResultListView.BackColor = System.Drawing.Color.Gainsboro;
            FailedStreamsPanel.Controls.Add(FailedResultListView);
            FailedResultListView.Dock = DockStyle.Fill;
            FailedResultListView.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.FailedResultListView_FormatRow);
            //FailedResultListView.FormatRow -= new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.FailedResultListView_FormatRow);//Melek - remove the eventhandler - Memory Allocation
            FailedResultListView.EmptyListMsg = "No result at the moment.";
            FailedResultListView.AllowColumnReorder = true;

            #region defines all columns of result view

            OLVColumn RColStreamName = new OLVColumn();
            RColStreamName.AspectName = "StreamFileName";
            RColStreamName.HeaderFont = null;
            RColStreamName.IsTileViewColumn = true;
            RColStreamName.Text = "Name";
            RColStreamName.Width = 610;
            RColStreamName.ImageGetter = delegate(object row)
            {
                switch (((VStream)row).Final_Decision.Made)
                {
                    case Decision.FAIL:
                        return "lred16.png";
                    case Decision.BORDERLINE:
                        return "lamber16.png";
                    case Decision.PASS:
                        return "lgreen16.png";
                    default:
                        return "lgrey16.png";
                }

            };

            FailedResultListView.AllColumns.Add(RColStreamName);

            #endregion

            FailedResultListView.DataSource = SummaryList;

            FailedResultListView.Name = "FailedResultDataListView";
            FailedResultListView.ShowGroups = false;
            FailedResultListView.GridLines = true;
            FailedResultListView.FullRowSelect = true;

            //FailedResultListView.DoubleClick += new System.EventHandler(ListViewDataSetSelectedIndexChanged);
            FailedResultListView.DoubleClick += new System.EventHandler(ListViewDataSetSelectedIndexChanged);
            // FailedResultListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(ListViewDataSetSelectedIndexChanged);


            //FailedResultListView.SelectedIndexChanged += new System.EventHandler(this.FailedResultsListViewDataSetSelectedIndexChanged);//Melek
            FailedResultListView.CellClick += new EventHandler<BrightIdeasSoftware.CellClickEventArgs>(this.FailedResultsListViewDataSetSelectedIndexChanged);//Melek



            //  FailedResultListView.Click += new System.EventHandler(ListViewDataSetSelectedIndexChanged2);//Melek - Click event is not working

            //  FailedResultListView.DoubleClick -= new System.EventHandler(ListViewDataSetSelectedIndexChanged); //Melek - remove the eventhandler - Memory Allocation
            //   FailedResultListView.MouseHover += new System.EventHandler(ListViewMouseHover); //Melek - display the thumbnail of the video clip


            // FailedResultListView.MouseEnter += new System.EventHandler(ListViewMouseEnter); //Melek - 

            // FailedResultListView.MouseMove += new System.Windows.Forms.MouseEventHandler(ListViewThumbnailDisplay);

            // FailedResultListView.MouseLeave += new System.EventHandler(ListViewThumbnailDisplay); //Melek -Close the popup window
            // FailedResultListView.Click += new System.EventHandler(ListViewMouseClick);

            //FailedResultListView.HighlightBackgroundColor = System.Drawing.Color.Crimson;
            //FailedResultListView.HighlightForegroundColor = System.Drawing.Color.White;
            FailedResultListView.UseCellFormatEvents = true;
            // Hot item settings
            FailedResultListView.UseHotItem = true;
            FailedResultListView.UseTranslucentHotItem = true;

            RowBorderDecoration rbd = new RowBorderDecoration();
            rbd.BorderPen = new Pen(Color.Black, 2);
            rbd.FillBrush = null;
            rbd.CornerRounding = 4.0f;


            HotItemStyle hotItemStyle2 = new HotItemStyle();
            hotItemStyle2.Decoration = rbd;

            HotItemStyle hotItemStyle = new HotItemStyle();
            hotItemStyle.ForeColor = Color.AliceBlue;


            #region new column style - Melek

            hotItemStyle.BackColor = Color.FromArgb(255, 64, 64, 64);
            hotItemStyle.BackColor = Color.Gainsboro;
            hotItemStyle.Font = new System.Drawing.Font(Font.FontFamily, Font.Size + 2, FontStyle.Regular);
            hotItemStyle.Decoration = rbd;
            hotItemStyle.ForeColor = Color.Black;

            #endregion end of new column style - Melek

            FailedResultListView.HotItemStyle = hotItemStyle;
            FailedResultListView.UseFiltering = true;
            FailedResultListView.CellRightClick += new EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.FailedResultListView_CellRightClick);
            //FailedResultListView.CellRightClick -= new EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.FailedResultListView_CellRightClick); //Melek - remove the eventhandler - Memory Allocation
            // layout
            FailedResultListView.HeaderFormatStyle = headerFormatStyleData;
            FailedResultListView.HeaderUsesThemes = false;

            FailedResultListView.RebuildColumns();

        }
        #endregion end of Failed Results Set function

        /// <summary>
        /// Right click on resultListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultListView_CellRightClick(object sender, BrightIdeasSoftware.CellRightClickEventArgs e)
        {
            // sender : (string)text of the item
            // e : object (VStream) that it refers too

            if (e.Model != null)
            {

                #region Report Config Melek
                //// Update Monitoring list Contextual menu
                //ResultListContextMenu = new ContextMenuStrip();
                //ToolStripMenuItem QualityProfileItem;
                //// define 1 sub item per quality profile in this sub menu
                ////QualityProfileItem = new ToolStripMenuItem("Generate Report ");
                ////QualityProfileItem.Tag = e.Model;
                ////ResultListContextMenu.Items.Add(QualityProfileItem);   
                //QualityProfileItem = new ToolStripMenuItem("Apply a new quality profile ");
                //QualityProfileItem.Tag = e.Model;
                //ResultListContextMenu.Items.Add(QualityProfileItem);              
                //QualityProfileItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(ResultListView_QualityProfile_ItemClicked);
                //QualityProfileItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(ResultListView_QualityProfile_ItemClicked);
                //ToolStripMenuItem QualityProfileListItem;
                // Update Monitoring list Contextual menu
                #endregion Report end Melek

                ResultListContextMenu = new ContextMenuStrip();
                ToolStripMenuItem QualityProfileItem;

                // define 1 sub item per quality profile in this sub menu
                QualityProfileItem = new ToolStripMenuItem("Apply a new quality profile ");
                QualityProfileItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(ResultListView_QualityProfile_ItemClicked);
                //QualityProfileItem.DropDownItemClicked -= new ToolStripItemClickedEventHandler(ResultListView_QualityProfile_ItemClicked);//Melek - remove the eventhandler - Memory Allocation
                ToolStripMenuItem QualityProfileListItem;

                foreach (string name in qaClass.profManager.List)
                {
                    QualityProfileListItem = new ToolStripMenuItem(name);
                    QualityProfileListItem.Tag = e.Model;
                    QualityProfileItem.DropDownItems.Add(QualityProfileListItem);
                }

                ResultListContextMenu.Items.Add(QualityProfileItem);

                e.MenuStrip = ResultListContextMenu;
            }
        }

        #region failed results cell right click
        private void FailedResultListView_CellRightClick(object sender, BrightIdeasSoftware.CellRightClickEventArgs e)
        {
            // sender : (string)text of the item
            // e : object (VStream) that it refers too

            if (e.Model != null)
            {
                // Update Monitoring list Contextual menu
                FailedResultListContextMenu = new ContextMenuStrip();
                ToolStripMenuItem QualityProfileItem;

                // define 1 sub item per quality profile in this sub menu
                QualityProfileItem = new ToolStripMenuItem("Apply a new quality profile ");
                QualityProfileItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(FailedResultListView_QualityProfile_ItemClicked);
                // QualityProfileItem.DropDownItemClicked -= new ToolStripItemClickedEventHandler(FailedResultListView_QualityProfile_ItemClicked);//Melek - remove the eventhandler - Memory Allocation
                ToolStripMenuItem QualityProfileListItem;

                foreach (string name in qaClass.profManager.List)
                {
                    QualityProfileListItem = new ToolStripMenuItem(name);
                    QualityProfileListItem.Tag = e.Model;
                    QualityProfileItem.DropDownItems.Add(QualityProfileListItem);
                }

                FailedResultListContextMenu.Items.Add(QualityProfileItem);

                e.MenuStrip = FailedResultListContextMenu;
            }
        }
        #endregion end of  failed right click


        /// <summary>
        /// A new quality profile has been chosen for a stream in the monitoring view
        /// (A subitem in the dropdown list of the context menu has been clicked)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ResultListView_QualityProfile_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // event fired when the contextual menu was clicked
            ToolStripMenuItem mi = (ToolStripMenuItem)e.ClickedItem;
            // mi : item clicked in the contextual menu
            // VStream tvs = (VStream)mi.Tag;

            IList Ltvs = ResultListView.GetSelectedObjects();
            foreach (object tvs in Ltvs)
            {
                try
                {
                    ForceNewProfile(mi.Text, ((VStream)tvs));
                }
                catch
                {

                }
            }
        }


        void FailedResultListView_QualityProfile_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // event fired when the contextual menu was clicked
            ToolStripMenuItem mi = (ToolStripMenuItem)e.ClickedItem;
            // mi : item clicked in the contextual menu
            // VStream tvs = (VStream)mi.Tag;


            IList Ltvs = FailedResultListView.GetSelectedObjects();
            foreach (object tvs in Ltvs)
            {
                try
                {
                    ForceNewProfile(mi.Text, ((VStream)tvs));
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// High priority has been set for a job in joblist
        /// the item in the context menu of the joblist has been clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void JobListItemRightClick(object sender, ToolStripItemClickedEventArgs e)
        {
            string text = e.ClickedItem.Text;
            ToolStripMenuItem mi = (ToolStripMenuItem)e.ClickedItem;

            #region new algo - Melek
            IList joblistselecteditems = JobListView.GetSelectedObjects();

            //foreach (object sljobs in joblistselecteditems)
            foreach (Job js in joblistselecteditems)
            {
                try
                {
                    // ForceNewProfile(mi.Text, ((VStream)sljobs));
                    // Job js = (Job)mi.Tag;
                    //js = (Job)mi.Tag;
                    if (text == "1-Hot")
                    {
                        js.priority = 0;
                        js.priorityname = "1-Hot";
                        js.Hot = true;
                    }

                    else if (text == "2-High Priority")
                    {
                        js.priority = 64;
                        js.priorityname = "2-High";

                    }
                    else if (text == "3-Medium Priority")
                    {
                        js.priority = 128;
                        js.priorityname = "3-Medium";
                    }
                    else if (text == "4-Low Priority")
                    {
                        js.priority = 255;
                        js.priorityname = "4-Low";
                    }
                    //GjobList.Add(js); //Update the Priority Status of the stream on Review tab
                    //GjobList.Remove(js); //No Dublication in joblist                 
                    //lock (padlock)
                    //{

                    //    var job = jobList.FirstOrDefault(j => j.path.RootName == js.path.RootName);
                    //    if (job != null)
                    //    {
                    //        int index = jobList.IndexOf(job);
                    //        // Replace the existing job
                    //        jobList[index] = js;
                    //    }
                    //}


                    EditJobInList(js, null);
                }
                catch
                {

                }
            }
            #endregion end of new right click algo
        }

        /// <summary>
        /// Set the row color in the result list view based on the final decisison
        /// (if mode enabled)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultListView_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            try//Melek try catch added
            {
                VStream tvs = (VStream)e.Model;
                if (this.DecisionColoredRow.Checked)
                {
                    ResultListView.UseAlternatingBackColors = false;
                    if (tvs.sumToUpdate == true)
                        e.Item.BackColor = CDecision.GetDecisionColor(Decision.NOTMADE);
                    else
                        e.Item.BackColor = tvs.Final_Decision.GetBackColor();
                }
                else
                {
                    ResultListView.UseAlternatingBackColors = true;
                    ResultListView.AlternateRowBackColor = System.Drawing.Color.FromArgb(239, 235, 245);
                }
            }
            catch { }

        }


        #region Failed Result list view format row - on Review tab - Melek
        private void FailedResultListView_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            try//Melek - Try catch added
            {
                VStream tvs = (VStream)e.Model;
                if (this.FailedDecisionColoredRow.Checked)
                {
                    FailedResultListView.UseAlternatingBackColors = false;
                    if (tvs.sumToUpdate == true)
                        e.Item.BackColor = CDecision.GetDecisionColor(Decision.NOTMADE);
                    else
                        e.Item.BackColor = tvs.Final_Decision.GetBackColor();
                }
                else
                {
                    FailedResultListView.UseAlternatingBackColors = true;
                    FailedResultListView.AlternateRowBackColor = System.Drawing.Color.FromArgb(239, 235, 245);
                }
            }
            catch { }


        }
        #endregion End of Failed Result list view format row

        /// <summary>
        /// Single click on a stream in ResultListView //Double not single - Melek
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ListViewDataSetSelectedIndexChanged(object sender, System.EventArgs e)
        {
            try
            {
                BrightIdeasSoftware.ObjectListView listView = (BrightIdeasSoftware.ObjectListView)sender;
                vs = (VStream)listView.GetSelectedObject();

                if ((vs != null) && (this.vs.Final_Decision.String != "Not made")) //Melek - if decision is not made do not play the clip
                {
                    StopClip();
                    openDetails(vs);
                    playbutton_Click("", EventArgs.Empty); //Melek Double click play option

                    //Enable User Decision buttons
                    Pass_Button.Enabled = true;
                    Fail_Button.Enabled = true;
                    CancelUserDecision_Button.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Clip Decision Not Made - This clip format is not compatible with v.Cortex");

                }
            }
            catch (Exception ex) { }
        }

        void FailedResultsListViewDataSetSelectedIndexChanged(object sender, System.EventArgs e)
        {

            try
            {
                {
                    BrightIdeasSoftware.ObjectListView listView = (BrightIdeasSoftware.ObjectListView)sender;
                    vs1 = (VStream)listView.GetSelectedObject();

                    //Enable User Decision buttons
                    if (vs1 != null && (vs1 != vs))
                    {
                        Pass_Button.Enabled = true;
                        Fail_Button.Enabled = true;
                        CancelUserDecision_Button.Enabled = true;
                        StopClip();
                        openDetails(vs1);

                        if (this.vs.Final_Decision.String == "Not made")//Melek
                        {
                            playbutton.Enabled = false;
                            VideoPanelLabel.Enabled = false;

                        }

                    }
                }

            }
            catch (Exception ex) { }

        }
        void ResultsListViewDataSetSelectedIndexChanged(object sender, System.EventArgs e) //Melek
        {

        }

        //Melek - show the thumbnail
        void ListViewMouseHover(object sender, System.EventArgs e)
        {
            MessageBox.Show("Mouse Hover");

        }
        void ListViewMouseEnter(object sender, System.EventArgs e)
        {
            MessageBox.Show("Mouse Enter");

        }

        void ListViewMouseClick(object sender, System.EventArgs e)
        {
            MessageBox.Show("Mouse Move");
        }

        void ListViewThumbnailDisplay(object sender, System.EventArgs e)
        {

            MessageBox.Show("Display thumbnail - mouse event handler");
            BrightIdeasSoftware.ObjectListView listView = (BrightIdeasSoftware.ObjectListView)sender;
            vs = (VStream)listView.GetSelectedObject();


            // listView.MouseCaptureChanged += new EventHandler(ListViewThumbnailDisplay);//Melek

            //vs = (VStream)listView.Click+=new System.EventHandler(ListViewSelectedClipInfo);

            if (vs != null)
            {
                MessageBox.Show("clip selected");
                int i = listView.SelectedIndex;
                //  string name=listView.Items[i].;
                StopClip();
                openDetails(vs);
                //Initialize Picture Box
                InitializePictureBox();
            }
            //Enable User Decision buttons and report buttons
            Pass_Button.Enabled = true;
            Fail_Button.Enabled = true;
            CancelUserDecision_Button.Enabled = true;
            //GenerateReportButtonReviewView.Enabled = true; //Melek
            //ClearButtonReviewMenu.Enabled = true; //Melek      
        }

        //Melek- Initialize Picture Box for thumbnails
        private void InitializePictureBox()
        {
            //// PictureBox PictureBox1 = new PictureBox();

            // // Set the location and size of the PictureBox control.
            // this.PictureBox1.Location = new System.Drawing.Point(70, 120);
            // this.PictureBox1.Size = new System.Drawing.Size(140, 140);
            // this.PictureBox1.TabStop = false;

            // // Set the SizeMode property to the StretchImage value.  This
            // // will shrink or enlarge the image as needed to fit into
            // // the PictureBox.
            // this.PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            // // Set the border style to a three-dimensional border.
            // this.PictureBox1.BorderStyle = BorderStyle.Fixed3D;

            // // Add the PictureBox to the form.
            // this.Controls.Add(this.PictureBox1);          
        }

        /// <summary>
        /// Joblistview (databinding)
        /// </summary>
        public void SetJoblistBinding()
        {

            JobListView.Location = new System.Drawing.Point(6, 8);
            JobListView.Size = new System.Drawing.Size(1250, 835);
            JobListView.View = View.Details;
            JobListView.AllowColumnReorder = true;
            JobListView.UseAlternatingBackColors = true;
            JobListView.BackColor = System.Drawing.Color.Gainsboro;
            JobListView.AlternateRowBackColor = System.Drawing.Color.FromArgb(239, 235, 245);

            //JobListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)
            //            | System.Windows.Forms.AnchorStyles.Right)));

            // JobListTab.Controls.Add(JobListView);
            JoblistviewPanel.Controls.Add(JobListView);

            JobListView.Dock = DockStyle.Fill; //Melek
            //JobListView.Scrollable = true;
            JobListView.EmptyListMsg = "v.Cortex is searching for files (jobs) - No job at the moment.";
            JobListView.OwnerDraw = true;

            JobListView.HeaderFormatStyle = this.headerFormatStyleData;
            JobListView.HeaderUsesThemes = false;

            JobListView.ShowGroups = false;
            JobListView.GridLines = true;

            JobListView.FullRowSelect = true;
            JobListView.CellRightClick += new EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.JobListView_CellRightClick);
            //JobListView.CellRightClick -= new EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.JobListView_CellRightClick);//Melek - remove the eventhandler - Memory Allocation
            // this prevents column sorting.
            // we don't need this as this is a joblist, so jobs should be listed in the order they re going to be processed.
            JobListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;

            // this sorts the joblist by priority
            // ideally a custom sorting should be made so that the processing jobs always appear at the top of the list.
            // at the moment if a job is added with a priority lower than the job currently processing it will appear over it.
            // but this does not affect the processing order that is handled by the job manager.
            JobListView.Sort("Priority");

            //JobListView.DataSource = GjobList;
            JobListView.DataSource = jobList;

            #region columns definition
            //BrightIdeasSoftware.OLVColumn ColJobID = new BrightIdeasSoftware.OLVColumn();
            //ColJobID.AspectName = "JobID";
            //ColJobID.HeaderFont = null;
            //ColJobID.IsTileViewColumn = true;
            //ColJobID.Text = "Job ID";
            //ColJobID.Width = 60;
            // JobListView.AllColumns.Add(ColJobID);

            BrightIdeasSoftware.OLVColumn ColPriority = new BrightIdeasSoftware.OLVColumn();
            ColPriority.AspectName = "Priority";
            ColPriority.HeaderFont = null;
            ColPriority.IsTileViewColumn = true;
            ColPriority.Text = "Priority";
            ColPriority.Width = 50;
            //JobListView.AllColumns.Add(ColPriority);


            BrightIdeasSoftware.OLVColumn ColPriorityName = new BrightIdeasSoftware.OLVColumn(); //Melek
            ColPriorityName.AspectName = "PriorityName";
            ColPriorityName.HeaderFont = null;
            ColPriorityName.IsTileViewColumn = true;
            ColPriorityName.Text = "Priority";
            ColPriorityName.Width = 80;
            JobListView.AllColumns.Add(ColPriorityName);

            OLVColumn ColProgression = new OLVColumn();
            ColProgression.AspectName = "ProgTask";
            ColProgression.HeaderFont = null;
            ColProgression.IsTileViewColumn = true;
            ColProgression.Text = "Status";
            ColProgression.Width = 120;
            ColProgression.TextAlign = HorizontalAlignment.Center;
            JobProgRenderer ProgRenderer = new JobProgRenderer();
            //ProgRenderer.BackgroundColor = System.Drawing.Color.CornflowerBlue;
            ProgRenderer.BackgroundColor = System.Drawing.Color.LightSkyBlue;
            //ProgRenderer.GradientStartColor = Color.LightSteelBlue;
            ProgRenderer.TextColor = Brushes.White;
            ProgRenderer.MaximumValue = 100;
            ProgRenderer.UseStandardBar = false;
            ColProgression.Renderer = ProgRenderer;
            JobListView.AllColumns.Add(ColProgression);


            BrightIdeasSoftware.OLVColumn ColName = new BrightIdeasSoftware.OLVColumn();
            ColName.AspectName = "StreamFileName";
            ColName.HeaderFont = null;
            ColName.IsTileViewColumn = true;
            ColName.Text = "Name";
            ColName.Width = 420;
            JobListView.AllColumns.Add(ColName);

            BrightIdeasSoftware.OLVColumn ColProfile = new BrightIdeasSoftware.OLVColumn();
            ColProfile.AspectName = "Profile";
            ColProfile.HeaderFont = null;
            ColProfile.IsTileViewColumn = true;
            ColProfile.Text = "Profile";
            ColProfile.Width = 120;
            JobListView.AllColumns.Add(ColProfile);


            BrightIdeasSoftware.OLVColumn ColPath = new BrightIdeasSoftware.OLVColumn();
            ColPath.AspectName = "Folder";
            ColPath.HeaderFont = null;
            ColPath.IsTileViewColumn = true;
            ColPath.Text = "Path";
            ColPath.Width = 430;
            JobListView.AllColumns.Add(ColPath);

            BrightIdeasSoftware.OLVColumn ColFormat = new BrightIdeasSoftware.OLVColumn();
            ColFormat.AspectName = "Format";
            ColFormat.HeaderFont = null;
            ColFormat.IsTileViewColumn = true;
            ColFormat.Text = "Format";
            ColFormat.Width = 250;
            ColFormat.IsVisible = true;
            JobListView.AllColumns.Add(ColFormat);

            BrightIdeasSoftware.OLVColumn ColFileSize = new BrightIdeasSoftware.OLVColumn();
            ColFileSize.AspectName = "FileSize";
            ColFileSize.HeaderFont = null;
            ColFileSize.TextAlign = HorizontalAlignment.Right;
            ColFileSize.IsTileViewColumn = true;
            ColFileSize.Text = "File size";
            ColFileSize.Width = 60;
            ColFileSize.IsVisible = true;
            JobListView.AllColumns.Add(ColFileSize);

            BrightIdeasSoftware.OLVColumn ColDuration = new BrightIdeasSoftware.OLVColumn();
            ColDuration.AspectName = "Duration";
            ColDuration.HeaderFont = null;
            ColDuration.IsTileViewColumn = true;
            ColDuration.Text = "Duration";
            ColDuration.Width = 90;
            ColDuration.IsVisible = true;
            JobListView.AllColumns.Add(ColDuration);
            /*
            BrightIdeasSoftware.OLVColumn ColTask = new BrightIdeasSoftware.OLVColumn();
            ColTask.AspectName = "Task";
            ColTask.HeaderFont = null;
            ColTask.IsTileViewColumn = true;
            ColTask.Text = "Task";
            ColTask.Width = 70;
            JobListView.AllColumns.Add(ColTask);

            BrightIdeasSoftware.OLVColumn ColStatus = new BrightIdeasSoftware.OLVColumn();
            ColStatus.AspectName = "Status";
            ColStatus.HeaderFont = null;
            ColStatus.IsTileViewColumn = true;
            ColStatus.Text = "Status";
            ColStatus.Width = 100;
            JobListView.AllColumns.Add(ColStatus);
            */
            //BrightIdeasSoftware.OLVColumn ColStartTime = new BrightIdeasSoftware.OLVColumn();
            //ColStartTime.AspectName = "StartTime";
            //ColStartTime.HeaderFont = null;
            //ColStartTime.IsTileViewColumn = true;
            //ColStartTime.Text = "Start time";
            //ColStartTime.Width = 120;
            //JobListView.AllColumns.Add(ColStartTime);

            //BrightIdeasSoftware.OLVColumn ColEndTime = new BrightIdeasSoftware.OLVColumn();
            //ColEndTime.AspectName = "EndTime";
            //ColEndTime.HeaderFont = null;
            //ColEndTime.IsTileViewColumn = true;
            //ColEndTime.Text = "Completed in";
            //ColEndTime.Width = 105;

            //JobListView.AllColumns.Add(ColEndTime);
            #endregion

            JobListView.RebuildColumns();
        }


        /// <summary>
        /// Right click on resultListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JobListView_CellRightClick(object sender, BrightIdeasSoftware.CellRightClickEventArgs e)
        {
            // sender : (string)text of the item
            // e : object (JobStruct) that it refers too

            if (e.Model != null) //Melek
            {
                // Update Monitoring list Contextual menu
                JobListContextMenuStrip = new ContextMenuStrip();
                ToolStripMenuItem tsmi;
                JobListContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(JobListItemRightClick);
                //JobListContextMenuStrip.ItemClicked -= new ToolStripItemClickedEventHandler(JobListItemRightClick);//Melek - remove the eventhandler - Memory Allocation
                // define a sub menu
                tsmi = new ToolStripMenuItem("1-Hot");
                tsmi.Tag = e.Model;
                JobListContextMenuStrip.Items.Add(tsmi);
                tsmi = new ToolStripMenuItem("2-High Priority");
                tsmi.Tag = e.Model;
                JobListContextMenuStrip.Items.Add(tsmi);
                tsmi = new ToolStripMenuItem("3-Medium Priority");
                tsmi.Tag = e.Model;
                JobListContextMenuStrip.Items.Add(tsmi);
                tsmi = new ToolStripMenuItem("4-Low Priority");
                tsmi.Tag = e.Model;
                JobListContextMenuStrip.Items.Add(tsmi);
                e.MenuStrip = JobListContextMenuStrip;
            }
        }

        /// <summary>
        /// Add a new result to the resultlist
        /// Used to get the GUI to edit the stream itself
        /// to avoid cross thread problems with the bindedlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSummaryToList(object sender, EventArgs e)
        {
            AddVStream((VStream)sender);
            //if (InvokeRequired)
            //{
            //    ThreadUpdateCallback d = new ThreadUpdateCallback(AddSummaryToList);
            //    Invoke(d, new object[] { sender, e });
            //    //BeginInvoke(d, new object[] { sender, e });
            //}
            //else
            //{
            //    try
            //    {
            //        //foreach (VStream lvs in GsummaryList)
            //        //{

            //        //        if (lvs.sum.ModelDecision.Made == Decision.FAIL)
            //        //            GsummaryList.Add((VStream)sender);

            //        //}

            //        //Debug.WriteLine("Add someting");



            //        GsummaryList.Add((VStream)sender);
            //    }
            //    catch (SystemException ex)
            //    {
            //        Global.log("Problem updating the list of streams to remove from the GUI.\n" + ex);
            //    }
            //}

        }


        /// <summary>
        /// Remove, edit or add a stream in the stream list stored in G.interface thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSummaryFromList(object sender, EventArgs e)
        {
            RemoveVStream((VStream)sender);
            //if (InvokeRequired)
            //{
            //    ThreadUpdateCallback d = new ThreadUpdateCallback(RemoveSummaryFromList);
            //    Invoke(d, new object[] { sender, e });
            //    //BeginInvoke(d, new object[] { sender, e });
            //}
            //else
            //{
            //    try
            //    {
            //        GsummaryList.Remove((VStream)sender);
            //    }
            //    catch (SystemException ex)
            //    {
            //        Global.log("Problem removing the results from +\"" + ((VStream)sender).StreamFileName + "\" the monitoring list.\n" + ex); //Melek - path.name => streamfilename
            //    }
            //}
        }

        /// <summary>
        /// Used to get the GUI to edit the stream itself
        /// to avoid cross thread problems with the bindedlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditSummaryInList(object sender, EventArgs e)
        {
            VStream stream = sender as VStream;
            if (stream != null)
            {
                var summaryQuery = GUI.FindVStream(s => s.path.StreamFileName == stream.path.StreamFileName);
                if (!GUI.UpdateVStream(summaryQuery, stream))
                {
                    Global.log("Issue editing job in GUI: " + stream.StreamFileName);
                }
            }


            //try
            //{
            //    if (InvokeRequired)
            //    {
            //        ThreadUpdateCallback d = new ThreadUpdateCallback(EditSummaryInList);
            //        Invoke(d, new object[] { sender, EventArgs.Empty });
            //        //BeginInvoke(d, new object[] { sender, EventArgs.Empty });
            //    }
            //    else
            //    {
            //        try
            //        {
            //            VStream ste = (VStream)sender;

            //            IEnumerable<VStream> summaryQuery =
            //            from stream in GUI.GsummaryList
            //            where stream.path.StreamFileName == ste.path.StreamFileName //Melek - RootName changed to streamfilename 
            //            select stream;


            //            // should only be one job found
            //            foreach (VStream stream in summaryQuery)
            //            {
            //                int index = GUI.GsummaryList.IndexOf(stream);
            //                GUI.GsummaryList[index] = ste;

            //                break;
            //            }

            //        }
            //        catch (SystemException ex)
            //        {
            //            Global.log("Problem editing a job to the list of jobs to edit in GUI.\n" + ex);
            //        }
            //    }
            //}
            //catch
            //{
            //    Job jte = (Job)sender;
            //    Global.log("Issue editing job in GUI: " + jte.StreamFileName); //Melek - path.name => streamfilename
            //}
        }



        /// <summary>
        /// Remove job from list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveJobFromList(object sender, EventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    ThreadUpdateCallback d = new ThreadUpdateCallback(RemoveJobFromList);
                    Invoke(d, new object[] { sender, e });
                    //BeginInvoke(d, new object[] { sender, e });
                }
                else
                {
                    try
                    {
                        //GjobList.Remove((Job)sender);
                        RemoveJob((Job)sender);
                       // Global.log("RemoveJob " + (Job)sender);//Melek
                    }
                    catch (SystemException ex)
                    {
                        Global.log("Problem adding a job to the list of jobs to remove from GUI.\n" + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.log("Problem while removing job \"" + ((Job)sender).path.StreamPath + "\"from the joblist.\n" + ex);
            }
        }
        /// <summary>
        /// Edit jobs in joblist
        /// e.g. profile or priorities have been changed
        /// or processing job status update (demux/audio/video %..)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditJobInList(object sender, EventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    ThreadUpdateCallback d = new ThreadUpdateCallback(EditJobInList);
                    Invoke(d, new object[] { sender, EventArgs.Empty });
                    //BeginInvoke(d, new object[] { sender, EventArgs.Empty });
                }
                else
                {
                    try
                    {
                        // Trace.WriteLine("WHAT IS THIS SUPPOSED TO DO????");
                        // Job jte = (Job)sender;

                        //IEnumerable<Job> jobQuery =
                        //from job in GUI.ReadOnlyJobs 
                        //where job.path.RootName == jte.path.RootName
                        //select job;

                        //// should only be one job found
                        //foreach (Job job in jobQuery)
                        //{
                        //    int index = GUI.ReadOnlyJobs.IndexOf(job);
                        //    GUI.ReadOnlyJobs[index] = jte;

                        //    JobListView.Sort("Priority");
                        //    break;
                        //}
                        Job jte = (Job)sender;

                        lock (padlock)
                        {

                            var job = jobList.FirstOrDefault(j => j.path.StreamFileName == jte.path.StreamFileName); //MElek- Rootname changed to filenamme
                            if (job != null)
                            {
                                int index = jobList.IndexOf(job);
                                // Replace the existing job
                                jobList[index] = jte;

                            }
                        }
                    }
                    catch (SystemException ex)
                    {
                        Global.log("Problem editing a job to the list of jobs to edit in GUI.\n" + ex);
                    }
                }
            }
            catch
            {
                Job jte = (Job)sender;
                Global.log("Issue editing job in GUI: " + jte.StreamFileName); //Melek - path.name => streamfilename
            }
        }

        /// <summary>
        /// Add new detected job to list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddJobToList(object sender, EventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    ThreadUpdateCallback d = new ThreadUpdateCallback(AddJobToList);
                    Invoke(d, new object[] { sender, EventArgs.Empty });
                    //BeginInvoke(d, new object[] { sender, EventArgs.Empty });
                }
                else
                {
                    try
                    {
                        //if(((Job)sender).){}//Melek
                        //GjobList.Add((Job)sender);                     
                        //if (!((Job)sender).path.MonitoredFolderDeleted)//Melek
                        //{
                        AddJob((Job)sender);
                         //}
                    }
                    catch (SystemException ex)
                    {
                        Global.log("Problem adding a job to the list of jobs to add to GUI.\n" + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.log("Problem while adding a job \"" + ((Job)sender).path.StreamFileName + "\" to the list.\n" + ex);//Melek- (Job)sender).path.StreamPath changed to Streamfilename
            }

        }
        #endregion


        /// <summary>
        /// Update info bar
        /// Only used when applying a user decision
        /// </summary>
        private void updateInfoStatus()
        {
            rejected = 0;
            borderline = 0;
            passed = 0;
            streamsCurrentlyOnServer = 0;

            lock (padlock)
            {
                foreach (VStream lvs in SummaryList)
                {
                    if (lvs.sum.UserDecision.Made == Decision.NOTMADE)
                    {
                        if (lvs.sum.ModelDecision.Made == Decision.FAIL) rejected++;
                        else if (lvs.sum.ModelDecision.Made == Decision.BORDERLINE) borderline++;
                        else if (lvs.sum.ModelDecision.Made == Decision.PASS) passed++;
                    }
                    else
                    {
                        if (lvs.sum.UserDecision.Made == Decision.FAIL) rejected++;
                        else if (lvs.sum.UserDecision.Made == Decision.BORDERLINE) borderline++;
                        else if (lvs.sum.UserDecision.Made == Decision.PASS) passed++;
                    }
                    if (lvs.onserver == true) streamsCurrentlyOnServer++;
                }
            }

            status_analysed.Text = Convert.ToString(SummaryList.Count) + " ( " + Global.secondsToDurationString((uint)totalstreamduration) + ")";
            status_failed.Text = Convert.ToString(rejected, 10);
            status_borderline.Text = Convert.ToString(borderline, 10);
            status_pass.Text = Convert.ToString(passed, 10);
        }

        /// <summary>
        /// Shows the start time and duration of a full scan of the input folders
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        public void scanevent(object Sender, EventArgs e)
        {
            string scantext;
            scantext = (string)Sender;
            try
            {
                if (ScanEventBox.InvokeRequired)
                {
                    ThreadUpdateCallback d = new ThreadUpdateCallback(scanevent);
                    Invoke(d, new object[] { Sender, e });
                    //BeginInvoke(d, new object[] { Sender, e });
                }
                else
                {
                    ScanEventBox.Text = scantext;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while updating the folder scan box.\n" + ex);
                Global.log("Error while updating the folder scan box.\n" + ex);
            }
        }

        // Log events in log window
        public void GuilogEvent(object Sender, EventArgs e)
        {
            string logtext;
            try
            {
                if (LogText.InvokeRequired)
                {
                    ThreadUpdateCallback d = new ThreadUpdateCallback(GuilogEvent);
                    Invoke(d, new object[] { Sender, e });
                    //BeginInvoke(d, new object[] { Sender, e });
                }
                else
                {
                    logtext = (string)Sender;
                    Console.WriteLine(logtext);
                    // add the log event text in the notification list box and focus on it
                    LogText.Text = logtext + Environment.NewLine + LogText.Text;
                    string firstlineonly = logtext.Split('\n')[0];
                    status_notification.Text = firstlineonly;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem with notification box or log window update\n" + ex);
            }
        }

        // update start/pause joblist processing buttons
        private void updatejoblistbuttons(object pause, EventArgs e)
        {
            if ((bool)pause == true)
            {
                PauseJobListProcessingButton.Enabled = true;    // reenable the pause button to be able to pause the process;
                ProcessJobListButton.Enabled = false;           // disabled the start button when the process is on
            }
            else
            {
                PauseJobListProcessingButton.Enabled = false;   // reenable the pause button to be able to pause the process;
                ProcessJobListButton.Enabled = true;            // disabled the start button when the process is on
            }
        }

        /// <summary>
        /// Updates the datagridview in settings tab with streams folders / properties 
        /// </summary>
        public void UpdateFoldersGui()
        {
            try
            {
                if (StreamFoldersBox.InvokeRequired)
                {
                    UpdateCallback d = new UpdateCallback(UpdateFoldersGui);
                    Invoke(d, new object[] { });
                    //BeginInvoke(d, new object[] { });
                }
                else
                {

                    // Update profile column data
                    ProfileC.Items.Clear();
                    foreach (string name in qaClass.profManager.List)
                    {
                        ProfileC.Items.Add(name);
                    }

                    StreamFoldersBox.Rows.Clear();
                    
                    DataGridViewRow GridViewRow;

                    foreach (WatchedFolder wf in FoldersManager.WstreamFolders)
                    {

                        GridViewRow = new DataGridViewRow();
                        GridViewRow.CreateCells(StreamFoldersBox);
                        if (wf.watcher == null)
                            GridViewRow.Cells[0].Value = false;
                        else

                            GridViewRow.Cells[0].Value = wf.watcher.isFolderAvailable;

                        GridViewRow.Cells[1].Value = wf.folder.path;
                       
                        GridViewRow.Cells[2].Value = wf.folder.hot;

                        GridViewRow.Cells[3].Value = wf.folder.priority.ToString();

                        if (qaClass.profManager.ProfileExists(wf.folder.qualityProfile))
                        {
                            GridViewRow.Cells[4].Value = wf.folder.qualityProfile;
                        }
                        else
                        {
                            GridViewRow.Cells[4].Value = "default";
                        }


                        GridViewRow.Cells[5].Value = wf.folder.priorityname.ToString();

                        StreamFoldersBox.Rows.Add(GridViewRow);
                    }

                }

                ResDirTextBox.Text = qaClass.settings.ResFolder.path;
                //XMLdirTextBox.Text = qaClass.settings.XMLjobFolder.path; //Melek -disabled
                // XMLupdateDir.Text = qaClass.settings.XMLupdateFolder.path; //Melek -disabled
                LocalCopyCheckedBox.Checked = qaClass.settings.Keeplocalcopy;

                // Update summary update condition
                if (qaClass.settings.UpdateSummary == 1)
                {
                    UpdateSummaryFiles.Checked = true;
                    UpdateMonitoredStreamsOnlyBox.Enabled = true;
                }
                else
                {
                    UpdateSummaryFiles.Checked = false;
                    UpdateMonitoredStreamsOnlyBox.Enabled = false;
                }


                if (qaClass.settings.UpdateMonitoredStreamsOnly == 1)
                    UpdateMonitoredStreamsOnlyBox.Checked = true;
                else
                    UpdateMonitoredStreamsOnlyBox.Checked = false;

                if (qaClass.settings.StreamFolders.Count != 0)//Mel - Delete btn synch
                {
                    DeleteStreamFolderBox.Enabled = true;
                    //ApplyFolderSettingsBox.Enabled = true;
                }
                else
                {
                    DeleteStreamFolderBox.Enabled = false;
                    //ApplyFolderSettingsBox.Enabled = false;
                }

                if (qaClass.settings.StreamFolders.Count >= 1)//demostick
                {
                    AddStreamFolderBox.Enabled = false;
                }
                else
                {
                    AddStreamFolderBox.Enabled = true;
                }
                
                if (qaClass.settings.ReportsFolder!= "") //Melek
                {
                    RepDirTextBox.Text = qaClass.settings.ReportsFolder; 
                }
            }
            catch (Exception ex)
            {
                Global.log("Problem while updating the folders preferences.\n" + ex);
            }
        }

        /// <summary>
        /// Updates the profile tab with selected profile
        /// </summary>
        private void displaySelectedProfile()
        {

            if (ProfilesListBox2.SelectedItem != null && ProfilesListBox2.SelectedItem.ToString() == "default") //Do not allow users to change default settings -Melek
            {
                ApplyVideoQualityProfileButton2.Enabled = false;
                CancelQualityProfileButton2.Enabled = false;
                QosAlert2.Enabled = false;
                BadSceneMinimumDurationBox2.Enabled = false;
                RejectedThreshold2.Enabled = false;
                QualityTroughDetection2.Enabled = false;
                TroughMosThresholdBox2.Enabled = false;
                TroughMinDurationBox2.Enabled = false;
                BlackWhiteSceneDetectionBox2.Enabled = false;
                BlackSceneIntTBox2.Enabled = false;
                WhiteSceneIntTBox2.Enabled = false;
                BlackSceneDurationTBox2.Enabled = false;

            }
            else
            {
                ApplyVideoQualityProfileButton2.Enabled = true;
                CancelQualityProfileButton2.Enabled = true;
                QosAlert2.Enabled = true;
                BadSceneMinimumDurationBox2.Enabled = true;
                RejectedThreshold2.Enabled = true;
                QualityTroughDetection2.Enabled = true;
                //TroughMosThresholdBox2.Enabled = true;
                //TroughMinDurationBox2.Enabled = true;
                BlackWhiteSceneDetectionBox2.Enabled = true;
                //BlackSceneIntTBox2.Enabled = true;
                //WhiteSceneIntTBox2.Enabled = true;
                //BlackSceneDurationTBox2.Enabled = true;

            }
                CQualityProfile GUIprof;

                //if (qaClass.profManager.ProfileExists(ProfilesListBox2.SelectedItem.ToString()))
                //{
                //    GUIprof = qaClass.profManager.Profile(ProfilesListBox2.SelectedItem.ToString());
                //}
                //else
                //{
                //    GUIprof = qaClass.profManager.Profile("default");
                //}

                //if (newprofilebtn_clicked == false) { GUIprof = qaClass.profManager.Profile("default"); }//Melek - first time display default
                //else
                //{
                if (ProfilesListBox2.SelectedItem != null && qaClass.profManager.ProfileExists(ProfilesListBox2.SelectedItem.ToString()))//Melek - new condition added for remove profile function
                {
                    GUIprof = qaClass.profManager.Profile(ProfilesListBox2.SelectedItem.ToString());
                }
                else
                {
                    GUIprof = qaClass.profManager.Profile("default");
                }
                // }
                label30.Text = "Profile \"" + GUIprof.name + "\" (" + Global.DateToFileString(GUIprof.creationDate) + ")";
                // ProfileGroupBox2.Text = "Profile \"" + GUIprof.name + "\" (" + Global.DateToFileString(GUIprof.creationDate) + ")";
                //
                // Bad scene parameters update
                //
                QosAlert2.Value = (decimal)GUIprof.badSceneDetector.MosThreshold;	// force the customisable field for QT
                GUIprof.badSceneDetector.MosThreshold = (float)QosAlert2.Value;		// update static value of qos threshold

                BadSceneMinimumDurationBox2.Value = (decimal)GUIprof.badSceneDetector.WindowLength;

                //
                // Aggregated bad scenes settings update on GUI
                //
                //AggBadSceneBox2.Checked = GUIprof.aggregatedBadSceneDetector.Enabled;
                //AggToleranceBox2.Value = (decimal)GUIprof.aggregatedBadSceneDetector.Tolerance;

                //if (GUIprof.aggregatedBadSceneDetector.Enabled == true) AggBadSceneBox2.Checked = true;
                //else AggBadSceneBox2.Checked = false;



                //
                // Clip decision settings update on GUI
                //
                //BorderlineThreshold2.Value = GUIprof.badClipDetector.BorderlineThreshold2;
                RejectedThreshold2.Value = GUIprof.badClipDetector.FailThreshold;

                //
                // Black/white scene detector settings update on GUI
                //
                if (GUIprof.blankSceneDetector.Enabled == true) BlackWhiteSceneDetectionBox2.Checked = true;
                else BlackWhiteSceneDetectionBox2.Checked = false;

                BlackSceneDurationTBox2.Value = (decimal)GUIprof.blankSceneDetector.DurationT;
                BlackSceneIntTBox2.Value = (decimal)GUIprof.blankSceneDetector.BlackIntensityThreshold;
                WhiteSceneIntTBox2.Value = (decimal)GUIprof.blankSceneDetector.WhiteIntensityThreshold;
                BlackSceneUniTBox2.Value = (decimal)GUIprof.blankSceneDetector.UniformityT;
                BlackSceneDecisionBox2.Text = GUIprof.blankSceneDetector.decisionToTake.ToString();

                //
                // Frozen frame detector settings update on GUI
                //
                //if (GUIprof.frozenSceneDetector.Enabled == true) FrozenDetectionBox2.Checked = true;
                //else FrozenDetectionBox2.Checked = false;

                //FrozenDurationTBox2.Value = (decimal)GUIprof.frozenSceneDetector.DurationThreshold;
                //FrozenDecisionBox2.Text = GUIprof.frozenSceneDetector.decisionToTake.ToString();

                //
                // Aggregate bad scenes parameters update
                //
                //if (GUIprof.aggregatedBadSceneDetector.Enabled == true) AggBadSceneBox2.Checked = true;
                //else AggBadSceneBox2.Checked = false;
                //AggToleranceBox2.Value = (decimal)GUIprof.aggregatedBadSceneDetector.Tolerance;

                //
                // Troughs detection update on GUI
                //
                if (GUIprof.troughDetector.Enabled == true) QualityTroughDetection2.Checked = true;
                else QualityTroughDetection2.Checked = false;
                TroughMinDurationBox2.Value = (decimal)GUIprof.troughDetector.DurationThreshold;
                TroughMosThresholdBox2.Value = (decimal)GUIprof.troughDetector.MosThreshold;
                TroughDecisionBox2.Text = GUIprof.troughDetector.decisionToTake.ToString();

                //
                // Field order detector on GUI
                //
                //if (GUIprof.fieldOrderDetector.Enabled == true)
                //{
                //    FieldOrderBox2.Checked = true;
                //    FieldDecisionBox2.Enabled = true;
                //}
                //else
                //{
                //    FieldOrderBox2.Checked = false;
                //    FieldDecisionBox2.Enabled = false;
                //}
                //FieldDecisionBox2.Text = GUIprof.fieldOrderDetector.decisionToTake.ToString();

                //
                // Audio detector on GUI
                //
                //    if (GUIprof.audioDetector.Enabled == true)
                //    {
                //        AudioCheckDetectionBox2.Checked = true;
                //        audioAlertDecisionBox2.Enabled = true;
                //        IgnoreUnsupportedStreamsBox2.Enabled = true;
                //    }
                //    else
                //    {
                //        AudioCheckDetectionBox2.Checked = false;
                //        audioAlertDecisionBox2.Enabled = false;
                //        IgnoreUnsupportedStreamsBox2.Enabled = false;
                //    }
                //    audioAlertDecisionBox2.Text = GUIprof.audioDetector.decisionToTake.ToString();
                //    IgnoreUnsupportedStreamsBox2.Checked = GUIprof.audioDetector.IgnoreUnsupportedFormat;

            
        }

        /// <summary>
        /// Update the profiles tab
        /// </summary>
        private void refreshProfilesTab(string operation)
        {
            int selected = -1;
            if (newprofilebtn_clicked) { selected = -1; }//Melek-new profile added 
            else
            {
                if (ProfilesListBox2.SelectedIndex >= 0)
                    selected = ProfilesListBox2.SelectedIndex;
            }

            if (operation == "updategui")
            {
                ProfilesListBox2.Items.Clear();
                foreach (string p in qaClass.profManager.List)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Name = p;
                    ProfilesListBox2.Items.Add(lvi.Name);
                }
                //if (selected > 0)
                //   ProfilesListBox2.SelectedItem = selected;
                //else 
                //if (ProfilesListBox2.SelectedIndex < 0)
                   
                if (selected< 0) //if New profile added -Melek 
                {
                    if (newprofilebtn_clicked != true) //Melek - start with default profile
                    {
                        ProfilesListBox2.SelectedIndex = 0;
                    }
                    else
                    {
                        selected = qaClass.profManager.List.Count - 1;
                        ProfilesListBox2.SelectedIndex = selected;//Melek
                    }
                }
            }

            else if (operation == "Remove")
            {

                if (ProfilesListBox2.SelectedItem == null)
                {
                    MessageBox.Show("No profile is selected");
                    return;
                }
                else if (ProfilesListBox2.SelectedItem.ToString() == "default")
                {
                    MessageBox.Show("'default' profile can not be deleted.");
                    return;
                }
                else
                {
                    for (int i = 0; i < ProfilesListBox2.Items.Count; i++)
                    {
                        foreach (string p in qaClass.profManager.List)
                        {
                            if (p == ProfilesListBox2.SelectedItem)
                            {
                                ListViewItem lvi = new ListViewItem();
                                lvi.Name = p;
                                try
                                {
                                    ProfilesListBox2.Items.Remove(lvi.Name);
                                    Global.log("Removed Profile: "+lvi.Name);
                                    qatool.ForceSummaryUpdate(lvi.Name,true);
                                    //updategui();
                                    //MessageBox.Show("Profile was updated");
                                }
                                catch (Exception ex) { }
                                qaClass.profManager.DeleteProfile(p);//Melek
                            }
                        }
                        

                    }

                    if (qaClass.profManager.List.Count < 2) //demostick
                    {
                        this.NewProfile.Enabled = true;
                    }

                    if (selected > 0) //Melek
                        ProfilesListBox2.SelectedIndex = 0;

                    else if (ProfilesListBox2.SelectedIndex < 0) // case where the profiles have not been loaded yet
                    {
                        if (qaClass.profManager.List.Count > 0)
                            ProfilesListBox2.SelectedIndex = 0;
                    }
                    UpdateFoldersGui();
                }

            }
        }

        /// <summary>
        /// Updates : settings tab and profiles tab 
        /// </summary>
        public void updategui()
        {
            try
            {

                if (InvokeRequired)
                {
                    UpdateCallback d = new UpdateCallback(updategui);
                    Invoke(d, new object[] { });
                    //BeginInvoke(d, new object[] { });
                }

                else
                {
                    /*
                    // Update Monitoring list Contextual menu
                    ResultListContextMenu = new ContextMenuStrip();
                   
                    // define a sub menu (csm1)
                    ToolStripMenuItem csm1 = new ToolStripMenuItem("Apply a quality profile");
                    
                    // define 1 sub item per quality profile in this sub menu
                    //foreach (string name in qaClass.profManager.List)
                    {
                        //csm1.DropDownItems.Add(name, null,new EventHandler(ForceNewProfile));
                    }
                    // test
                    //ResultListContextMenu.Items.Add(csm1);

                    // link the updated context menu strip to the monitoring list view.
                    
                    //ResultListView.ContextMenuStrip = MonitoringListContextMenu;
                    */
                    // settings tab 

                    //NbThreadsNumeric.Value = qaClass.settings.nbThreads;
                    //    ProcessorsLabel.Text = qaClass.jobManager.ProcessorsString; //Melek - No more processor label on the UI
                    //  CacheDirSizeBox.Value = qaClass.settings.CacheLimit; //Melek - cache was disabled
                    //   ZipResBox.Checked = qaClass.settings.ZipRes; //Melek - no more zip functionality

                    // profiles tab
                    //string op = "updategui";
                    refreshProfilesTab("updategui");//Melek - updategui parameter added
                   
                }
            }
            catch (Exception ex)
            {
                Global.log("Problem while updating quality settings on GUI\n" + ex);
            }
        }//end of updategui

        // change the profile for the selected stream in monitoring view stream and put it in the summary queue
        // private void ForceNewProfile(Object o, BrightIdeasSoftware.CellRightClickEventArgs e)
        // private void ForceNewProfile(Object o, EventArgs e)
        private void ForceNewProfile(string newprofile, VStream tvs)
        {
            //BrightIdeasSoftware.CellRightClickEventArgs re = (BrightIdeasSoftware.CellRightClickEventArgs)e;
            //VStream tvs = (VStream)re.Model;
            //string newprofile = o.ToString();

            if (tvs.sum.qualityProfile.name != newprofile)
            {
                tvs.sum.profile_to_use = newprofile;
                qatool.ForceSummaryUpdate(tvs);

                //TODO: This doesn't seem necessary. Do you know why it was here??
                //EditSummaryInList(tvs, EventArgs.Empty);
            }
            /*
            VStream tvs;

            if (ResultListView.SelectedItems.Count > 0)
            {
                for (int i = 0; i < ResultListView.SelectedItems.Count; i++)
                {
                    tvs = (VStream)ResultListView.SelectedItems[i].Tag;
                    tvs.sum.profile_to_use = newprofile;
                    qatool.ForceSummaryUpdate(tvs);
                }
            }
            */
        }


        public struct BadScene
        {
            public float Start;
            public float Duration;
            public float MosAverage;

            public BadScene(float start, float duration, float mosaverage)
            {
                Start = start;
                Duration = duration;
                MosAverage = mosaverage;
            }
        }


        //  Fill the Key scene list box 
        void FillBadSceneList()
        {
            int i;
            if (vs == null)
                return;

            //ArrayList tempa;            // temp array to store either "alert scenes" or "aggregated alert scenes"
            //tempa = new ArrayList();
            List<AlertScene> tempa = new List<AlertScene>();
            AlertScene AlScn;

            BadSceneListView.Items.Clear();

            // fill the badscene array list view

            if (vs.sum.qualityProfile.aggregatedBadSceneDetector.Enabled == true)
                if (ShowAggBSBox.Checked == true)
                    tempa = vs.sum.AggAlertScnA;
                else
                    tempa = vs.sum.AlertScnA;
            else
                tempa = vs.sum.AlertScnA;

            //for (i = 0; i < tempa.Count; i++)
            for (i = 1; i <= tempa.Count; i++)//Melek 
            {
                ListViewItem lviItem = new ListViewItem();
                // if the scene is bad quality
                try
                {
                    AlScn = (AlertScene)tempa[i];
                    lviItem.Text = Convert.ToString(i);
                    lviItem.Tag = AlScn;
                    lviItem.SubItems.Add(AlScn.Type);
                    lviItem.SubItems.Add(Global.frameToDurationString((int)AlScn.Start, vs.cv.format.frameRate, true));
                    lviItem.SubItems.Add(Global.frameToDurationString((int)AlScn.DurationInFrames, vs.cv.format.frameRate, true));
                    if (AlScn.Type == "Bad Scene")
                    {
                        if (ShowBadSceneBox.Checked == true)
                        {
                            // cap the mos value here. As mos average are calculated based on frames that go slightly out of boundaries (hysteresis)
                            // the average could be over the threshold set in the quality profile.
                            double mosav = Math.Round(AlScn.Value, 2);
                            if (mosav > vs.sum.qualityProfile.badSceneDetector.MosThreshold) mosav = vs.sum.qualityProfile.badSceneDetector.MosThreshold;
                            lviItem.SubItems.Add("mos=" + Convert.ToString(Math.Round(mosav, 2)));
                            lviItem.BackColor = Colours.badScene_backColor;
                            BadSceneListView.Items.Add(lviItem);
                        }
                    }
                    else if (AlScn.Type == "Agg Bad Scene")
                    {
                        if (ShowAggBSBox.Checked == true)
                        {
                            lviItem.SubItems.Add("mos=" + Convert.ToString(Math.Round(AlScn.Value, 2)));
                            lviItem.BackColor = Colours.aggBadScene_backColor;
                            BadSceneListView.Items.Add(lviItem);
                        }
                    }
                    else if (AlScn.Type == "Black")
                    {
                        if ((ShowBWBox.Checked == true) || (ShowAllScenesBox.Checked == true))
                        {
                            lviItem.SubItems.Add("Int=" + Convert.ToString(Math.Round(AlScn.Value, 2)));
                            lviItem.BackColor = Colours.blackScene_backColor;
                            lviItem.ForeColor = Colours.blackScene_foreColor;
                            BadSceneListView.Items.Add(lviItem);
                        }
                    }
                    else if (AlScn.Type == "White")
                    {
                        if ((ShowBWBox.Checked == true) || (ShowAllScenesBox.Checked == true))
                        {

                            lviItem.SubItems.Add("Int=" + Convert.ToString(Math.Round(AlScn.Value, 2)));
                            lviItem.BackColor = Colours.whiteScene_backColor;

                            BadSceneListView.Items.Add(lviItem);
                        }
                    }

                    else if (AlScn.Type == "Frozen")
                    {
                        if ((ShowFrozenBox.Checked == true) || (ShowAllScenesBox.Checked == true))
                        {
                            lviItem.BackColor = Colours.frozenScene_backColor;
                            BadSceneListView.Items.Add(lviItem);
                        }
                    }

                    else if (AlScn.Type == "Trough")
                    {
                        if ((ShowTroughsBox.Checked == true) || (ShowAllScenesBox.Checked == true))
                        {
                            lviItem.SubItems.Add("mos=" + Convert.ToString(Math.Round(AlScn.Value, 2)));
                            lviItem.BackColor = Colours.troughScene_backColor;
                            lviItem.ForeColor = Colours.troughScene_foreColor;

                            BadSceneListView.Items.Add(lviItem);
                        }
                    }
                    else if (AlScn.Type == "Field order")
                    {
                        if ((ShowFieldsBox.Checked == true) || (ShowAllScenesBox.Checked == true))
                        {
                            lviItem.SubItems.Add(AlScn.Comment);
                            lviItem.BackColor = Colours.fieldScene_backColor;
                            lviItem.ForeColor = Colours.fieldScene_foreColor;

                            BadSceneListView.Items.Add(lviItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Problem updating the badscene arraylist view\n" + ex);
                }
            }
        }

        // Starts playback for the selected stream
        int OpenStream(string videopath)
        {
            OpenStream(videopath, 0);
            return 0;
        }

        //
        // Starts playback for the selected stream at the specified time
        //
        int OpenStream(string videopath, double videotime)
        {
            double td;  // store stream duration;
            int hr = 0;

            // animate bt logo
            //   logo_webbrowser.Refresh();

            // cover overlay options with video or not
            // if overlay is active
            ////////if (OverlayButton.Checked) // Overlay button config - Melek
            ////////{
            ////////    VideoPanel.Width = VideoPlayerPanel.Width - OverlayPanel.Width;
            ////////    OverlayBar.Location = new System.Drawing.Point(-1, -1); ;
            ////////    OverlayBar.Text = ">";
            ////////}
            ////////else
            ////////// if overlay is not active
            ////////{
            ////////    VideoPanel.Width = VideoPlayerPanel.Width - OverlayBar.Width;
            ////////    OverlayBar.Location = new System.Drawing.Point(OverlayPanel.Width - OverlayBar.Width - 1, -1); ;
            ////////    OverlayBar.Text = "<";
            ////////}

            filename = videopath;
            try//Melek
            {
                if (filename == string.Empty)
                    return -1;

                // this.graphBuilder = (IGraphBuilder)new FilterGraph();
                this.graphBuilder = (IFilterGraph2)new FilterGraph();

                // We manually add the VMR (video mixer/renderer) filter so that we can set it into "mixing" mode.
                // That is needed so that the VMR will deinterlace before playing back
                VideoMixingRenderer vmr = new VideoMixingRenderer(); //Tom's deinterlace changes
                graphBuilder.AddFilter((IBaseFilter)vmr, "vmr");//Tom's deinterlace changes
                IVMRFilterConfig vmrConfig = vmr as IVMRFilterConfig;//Tom's deinterlace changes
                int nRet = vmrConfig.SetNumberOfStreams(1);//Tom's deinterlace changes


                // BuildVideoGraph(videopath); //No more overlay - Melek
                hr = this.graphBuilder.RenderFile(filename, null);//MELek - instead of calling BuildVideoGraph function call RenderFile function directly
                DsError.ThrowExceptionForHR(hr);//Melek

                // QueryInterface for DirectShow interfaces
                this.mediaControl = (IMediaControl)this.graphBuilder;
                this.mediaEventEx = (IMediaEventEx)this.graphBuilder;
                this.mediaSeeking = (IMediaSeeking)this.graphBuilder;
                this.mediaPosition = (IMediaPosition)this.graphBuilder;

                // Query for video interfaces, which may not be relevant for audio files
                this.videoWindow = this.graphBuilder as IVideoWindow;
                this.basicVideo = this.graphBuilder as IBasicVideo;

                // Query for audio interfaces, which may not be relevant for video-only files
                this.basicAudio = this.graphBuilder as IBasicAudio;

                // Is this an audio-only file (no video component)?
                CheckVisibility();

                // Have the graph signal event via window callbacks for performance
                hr = this.mediaEventEx.SetNotifyWindow(this.Handle, WMGraphNotify, IntPtr.Zero);

                DsError.ThrowExceptionForHR(hr);

                //if (!this.isAudioOnly)
                //{
                // Setup the video window
                hr = this.videoWindow.put_Owner(this.VideoPanel.Handle);

                DsError.ThrowExceptionForHR(hr);

                hr = this.videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
                DsError.ThrowExceptionForHR(hr);

                hr = InitVideoWindow(1, 1);
                DsError.ThrowExceptionForHR(hr);

                GetFrameStepInterface();
                //}
                //else
                //{
                // Initialize the default player size
                //    hr = InitPlayerWindow();
                //    DsError.ThrowExceptionForHR(hr);

                //EnablePlaybackMenu(true, MediaType.Audio);
                //}

                // Complete window initialization
                //CheckSizeMenu(menuFileSizeNormal);
                //this.isFullScreen = false;
                this.currentPlaybackRate = 1.0;

#if DEBUG
                rot = new DsROTEntry(this.graphBuilder);
#endif

                // check the mute button
                MuteStatus();

                // Run the graph to play the media file
                this.mediaPosition.put_CurrentPosition(videotime);
                hr = this.mediaControl.Run();


                DsError.ThrowExceptionForHR(hr);

                this.currentState = PlayState.Running;

                UpdateMainTitle();

                try
                {
#if !DEMO
                    this.mediaPosition.get_Duration(out td);
#else
                    td=vs.getDurationInSeconds();
#endif

                    this.VideoProgressionBar.Minimum = 0;
                    this.VideoProgressionBar.Maximum = (int)(td * 100);

                    isTimer = true;
                    // disable if raw 264 files are open (.264 or .h264) as they dont allow seeking
                    if (videopath.EndsWith("264"))
                        this.mediaSeeking = null;
                    return 0;


                }
                catch (Exception ex)
                {
                    //Global.log("Problem opening " + vs.path.Name + ".ts\n" + ex);
                    Global.log("Problem opening " + vs.StreamFileName + "\n" + ex); //Melek - path.name => streamfilename
                    EnablePlayback(false); //MElek
                    return -1;
                }

            }
            catch (Exception ex) //Melek
            {
                //  MessageBox.Show(ex.Message);
                EnablePlayback(false);
                return -1;
            }
        }


        // build the graph to use the correct decoder filter to display the video with or without overlay
        private int BuildVideoGraph(string sourcestr)
        {
            //#if DEBUG
            // rot = new DsROTEntry(this.graphBuilder);
            //#endif
            int hr = -1;

            if (((OverlayButton.Enabled) && (OverlayButton.Checked)))
            {
                hr = BuildOVerlayGraph(sourcestr);
            }

            // if there was an error or if overlay disabled build the default rendering graph
            if (hr < 0)
            {
                hr = this.graphBuilder.RenderFile(filename, null);
                DsError.ThrowExceptionForHR(hr);
            }

            return hr;
        }

        private bool EnableOverlay()
        {
            if ((vs.cv.codecSpecs.codec.name == VideoCodec.AVC.name) && (vs.cv.format.resY == 576) && (vs.cv.format.resX == 720))
                return true;
            else
                return false;
        }

        private int BuildOVerlayGraph(string sourcestr)
        {
            int hr = 0;
            DirectShowLib.IPin inpin = null, outpin = null;
            Type comtype = null;
            DsGuid guid;


            if (EnableOverlay())
            {
                DirectShowLib.IBaseFilter source = null; // video file demuxer interface
                DirectShowLib.IFileSourceFilter file_source = null; // video source file interface

                // Haali
                //
                // load Haali in the graph
                //
                guid = new DsGuid("55DA30FC-F16B-49FC-BAA5-AE59FC65F82D");// haali
                comtype = System.Type.GetTypeFromCLSID(guid);
                source = (DirectShowLib.IBaseFilter)Activator.CreateInstance(comtype);
                hr = this.graphBuilder.AddFilter(source, "Haali");

                // set the source file for Haali
                //
                file_source = (DirectShowLib.IFileSourceFilter)source;
                DirectShowLib.AMMediaType media = new AMMediaType();
                //media.majorType= DirectShowLib.MediaType.vide;
                hr = file_source.Load(sourcestr, media);
                DsError.ThrowExceptionForHR(hr);

                // BT AVC overlay internal DMO filter
                //
                DirectShowLib.IBaseFilter avc_overlay_dmo_filter = null; // BT audio check
                DirectShowLib.IDMOWrapperFilter avc_overlay_dmo_wrapper = null;

                //guid = new DsGuid("04FE9017-F873-410E-871E-AB91661A4EF7");// FFDShow decoder filter
                DsGuid avc_overlay_guid = new DsGuid("8ac1be3b-70ef-4b0c-ad91-dbe9d41db405"); // BT AVC overlay decoder guid

                avc_overlay_dmo_filter = (DirectShowLib.IBaseFilter)new DMOWrapperFilter();
                avc_overlay_dmo_wrapper = (IDMOWrapperFilter)avc_overlay_dmo_filter;

                /*
                hr = avc_overlay_dmo_wrapper.Init(avc_overlay_guid, DirectShowLib.DMO.DMOCategory.VideoDecoder);
                DMOError.ThrowExceptionForHR(hr);
                */

                // But it is more useful to show how to scan for the DMO
                //Guid g = FindGuid("BT H.264 Decoder", DMOCategory.VideoEffect);

                hr = avc_overlay_dmo_wrapper.Init(avc_overlay_guid, DMOCategory.VideoDecoder);
                DMOError.ThrowExceptionForHR(hr);

                hr = this.graphBuilder.AddFilter(avc_overlay_dmo_filter, "BT AVC Overlay");
                DsError.ThrowExceptionForHR(hr);

                // get the input pin of the video overlay filter
                Global.debuglog("looking for input pin on avc overlay filter", EventArgs.Empty);
                inpin = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)avc_overlay_dmo_filter, PinDirection.Input, 0);
                if (inpin == null)
                    Global.log("!Could not find input pin on Audio Check", EventArgs.Empty);
                else
                    Global.debuglog("found input pin on Audio Check", EventArgs.Empty);

                //find the video output pin of Haali
                outpin = DsFindPin.ByDirection(source, PinDirection.Output, 0);
                //if (!CheckVideo(outpin)) // if the first pin is not the video, use the second pin
                //    outpin = DsFindPin.ByDirection(source, PinDirection.Output, 1);

                // connect haali video output pin (avc) to BT AVC overlay filter
                hr = this.graphBuilder.Connect(outpin, inpin);

                // render the output pin of the AVC overlay filter
                outpin = DsFindPin.ByDirection((DirectShowLib.IBaseFilter)avc_overlay_dmo_filter, PinDirection.Output, 0);

                hr = this.graphBuilder.Render(outpin);
            }

            return hr;
        }

        private void SetDMOParams(DirectShowLib.IBaseFilter dmoWrapperFilter)
        {
            int hr;
            Guid g;
            int i;
            int pc;
            ParamInfo pInfo;
            IMediaParamInfo paramInfo = dmoWrapperFilter as IMediaParamInfo;

            // With a little effort, a generic parameter handling routine
            // could be produced.  You know the number of parameters (GetParamCount),
            // the type of the parameter (pInfo.mpType), the range of values for
            // int and float (pInfo.mpdMinValue, pInfo.mpdMaxValue), if the parameter is an
            // enum, you have the strings (GetParamText).

            hr = paramInfo.GetParamCount(out pc);
            DMOError.ThrowExceptionForHR(hr);

            // Walk all the parameters
            for (int pCur = 0; pCur < pc; pCur++)
            {
                IntPtr ip;

                hr = paramInfo.GetParamInfo(pCur, out pInfo);
                DMOError.ThrowExceptionForHR(hr);

                hr = paramInfo.GetParamText(0, out ip);
                DMOError.ThrowExceptionForHR(hr);

                try
                {
                    string sName, sUnits;
                    string[] sEnum;
                    ParseParamText(ip, out sName, out sUnits, out sEnum);

                    Debug.WriteLine(string.Format("Parameter name: {0}", sName));
                    Debug.WriteLine(string.Format("Parameter units: {0}", sUnits));

                    // Not all params will have enumerated strings.
                    if (pInfo.mpType == MPType.ENUM)
                    {
                        // The final entry in "splitted" will be a blank (used to terminate the list).
                        for (int x = 0; x < sEnum.Length; x++)
                        {
                            Debug.WriteLine(string.Format("Parameter Enum strings: {0} = {1}", x, sEnum[x]));
                        }
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ip);
                }
            }

            hr = paramInfo.GetCurrentTimeFormat(out g, out i);
            DMOError.ThrowExceptionForHR(hr);

            hr = paramInfo.GetSupportedTimeFormat(0, out g);
            DMOError.ThrowExceptionForHR(hr);

            MPData o = new MPData();
            m_param = dmoWrapperFilter as IMediaParams;

            o.vInt = 0;
            hr = m_param.SetParam(0, o);
            DMOError.ThrowExceptionForHR(hr);
        }

        // Break an the pointer to some ParamText into usable fields
        private void ParseParamText(IntPtr ip, out string ParamName, out string ParamUnits, out string[] ParamEnum)
        {
            int iCount = 0;
            string s;

            // Up to the first null is the display name
            ParamName = Marshal.PtrToStringUni(ip);
            ip = (IntPtr)(ip.ToInt32() + ((ParamName.Length + 1) * 2));

            // Next is the units
            ParamUnits = Marshal.PtrToStringUni(ip);
            ip = (IntPtr)(ip.ToInt32() + ((ParamUnits.Length + 1) * 2));

            // Following, there may b zero or more enum strings.  First we count them.
            IntPtr ip2 = ip;
            while (Marshal.ReadInt16(ip2) != 0) // Terminate on a zero length string
            {
                s = Marshal.PtrToStringUni(ip2);
                ip2 = (IntPtr)(ip2.ToInt32() + ((s.Length + 1) * 2));
                iCount++;
            }

            // Now we allocate the array, and copy the values in.
            ParamEnum = new string[iCount];
            for (int x = 0; x < iCount; x++)
            {
                ParamEnum[x] = Marshal.PtrToStringUni(ip);
                ip = (IntPtr)(ip.ToInt32() + ((ParamEnum[x].Length + 1) * 2));
            }
        }

        //
        // This method checks if a pin is processing a video stream
        //
        bool CheckVideo(DirectShowLib.IPin pin)
        {
            AMMediaType mt = new AMMediaType();
            mt.majorType = DirectShowLib.MediaType.Video;
            if (pin.QueryAccept(mt) == 0)
                return true;
            else
                return false;
        }

        private int InitPlayerWindow()
        {
            // Check the 'full size' menu item
            //CheckSizeMenu(menuFileSizeNormal);
            //EnablePlaybackMenu(false, MediaType.Audio);

            return 0;
        }

        //
        // Some video renderers support stepping media frame by frame with the
        // IVideoFrameStep interface.  See the interface documentation for more
        // details on frame stepping.
        //
        private bool GetFrameStepInterface()
        {
            int hr = 0;

            IVideoFrameStep frameStepTest = null;

            // Get the frame step interface, if supported
            frameStepTest = (IVideoFrameStep)this.graphBuilder;

            // Check if this decoder can step
            hr = frameStepTest.CanStep(0, null);
            if (hr == 0)
            {
                this.frameStep = frameStepTest;
                return true;
            }
            else
            {
                //Marshal.ReleaseComObject(frameStepTest);
                this.frameStep = null;
                return false;
            }
        }

        private void ClosePlayback()
        {
            int hr = 0;

            try
            {
                if (this.mediaEventEx != null)
                {
                    hr = this.mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                    DsError.ThrowExceptionForHR(hr);
                }

#if DEBUG
                if (rot != null)
                {
                    rot.Dispose();
                    rot = null;
                }
#endif
                // Release and zero DirectShow interfaces
                if (this.mediaEventEx != null) this.mediaEventEx = null;
                if (this.mediaSeeking != null) this.mediaSeeking = null;
                if (this.mediaPosition != null) this.mediaPosition = null;
                if (this.mediaControl != null) this.mediaControl = null;
                if (this.basicAudio != null) this.basicAudio = null;
                if (this.basicVideo != null) this.basicVideo = null;
                if (this.videoWindow != null) this.videoWindow = null;
                if (this.frameStep != null) this.frameStep = null;
                if (this.graphBuilder != null) Marshal.ReleaseComObject(this.graphBuilder);
                this.graphBuilder = null;

                //GC.Collect();
            }
            catch
            {
            }
        }

        private void CloseInterfaces()
        {
            ClosePlayback();

            try
            {
                lock (this)
                {
                    Console.WriteLine("Close interfaces function");
                    VideoProgressionBar.Value = 0;
                    VideoProgressionBar.Enabled = false;
                    this.isTimer = false;

                    try
                    {
                        if (MOSchart != null)
                        {
                            MOSchart.ResetGraph();
                        }

                    }
                    catch (Exception ex)
                    {
                        Global.log("Test on MOSchart when value not init.\n" + ex);
                    }

                    PlaybackState(false);

                    //  currentfilename.Text = "";
                    currentfilename.Text = "No Clip Selected"; //MElek

                    if (defaultbackcolor)
                    {
                        currentfilename.BackColor = Control.DefaultBackColor;
                        defaultbackcolor = true;
                    }

                    redflaggedPerCent.Text = "";
                    redflaggedPerCent.BackColor = Control.DefaultBackColor;
                    AvMosLabel.Text = "";
                    AvMosLabel.BackColor = Control.DefaultBackColor;
                    BadLabel.BackColor = Color.White;
                    AvMosTextLabel.BackColor = Color.White;

                    ShowAggBSBox.Enabled = false;
                    BadSceneListView.Items.Clear();
                    ClipDetailsListView.Items.Clear();
                }
            }
            catch
            {
            }
        }

        //
        // Media Related methods
        //
        private void PauseClip()
        {
            if (this.mediaControl == null)
                return;

            // Toggle play/pause behavior
            if ((this.currentState == PlayState.Paused) || (this.currentState == PlayState.Stopped))
            {
                if (this.mediaControl.Run() >= 0)
                    this.currentState = PlayState.Running;
            }
            else
            {
                if (this.mediaControl.Pause() >= 0)
                    this.currentState = PlayState.Paused;
            }

            UpdateMainTitle();
        }

        private void CloseClip()
        {
            int hr = 0;

            // Stop media playback
            if (this.mediaControl != null)
                hr = this.mediaControl.Stop();

            // Clear global flags
            this.currentState = PlayState.Stopped;
            //this.isAudioOnly = true;
            //this.isFullScreen = false;

            // Free DirectShow interfaces and other GUI stuff
            CloseInterfaces();

            // Clear file name to allow selection of new file with open dialog
            filename = string.Empty;

            // No current media state
            this.currentState = PlayState.Init;

            UpdateMainTitle();
            InitPlayerWindow();
        }

        private void StopClip()
        {
            int hr = 0;
            DsLong pos = new DsLong(0);

            if ((this.mediaControl == null) || (this.mediaSeeking == null))
                return;

            // Stop and reset postion to beginning
            if ((this.currentState == PlayState.Paused) || (this.currentState == PlayState.Running))
            {
                hr = this.mediaControl.Stop();
                this.currentState = PlayState.Stopped;

                // Seek to the beginning
                hr = this.mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);

                // Display the first frame to indicate the reset condition
                hr = this.mediaControl.Pause();

                // Display ">" on the play button rather than "||"
                playbutton.Text = "";
                playbutton.Image = global::QA.Properties.Resources.bt_play_12;
            }
            UpdateMainTitle();
            ClosePlayback();
        }


        // Check the Mute button status
        //
        private int MuteStatus()
        {
            int hr = 0;

            if ((this.graphBuilder == null) || (this.basicAudio == null))
                return 0;

            // Read current volume
            hr = this.basicAudio.get_Volume(out this.currentVolume);
            if (hr == -1) //E_NOTIMPL
            {
                // Fail quietly if this is a video-only media file
                return 0;
            }
            else if (hr < 0)
            {
                return hr;
            }

            // Switch volume levels
            if (this.MuteButton.Checked)
                this.currentVolume = VolumeSilence;
            else
                this.currentVolume = VolumeFull;

            // Set new volume
            hr = this.basicAudio.put_Volume(this.currentVolume);

            UpdateMainTitle();
            return hr;

        }

        private int ToggleMute()
        {
            int hr = 0;

            if ((this.graphBuilder == null) || (this.basicAudio == null))
                return 0;

            // Read current volume
            hr = this.basicAudio.get_Volume(out this.currentVolume);
            if (hr == -1) //E_NOTIMPL
            {
                // Fail quietly if this is a video-only media file
                return 0;
            }
            else if (hr < 0)
            {
                return hr;
            }

            // Switch volume levels
            if (this.currentVolume != VolumeSilence)
                this.currentVolume = VolumeSilence;
            else
                this.currentVolume = VolumeFull;

            // Set new volume
            hr = this.basicAudio.put_Volume(this.currentVolume);

            UpdateMainTitle();
            return hr;
        }

        private void UpdateMainTitle()
        {
            Version vrs = new Version(Application.ProductVersion);

            // If no file is loaded, just show the application title
            if (this.vs == null)
            {
#if DEMO
                this.Text = "Path 1 - v.Cortex. v" + vrs.Major + "." + vrs.Minor + "." + vrs.Build + "." + vrs.Revision + " - DEMO (1min)";
#elif DEMO5MIN
                this.Text = "Path 1 - v.Cortex. v" + vrs.Major + "." + vrs.Minor + "." + vrs.Build + "." + vrs.Revision + " - DEMO (5min)";             
#else
                this.Text = "Path 1 - v.Cortex ";
#endif
            }
            else
            {
                //string media = (isAudioOnly) ? "Audio" : "Video";
                string muted = (currentVolume == VolumeSilence) ? "Mute" : "";
                string paused = (currentState == PlayState.Paused) ? "Paused" : "";

#if DEMO
                this.Text = String.Format("Path 1 - v.Cortex. v{3}.{4}.{5}.{6} - DEMO (1min) - {0} {1}{2}", this.vs.path.Name, muted, paused, vrs.Major, vrs.Minor, vrs.Build, vrs.Revision);
#elif DEMO5MIN
                this.Text = String.Format("Path 1 - v.Cortex. v{3}.{4}.{5}.{6} - DEMO (5min) - {0} {1}{2}", this.vs.path.Name, muted, paused, vrs.Major, vrs.Minor, vrs.Build, vrs.Revision);
#else
                this.Text = String.Format("Path 1 - v.Cortex - {0} {1}{2} ", this.vs.path.Name, muted, paused);
#endif



                if (vs.path.StreamPath != "")
                {
                    this.VideoPanelLabel.Text = "";
                    this.VideoPanelLabel.Image = global::QA.Properties.Resources.play_38;
                    this.VideoPanelLabel.Enabled = true;
                    this.VideoPanelLabel.Visible = true;
                    this.label41.Text = this.vs.StreamName;

                }
                else
                {
                    this.VideoPanelLabel.Text = "The clip is not available for playback";
                    this.VideoPanelLabel.Image = null;
                    this.VideoPanelLabel.Enabled = false;
                    this.label41.Text = "This reduced size progressive preview does not reflect the quality of the actual video.";
                }
            }

#if DEMO
                this.Text+=" - DEMO";
#endif

        }

        private int ToggleFullScreen()
        {
            int hr = 0;
            OABool lMode;

            // Don't bother with full-screen for audio-only files
            //if ((this.isAudioOnly) || (this.videoWindow == null))
            //    return 0;

            // Read current state
            hr = this.videoWindow.get_FullScreenMode(out lMode);
            DsError.ThrowExceptionForHR(hr);

            if (lMode == OABool.False)
            {
                // Save current message drain
                hr = this.videoWindow.get_MessageDrain(out hDrain);
                DsError.ThrowExceptionForHR(hr);

                // Set message drain to application main window
                hr = this.videoWindow.put_MessageDrain(this.Handle);
                DsError.ThrowExceptionForHR(hr);

                // Switch to full-screen mode
                lMode = OABool.True;
                hr = this.videoWindow.put_FullScreenMode(lMode);
                DsError.ThrowExceptionForHR(hr);
                //this.isFullScreen = true;
            }
            else
            {
                // Switch back to windowed mode
                lMode = OABool.False;
                hr = this.videoWindow.put_FullScreenMode(lMode);
                DsError.ThrowExceptionForHR(hr);

                // Undo change of message drain
                hr = this.videoWindow.put_MessageDrain(hDrain);
                DsError.ThrowExceptionForHR(hr);

                // Reset video window
                hr = this.videoWindow.SetWindowForeground(OABool.True);
                DsError.ThrowExceptionForHR(hr);

                // Reclaim keyboard focus for player application
                //this.Focus();
                //this.isFullScreen = false;
            }

            return hr;
        }

        private int StepOneFrame()
        {
            int hr = 0;

            // If the Frame Stepping interface exists, use it to step one frame
            if (this.frameStep != null)
            {
                // The graph must be paused for frame stepping to work
                if (this.currentState != PlayState.Paused)
                    PauseClip();

                // Step the requested number of frames, if supported
                hr = this.frameStep.Step(1, null);

                // hr = this.frameStep.Step(-1, null);//Melek check if its going to work backwards
                //  StepFrames(-1);
            }

            return hr;
        }

        private int StepFrames(int nFramesToStep)
        {
            int hr = 0;

            // If the Frame Stepping interface exists, use it to step frames
            if (this.frameStep != null)
            {
                // The renderer may not support frame stepping for more than one
                // frame at a time, so check for support.  S_OK indicates that the
                // renderer can step nFramesToStep successfully.
                hr = this.frameStep.CanStep(nFramesToStep, null);
                if (hr == 0)
                {
                    // The graph must be paused for frame stepping to work
                    if (this.currentState != PlayState.Paused)
                        PauseClip();

                    // Step the requested number of frames, if supported
                    hr = this.frameStep.Step(nFramesToStep, null);
                }
            }

            return hr;
        }

        private int ModifyRate(double dRateAdjust)
        {
            int hr = 0;
            double dRate;

            // If the IMediaPosition interface exists, use it to set rate
            if ((this.mediaPosition != null) && (dRateAdjust != 0.0))
            {
                hr = this.mediaPosition.get_Rate(out dRate);
                if (hr == 0)
                {
                    // Add current rate to adjustment value
                    double dNewRate = dRate + dRateAdjust;
                    hr = this.mediaPosition.put_Rate(dNewRate);

                    // Save global rate
                    if (hr == 0)
                    {
                        this.currentPlaybackRate = dNewRate;
                        UpdateMainTitle();
                    }
                }
            }

            return hr;
        }

        private int SetRate(double rate)
        {
            int hr = 0;

            // If the IMediaPosition interface exists, use it to set rate
            if (this.mediaPosition != null)
            {
                hr = this.mediaPosition.put_Rate(rate);
                if (hr >= 0)
                {
                    this.currentPlaybackRate = rate;
                    UpdateMainTitle();
                }
            }

            return hr;
        }

        private void HandleGraphEvent()
        {
            int hr = 0;
            EventCode evCode;
            int evParam1, evParam2;

            // Make sure that we don't access the media event interface
            // after it has already been released.
            if (this.mediaEventEx == null)
                return;

            // Process all queued events
            while (this.mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0)
            {
                // Free memory associated with callback, since we're not using it
                hr = this.mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);

                // If this is the end of the clip, reset to beginning
                if (evCode == EventCode.Complete)
                {
                    DsLong pos = new DsLong(0);
                    // Reset to first frame of movie
                    hr = this.mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning,
                      null, AMSeekingSeekingFlags.NoPositioning);
                    if (hr < 0)
                    {
                        // Some custom filters (like the Windows CE MIDI filter)
                        // may not implement seeking interfaces (IMediaSeeking)
                        // to allow seeking to the start.  In that case, just stop
                        // and restart for the same effect.  This should not be
                        // necessary in most cases.
                        hr = this.mediaControl.Stop();
                        hr = this.mediaControl.Run();
                    }
                }
            }
        }
        private void CheckVisibility()
        {
            int hr = 0;
            OABool lVisible;

            if ((this.videoWindow == null) || (this.basicVideo == null))
            {
                // Audio-only files have no video interfaces.  This might also
                // be a file whose video component uses an unknown video codec.
                //this.isAudioOnly = true;
                return;
            }
            else
            {
                // Clear the global flag
                //this.isAudioOnly = false;
            }
            try
            {
                hr = this.videoWindow.get_Visible(out lVisible);
                if (hr < 0)
                {
                    // If this is an audio-only clip, get_Visible() won't work.
                    //
                    // Also, if this video is encoded with an unsupported codec,
                    // we won't see any video, although the audio will work if it is
                    // of a supported format.
                    // int vlu = unchecked((int)0x80004002);//Melek
                    if (hr == unchecked((int)0x80004002)) //E_NOINTERFACE
                    {

                        //this.isAudioOnly = true;
                    }
                    else
                        DsError.ThrowExceptionForHR(hr);

                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); } //Melek - try catch block added
        }

        private int InitVideoWindow(int nMultiplier, int nDivider)
        {
            int hr = 0;
            int lHeight, lWidth;

            if (this.basicVideo == null)
                return 0;

            lWidth = this.VideoPanel.Width;
            lHeight = this.VideoPanel.Height;

            // Read the default video size
            //hr = this.basicVideo.GetVideoSize(out lWidth, out lHeight);

            if (hr == DsResults.E_NoInterface)
                return 0;

            //this.ClientSize = new Size(lWidth, lHeight);
            Application.DoEvents();

            hr = this.videoWindow.SetWindowPosition(0, 0, lWidth, lHeight);

            return hr;
        }

        // Called on GUI resize event
        // 
        private void ResizeVideoWindow(object sender, EventArgs e)
        {
            int hr = 0;
            if ((VideoPanel != null) && (videoWindow != null))
            {
                hr = this.videoWindow.SetWindowPosition(0, 0, this.VideoPanel.Width, this.VideoPanel.Height);
                DsError.ThrowExceptionForHR(hr);

            }

        }

        private void playbutton_Click(object sender, System.EventArgs e)
        {
            int pb;
            this.VideoTimer.Tick += new System.EventHandler(this.VideoTimer_Tick); //Melek
            try
            {
                if (VideoProgressionBar.Enabled == false)
                {
                    pb = 0;

                    if (vs.path.StreamIsAvailable && vs.Final_Decision.String != "Not made") //Melek - play the video if the decision is different than not made 
                    {
                        PlaybackState(true);
                        pb = OpenStream(vs.path.StreamPath);
                    }

                    if (pb == -1)
                        PlaybackState(false);
                    else
                    {
                        //playbutton.Text = "||";
                        this.playbutton.Image = global::QA.Properties.Resources.bt_pause_12;
                    }
                }
                else // playback is already enabled
                {
                    // Toggle play/pause behavior
                    if ((this.currentState == PlayState.Paused) || (this.currentState == PlayState.Stopped))
                    {
                        if (this.mediaControl.Run() >= 0)
                        {
                            this.currentState = PlayState.Running;
                            //playbutton.Text = "||";
                            this.playbutton.Image = global::QA.Properties.Resources.bt_pause_12;
                        }
                    }
                    else
                    {
                        if (this.mediaControl.Pause() >= 0)
                        {
                            this.currentState = PlayState.Paused;
                            //playbutton.Text = ">";
                            this.playbutton.Image = global::QA.Properties.Resources.bt_play_12;
                        }
                    }

                    //                    UpdateMainTitle();
                }

            }
            catch (Exception ex)
            {
                Global.log("Exception while starting playback\n" + ex);
                this.VideoTimer.Tick -= new System.EventHandler(this.VideoTimer_Tick);//Melek
            }
        }

        private void seekbutton_Click(object sender, System.EventArgs e)
        {
            this.mediaPosition.put_CurrentPosition(0);
        }

        //
        //when the user drag the trackbar, we seek the graph filter by
        //updating the CurrentPosition property
        //
        private void trackBar1_ValueChanged(object sender, System.EventArgs e)
        {
            if ((this.mediaSeeking != null) && !isTimer)
            {
                this.mediaPosition.put_CurrentPosition((double)VideoProgressionBar.Value / 100.0);
            }
        }

        public void TraceMOSchartDist()
        {
            MOSchartDist.ResetGraph(vs);
            //MOSchartDist.fQT = (float)vs.used_qp.BadSceneD.MosT;
            MOSchartDist.fQT = (float)vs.sum.qualityProfile.badSceneDetector.MosThreshold;

            if ((vs.rawdata.MosDistA[1, 4] != 0) || (vs.rawdata.MosDistA[1, 3] != 0) || (vs.rawdata.MosDistA[1, 2] != 0) || (vs.rawdata.MosDistA[1, 1] != 0))
            {
                MOSchartDist.AddDistPoints(vs.rawdata.MosDistA);
            }
        }

        public void TraceMOSchart()
        {
            MOSchart.ResetGraph(vs);
            MOSchart.fQT = (float)vs.sum.qualityProfile.badSceneDetector.MosThreshold;
            //MOSchart.fQT = (float)vs.used_qp.BadSceneD.MosT;

            if (vs.rawdata.sMosA.Count != 0)
            {
                MOSchart.AddPointsF(vs.rawdata.sMosA);	// pass the new array containing all mos values to the graph class
            }
        }

        /// <summary>
        /// populate video info list view in detailed view
        /// </summary>
        public void VideoInfo()
        {
            int i;
            string subtext;
            ListViewItem lvi;
            TreeNode tn, tn1, tn0;

            #region  Clip Summary

            // summary_duration.Text = vs.TotalDurationString;
            summary_duration.Text = vs.ProcessedDurationString; //Melek
            summary_videocodec.Text = vs.VideoSpecsSummaryString;
            summary_audiocodec.Text = vs.AudioSummaryLong;


            #region Clip summary warnings

            // user decision
            if (vs.sum.UserDecision.Made != Decision.NOTMADE)
            {
                lvi = new ListViewItem();
                lvi.Text = "User Decision";
                lvi.SubItems.Add(vs.sum.UserDecision.String + " by " + vs.sum.UserDecision.User + ", on " + vs.sum.UserDecision.Date);
                ClipDetailsListView.Items.Add(lvi);

                lvi = new ListViewItem();
                lvi.Text = "User comment";
                lvi.SubItems.Add("\"" + vs.sum.UserDecision.Comment + "\"");
                ClipDetailsListView.Items.Add(lvi);

            }

            // model decision
            lvi = new ListViewItem();
            lvi.Text = "Model Decision";
            subtext = " [ Using profile : " + vs.sum.qualityProfile.name;

            if (vs.sum.qualityProfile.creationDate.ToString() != "")
                subtext += " (created on " + vs.sum.qualityProfile.creationDate.ToString() + ") ]";
            else subtext += " ]";

            lvi.BackColor = vs.sum.ModelDecision.GetBackColor();

            lvi.SubItems.Add(vs.sum.ModelDecision.String + subtext);
            ClipDetailsListView.Items.Add(lvi);

            // decoder warnings
            if (vs.sum.DecoderWarningA.Count > 0)
            {
                lvi = new ListViewItem();
                lvi.Text = "Decoder ";
                lvi.SubItems.Add(vs.TotalDecoderWarnings() + " decoding warnings reported.");
                ClipDetailsListView.Items.Add(lvi);
            }

            // Demuxer warnings
            lvi = new ListViewItem();
            if (vs.demuxReport.ErrorPackets > 0)
            {
                lvi.Text = "Container";
                lvi.SubItems.Add(vs.demuxReport.ErrorPackets + " packet warnings.");
                ClipDetailsListView.Items.Add(lvi);
            }

            // Reconstructed frames warning
            lvi = new ListViewItem();
            if (vs.videoModelCheck.recStats.affectedFrames > 0)
            {
                lvi.Text = "Packet loss";
                lvi.SubItems.Add("Reconstructed frames : " + vs.videoModelCheck.recStats.affectedFrames + " ( " + Math.Round((double)vs.videoModelCheck.recStats.affectedFrames / (double)vs.videoModelCheck.processedDurationInFrames, 2) + "%).");
                ClipDetailsListView.Items.Add(lvi);
            }

            // Warning on various video aspects
            lvi = new ListViewItem();
            GUIwarning gw = new GUIwarning();
            for (i = 0; i < vs.sum.VideoWarningA.Count; i++)
            {
                lvi = new ListViewItem();
                lvi.Text = "Video";

                gw = (GUIwarning)vs.sum.VideoWarningA[i];
                lvi.SubItems.Add(gw.Detail);

                lvi.BackColor = gw.Decision.GetBackColor();

                ClipDetailsListView.Items.Add(lvi);
            }


            // audio summary
            //for (i = 0; i < vs.audioCheck.notes.Count; i++)
            //{
            //    lvi = new ListViewItem();
            //    lvi.Text = "Audio";
            //    lvi.SubItems.Add((string)vs.audioCheck.notes[i]);

            //    lvi.BackColor = CDecision.GetDecisionColor(vs.sum.qualityProfile.audioDetector.decisionToTake);

            //    ClipDetailsListView.Items.Add(lvi);
            //}

            // Quality profile based warnings
            lvi = new ListViewItem();
            for (i = 0; i < vs.sum.QualityWarningA.Count; i++)
            {
                GUIwarning vw = new GUIwarning();
                lvi = new ListViewItem();
                lvi.Text = "Quality";
                vw = (GUIwarning)vs.sum.QualityWarningA[i];
                lvi.SubItems.Add(vw.Detail);
                lvi.BackColor = vw.Decision.GetBackColor();
                ClipDetailsListView.Items.Add(lvi);
            }

            #endregion

            #region Warnings tabs

            // Container demux warning
            //StreamWarningTreeView.Nodes.Clear();

            //if (vs.demuxReport.TotalPackets != -1)
            //{
            //    tn = new TreeNode(vs.demuxReport.ErrorPackets + " container warnings (out of " + vs.demuxReport.TotalPackets + " packets)");
            //    foreach (String s in vs.demuxReport.detectedDemuxErrors)
            //    {
            //        tn.Nodes.Add(s);
            //    }
            //}
            //else
            //{
            //    tn = new TreeNode("No container analysis was made on this asset (TS only).");
            //}
            //StreamWarningTreeView.Nodes.Add(tn);

            // Decoder warning
            //tn = new TreeNode(vs.TotalDecoderWarnings() + " decoder warnings (out of " + vs.videoModelCheck.processedDurationInFrames + " frames)");
            //for (i = 0; i < vs.sum.DecoderWarningA.Count; i++)
            //{
            //    DecoderWarning dw = (DecoderWarning)vs.sum.DecoderWarningA[i];
            //    tn.Nodes.Add(dw.counter + "x Error " + dw.errorCode + ": " + BTDecoderWarnings.ErrorLabel(dw.errorCode));
            //}
            //StreamWarningTreeView.Nodes.Add(tn);

            //tn = new TreeNode(vs.videoModelCheck.recStats.affectedFrames + " reconstructed frames (due to packet loss) : ( " + Math.Round((double)vs.videoModelCheck.recStats.affectedFrames / (double)vs.videoModelCheck.processedDurationInFrames, 2) + "%)");
            //if (vs.videoModelCheck.recStats.affectedFrames > 0)
            //{
            //    tn1 = new TreeNode("Percentage of reconstructed Block per affected frame");
            //    tn1.Nodes.Add("Max. : " + vs.videoModelCheck.recStats.RecPercentage.max + " %");
            //    tn1.Nodes.Add("Min. : " + vs.videoModelCheck.recStats.RecPercentage.min + " %");
            //    tn1.Nodes.Add("Average : " + vs.videoModelCheck.recStats.RecPercentage.average + " %");
            //    tn.Nodes.Add(tn1);
            //}
            //StreamWarningTreeView.Nodes.Add(tn);

            //StreamWarningTreeView.ExpandAll();

            #endregion

            //#region Model Info
            ////
            //ModelInfoTreeView.Nodes.Clear();

            //tn = new TreeNode("Models Versions");
            //tn.Nodes.Add("Demuxer : " + vs.tasks.Demux.application);
            //tn.Nodes.Add("Video model : " + vs.tasks.Video.version);
            //tn.Nodes.Add("Audio model : " + vs.tasks.Audio.version);
            //ModelInfoTreeView.Nodes.Add(tn);

            //tn = new TreeNode("Processing Time");
            //tn.Nodes.Add("Total : " + vs.tasks.Overall.stats.DurationString + " " + vs.tasks.Overall.stats.ProcessingRatioString);
            //tn.Nodes.Add("Demux : " + vs.tasks.Demux.stats.DurationString + " " + vs.tasks.Demux.stats.ProcessingRatioString);
            //tn.Nodes.Add("Audio : " + vs.tasks.Audio.stats.DurationString + " " + vs.tasks.Audio.stats.ProcessingRatioString);
            //tn.Nodes.Add("Video : " + vs.tasks.Video.stats.DurationString + " " + vs.tasks.Video.stats.ProcessingRatioString);
            //ModelInfoTreeView.Nodes.Add(tn);

            //ModelInfoTreeView.ExpandAll();
            //#endregion

            #endregion Clip Summary

            #region SPECS treeview tab
            //
            SpecsTreeView.Nodes.Clear();

            string tracklabel;

            try
            {
                tn = new TreeNode("File : " + vs.path.StreamFileName);
                tn.Nodes.Add("File path : " + vs.path.StreamPath);
                tn.Nodes.Add("File Size : " + vs.format.overall.FileSizeinMBString);
                tn.Nodes.Add("File Modified: " + vs.path.StreamDateString);
                SpecsTreeView.Nodes.Add(tn);
            }
            catch
            {

            }


            try
            {
                tracklabel = "";
                if (vs.format.overall.containerID != "")
                    tracklabel = " (ID : " + vs.format.overall.containerID + ")";
                tn = new TreeNode("Container : " + vs.format.overall.ContainerLabel + tracklabel);
                tn.Nodes.Add("Tracks : " + vs.format.overall.nbVideoStreams + " video, " + vs.format.overall.nbAudioStreams + " audio.");
                tn.Nodes.Add("Duration : " + vs.format.overall.detectedTotaldurationString);

                tn.Nodes.Add("Overall bitrate : " + vs.format.OverallBitrateString);
                SpecsTreeView.Nodes.Add(tn);
            }
            catch
            {

            }

            tracklabel = "Video track";
            if (vs.cv.ID != 0)
                tracklabel += " (ID : " + vs.cv.ID + ")";
            tn0 = new TreeNode(tracklabel);
            try
            {
                tn = new TreeNode("Format");
                tn.Nodes.Add("Duration : " + vs.VideoDurationString + " (" + vs.cv.format.durationInFrames + " frames) ");
                tn.Nodes.Add("Resolution : " + vs.cv.format.ResolutionString);
                tn.Nodes.Add("Aspect ratio : " + vs.DisplayAspectRatioFullString);
                tn.Nodes.Add("Framerate : " + vs.cv.format.frameRate + " fps. (VFR : " + vs.cv.format.VFR.ToString() + ")");
                tn.Nodes.Add("Video format : " + VideoModelInfo.Get_SPS_VIDEO_FORMAT(vs.videoModelCheck.videoFormatID));
                if ((vs.cv.format.colorFormat != "") && (vs.cv.format.colorFormat != null))
                    tn.Nodes.Add("Color format : " + vs.cv.format.colorFormat);
                if ((vs.cv.format.colorRes != "") && (vs.cv.format.colorRes != null))
                    tn.Nodes.Add("Color resolution : " + vs.cv.format.colorRes);
                tn0.Nodes.Add(tn);
            }
            catch
            {
            }

            try
            {
                tn = new TreeNode("Encoding");
                tn.Nodes.Add("Codec : " + vs.cv.codecSpecs.name);
                tn.Nodes.Add("Profile : " + vs.VideoCodecProfile);

                tn1 = new TreeNode("Bitrate : " + vs.VideoBitrateReportedToString);
                tn1.Nodes.Add("HRD CBR : " + vs.videoModelCheck.HrdCBR);
                tn1.Nodes.Add("HRD bitrate : " + vs.videoModelCheck.HrdBitrateInMbpsString);
                tn1.Nodes.Add("Payload max. : " + ConvertBitrate.KbpsToMbps_String(vs.videoModelCheck.bitrate.max, 3) + " (measured over 1s)");
                tn1.Nodes.Add("Payload min. : " + ConvertBitrate.KbpsToMbps_String(vs.videoModelCheck.bitrate.min, 3));
                tn1.Nodes.Add("Payload average : " + ConvertBitrate.KbpsToMbps_String(vs.videoModelCheck.bitrate.average, 3));

                tn.Nodes.Add(tn1);
                tn.Nodes.Add("CPB delay : " + vs.videoModelCheck.HrdCPBdelayInMsecString + " (CPB size: " + vs.videoModelCheck.HrdCPBsizeInKbitsString + ")");

                if (vs.videoModelCheck.gopStats.GopSizeStat.max > 0)
                {
                    tn1 = new TreeNode("GOP size max. : " + vs.videoModelCheck.gopStats.GopSizeStat.max);
                    tn1.Nodes.Add("Min. : " + vs.videoModelCheck.gopStats.GopSizeStat.min);
                    tn1.Nodes.Add("Average : " + vs.videoModelCheck.gopStats.GopSizeStat.average);
                    tn.Nodes.Add(tn1);
                }
                else
                    tn.Nodes.Add("GOP size : N/A");

                if (vs.videoModelCheck.gopStats.BframeStat.max > 0)
                {
                    tn1 = new TreeNode("B frames max : " + vs.videoModelCheck.gopStats.BframeStat.max);
                    tn1.Nodes.Add("Min. : " + vs.videoModelCheck.gopStats.BframeStat.min);
                    tn1.Nodes.Add("Average : " + vs.videoModelCheck.gopStats.BframeStat.average);
                    tn.Nodes.Add(tn1);
                }
                else
                    tn.Nodes.Add("B frames : N/A");

                tn.Nodes.Add("Reference frames : " + vs.cv.codecSpecs.referenceFrames);
                tn.Nodes.Add("Cabac : " + vs.videoModelCheck.cabac.ToString());
                if (vs.videoModelCheck.qWeighting != "") tn.Nodes.Add("QWeighting : " + vs.videoModelCheck.qWeighting);
                if (vs.videoModelCheck.picEncoding != "") tn.Nodes.Add("Frame encoding : " + vs.videoModelCheck.picEncoding);
                tn.Nodes.Add("Field order: " + VideoModelInfo.Get_SEIPicStr(vs.videoModelCheck.picStrID));
                tn0.Nodes.Add(tn);
                //
            }

            catch
            {
            }

            SpecsTreeView.Nodes.Add(tn0);
            SpecsTreeView.ExpandAll();
            // Audio track
            if (vs.format.overall.nbAudioStreams > 0)
            {
                tracklabel = "Audio track";
                if (vs.ca.ID > 0)
                    tracklabel += " (ID : " + vs.ca.ID + ")";
                tn0 = new TreeNode(tracklabel);

                try
                {
                    tn = new TreeNode("Format");
                    tn.Nodes.Add("Duration : " + vs.AudioDurationString);
                    tn.Nodes.Add("Number of channels : " + vs.AudioChannels);
                    if ((vs.ca.format.channelPosition != "") && (vs.ca.format.channelPosition != null))
                        tn.Nodes.Add("Channels position : " + vs.ca.format.channelPosition);
                    if (vs.AudioSampleRate > 0)
                        tn.Nodes.Add("Sample rate : " + vs.AudioSampleRateString);
                    if (vs.ca.format.resolution > 0)
                        tn.Nodes.Add("Audio resolution : " + vs.ca.format.resolution + " bits");
                    tn0.Nodes.Add(tn);
                }
                catch
                {
                }

                try
                {
                    tn = new TreeNode("Encoding");
                    tn.Nodes.Add("Codec : " + vs.ca.codecSpecs.AudioCodecString);
                    if (vs.ca.codecSpecs.AudioFormatString != null) tn.Nodes.Add("Format : " + vs.ca.codecSpecs.AudioFormatString);
                    if (string.IsNullOrEmpty(vs.ca.codecSpecs.formatProfile)) tn.Nodes.Add("Profile : " + vs.ca.codecSpecs.formatProfile);
                    //if (!(vs.ca.codecSpecs.formatProfile == null) || (vs.ca.codecSpecs.formatProfile =="" )) tn.Nodes.Add("Profile : " + vs.ca.codecSpecs.formatProfile);
                    if (!string.IsNullOrEmpty(vs.ca.codecSpecs.formatVersion)) tn.Nodes.Add("Version : " + vs.ca.codecSpecs.formatVersion);
                    //if (!(vs.ca.codecSpecs.formatVersion == null) || (vs.ca.codecSpecs.formatVersion=="")) tn.Nodes.Add("Version : " + vs.ca.codecSpecs.formatVersion);
                    tn.Nodes.Add("Bitrate : " + vs.AudioBitrateReportedToString);
                    //if (!String.IsNullOrEmpty(vs.ca.codecSpecs.bitrateMode)) tn.Nodes.Add("Bitrate mode : " + vs.ca.codecSpecs.bitrateMode); 
                    if (vs.ca.codecSpecs.bitrateMode != null) tn.Nodes.Add("Bitrate mode : " + vs.ca.codecSpecs.bitrateMode);
                    if (!String.IsNullOrEmpty(vs.ca.codecSpecs.muxingMode)) tn.Nodes.Add("Muxing mode : " + vs.ca.codecSpecs.muxingMode);
                    //if ((vs.ca.codecSpecs.muxingMode != null) && (vs.ca.codecSpecs.muxingMode!="") ) tn.Nodes.Add("Muxing mode : " + vs.ca.codecSpecs.muxingMode);
                    if (vs.ca.delay != 0) tn.Nodes.Add("Delay : " + Math.Round(vs.ca.delay, 3) + "s");
                    if (vs.ca.videoDelay != 0) tn.Nodes.Add("Video delay : " + Math.Round(vs.ca.videoDelay, 3) + "s");

                    tn0.Nodes.Add(tn);
                }
                catch (Exception e)
                {
                    Global.debuglog("Problem while displaying audio codec specs\n" + e);
                }

                try
                {
                    tn = new TreeNode("Measures (Activity / Saturation / Audio level)");
                    for (int ind = 0; ind < vs.audioCheck.channels; ind++)
                    {
                        tn.Nodes.Add("Channel " + ind + " : " + vs.audioCheck.activity[ind] + " / " + vs.audioCheck.saturation[ind] + " / " + vs.audioCheck.audioLevel[ind]);
                    }
                    tn0.Nodes.Add(tn);

                }
                catch
                {

                }

                SpecsTreeView.Nodes.Add(tn0);
            }

            //  SpecsTreeView.ExpandAll();//Let the user expand audio -Melek
            #endregion SPECS treeview tab


        }


        private void PreviousAlertButton_Click(object sender, System.EventArgs e)
        {
            double BadSceneTime = 0, CurrentTime;
            int i;
            int done = 0;
            List<AlertScene> tempa = new List<AlertScene>();

            if (ShowAggBSBox.Checked == true)
            {
                i = vs.sum.AggAlertScnA.Count - 1;
                tempa = vs.sum.AggAlertScnA;
            }
            else
            {
                i = vs.sum.AlertScnA.Count - 1;
                tempa = vs.sum.AlertScnA;
            }

            while ((i >= 0) && (done == 0))
            {
                try
                {
                    BadSceneTime = (float)((((AlertScene)tempa[i]).Start) / vs.cv.format.frameRate);	// convert the KeyScene [i] frame number into seconds 
                }
                catch
                {
                }

                this.mediaPosition.get_CurrentPosition(out CurrentTime);
                if (CurrentTime > BadSceneTime)
                {
                    // if current position is more than 1 second after the closest previous badscene start
                    // then go back to that scene start
                    if (CurrentTime - BadSceneTime > 1)
                    {
                        this.BadSceneListView.Items[i].Selected = true;
                    }
                    else
                        // if there is less than 1 second between current position and closest previous bad scene
                        // then go the previous start frame
                        // unless there are no previous bad scene
                        if (i > 0)
                        {
                            this.BadSceneListView.Items[i - 1].Selected = true;
                        }
                    done = 1;
                }
                i--;
            }//while
        }

        private void NextAlertButton_Click(object sender, System.EventArgs e)
        {
            double BadSceneTime, CurrentTime;
            int i = 0;
            int done = 0;
            //ArrayList tempa = new ArrayList();
            List<AlertScene> tempa = new List<AlertScene>();

            if (ShowAggBSBox.Checked == true)
            {
                i = vs.sum.AggAlertScnA.Count - 1;
                tempa = vs.sum.AggAlertScnA;
            }
            else
            {
                i = vs.sum.AlertScnA.Count - 1;
                tempa = vs.sum.AlertScnA;
            }

            while ((i < vs.sum.BSA.Count) && (done == 0))
            {
                try
                {
                    BadSceneTime = (float)((((AlertScene)tempa[i]).Start) / vs.cv.format.frameRate);	// convert the KeyScene [i] frame number into seconds 
                }
                catch
                {
                }

                // convert the BadScene [i] frame number into seconds to seek into the video
                BadSceneTime = (float)((((AlertScene)vs.sum.BSA[i]).Start) / vs.cv.format.frameRate);

                this.mediaPosition.get_CurrentPosition(out CurrentTime);
                if (CurrentTime < BadSceneTime)
                {
                    this.BadSceneListView.Items[i].Selected = true;
                    done = 1;
                }
                i++;
            }
        }

        // Handler to update the Position label and move the trackbar 
        // tick postion, we set isTimer to true, then changing the
        // trackbar Value property will generate a call to the ValueChanged
        // handler but we don't want to seek the filter, so ValueChanged
        // check if isTimer is true and ignore timer update to seek the filter
        //
        private void VideoTimer_Tick(object sender, System.EventArgs e)
        {
            int minutes;
            int seconds;
            double cp = 0; //current position
            double td = 0; // total duration
            if (this.mediaPosition != null) //Melek
            {
                try//Melek try-catch block added 
                {
                    this.mediaPosition.get_CurrentPosition(out cp);
                    this.mediaPosition.get_Duration(out td);

#if DEMO
                td=vs.GetDurationInSeconds();
#elif DEMO5MIN
            td = vs.GetDurationInSeconds();
#endif

                    if (cp >= td)
                    {
                        if (LoopBox.Checked == false)
                            stopbutton_Click(null, EventArgs.Empty);
                        else
                            seekbutton_Click(null, EventArgs.Empty);
                    }

                    if (this.mediaSeeking != null)
                    {
                        isTimer = true;
                        try
                        {
                            VideoProgressionBar.Value = (int)cp * 100;
                        }
                        catch
                        {
                            VideoProgressionBar.Value = 0;
                        }

                        isTimer = false;
                        minutes = (int)cp / 60;
                        seconds = (int)cp % 60;
                        VideoTimeBox.Text = Global.secondsToDurationString((uint)cp) + " / " + Global.frameToDurationString((int)vs.videoModelCheck.processedDurationInFrames, vs.cv.format.frameRate, false);

                        //iMosUpdate();

                        // transmit the current position to the graph control to draw the cursor over the graph
                        // handle the cases where we are running the demo (stop the cursor to get out of the graph
                        // should be handled in the graph but the compilation problems previously encountered (graph control not recognised and lost hours of work several times)
                        // so if it can be handled quickly here, that will do for a first fix.
#if DEMO
                // The demo version BTqaAVC-demo and BTqaMPEG2 process 1 min of video (60s)
                if (cp>60)
                    cp=60;
#elif DEMO5MIN
                // The 5min demo version BTqaAVC-demo5min processes 5 minutes of video (300s)
                // The mpeg2 demo version still runs 1 minute of video
                if (vs.cv.codecSpecs.codec.name == VideoCodec.AVC.name)
                {
                    if (cp > 300)
                        cp = 300;
                }
                else if (vs.cv.codecSpecs.codec.name == VideoCodec.MPEG2.name)
                {
                    if (cp > 300)
                        cp = 300;
                }
#endif

                        if (cp > (double)vs.videoModelCheck.processedDurationInFrames / vs.cv.format.frameRate)
                            cp = (double)vs.videoModelCheck.processedDurationInFrames / vs.cv.format.frameRate;

                        MOSchart.SetTime(cp);
                    }
                }
                catch (Exception ex) //Melek
                {
                    // MessageBox.Show(ex.Message);
                    VideoProgressionBar.Value = 0;
                }
            }
            else //MElek 
            {
                this.VideoTimer.Tick -= new System.EventHandler(this.VideoTimer_Tick);
                //Global.log("Video Progression trackbar position could not be updated for the "+ vs.path.streamFileName + " on Review Tab" ); 

            }
        }

        private object lockObject
        {
            get
            {
                return this.GetType();
            }
        }

        /// <summary>
        /// edit list every 2 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshListTimer_Tick(object sender, System.EventArgs e)
        {
            JobListView.Sort("Priority");

            if (summaryListIsDirty)
            {
                lock (GUI.padlock)
                {
                    try
                    {

                        var resultSelectedObjs = ResultListView.SelectedObjects;

                        ResultListView.DataSource = null;
                        FailedResultListView.DataSource = null;

                        ResultListView.DataSource = SummaryList;
                        FailedResultListView.DataSource = SummaryList;

                        ResultListView.SelectedObjects = resultSelectedObjs;

                        //ResultListView.BeginUpdate();
                        //FailedResultListView.BeginUpdate();

                        //foreach (var item in VStreamActionList)
                        //{
                        //    //ResultListView.AddObject()

                        //    if (item.IsAdd) SummaryBindingList.Add(item.VStream);
                        //    else SummaryList.Remove(item.VStream);
                        //    //if (item.IsAdd)
                        //    //{
                        //    //    ResultListView.AddObject(item.VStream);
                        //    //    FailedResultListView.AddObject(item.VStream);
                        //    //}
                        //    //else
                        //    //{
                        //    //    ResultListView.RemoveObject(item.VStream);
                        //    //    FailedResultListView.RemoveObject(item.VStream);
                        //    //}
                        //}

                        //VStreamActionList.Clear();

                        //ResultListView.EndUpdate();
                        //FailedResultListView.EndUpdate();
                    }
                    catch { }
                }

                summaryListIsDirty = false;
            }
        }

        /// <summary>
        /// Video playback Progress bar is moved by user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_Scroll(object sender, System.EventArgs e)
        {
            this.mediaPosition.put_CurrentPosition(VideoProgressionBar.Value / 100.0);
        }

        /// <summary>
        /// Seek 1s earlier in playback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void minus1secButton_Click(object sender, System.EventArgs e)
        {
            double cp;
            this.mediaPosition.get_CurrentPosition(out cp);
            //try
            //{
            if (cp > 3.0)
                this.mediaPosition.put_CurrentPosition(cp - 3.0);
            else
                this.mediaPosition.put_CurrentPosition(0);
            //}
        }

        /// <summary>
        /// seek 1s later in playback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void plus1sButton_Click(object sender, System.EventArgs e)
        {
            double cp;
            double td;

            this.mediaPosition.get_CurrentPosition(out cp);
            this.mediaPosition.get_Duration(out td);

            if (cp + 3.0 < td)
                this.mediaPosition.put_CurrentPosition(cp + 3.0);
        }


        /// <summary>
        /// delete result files
        /// </summary>
        /// <param name="lvs"></param>
        private void deleteResultsFiles(VStream lvs)
        {
            if (lvs != null)
            {
                try
                {
                    if (File.Exists(lvs.path.QVRPath)) File.Delete(lvs.path.QVRPath);
                    if (File.Exists(lvs.path.QPLPath)) File.Delete(lvs.path.QPLPath);
                    if (File.Exists(lvs.path.QARPath))
                        File.Delete(lvs.path.QARPath);
                    if (File.Exists(lvs.path.ArchivePath))
                        File.Delete(lvs.path.ArchivePath);
                }
                catch (SystemException ex)
                {
                    Global.log("Error deleting results files for " + lvs.StreamFileName + ".ts.\n" + ex); //Melek - path.name => streamfilename
                }
            }
        }


        /// <summary>
        /// Open the detailed results of a processed stream in detailed view
        /// </summary>
        /// <param name="i">Index of the clip in the result list</param>
        private void openDetails(int i)
        {
            openDetails(SummaryList[i]);
        }

        /// <summary>
        /// open the detailed results of a processed stream in detailed view
        /// </summary>
        /// <param name="lvs">clip</param>
        private void openDetails(VStream lvs)
        {
            CloseInterfaces();

            // forces video panel to cover internals overlay option
            // VideoPanel.Width = VideoPlayerPanel.Width;

            vs = new VStream();

            // load 
            vs = lvs;

            // reset arraylist used to store curves in detailed view
            vs.rawdata.iMosA = new ArrayList();
            vs.rawdata.sMosA = new ArrayList();
            vs.rawdata.AvYA = new ArrayList();
            vs.rawdata.TxturA = new ArrayList();
            vs.sum.BSA = new List<AlertScene>();
            vs.sum.AggBSA = new List<AlertScene>();
            vs.sum.BWScnA = new List<AlertScene>();
            vs.sum.FzScnA = new List<AlertScene>();
            vs.sum.FieldScnA = new List<AlertScene>();
            vs.sum.TroughsA = new List<AlertScene>();
            vs.sum.FieldScnA = new List<AlertScene>();
            vs.sum.VideoWarningA = new List<GUIwarning>();
            vs.sum.DecoderWarningA = new List<DecoderWarning>();
            vs.sum.RFframes = 0;
            vs.sum.AggRFframes = 0;

            MainTabs.SelectedIndex = 2; // switch to the (detailed) 'review' view when clicking on a stream in the monitoring view
            if (vs.sum.qualityProfile.aggregatedBadSceneDetector.Enabled == true)
                ShowAggBSBox.Enabled = true;
            else
                ShowAggBSBox.Enabled = false;

            vs = qatool.Sum.ReadRes(vs, 1);	//1: copy the mos array to the global mos array for display in the detailed view;

            // decoder warning array is populated from readres so if it is read from summary file, it needs to be reset before
            vs.sum.DecoderWarningA.Clear();


            vs.ReadSum();

            if (vs.sum.error != "")
            {
                currentfilename.Text = "  Problem opening " + vs.StreamFileName; //Melek - path.name => streamfilename
            }
            else
            {
                // currentfilename.Text = "  " + vs.path.Name;	// fill label above video window

                if (vs.sum.qualityProfile.aggregatedBadSceneDetector.Enabled == true)
                    redflaggedPerCent.Text = " " + vs.sum.AggRFpercent + " %";
                else
                    redflaggedPerCent.Text = " " + vs.sum.RFpercent + " %";

                //AvMosLabel.Text = Math.Round(vs.sum.MOSaverage, 2) + " (" + Math.Round(vs.sum.MOSstandardDeviation, 2) + ")";
                string displaypmosvalue = Math.Round(vs.sum.MOSaverage, 2).ToString();

                currentfilename.Text = "  " + vs.StreamFileName + "  Avg. MOS:" + displaypmosvalue;	// fill label above video window  //Melek - path.name => streamfilename



                // set the colour elements according to the final decision
                currentfilename.BackColor = vs.Final_Decision.GetBackColor();
                redflaggedPerCent.BackColor = vs.Final_Decision.GetBackColor();
                AvMosLabel.BackColor = vs.Final_Decision.GetBackColor();


                if (vs.sum.UserDecision.Made == Decision.NOTMADE)
                {
                    currentfilename.Text += " [ " + vs.sum.ModelDecision.String + " : " + vs.sum.qualityProfile.name + "]";
                }
                else
                {
                    currentfilename.Text += " [ " + vs.sum.UserDecision.String + " by user: " + vs.sum.UserDecision.User + " ] (model decision: " + vs.sum.ModelDecision.String + ")";
                }

                ShowAllScenesBox.Enabled = true;

                // adding 
                ShowAllScenesBox.Checked = true;
                ShowAllScenesBox_CheckedChanged("", EventArgs.Empty);

                VideoInfo();

                TraceMOSchartDist();
                TraceMOSchart();// will use mos array in the global vs stream

                // Thread.Sleep(1000); //Melek - wait for a sec 

                //VideoTimeBox.Text = "";
                VideoTimeBox.Text = "00:00:00"; //Melek 
                UpdateMainTitle();

                if (vs.path.StreamIsAvailable)
                {
                    EnablePlayback(true);
                }
                else
                {
                    MessageBox.Show(vs.path.streamFileName + " is not available under " + vs.path.inputDir);
                }
            }
        }

        /// <summary>
        /// Dialog box to select a folder to monitor
        /// </summary>
        private void AddStreamDir()
        {
           
            try
            {
                string folder;
                FolderBrowserDialog fnd = new FolderBrowserDialog();
                 // fnd.ShowNewFolderButton = false; //Melek              
                //fnd.RootFolder
                fnd.Description = "Select a folder to monitor";
                if (fnd.ShowDialog() == DialogResult.OK)
                {
                    folder = fnd.SelectedPath;
                  
                }
                // else if ok was not clicked 
                else
                {
                    return ;
                }
                
                
                //#region NewDialog -Melek
                //OpenFileDialog fdlg = new OpenFileDialog();
                //fdlg.Title = "Select a folder to monitor";
                //fdlg.InitialDirectory = @"c:\";
                //fdlg.Filter = "All files (*.*)|*.*|All files (*.*)|*.*";
                //fdlg.FilterIndex = 2;
                //fdlg.RestoreDirectory = true;
                //if (fdlg.ShowDialog() == DialogResult.OK)
                //{
                //   // textBox1.Text = fdlg.FileName;
                //}
                //#endregion new dialog


                // The stream monitor thread needs to know the new streamdir
                try
                {
                    SourceFolder sfolder = new SourceFolder(folder, "Monitored Folder", false, 128, "default", "3-Medium"); //Hot and Priorityname added - Melek
                    //SourceFolder sfolder = new SourceFolder(folder, "Monitored Folder", 128, "default");
                    qatool.AddNewSourceFolder(sfolder);
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting new monitored folder\n" + ex);
                    
                }
              
            }
            catch (Exception ex)
            {
                MessageBox.Show("Choose monitored folder exception\n" + ex);
               
            }
        }

        /// <summary>
        /// Remove sream dir
        /// </summary>
        public void RemoveStreamDir()
        {
            // only remove 1 folder at a time
           // if (StreamFoldersBox.SelectedRows.Count == 1)
                if ((string)StreamFoldersBox.Rows[StreamFoldersBox.CurrentRow.Index].Cells[1].Value!=null)//Melek
            {
                qatool.RemoveSourceFolder((string)StreamFoldersBox.Rows[StreamFoldersBox.CurrentRow.Index].Cells[1].Value);
                DeleteResFiles(); //demostick
            }
            else
            {
                MessageBox.Show("For security reasons, only one folder can be removed at a time.");
            }
        }

        public void DeleteResFiles() //demostick
        {
            try
            {
                string[] files = Directory.GetFiles(@".\results");
                foreach (string file in files)
                    File.Delete(file);
            }
            catch (Exception)
            {
                //do nothing!
            }
        }

        /// <summary>
        /// Choose a new result folder
        /// </summary>
        /// <returns></returns>
        private bool ChooseResDir()
        {
            try
            {
                QAFolder newfolder = new QAFolder();
                FolderBrowserDialog fnd = new FolderBrowserDialog();
                fnd.Description = "Select the results folder (need write access)";
                if (fnd.ShowDialog() == DialogResult.OK)
                {
                    if (fnd.SelectedPath.EndsWith("\\"))
                    {  //newfolder.path = fnd.SelectedPath;
                        newfolder = new QAFolder(fnd.SelectedPath, "Local results folder", FolderType.ResultFolder);
                        //return true;//Melek
                    }
                    else
                    {   //newfolder.path = fnd.SelectedPath + "\\";
                        newfolder = new QAFolder(fnd.SelectedPath + "\\", "Local results folder", FolderType.ResultFolder);
                       // return true;//Melek
                    }
                }
                else
                    return false; // exit if cancelled by user
              
              //  the stream monitor thread needs to know the new result dir
                try
                {
                    if (qatool.SetNewResDir(newfolder))
                    {
                        MessageBox.Show("Results folder changed to " + newfolder.path); //Melek
                        return true;
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    Global.log("Error while changing to a new result folder.\n" + ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occurred when choosing a result folder.\n" + ex);
                return false;
            }
        }


        private bool ChooseRepDir() //Melek
        {

            try
            {
                FolderBrowserDialog fnd = new FolderBrowserDialog();
                fnd.Description = "Select the reports folder (need write access)";
                if (fnd.ShowDialog() == DialogResult.OK)
                {
                    if (!fnd.SelectedPath.EndsWith("\\"))
                    {
                        reportsfoldertemp = fnd.SelectedPath + "\\";
                    }
                    else
                    {
                        reportsfoldertemp = fnd.SelectedPath;
                    }
                    if (reportsfoldertemp != null)
                    {
                        RepDirTextBox.Text = reportsfoldertemp;
                    }
                    qatool.SetNewRepDir(reportsfoldertemp);
                    MessageBox.Show("Reports folder changed to " + reportsfoldertemp);
                    reportsfolderchangedtemp = false;
                    return true;
                }

                else
                { return false; }
            }
            catch (Exception ex) { return false;}
        }

        /// <summary>
        /// Choose a new XML job folder
        /// </summary>
        /// <returns></returns>
        //private bool ChooseXMLjobFolder() //Melek -disabled
        //{
        //    try
        //    {
        //        string newfolder;
        //        FolderBrowserDialog fnd = new FolderBrowserDialog();
        //        fnd.Description = "Select the XML job folder (need read access)";
        //        if (fnd.ShowDialog() == DialogResult.OK)
        //        {
        //            newfolder = fnd.SelectedPath;
        //        }
        //        else
        //            return false; // exit if cancelled by user

        //        // the stream monitor thread needs to know the new XML dir
        //        try
        //        {
        //            qatool.SetNewXMLdir(newfolder);
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            Global.log("Error while changing to a new XML folder.\n" + ex);
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Exception when choosing an XML job folder.\n" + ex);
        //        return false;
        //    }
        //}



        /// <summary>
        /// Set the update xml folder
        /// </summary>
        /// <returns></returns>
        //private bool ChooseXMLupdateDir() //Melek- disabled
        //{
        //    try
        //    {
        //        string newfolder;
        //        FolderBrowserDialog fnd = new FolderBrowserDialog();
        //        fnd.Description = "Select the update xml folder (need write access)";
        //        if (fnd.ShowDialog() == DialogResult.OK)
        //        {
        //            newfolder = fnd.SelectedPath;
        //        }
        //        else
        //            return false; // exit if cancelled by user

        //        //the stream monitor thread needs to know the new XML dir
        //        try
        //        {
        //            qatool.SetNewXMLupdateDir(newfolder);
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            Global.log("Error while changing to a new XML folder.\n" + ex);
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Exception when choosing an XML folder.\n" + ex);
        //        return false;
        //    }
        //}

        /// <summary>
        /// OBSOLETE
        /// WAS used with .net standard listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == _lvwItemComparer.SortColumn)
            {
                // Reverse the current sort direction for this column.        
                if (_lvwItemComparer.Order == SortOrder.Ascending)
                    _lvwItemComparer.Order = SortOrder.Descending;
                else
                    _lvwItemComparer.Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                _lvwItemComparer.SortColumn = e.Column;
                _lvwItemComparer.Order = SortOrder.Ascending;
            }

            // finally sort
            this.ResultListView.Sort();
        }

        /// <summary>
        /// Bad scene listview column click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BadSceneColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.    
            if (e.Column == _lvwItemComparerBadScenes.SortColumn)
            {
                // Reverse the current sort direction for this column.        
                if (_lvwItemComparerBadScenes.Order == SortOrder.Ascending)
                    _lvwItemComparerBadScenes.Order = SortOrder.Descending;
                else
                    _lvwItemComparerBadScenes.Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.        
                _lvwItemComparerBadScenes.SortColumn = e.Column;
                _lvwItemComparerBadScenes.Order = SortOrder.Ascending;
            }

            // finally sort
            this.BadSceneListView.Sort();
        }

        private void BadSceneListView_Click(object sender, System.EventArgs e)
        {
            AlertScene AlScn = new AlertScene();
            if (this.mediaSeeking != null)
            {
                if (BadSceneListView.SelectedItems.Count != 0)
                {
                    AlScn = (AlertScene)BadSceneListView.SelectedItems[0].Tag;
                    this.mediaPosition.put_CurrentPosition((double)((double)AlScn.Start / vs.cv.format.frameRate) - .2);
                }
            }
        }

        private void SetResDirButton_Click(object sender, System.EventArgs e)
        {
            ChooseResDir();
        }

        //private void XMLdirSetButton_Click(object sender, EventArgs e)//Melek -disabled
        //{
        //    ChooseXMLjobFolder();
        //}


        /// <summary>
        /// APPLY change on a quality profile was clicked
        /// check the profile is new
        /// and update results of streams that have this profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewVideoQSettingsButton_Click(object sender, System.EventArgs e)
        {
            if (ProfilesListBox2.SelectedItem == null)
            {
                MessageBox.Show("No profile is selected");
                //CancelQualityProfileButton2.Enabled = false; //MElek
                return;
            }

            CQualityProfile prof;
            prof = new CQualityProfile();
            prof.name = (ProfilesListBox2.SelectedItem.ToString());
            Global.log("Applying change to quality profile: " + prof.name);

            prof.badSceneDetector.MosThreshold = Convert.ToDouble(QosAlert2.Value);
            prof.badSceneDetector.WindowLength = (int)BadSceneMinimumDurationBox2.Value;

            //prof.aggregatedBadSceneDetector.Enabled = (bool)AggBadSceneBox2.Checked;
            //prof.aggregatedBadSceneDetector.Tolerance = Convert.ToDouble(AggToleranceBox2.Value);

            prof.troughDetector.Enabled = QualityTroughDetection2.Checked;
            prof.troughDetector.MosThreshold = Convert.ToDouble(TroughMosThresholdBox2.Value);
            prof.troughDetector.DurationThreshold = Convert.ToDouble(TroughMinDurationBox2.Value);
            prof.troughDetector.DecisionToTake = TroughDecisionBox2.Text;

            //prof.fieldOrderDetector.Enabled = FieldOrderBox2.Checked;
            //prof.fieldOrderDetector.DecisionToTake = FieldDecisionBox2.Text;

            prof.blankSceneDetector.Enabled = BlackWhiteSceneDetectionBox2.Checked;
            prof.blankSceneDetector.DurationT = Convert.ToDouble(BlackSceneDurationTBox2.Value);
            prof.blankSceneDetector.BlackIntensityThreshold = (int)BlackSceneIntTBox2.Value;
            prof.blankSceneDetector.WhiteIntensityThreshold = (int)WhiteSceneIntTBox2.Value;
            prof.blankSceneDetector.UniformityT = Convert.ToDouble(TroughMinDurationBox2.Value);
            prof.blankSceneDetector.DecisionToTake = BlackSceneDecisionBox2.Text;

            //prof.frozenSceneDetector.Enabled = FrozenDetectionBox2.Checked;
            // prof.frozenSceneDetector.DurationThreshold = Convert.ToDouble(FrozenDurationTBox2.Value);
            //prof.frozenSceneDetector.DecisionToTake = FrozenDecisionBox2.Text;

            // updateRejectConditions(ref prof);
            // prof.badClipDetector.BorderlineThreshold2 = (int)Convert.ToDouble(BorderlineThreshold2.Value);
            prof.badClipDetector.FailThreshold = (int)Convert.ToDouble(RejectedThreshold2.Value);

            //prof.audioDetector.Enabled = (bool)AudioCheckDetectionBox2.Checked;
            //prof.audioDetector.ClippingThreshold = (double)AudioClippingThreshold2.Value;
            //prof.audioDetector.SaturationThreshold = (double)AudioSaturationThreshold2.Value;
            //prof.audioDetector.InactivityThreshold = (double)AudioInactivityThreshold2.Value;
            //prof.audioDetector.IgnoreUnsupportedFormat = IgnoreUnsupportedStreamsBox2.Checked;

            int selected = -1;//Melek
              if (ProfilesListBox2.SelectedIndex >= 0)//Melek
                    selected = ProfilesListBox2.SelectedIndex;
            
            if (qaClass.profManager.profileChanged(prof)) // check and update profile if updated
            {                        
                qatool.ForceSummaryUpdate(prof.name, false);
                updategui();                
                MessageBox.Show("Profile was updated");
               // ProfilesListBox2.SelectedIndex = selected;//Melek

            }
            else
            {              
                MessageBox.Show("No Change was made to the profile");
                
                //ProfilesListBox2.SelectedIndex = selected;   //Melek

            }
            ProfilesListBox2.SelectedIndex = selected;//Melek
        }

        //private void updateRejectConditions(ref CQualityProfile GUIprof)
        //{
        //    if (BorderlineThreshold2.Value > RejectedThreshold2.Value)
        //    {
        //        MessageBox.Show("The Rejected Threshold must be greater than the Borderline threshold");
        //        if (RejectedThreshold2.Value - 1 > 0)
        //            BorderlineThreshold2.Value = RejectedThreshold2.Value - 1;
        //        else
        //            BorderlineThreshold2.Value = RejectedThreshold2.Value;
        //    }
        //    GUIprof.badClipDetector.BorderlineThreshold2 = Convert.ToInt16(BorderlineThreshold2.Value);
        //    GUIprof.badClipDetector.FailThreshold = Convert.ToInt16(RejectedThreshold2.Value);
        //}

        // Grey in/out buttons enabling/disabling playback
        private void EnablePlayback(bool enable)
        {

            playbutton.Enabled = enable;
            if (enable == false) { stopbutton.Enabled = false; MuteButton.Enabled = false; VideoPanelLabel.Enabled = false; } //else { MuteButton.Enabled = true; }//Melek
            //playbutton.Text = ">";
            this.playbutton.Image = global::QA.Properties.Resources.bt_play_12;
            this.FrameStepButton.Enabled = false;
            MaximizeWindowCheckBox.Enabled = false;
            SeekStartButton.Enabled = false;
            PreviousAlertButton.Enabled = false;
            NextAlertButton.Enabled = false;
            VideoProgressionBar.Enabled = false;
            VideoTimer.Enabled = false;
            minus1secButton.Enabled = false;
            plus1sButton.Enabled = false;
            VideoProgressionBar.Minimum = 0;
            VideoProgressionBar.Maximum = 0;
            VideoPanelLabel.Enabled = true;
        }

        // Enable / disable stream playback elements if ne
        private void PlaybackState(bool enable)
        {
            try
            {
                //if (this.vs != null && this.vs.Final_Decision.String == "Not made") //Not necessary any more - Melek
                //{
                //    MessageBox.Show("Clip Decision Not Made - This clip format is not compatible with v.Cortex");

                //}
                //else
                //{
                // disable playback;
                if (!enable)
                {
                    playbutton.Enabled = false;
                    FrameStepButton.Enabled = false;
                    MaximizeWindowCheckBox.Enabled = false;
                    //playbutton.Text         = ">";
                    this.playbutton.Image = global::QA.Properties.Resources.bt_play_12;
                    stopbutton.Enabled = false;
                    SeekStartButton.Enabled = false;
                    PreviousAlertButton.Enabled = false;
                    NextAlertButton.Enabled = false;
                    VideoProgressionBar.Enabled = false;
                    VideoTimer.Enabled = false;
                    minus1secButton.Enabled = false;
                    plus1sButton.Enabled = false;
                    VideoProgressionBar.Minimum = 0;
                    VideoProgressionBar.Maximum = 0;
                    MOSchart.EnableTimer(false);    // disable refresh of cursor in mos chart
                    MuteButton.Enabled = false;//Melek
                    stopbutton.Enabled = false;//Melek
                    VideoPanelLabel.Enabled = false;//Melek
                    VideoTimeBox.Enabled = false;//Melek
                    // GenerateReportButtonReviewView.Enabled = false;
                    // ClearButtonReviewMenu.Enabled = false;
                    //  OverlayButton.Enabled = false;
                }
                // playback enabled
                else
                {
                    playbutton.Enabled = true;
                    stopbutton.Enabled = true;//Melek
                    VideoTimeBox.Enabled = true;//Melek
                    MuteButton.Enabled = true;//Melek
                    FrameStepButton.Enabled = true;
                    MuteButton.Enabled = true;
                    MaximizeWindowCheckBox.Enabled = true;
                    //playbutton.Text         = ">";
                    this.playbutton.Image = global::QA.Properties.Resources.bt_play_12;
                    stopbutton.Enabled = true;
                    SeekStartButton.Enabled = true;
                    PreviousAlertButton.Enabled = false;    // temporary disabled, needs fixing
                    NextAlertButton.Enabled = false;
                    VideoProgressionBar.Enabled = true;
                    VideoTimer.Interval = 10;
                    VideoTimer.Enabled = true;
                    minus1secButton.Enabled = true;
                    plus1sButton.Enabled = true;
                    OverlayButton.Enabled = true;
                    MOSchart.EnableTimer(true);
                    VideoPanelLabel.Enabled = true; //Melek

                    if (EnableOverlay())         // Check overlay is available for current stream
                    {
                        OverlayButton.Enabled = true;
                        //OverlayBar.Enabled = true;
                    }
                    else
                    {
                        OverlayButton.Enabled = false;
                        //OverlayBar.Enabled = false;
                    }
                }
            }
            //}
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // get the previous stream in the resultlist after clicking on previous stream in detailed view
        private void prevStreamButton_Click(object sender, System.EventArgs e)
        {
            int i = 0;
            bool changed = false;

            //TODO: Is this quick? if not then we shouldn't lock the whole thing
            lock (padlock)
            {
                if (SummaryList != null)
                {
                    if (vs == null)
                    {
                        i = SummaryList.Count - 1;
                        changed = true;
                    }
                    else
                    {
                        foreach (VStream lvs in SummaryList)
                        {
                            if (lvs.tasks.ID == vs.tasks.ID)
                            {
                                if (i > 0)
                                {
                                    i--;
                                    changed = true;
                                }
                                break;
                            }
                            changed = false;
                            i++;
                        }//foreach;
                    }
                    if (changed == true)
                    {
                        StopClip();
                        openDetails(i);
                    }
                }
            }
        }

        private void nextStreamButton_Click(object sender, System.EventArgs e)
        {
            int i = 0;
            bool changed;
            changed = false;

            //TODO: Is this quick? if not then we shouldn't lock the whole thing
            lock (padlock)
            {
                if (SummaryList != null)
                {
                    if (vs == null)
                    {
                        i = 0;
                        changed = true;
                    }
                    else
                    {
                        foreach (VStream lvs in SummaryList)
                        {
                            if ((lvs.path.Name == vs.path.Name) && (lvs != null))
                            {
                                if (i < SummaryList.Count - 1)
                                {
                                    i++;
                                    changed = true;
                                }
                                break;

                            }
                            changed = false;
                            i++;
                        }//foreach;
                    }
                    if (changed == true)
                    {
                        StopClip();
                        openDetails(i);
                    }
                }
            }
        }

        /// <summary>
        /// local copy of streams for later viewing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void localcopybox_CheckedChanged(object sender, EventArgs e)
        {
            if (LocalCopyCheckedBox.Checked == true)
            {
                LocalCopyFolderButton.Enabled = true;
                LocalCopyFolderNameBox.Enabled = true;
            }
            else
            {
                LocalCopyFolderButton.Enabled = false;
                LocalCopyFolderNameBox.Enabled = false;
            }
        }

        private void BlackWhiteSceneDetectionBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (BlackWhiteSceneDetectionBox2.Checked == true)
            //{

            //    BlackSceneDurationTBox2.Enabled = true;
            //    BlackSceneIntTBox2.Enabled = true;
            //    WhiteSceneIntTBox2.Enabled = true;
            //    BlackSceneUniTBox2.Enabled = false;
            //    BlackSceneDecisionBox2.Enabled = true;
                BlackSceneDecisionBox2.Text = "Fail";
            //}
            //else
            //{

            //    BlackSceneDurationTBox2.Enabled = false;
            //    BlackSceneIntTBox2.Enabled = false;
            //    WhiteSceneIntTBox2.Enabled = false;
            //    BlackSceneUniTBox2.Enabled = false;
            //    BlackSceneDecisionBox2.Enabled = false;
            //}
            BlackSceneDurationTBox2.Enabled = BlackWhiteSceneDetectionBox2.Checked;//Melek
            BlackSceneIntTBox2.Enabled = BlackWhiteSceneDetectionBox2.Checked;//Melek
            WhiteSceneIntTBox2.Enabled = BlackWhiteSceneDetectionBox2.Checked;//Melek
            BlackSceneUniTBox2.Enabled = BlackWhiteSceneDetectionBox2.Checked;//Melek
            BlackSceneDecisionBox2.Enabled = BlackWhiteSceneDetectionBox2.Checked;//Melek
            BlackSceneDecisionBox2.Enabled = BlackWhiteSceneDetectionBox2.Checked;//MElek
        }

        //private void FrozenDetectionBox2_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (FrozenDetectionBox2.Checked == true)
        //    {
        //        FrozenDurationTBox2.Enabled = true;
        //        FrozenDecisionBox2.Enabled = true;
        //    }
        //    else
        //    {
        //        FrozenDurationTBox2.Enabled = false;
        //        FrozenDecisionBox2.Enabled = false;
        //    }
        //}


        //private void AggBadSceneBox2_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (AggBadSceneBox2.Checked == true)
        //        AggToleranceBox2.Enabled = true;
        //    else
        //        AggToleranceBox2.Enabled = false;
        //}

        private void QualityTroughDetection2_CheckedChanged(object sender, EventArgs e)
        {
           TroughDecisionBox2.Enabled = QualityTroughDetection2.Checked;//Melek
           TroughMinDurationBox2.Enabled = QualityTroughDetection2.Checked;//Melek
           TroughMosThresholdBox2.Enabled = QualityTroughDetection2.Checked;//Melek
            //if (QualityTroughDetection2.Checked == true)
            //{
            //    TroughDecisionBox2.Enabled = true;
                
            //    TroughMinDurationBox2.Enabled = true;
            //    TroughMosThresholdBox2.Enabled = true;
            //    //label117.Enabled = true;
            //    //label118.Enabled = true;
            //    //label119.Enabled = true;
            //    //label121.Enabled = true;
            //}
            //else
            //{
            //    TroughDecisionBox2.Enabled = false;
            //    TroughMinDurationBox2.Enabled = false;
            //    TroughMosThresholdBox2.Enabled = false;
            //    //label117.Enabled = false;
            //    //label118.Enabled = false;
            //    //label119.Enabled = true;
            //    //label121.Enabled = false;

            //}
        }

        private void ShowAggBSBox_CheckedChanged(object sender, EventArgs e)
        {
            // if the show all box is enabled, 
            // we still need to show either the normal bad scnenes or the aggregated bad scenes
            if (ShowAllScenesBox.Checked == true)
            {
                if (ShowAggBSBox.Enabled == true)
                {
                    if (ShowAggBSBox.Checked == true)
                        ShowBadSceneBox.Checked = false;
                    else
                        ShowBadSceneBox.Checked = true;
                }
            }
            // if the show all box is disabled, we can choose to
            // 1. display none of the normal or aggregated bad scenes (checking out one of the two doesnt enable the other one)
            // 2. display either (but not both) (clicking one of the two disables the other)
            else
            {
                if (ShowAggBSBox.Checked == true)
                    ShowBadSceneBox.Checked = false;
                else
                    ShowBadSceneBox.Checked = true;
            }

            // make the MOS chart display the AGG scenes or the bad scenes
            if (ShowAggBSBox.Checked == true)
            {
                MOSchart.Show_Agg_Alerts(true);
            }
            else
            {
                MOSchart.Show_Agg_Alerts(false);
            }
            this.MOSchart.Invalidate();

        }

        // click on audio detection check box
        //private void AudioCheckDetectionBox2_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (AudioCheckDetectionBox2.Checked == true)
        //    {
        //        audioAlertDecisionBox2.Enabled = true;
        //        IgnoreUnsupportedStreamsBox2.Enabled = true;
        //    }
        //    else
        //    {
        //        audioAlertDecisionBox2.Enabled = false;
        //        IgnoreUnsupportedStreamsBox2.Enabled = false;
        //    }
        //}


        /// <summary>
        /// User clicked on process joblist button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartJobListProcessingButton_Click(object sender, System.EventArgs e)
        {
            ProcessJobListButton.Enabled = false;    // reenabling the start button will be picked up on next joblist update and interepreted as a pause in the process;
            PauseJobListProcessingButton.Enabled = true;

            qaClass.jobManager.JobListProcessing = true;
            //qaClass.jobManager.Checkjobsstatus();

            //qatool.JobListProcessing = true;
            //qatool.Checkjobsstatus();
        }

        /// <summary>
        /// User clicked on process joblist button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseJobListProcessingButton_Click(object sender, EventArgs e)
        {
            if (ProcessJobListButton.Enabled == false)  // if the process button is not enabled: the process is on
            {
                qaClass.jobManager.JobListProcessing = false;
                ProcessJobListButton.Enabled = true;    // reenabling the start button will be picked up on next joblist update and interepreted as a pause in the process;
                PauseJobListProcessingButton.Enabled = false;
            }
        }

        private void AddStreamFolderBox_Click(object sender, EventArgs e)
        {
            AddStreamDir();          
        }

        /// <summary>
        /// Apply changes made to the monitored folders : settings + folder manager
        /// by scanning through all datagridview rows.
        ///
        /// TO IMPROVE
        /// USE DATABINDING
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyFolderSettingsBox_Click(object sender, EventArgs e)
        {
            SourceFolder Streamdir;
            DataGridViewRow row = new DataGridViewRow();
            string tmpstr = "";
            bool appliedchangesonlyfolders = true;//Melek
            bool nochangesappliedsourcefolder = true; //Melek
            bool nochangesappliedresfolder = true;//Melek
            bool nochangesappliedrepfolder = true;//Melek
            // go through each folder in the list of folder
            for (int j = 0; j < StreamFoldersBox.Rows.Count; j++) // When the new Priority values are selected set priority values
            {
                row = StreamFoldersBox.Rows[j];

                if ((string)row.Cells[5].Value == "1-Hot") //Set Priority Values and Hot - Melek
                {
                    row.Cells[3].Value = 0;
                    row.Cells[2].Value = true; //hot true

                }
                else if ((string)row.Cells[5].Value == "2-High")
                {
                    row.Cells[3].Value = 64;
                    row.Cells[2].Value = false; //hot false
                }
                else if ((string)row.Cells[5].Value == "3-Medium")
                {
                    row.Cells[3].Value = 128;
                    row.Cells[2].Value = false; //hot false
                }
                else if ((string)row.Cells[5].Value == "4-Low")
                {
                    row.Cells[3].Value = 255;
                    row.Cells[2].Value = false; //hot false
                }


                QAFolder qaf = new QAFolder((string)row.Cells[1].Value, "XML source folder", FolderType.SourceFolder);
                Streamdir = new SourceFolder(qaf, (bool)row.Cells[2].Value, Convert.ToInt16(row.Cells[3].Value), (string)row.Cells[4].Value, (string)row.Cells[5].Value);
                // Streamdir = new SourceFolder(qaf, Convert.ToInt16(row.Cells[3].Value), (string)row.Cells[4].Value, (string)row.Cells[5].Value);

                // check each folder in the GUI with the actual list of source folders
                // and update it if a difference is found
                if (qatool.EditSourceFolder(Streamdir))
                    tmpstr += " " + Streamdir.path + "\n";
            }

            if (tmpstr != "")
            {
                tmpstr = "Applied changes to the following folders:\n" + tmpstr + "\n" + " NOTE: These changes will be applied only to the jobs to be processed on the Process Tab, not to the completed jobs on the Review and Results Tabs."; //Melek - comment is updated
                MessageBox.Show(tmpstr);
                Global.log(tmpstr);
                appliedchangesonlyfolders = false;//Melek
              
            }
          
            #region  resultfolderconfig
            //enter results folder info - Melek
            string nf = ResDirTextBox.Text;
           QAFolder newfolder = new QAFolder();
            
            if (nf == ""||(!Directory.Exists(nf)))
            {
                newfolder = new QAFolder(nf, "Local results folder", FolderType.ResultFolder);
                if (!qatool.SetNewResDir(newfolder))
                {

                    nf = newfolder.path;
                    ResDirTextBox.Text=nf;
                }
            }
                  
            try                  
            {

                if (nf.EndsWith("\\"))     
                  newfolder = new QAFolder(nf, "Local results folder", FolderType.ResultFolder);               
                else   
                    newfolder = new QAFolder(nf + "\\", "Local results folder", FolderType.ResultFolder);

                if (qatool.SetNewResDir(newfolder))
                {
                    MessageBox.Show("Results folder changed to " + newfolder.path);
                    nochangesappliedresfolder = false;
                }
            }
            catch (Exception ex)
            {
                Global.log("Error while changing to a new result folder.\n" + ex);

            }

            #endregion resultfolderconfig 
            #region ReportsConfig - Melek
            
            if (RepDirTextBox.Text == "")
            {
                MessageBox.Show("Reports Folder section can not be empty, please enter a folder path to save results.");

                if (qaClass.settings.ReportsFolder == "")
                {
                    RepDirTextBox.Text = "Set the folder where the pdf reports will be stored";
                }
                else
                {
                    RepDirTextBox.Text = qaClass.settings.ReportsFolder;
                }

            }
            else if (RepDirTextBox.Text != "Set the folder where the pdf reports will be stored\\" && RepDirTextBox.Text != "Set the folder where the pdf reports will be stored")//MElek
            {
                if (!Directory.Exists(RepDirTextBox.Text))
                {
                    MessageBox.Show("Directory \"" + RepDirTextBox.Text + "\" does not exist!");
                    if (qaClass.settings.ReportsFolder != null)
                    {
                        RepDirTextBox.Text = qaClass.settings.ReportsFolder; //Melek
                    }
                }
            else 
            {
                if (!RepDirTextBox.Text.EndsWith("\\")) { RepDirTextBox.Text = RepDirTextBox.Text + "\\"; }
                if (qatool.SetNewRepDir(RepDirTextBox.Text))
                {
                    nochangesappliedrepfolder = false;
                    MessageBox.Show("Reports folder changed to " + RepDirTextBox.Text);
                }
            }
            }
            #endregion Reportconfig

            if (nochangesappliedsourcefolder && nochangesappliedresfolder && nochangesappliedrepfolder && appliedchangesonlyfolders && !reportsfolderchangedtemp) //no changes has done
                MessageBox.Show("No changes applied.");               
        }



        private void BadSceneListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            BadSceneListView_Click(sender, e);
        }

        /// <summary>
        /// Remove a monitored folder using the GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteStreamFolderBox_Click(object sender, EventArgs e)
        {
            RemoveStreamDir();
        }

        private void UpdateSummaryFiles_CheckedChanged(object sender, EventArgs e)
        {
            CGeneralSettings GUIfolders = qaClass.settings;
            // enable or disable the update monitored streams only box
            UpdateMonitoredStreamsOnlyBox.Enabled = UpdateSummaryFiles.Checked;

            if (UpdateSummaryFiles.Checked)
                GUIfolders.UpdateSummary = 1;
            else
                GUIfolders.UpdateSummary = 0;

            qatool.setNewFolderPrefs(GUIfolders);
        }

        private void UpdateMonitoredStreamsOnlyBox_CheckedChanged(object sender, EventArgs e)
        {
            CGeneralSettings GUIfolders = qaClass.settings;
            if (UpdateMonitoredStreamsOnlyBox.Checked)
            {

                GUIfolders.UpdateMonitoredStreamsOnly = 1;
            }
            else
            {
                GUIfolders.UpdateMonitoredStreamsOnly = 0;
            }
            qatool.setNewFolderPrefs(GUIfolders);
        }

        //private void FieldOrderBox2_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (FieldOrderBox2.Checked == true)
        //    {
        //        //FieldDlabel.Enabled = true;
        //        FieldDecisionBox2.Enabled = true;
        //    }
        //    else
        //    {
        //        //FieldDlabel.Enabled = false;
        //        FieldDecisionBox2.Enabled = false;
        //    }
        //}

        private void stopbutton_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.currentState != PlayState.Stopped)
                {
                    VideoTimeBox.Text = "00:00:00";//Melek
                    this.isTimer = false;
                    PlaybackState(false);
                    EnablePlayback(true);
                    OverlayButton.Checked = false;

                }
            }
            catch (Exception ex)
            {
                Global.log("Problem stoping the video.\n" + ex);
            }

            StopClip();

        }


        private void ZipResBox_CheckedChanged(object sender, EventArgs e)
        {
            //CGeneralSettings GUIfolders = qaClass.settings;
            qaClass.settings.ZipRes = ZipResBox.Checked;
            if (qaClass.settings.ZipRes)
                Global.log("Enabled results compression.");
            else
                Global.log("Disabled results compression.");
            qatool.saveSettings();
        }

        private void CacheDirSizeApply_Click(object sender, EventArgs e)
        {
            long cachesize;
            qaClass.settings.CacheLimit = (int)CacheDirSizeBox.Value;
            Global.log("Changed the cache folder size limit to " + qaClass.settings.CacheLimit + " MB.");

            do
            {
                cachesize = Global.FolderSize(qaClass.settings);
            } while (cachesize > qaClass.settings.CacheLimit * 1024 * 1024);


            qatool.saveSettings();
        }

        #region Alert scenes type filters
        private void ShowAllScenesBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowAllScenesBox.Checked == true)
            {
                ShowBadSceneBox.Enabled = false;
                if (vs.sum.qualityProfile.aggregatedBadSceneDetector.Enabled == true)
                    ShowAggBSBox.Enabled = true; // just leave the agg bad scene box enabled to be able to switch between agg or non agg bad scenes
                else
                    ShowAggBSBox.Enabled = false;
                ShowTroughsBox.Enabled = false;
                ShowBWBox.Enabled = false;
                ShowFrozenBox.Enabled = false;
                ShowFieldsBox.Enabled = false;

                if (ShowAggBSBox.Checked == true)
                    ShowBadSceneBox.Checked = false;
                else
                    ShowBadSceneBox.Checked = true;

                if (vs.sum.qualityProfile.blankSceneDetector.Enabled == true) ShowBWBox.Checked = true;
                else ShowBWBox.Checked = false;
                if (vs.sum.qualityProfile.frozenSceneDetector.Enabled == true) ShowFrozenBox.Checked = true;
                else ShowFrozenBox.Checked = false;
                if (vs.sum.qualityProfile.troughDetector.Enabled == true) ShowTroughsBox.Checked = true;
                else ShowTroughsBox.Checked = false;
                if (vs.sum.qualityProfile.fieldOrderDetector.Enabled == true) ShowFieldsBox.Checked = true;
                else ShowFieldsBox.Checked = false;
            }
            else
            {
                ShowBadSceneBox.Enabled = true;
                if (vs.sum.qualityProfile.aggregatedBadSceneDetector.Enabled == true)
                    ShowAggBSBox.Enabled = true;    // just leave the agg bad scene box enabled to be able to switch between agg or non agg bad scenes
                else
                    ShowAggBSBox.Enabled = false;   // just leave the agg bad scene box enabled to be able to switch between agg or non agg bad scenes

                if (vs.sum.qualityProfile.blankSceneDetector.Enabled == true) ShowBWBox.Enabled = true;
                else ShowBWBox.Enabled = false;
                if (vs.sum.qualityProfile.frozenSceneDetector.Enabled == true) ShowFrozenBox.Enabled = true;
                else ShowFrozenBox.Enabled = false;
                if (vs.sum.qualityProfile.troughDetector.Enabled == true) ShowTroughsBox.Enabled = true;
                else ShowTroughsBox.Enabled = false;
                if (vs.sum.qualityProfile.fieldOrderDetector.Enabled == true) ShowFieldsBox.Enabled = true;
                else ShowFieldsBox.Enabled = false;
                ShowAggBSBox.Checked = false;
                ShowBadSceneBox.Checked = false;
                ShowBWBox.Checked = false;
                ShowFrozenBox.Checked = false;
                ShowTroughsBox.Checked = false;
                ShowFieldsBox.Checked = false;
            }
            FillBadSceneList();
        }

        private void ShowBadSceneBox_CheckedChanged(object sender, EventArgs e)
        {
            // see show agg bad scenes box for explanation
            if (ShowAllScenesBox.Checked == false)
            {
                if (ShowBadSceneBox.Checked == true)
                    ShowAggBSBox.Checked = false;
            }
            FillBadSceneList();
        }

        private void ShowTroughsBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowTroughsBox.Enabled == true)
                FillBadSceneList();
        }

        private void ShowFieldsBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowFieldsBox.Enabled == true)
                FillBadSceneList();
        }

        private void ShowBWBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowBWBox.Enabled == true)
                FillBadSceneList();
        }

        private void ShowFrozenBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowFrozenBox.Enabled == true)
                FillBadSceneList();
        }
        #endregion

        /// <summary>
        /// TO CHECK.
        /// WAS USED WITH STANDARD LISTVIEW
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteResultsBox_Click(object sender, EventArgs e)
        {
            VStream streamtodelete;
            try
            {
                if (ResultListView.SelectedItems.Count != 0)	// adds a check on the selected item size
                {
                    streamtodelete = (VStream)ResultListView.SelectedItems[0].Tag;

                    if (vs == streamtodelete)
                        StopClip();

                    // delete results file
                    deleteResultsFiles(streamtodelete);
                }
            }
            catch (Exception ex)
            {
                Global.log("Error while deleting results\n" + ex.Message);
            }

        }

        private void GUI_Load(object sender, EventArgs e)
        {
            CleanupHungProcesses();
        }


        private string getDefaultBrowser()
        {
            string browser = string.Empty;
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                //trim off quotes
                browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");
                if (!browser.EndsWith("exe"))
                {
                    //get rid of everything after the ".exe"
                    browser = browser.Substring(0, browser.LastIndexOf(".exe") +
                    4);
                }
            }
            finally
            {
                if (key != null) key.Close();
            }
            return browser;
        }

        private void FieldButton_CheckedChanged(object sender, EventArgs e)
        {
            int pb;
            double currentpos;

            if (mediaPosition == null)
            {
                FieldButton.Checked = false;
                return;
            }
            this.mediaPosition.get_CurrentPosition(out currentpos);

            //            if (this.currentState == PlayState.Running)
            {
                if ((FieldButton.Checked == true))
                {

                    // Close directshow 
                    ClosePlayback();

                    // generate an avs script that separate fields and resize them to frame size+ double frame rate 
                    // to check the field order is correct

                    //StreamWriter sw = new StreamWriter("c:\\qacache\\fieldcheck.avs");
                    StreamWriter sw = new StreamWriter(qaClass.settings.CacheFolder + "\\fieldcheck.avs");
                    //
                    sw.WriteLine("directshowsource(\"" + vs.path.StreamPath + "\",fps=25)");
                    sw.WriteLine("assumeTFF()");
                    sw.WriteLine("separatefields()");
                    sw.WriteLine("assumefps(50)");
                    sw.WriteLine("lanczosresize(720,576)");
                    sw.Close();

                    pb = OpenStream(qaClass.settings.CacheFolder + "\\fieldcheck.avs", currentpos);
                }
                else
                {
                    // Close directshow 
                    ClosePlayback();

                    if (File.Exists(qaClass.settings.CacheFolder + "\\fieldcheck.avs"))
                        File.Delete(qaClass.settings.CacheFolder + "\\fieldcheck.avs");

                    pb = OpenStream(vs.path.StreamPath, currentpos);
                }
            }
        }

        private void OverlayButton_CheckedChanged(object sender, EventArgs e)
        {
            int pb;
            double currentpos;

            if ((this.mediaPosition != null) && ((OverlayButton.Enabled == true)))
            {
                this.mediaPosition.get_CurrentPosition(out currentpos);

                // Close directshow 
                ClosePlayback();

                pb = OpenStream(vs.path.StreamPath, currentpos);
            }
        }

        private void FrameStepButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.currentState == PlayState.Running)
                playbutton_Click("", EventArgs.Empty);
            StepOneFrame();
        }

        private void MuteButton_CheckedChanged(object sender, EventArgs e)
        {
            MuteStatus();
        }

        private void VideoPanelLabel_Click(object sender, EventArgs e)
        {
            playbutton_Click("", EventArgs.Empty);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            AboutButton_Click("", EventArgs.Empty);
        }

        // if the user changes a parameter in the overlay panel
        // the registry is updated so the directshow overlay filter will use it for the next frames to be decoded
        //private void overlay_picture_changed(object sender, EventArgs e)
        //{
        //    // the directshow filter reads a 32bits number from the registry and interprets it to decide what to overlay.
        //    // there are 4 layers of overlay: each layer will show 1 map out of several possible.


        //    long reg_value = 0;

        //    // Layer 1: picture map
        //    //
        //    switch (overlay_picture.SelectedIndex)
        //    {
        //        case 0: reg_value = 0; break;
        //        case 1: reg_value = 1; break;
        //        case 2: reg_value += 1 << 1; break;
        //        case 3: reg_value += 1 << 2; break;
        //    }

        //    // layer 2: text overlay
        //    switch (overlay_text_combo.SelectedIndex)
        //    {
        //        case 0: reg_value += 0; break;
        //        case 1: reg_value += 1 << 8; break;
        //        case 2: reg_value += 1 << 9; break;
        //        case 3: reg_value += 1 << 10; break;
        //        case 4: reg_value += 1 << 11; break;
        //    }

        //    // layer 3: boundary overlay
        //    switch (overlay_boundary_combo.SelectedIndex)
        //    {
        //        case 0: reg_value += 0; break;
        //        case 1: reg_value += 1 << 16; break;
        //        case 2: reg_value += 1 << 17; break;
        //    }

        //    // layer 4: boundary extra
        //    // only usuable when layer 3 is not used
        //    if (overlay_boundary_combo.SelectedIndex > 0)
        //    {
        //        overlay_boundary_extra.Enabled = false;
        //    }
        //    else
        //    {
        //        overlay_boundary_extra.Enabled = true;

        //        if (overlay_skipped_check.Checked) reg_value += 1 << 18;
        //        if (overlay_recovered_check.Checked) reg_value += 1 << 19;
        //        if (overlay_88_check.Checked) reg_value += 1 << 20;
        //    }

        //    // layer 5: motion vectors
        //    if (overlay_mv.Checked)
        //        reg_value += 1 << 24;

        //    // converts binary to hex
        //    // and update the registry with the new value
        //    overlay_registry(reg_value);
        //}

        // actually update the registrey value read by the directshow overlay filter that determines what to overlay
        //public void overlay_registry(long reg_value)
        //{
        //    RegistryKey regkey;
        //    regkey = Registry.CurrentUser.OpenSubKey(@"Software\BT\H.264 Decoder App\1.4", true);
        //    if (regkey == null)
        //        regkey = Registry.CurrentUser.CreateSubKey(@"Software\BT\H.264 Decoder App\1.4");

        //    string overlay_key = "pqos_overlay_mode";

        //    //reg_string="Hex: {0:X}", reg_value;
        //    regkey.SetValue(overlay_key, reg_value, RegistryValueKind.DWord);
        //}

        private void AboutButton_Click(object sender, EventArgs e)
        {
            Cabout aboutbox = new Cabout();
            aboutbox.Show();
        }

        private void OverlayBar_Click(object sender, EventArgs e)
        {
            if (OverlayButton.Checked == true)
                OverlayButton.Checked = false;
            else
                OverlayButton.Checked = true;
        }

        private void PassButton_Click(object sender, EventArgs e)
        {
            UserDecisionCommentForm UserPassForm = new UserDecisionCommentForm();
            UserPassForm.Clip_to_comment = "\"" + vs.StreamFileName + "\" to PASS. (model decision is [" + vs.sum.ModelDecision.String + "] )"; //Melek - path.name => streamfilename

            if (UserPassForm.ShowDialog() == DialogResult.OK)
            {
                vs.sum.UserDecision = new CDecision(Decision.PASS);
                vs.sum.UserDecision.User = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
                vs.sum.UserDecision.Comment = UserPassForm.UserComment;
                UserPassForm.Dispose();

                // register the change
                qatool.UserDecisionChange(vs);
                // update the detailed view
                openDetails(vs);
                // update the monitorting view
                updateInfoStatus();
                //refreshStreamsView();
            }
            else
            {
                UserPassForm.Dispose();
            }
        }

        private void BorderlineButton_Click(object sender, EventArgs e)
        {
            UserDecisionCommentForm UserPassForm = new UserDecisionCommentForm();
            UserPassForm.Clip_to_comment = "\"" + vs.StreamFileName + "\" to BORDERLINE. (model decision is [" + vs.sum.ModelDecision.String + "] )"; //Melek - path.name => streamfilename

            if (UserPassForm.ShowDialog() == DialogResult.OK)
            {
                vs.sum.UserDecision = new CDecision(Decision.BORDERLINE);
                vs.sum.UserDecision.User = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
                vs.sum.UserDecision.Comment = UserPassForm.UserComment;
                UserPassForm.Dispose();

                // Register the change
                qatool.UserDecisionChange(vs);
                // Update the detailed view
                openDetails(vs);
                // Update the monitorting view
                //refreshStreamsView();
                updateInfoStatus();
            }
            else
            {
                UserPassForm.Dispose();
            }
        }

        private void FailButton_Click(object sender, EventArgs e)
        {
            UserDecisionCommentForm UserPassForm = new UserDecisionCommentForm();
            UserPassForm.Clip_to_comment = "\"" + vs.StreamFileName + "\" to FAIL. (model decision is [" + vs.sum.ModelDecision.String + "] )"; //Melek - path.name => streamfilename

            if (UserPassForm.ShowDialog() == DialogResult.OK)
            {
                vs.sum.UserDecision = new CDecision(Decision.FAIL);
                vs.sum.UserDecision.User = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
                vs.sum.UserDecision.Comment = UserPassForm.UserComment;
                UserPassForm.Dispose();

                // register the change
                qatool.UserDecisionChange(vs);
                // update the detailed view
                openDetails(vs);
                // update the monitorting view
                //refreshStreamsView();
                updateInfoStatus();
            }
            else
            {
                UserPassForm.Dispose();
            }
        }

        private void CancelUserDecisionButton_Click(object sender, EventArgs e)
        {
            vs.sum.UserDecision = new CDecision(Decision.NOTMADE);
            qatool.UserDecisionChange(vs);
            openDetails(vs);
            //refreshStreamsView();
            updateInfoStatus();
        }

        //private void LogClearButton_Click(object sender, EventArgs e)
        //{
        //    LogText.Clear();
        //}

        // a profile has been clicked in the quality profile list
        private void ProfilesListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            displaySelectedProfile();
          //  CancelQualityProfileButton2.Enabled = false; //MElek
        }

        private void CancelQualityProfileButton_Click(object sender, EventArgs e)
        {
            displaySelectedProfile();
        }
        public bool newprofilebtn_clicked = false;//Melek
        private void NewProfile_Click(object sender, EventArgs e)
        {
            newprofilebtn_clicked = true;
            string ProfileName;
            Form_NameProfile profileNameForm = new Form_NameProfile();

            if (profileNameForm.ShowDialog() == DialogResult.OK)
            {
                ProfileName = profileNameForm.Name;
                profileNameForm.Dispose();

                if (!qaClass.profManager.ProfileExists(ProfileName))
                {
                    qaClass.profManager.NewProfile(ProfileName);
                    UpdateFoldersGui();
                    updategui();
                    newprofilebtn_clicked = false;//Melek
                    //  displaySelectedProfile();//Melek
                }
                else
                {
                    MessageBox.Show("This name is already used");
                }
            }
            else
            {
                profileNameForm.Dispose();
            }

            if (qaClass.profManager.ProfileCount() >= 2) //demostick
            {
                this.NewProfile.Enabled = false;
            }
        }

        /// <summary>
        /// Deletes a quality profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteProfile_Click(object sender, EventArgs e)
        {
            // when deleting a quality profile from the list
            // have to deal with :
            // - jobs using this profile : just use another one
            // - results that used this profile : generate the summaries again with another existing profile
                    
             //string op = "Remove";
            // ProfilesListBox2.Items.Clear();
            //UpdateFoldersGui();
            //updategui();
      
            
             refreshProfilesTab("Remove");
             displaySelectedProfile();
        }

        //private void XMLupdateDirButton_Click(object sender, EventArgs e) //Melek- disabled
        //{
        //    ChooseXMLupdateDir();
        //}

        private void ProcessorsTextBox_Clicked(object sender, EventArgs e)
        {
            QA.GUI.JobprocessorForm jpform = new QA.GUI.JobprocessorForm();

            if (jpform.ShowDialog() == DialogResult.OK)
            {
                if (jpform.nbcores > 0)
                {
                    qaClass.settings.nbThreads = jpform.nbcores;
                    jpform.Dispose();
                }
            }
            else
            {
                jpform.Dispose();
            }

            ProcessorsLabel.Text = qaClass.jobManager.ProcessorsString;
        }

        private void ProcessorsLabel_Click(object sender, EventArgs e)
        {
            QA.GUI.JobprocessorForm jpform = new QA.GUI.JobprocessorForm();

            if (jpform.ShowDialog() == DialogResult.OK)
            {
                if (jpform.nbcores > 0)
                {
                    qaClass.settings.nbThreads = jpform.nbcores;
                    jpform.Dispose();
                }
            }
            else
            {
                jpform.Dispose();
            }

            ProcessorsLabel.Text = qaClass.jobManager.ProcessorsString;
        }

        /// <summary>
        /// Filter list by text
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="txt"></param>
        void ResultTextFilter(ObjectListView olv, string txt)
        {
            TextMatchFilter filter = null;
            if (!String.IsNullOrEmpty(txt))
                filter = new TextMatchFilter(olv, txt);

            // Setup a default renderer to draw the filter matches
            if (filter == null)
                olv.DefaultRenderer = null;
            else
                olv.DefaultRenderer = new HighlightTextRenderer(txt);

            // Some lists have renderers already installed
            HighlightTextRenderer highlightingRenderer = olv.GetColumn(0).Renderer as HighlightTextRenderer;
            if (highlightingRenderer != null)
                highlightingRenderer.TextToHighlight = txt;


            olv.ModelFilter = filter;
            /*
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            olv.ModelFilter = filter;
            stopWatch.Stop();

             
            IList objects = olv.Objects as IList;
            if (objects == null)
                this.toolStripStatusLabel1.Text =
                    String.Format("Filtered in {0}ms", stopWatch.ElapsedMilliseconds);
            else
                this.toolStripStatusLabel1.Text =
                    String.Format("Filtered {0} items down to {1} items in {2}ms",
                                  objects.Count,
                                  olv.Items.Count,
                                  stopWatch.ElapsedMilliseconds);
             */
            //GC.Collect();//Melek
        }

        private void ResultTextFilter_TextBox_TextChanged(object sender, EventArgs e)
        {

            // first disable the filter by decision
            // as filter by text works cannot be combined straight away with another filter
            //if (ResultTextFilter_TextBox.Text != "") //Melek - disabled
            //{
                ActivateTextFilter();
                /*
                FilterFail_Checkbox.Checked = false;
                FilterBorderline_Checkbox.Checked = false;
                FilterPass_Checkbox.Checked = false;
                FilterByFilename_TextBox.Text = "";
                FilterToDate_Checkbox.Checked = false;
                FilterFromDate_Checkbox.Checked = false;
                 */
           // }

            //ResultTextFilter(this.ResultListView, ResultTextFilter_TextBox.Text);
        }

        private void SetTextFilter()
        {

            ResultTextFilter(this.ResultListView, ResultTextFilter_TextBox.Text);
        }

        private void ClearFilterText_Button_Click(object sender, EventArgs e)
        {
            ResultTextFilter_TextBox.Text = "";
            ActivateTextFilter();
        }

        private void FilterAll_Radio_CheckedChanged(object sender, EventArgs e)
        {

            if (FilterAll_Radio.Checked)
                ResultListView.ListFilter = null;

        }

        private void FilterLast_Radio_CheckedChanged(object sender, EventArgs e)
        {
            if (FilterLast_Radio.Checked)
            {
                int tail;
                try
                {
                    tail = (int)FilterLastAssetsNumeric.Value;
                }
                catch
                {
                    tail = 10;
                }

                if (tail < 0)
                    tail = 0;
                else if (tail > 100)
                    tail = 100;

                ResultListView.ListFilter = new TailFilter(tail);
            }

        }

        /// <summary>
        /// Change the number of items to display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterLastAssetsNumeric_ValueChanged(object sender, EventArgs e)
        {
            FilterLast_Radio_CheckedChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// OnlyFailFilter test
        /// </summary>
        public class AssetFilter : IModelFilter
        {
            bool FDecision, FFail, FBorderline, FPass;
            string FFilename = "";
            bool filterreturn = false;
            bool Ffrom = false;
            bool Fto = false;
            DateTime Ffromdate;
            DateTime Ftodate;
            bool FAlerts, FBad, FAggbad, FTrough, FField, FBlank, FFrozen;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="ffail"></param>
            /// <param name="fborderline"></param>
            /// <param name="fpass"></param>
            /// <param name="ffilename"></param>
            /// <param name="ffrom"></param>
            /// <param name="ffromedate"></param>
            /// <param name="fto"></param>
            /// <param name="ftodate"></param>
            public AssetFilter(bool ffail,
                bool fdecision,
                bool fborderline,
                bool fpass,
                string ffilename,
                bool ffrom,
                DateTime ffromdate,
                bool fto,
                DateTime ftodate,
                bool falerts,
                bool fbad,
                bool faggbad,
                bool ftrough,
                bool ffield,
                bool fblank,
                bool ffrozen)
            {
                FDecision = fdecision;
                FFail = ffail;
                FPass = fpass;
                FBorderline = fborderline;
                FFilename = ffilename;
                Ffrom = ffrom;
                Fto = fto;
                Ffromdate = ffromdate;
                Ftodate = ftodate;
                FAlerts = falerts;
                FBad = fbad;
                FAggbad = faggbad;
                FTrough = ftrough;
                FField = ffield;
                FBlank = fblank;
                FFrozen = ffrozen;
            }

            public AssetFilter(bool ffail, bool fdecision, bool fpass, bool falerts, bool fbad, bool ftrough, bool fblank) //Melek
            {
                FFail = ffail;
                FDecision = fdecision;
                FPass = fpass;
                FBad = fbad;
                FTrough = ftrough;
                FBlank = fblank;
                FAlerts = falerts;
            }



            /// <summary>
            /// Returns true to keep the item visible
            /// False to hide it
            /// According to the filters selected
            /// </summary>
            /// <param name="modelObject"></param>
            /// <returns></returns>
            public bool Filter(object modelObject)
            {
                // Decision check
                // inclusive
                if (FDecision)
                {
                    filterreturn = false;
                    if (FPass)
                        if (((VStream)modelObject).Final_Decision.Made == Decision.PASS)
                            filterreturn = true;
                    if (FBorderline)
                        if (((VStream)modelObject).Final_Decision.Made == Decision.BORDERLINE)
                            filterreturn = true;
                    if (FFail)
                        if (((VStream)modelObject).Final_Decision.Made == Decision.FAIL)
                            filterreturn = true;
                    // if all are unchecked, return false for everything
                    if ((!FPass) && (!FBorderline) && (!FFail))
                        if (((VStream)modelObject).Final_Decision.Made == Decision.NOTMADE)
                            filterreturn = true;

                    if (filterreturn == false)
                        return false;
                }

                // Filter by filename
                // exclusive
                if (FFilename != "")
                    if (((VStream)modelObject).path.Name.ToLower().Contains((FFilename.ToLower())))
                        filterreturn = true;
                    else
                        return false;


                // date filtering
                // from date
                // exclusive
                if (Ffrom)
                    if (((VStream)modelObject).tasks.Overall.stats.endedTime < Ffromdate)
                        return false;

                // to date
                // exclusive
                if (Fto)
                    if (((VStream)modelObject).tasks.Overall.stats.endedTime > Ftodate)
                        return false;

                // Filter by alert scene
                // inclusive
                if (FAlerts)
                {
                    filterreturn = false;
                    if (FBad)
                        if (((VStream)modelObject).sum.BSA.Count > 0)
                            filterreturn = true;
                    if (FAggbad)
                        if (((VStream)modelObject).sum.AggBSA.Count > 0)
                            filterreturn = true;
                    if (FTrough)
                        if (((VStream)modelObject).sum.TroughsA.Count > 0)
                            filterreturn = true;
                    if (FField)
                        if (((VStream)modelObject).sum.FieldScnA.Count > 0)
                            filterreturn = true;
                    if (FBlank)
                        if (((VStream)modelObject).sum.BWScnA.Count > 0)
                            filterreturn = true;
                    if (FFrozen)
                        if (((VStream)modelObject).sum.FzScnA.Count > 0)
                            filterreturn = true;

                    if (filterreturn == false)
                        return false;
                }

                // if 
                return true;

            }
        }

        #region Result list asset filter
        /// <summary>
        /// Apply the updated ModelFilter to the result list view
        /// with the current filter parameters from the gui
        /// </summary>
        public void SetAssetFilter()
        {
            try
            {
                ResultListView.ModelFilter = new AssetFilter(FilterFail_Checkbox.Checked,
                FilterByDecisionCheckbox.Checked,
                FilterBorderline_Checkbox.Checked,
                FilterPass_Checkbox.Checked,
                FilterByFilename_TextBox.Text,
                FilterFromDate_Checkbox.Checked,
                FilterFromDatePicker.Value,
                FilterToDate_Checkbox.Checked,
                FilterToDatePicker.Value,
                FilterAlertScenesCheckbox.Checked,
                FilterBadCheckbox.Checked,
                FilterAggBadCheckbox.Checked,
                FilterTroughCheckbox.Checked,
                FilterFieldCheckbox.Checked,
                FilterBlankCheckbox.Checked,
                FilterFrozenCheckbox.Checked
                );
            }
            catch { }


        }
        public void SetAssetFilter1() //Melek 
        {
            try
            {
                FailedResultListView.ModelFilter = new AssetFilter(FailedResults_Checkbox.Checked,
                DisplayallStreamsCheckBox.Checked,
                PassedResults_Checkbox.Checked,
                FilterbyAlertScenesCheckbox.Checked,
                FilterBad_checkBox.Checked,
                FilterTrough_checkBox.Checked,
                FilterBlank_checkBox.Checked);
            }
            catch { }

        }


        private void FilterFail_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            // disable filter by text as we cannot combine model filter straightaway
            //           if (FilterFail_Checkbox.Checked==true) // to avoid erasing the 1st letter typed in the filter by text
            //               ResultTextFilter_TextBox.Text = "";

            ActivateAssetFilter();
        }

        private void FilterBorderline_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            // disable filter by text as we cannot combine model filter straightaway
            //           if (FilterBorderline_Checkbox.Checked == true) // to avoid erasing the 1st letter typed in the filter by text
            //               ResultTextFilter_TextBox.Text = "";

            ActivateAssetFilter();
        }

        private void FilterPass_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            // disable filter by text as we cannot combine model filter straightaway
            //           if (FilterPass_Checkbox.Checked == true) // to avoid erasing the 1st letter typed in the filter by text
            //              ResultTextFilter_TextBox.Text = "";

            ActivateAssetFilter();
        }

        private void FilterByFilename_TextBox_TextChanged(object sender, EventArgs e)
        {
            //           if (FilterByFilename_TextBox.Text!="")
            //               ResultTextFilter_TextBox.Text = "";

            ActivateAssetFilter();
        }

        private void ClearFilterbyFilenameButton_Click(object sender, EventArgs e)
        {
            FilterByFilename_TextBox.Text = "";
            ActivateAssetFilter();
        }


        private void FilterFromDatePicker_ValueChanged(object sender, EventArgs e)
        {

            //            if (FilterFromDate_Checkbox.Checked == true)
            //                ResultTextFilter_TextBox.Text = "";

            ActivateAssetFilter();
        }

        private void FilterToDatePicker_ValueChanged(object sender, EventArgs e)
        {
            //            if (FilterToDate_Checkbox.Checked==true)
            //                ResultTextFilter_TextBox.Text = "";

            ActivateAssetFilter();
        }

        private void FilterFromDate_Checkbox_CheckedChanged(object sender, EventArgs e)
        {

            //            if (FilterFromDate_Checkbox.Checked == true)
            //                ResultTextFilter_TextBox.Text = "";

            ActivateAssetFilter();
        }

        private void FilterToDate_Checkbox_CheckedChanged(object sender, EventArgs e)
        {

            //            if (FilterToDate_Checkbox.Checked == true)
            //                ResultTextFilter_TextBox.Text = "";

            ActivateAssetFilter();
        }

        private void ActivateTextFilter()
        {
            this.FilterByTextGroup.ForeColor = System.Drawing.SystemColors.ControlText;
            // FilterByAssetSpecGroup.ForeColor = System.Drawing.SystemColors.ControlDark;
            SetTextFilter();
        }

        private void ActivateAssetFilter()
        {
            FilterByTextGroup.ForeColor = System.Drawing.SystemColors.ControlDark;
            //FilterByAssetSpecGroup.ForeColor = System.Drawing.SystemColors.ControlText;
            SetAssetFilter();
            SetAssetFilter1(); //Review Tab filter streams

        }

        private void FilterByTextGroup_Enter(object sender, EventArgs e)
        {
            ActivateTextFilter();
        }

        private void FilterByAssetSpecGroup_Enter(object sender, EventArgs e)
        {
            ActivateAssetFilter();
        }

        private void FilterBadCheckbox_CheckedChanged(object sender, EventArgs e)
        {

            ActivateAssetFilter();
        }

        private void FilterAggBadCheckbox_CheckedChanged(object sender, EventArgs e)
        {

            ActivateAssetFilter();

        }

        private void FilterTroughCheckbox_CheckedChanged(object sender, EventArgs e)
        {

            ActivateAssetFilter();
        }

        private void FilterFieldCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter();
        }

        private void FilterBlankCheckbox_CheckedChanged(object sender, EventArgs e)
        {

            ActivateAssetFilter();
        }

        private void FilterFrozenCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter();
        }

        private void FilterAlertScenesCheckbox_CheckedChanged(object sender, EventArgs e)
        {

            ActivateAssetFilter();
            //FilterAlertPanel.Enabled = FilterAlertScenesCheckbox.Checked;
            FilterBadCheckbox.Enabled=FilterAlertScenesCheckbox.Checked;//Melek
            FilterBlankCheckbox.Enabled = FilterAlertScenesCheckbox.Checked;//Melek
            FilterTroughCheckbox.Enabled = FilterAlertScenesCheckbox.Checked;//Melek

        }
        #endregion

        private void FilterDecisionCheckbox_CheckedChanged(object sender, EventArgs e)
        {

            ActivateAssetFilter();
            //FilterByDecisionPanel.Enabled = FilterByDecisionCheckbox.Checked;
            FilterFail_Checkbox.Enabled = FilterByDecisionCheckbox.Checked;//MELEK
            FilterPass_Checkbox.Enabled = FilterByDecisionCheckbox.Checked;//MELEK
        }

        /// <summary>
        /// Color the resultlist rows alternatively or by decision (red, amber, green)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecisionColoredRow_CheckedChanged(object sender, EventArgs e)
        {
            // this is handled in the formatrow event (ResultListView_FormatRow)
            // we just need to rebuild the columns
            this.ResultListView.RebuildColumns();

        }

        /// <summary>
        /// use graphics in the resultlist: 
        /// red,amber,green ligth in front of the filename
        /// 5 stars scoring system for the pMOS
        /// red bar for the % of bad scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ownerDraw_CheckedChanged(object sender, EventArgs e)
        {

            this.ResultListView.OwnerDraw = this.ownerDrawCheckBox.Checked;
            this.ResultListView.RebuildColumns();
        }

        //private void LogClearButton2_Click(object sender, EventArgs e)
        //{
        //    LogText2.Clear();
        //}

        private void LogClearButton_Click(object sender, EventArgs e)
        {
            LogText.Clear();
        }


        private void FailedResults_Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter();
        }

        private void FailedownerDrawCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.FailedResultListView.OwnerDraw = this.FailedownerDrawCheckBox.Checked;

            this.FailedResultListView.RebuildColumns();
        }

        private void FailedDecisionColoredRow_CheckedChanged(object sender, EventArgs e)
        {
            this.FailedResultListView.RebuildColumns(); //Melek
        }

        private void DisplayallStreamsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter(); // Melek
            //if (DisplayallStreamsCheckBox.Checked)
            //    ReviewTabFilterPanel.Enabled = true;
            //else
            //    ReviewTabFilterPanel.Enabled = false;

            FailedResults_Checkbox.Enabled = DisplayallStreamsCheckBox.Checked;//Melek
            PassedResults_Checkbox.Enabled = DisplayallStreamsCheckBox.Checked;//Melek
            
            //if (enablefilter)
            //{
            //    DisplayallStreamsCheckBox.Text = "Disable Decision Filter";
            //    enablefilter = false;
            //}
            //else
            //{
            //    DisplayallStreamsCheckBox.Text = "Enable Decision Filter";
            //    enablefilter = true;
            //}
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter(); // Melek
        }

        private void FilterbyAlertScenesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter(); // Melek
            //if (FilterbyAlertScenesCheckbox.Checked)
            //{
            //    FilterbyDecision_Panel.Enabled = true;
            //}
            //else
            //    FilterbyDecision_Panel.Enabled = false;
            FilterBad_checkBox.Enabled = FilterbyAlertScenesCheckbox.Checked; //Melek
            FilterBlank_checkBox.Enabled = FilterbyAlertScenesCheckbox.Checked;//Melek
            FilterTrough_checkBox.Enabled = FilterbyAlertScenesCheckbox.Checked;//Melek

        }

        private void filterBad_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter();

        }

        private void filterBlank_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter();
        }

        private void filterTrough_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            ActivateAssetFilter();
        }

        private void GenerateReportButtonReviewView_Click(object sender, EventArgs e)
        {
            if (RepDirTextBox.Text == "" || RepDirTextBox.Text == "Set the folder where the pdf reports will be stored") 
            { MessageBox.Show("Please select a folder on configuration tab to save the reports."); }
            else
            {
                if (ReportNametextBoxReviewMenu.Text == "")
                {
                    MessageBox.Show("Report Title section cannot be empty, please enter a title to display in .pdf report(s).");
                }
                else
                {
                    IList Ltvs = FailedResultListView.GetSelectedObjects();
                    try
                    {
                        if (Ltvs.Count == 0)
                        {
                            MessageBox.Show("Please select a video to generate a report.");
                        }
                        else
                        {
                            int i = 0;
                            foreach (object tvs in Ltvs)
                            {
                                GenerateReportButtonReviewViewClicked = true;
                                reportdatedisplayed = false;
                                try
                                {
                                    Document pdfdoc = new Document(iTextSharp.text.PageSize.LETTER, 40, 10, 42, 35);
                                    if (CreateMultiplePDFFile(reportname, (VStream)tvs, pdfdoc))
                                    {
                                        i++;
                                    }
                                    docopened = false;
                                    pdfdoc.Close();
                                }
                                catch{ }
                            }
                            if (i == 1) MessageBox.Show(i + " Report generated successfully. Located in " + RepDirTextBox.Text);
                            if (i > 1)
                            {
                               MessageBox.Show(i + " Reports generated successfully. Located in " + RepDirTextBox.Text);
                            }
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }
        private void GenerateReportButtonResultsView_Click(object sender, EventArgs e)
        {
            if (RepDirTextBox.Text == "" || RepDirTextBox.Text == "Set the folder where the pdf reports will be stored") { MessageBox.Show("Please select a folder on configuration tab to save the reports."); }
            else
            {
                GenerateReportResultsViewClicked = true;
                try
                {
                    // event fired when the contextual menu was clicked
                    //ToolStripMenuItem mi = (ToolStripMenuItem)e.ClickedItem;
                    // mi : item clicked in the contextual menu
                    // VStream tvs = (VStream)mi.Tag;

                    if (ReportNameResultsViewtextBox.Text == "")
                    {
                        MessageBox.Show("Report Title section cannot be empty, please enter a title to display in .pdf report.");
                    }
                    else
                    {

                        //CreatePDFFile();
                        string reportname = SaveReportAstextBox.Text;
                        if (reportname == "")
                        {
                            MessageBox.Show("Save Report As section cannot be empty, please enter a file name.");

                        }
                        else
                        {
                            IList Ltvs = ResultListView.GetSelectedObjects();
                            if (Ltvs.Count == 0)
                            {
                                MessageBox.Show("Please select a video to generate a report.");
                            }
                            else
                            {
                                //  string imagefoldercreate = AppDomain.CurrentDomain.BaseDirectory + "Reports";
                                // Directory.CreateDirectory(imagefoldercreate);

                                if (Directory.Exists(RepDirTextBox.Text))
                                {
                                    Document pdfdoc = new Document(iTextSharp.text.PageSize.LETTER, 40, 10, 42, 35);
                                    //string pdfFilePath = AppDomain.CurrentDomain.BaseDirectory + "Reports\\" + reportname + ".pdf";
                                    string pdfFilePath = RepDirTextBox.Text + reportname + ".pdf";
                                    PdfWriter wri = PdfWriter.GetInstance(pdfdoc, new FileStream(pdfFilePath, FileMode.Create));
                                    pdfdoc.Open();//Open Document to write
                                    Paragraph par = new Paragraph(" ");

                                    int i = 0;
                                    //reportdatedisplayed = false;
                                    //GenerateReportResultsViewClicked = true;
                                    foreach (object tvs in Ltvs)
                                    {
                                        try
                                        {
                                            if (CreateMultiplePDFFile(reportname, (VStream)tvs, pdfdoc)) i++;
                                            par.SpacingBefore = 10f;
                                            Paragraph parend = new Paragraph("------------------------------------------------------------------------------------------------------------------------------------------");
                                            pdfdoc.Add(parend);
                                        }
                                        catch { }
                                    }
                                    if (i == 1)
                                    {
                                        MessageBox.Show("PDF report for " + i + " video generated successfully. Located in " + RepDirTextBox.Text);
                                    } if (i > 1)
                                    {
                                        MessageBox.Show("PDF report for " + i + " videos generated successfully. Located in " + RepDirTextBox.Text);
                                    }

                                    //   par.Alignment = Element.ALIGN_CENTER;
                                    //Anchor anchor1 = new Anchor("Help", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 255)));
                                    //anchor1.Reference = "http://www.path1.com";
                                    //anchor1.Name = "left";
                                    //par.Add(anchor1);
                                    //par.Add("/");

                                    //Anchor anchor2 = new Anchor("Contact", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 255)));
                                    //anchor2.Reference = "http://www.path1.com";
                                    //anchor2.Name = "middle";
                                    //par.Add(anchor2);
                                    //par.Add("/");

                                    //Anchor anchor3 = new Anchor("Whatever we want to show", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 255)));
                                    //anchor3.Reference = "http://www.path1.com";
                                    //anchor3.Name = "middle";
                                    //par.Add(anchor3);
                                    //par.Alignment = Element.ALIGN_CENTER;
                                    //doc.Add(par);

                                    pdfdoc.AddAuthor("Path1");
                                    pdfdoc.AddTitle("v.Cortex Report");
                                    pdfdoc.AddSubject("This report is created by v.Cortex");
                                    docopened = false;
                                    pdfdoc.Close();
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }


        #region CutStream -Melek

        //public void CutStream()
        //{
        //  // TimeSpan ts = new TimeSpan(0, 0, 0, 00);
        //  //TimeSpan tsAdd = new TimeSpan(0, 0, 0, 30);

        // //  string timeSpan = "00:00:30";

        //    //for (int i = 0; i <= 108; i++)
        //    //{
        //       // string fileName = string.Format("{0:00}{1:00}{2:00}.mpg", ts.Hours, ts.Minutes, ts.Seconds);
        //      //  string startTime = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        //  try
        //  {
        //      ProcessStartInfo info = new ProcessStartInfo();
        //      // info.FileName = @"C:\ffmpeg";
        //      //info.FileName = AppDomain.CurrentDomain.BaseDirectory + "Tools\\ffmpeg.exe";
        //      // info.Arguments = string.Format("-i C:\\Users\\QAvod\\Desktop\\Thumbnail Test\\Disney.mpg -sameq -ss " + startTime + " -t " + timeSpan + "C:\\Users\\QAvod\\Desktop\\Thumbnail Test\\" + fileName);

        //     // info.Arguments = string.Format("-i C:\\Users\\QAvod\\Desktop\\Thumbnail Test\\Disney.mpg -sameq -ss 00:00:01 -t 00:00:10 C:\\Users\\QAvod\\Desktop\\Thumbnail Test\\Output.mpg");
        //    //  info.Arguments=cutstreamargs;
        //        //info.CreateNoWindow = true;
        //      //info.UseShellExecute = false;
        //     // string cutstreamargs = "-i C:\\Users\\QAvod\\Desktop\\Thumbnail Test\\Disney.mpg -sameq -ss 00:00:01 -t 00:00:10 C:\\Users\\QAvod\\Desktop\\Thumbnail Test\\Output.mpg";

        //        string inputfile = @"C:\Users\QAvod\Desktop\Thumbnail Test\Disney.mpg";
        //        string outputname = @"C:\Users\QAvod\Desktop\Thumbnail Test\Disneyfivesec.mpg";
        //        Process process = new Process();
        //        process.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "Tools\\ffmpeg.exe";
        //        process.StartInfo.Arguments = string.Format( "-i \""+inputfile+"\" -sameq -ss 00:00:01 -t 00:00:05 \""+outputname+"\"");
        //        process.StartInfo.UseShellExecute = false;
        //        process.StartInfo.RedirectStandardError = true;
        //        process.StartInfo.CreateNoWindow = true;
        //        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

        //        try
        //        {
        //            process.Start();
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //        }
        //      // process.WaitForExit();
        //        process.Close();
        //       process.Dispose();
        //      //ts = ts.Add(tsAdd);
        //       // process.Kill();
        //                   // }
        //  }
        //  catch (Exception ex) { MessageBox.Show(ex.Message); }
        //}
        #endregion CutStream

        #region email configurations
        /*
       public System.Web.Mail.MailFormat pFormat;
        // MailMessage myMail= new MailMessage();
        public System.Web.Mail.MailMessage myMail = new System.Web.Mail.MailMessage();
 
        private void SendEmailButton_Click(object sender, EventArgs e)
        {
            EmailSentlabel.Visible = false;
            //try
            //{
            //    //PDFReport(); //PDF Report method call
            //    //Email the attachment & send it to "To"

            //    SmtpClient client = new SmtpClient("smtpserver.gmail.com", 25);
            //    client.Credentials = new NetworkCredential("user@company.com", "password");
             //    MailMessage msg = new MailMessage();
            //    msg.From = new MailAddress("v.cortex@ipvidnet.com");
            //    msg.To.Add(new MailAddress(ToRichTextBox.Text));
            //    string SourceFilePath = @"C:\Users\QAvod\Desktop\v.Cortex - Crystal report\CrystalReport1.rpt";
            //    string DestinationFilePath = @"C:\Users\QAvod\Desktop\v.Cortex - Crystal report\bin\Debug\CrystalReport1.rpt";

            //    File.Delete(DestinationFilePath);

            //    File.Copy(SourceFilePath, DestinationFilePath);


            //    string reportfile = "CrystalReport1.rpt"; //Make sure that you created report file under Debug-bin
            //    Attachment data = new Attachment(reportfile);

            //    msg.Attachments.Add(data); //public list parameter
            //    //  client.Send(msg);
            //}

            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
           ////new code
           ////////////////////////System.Web.Mail.MailFormat pFormat;
            ////////////////////////// MailMessage myMail= new MailMessage();
            ////////////////////////System.Web.Mail.MailMessage myMail = new System.Web.Mail.MailMessage();
            if (firsttimemailsent)
            {
                myMail.Fields.Add
                    ("http://schemas.microsoft.com/cdo/configuration/smtpserver",
                                  "smtp.gmail.com");
                myMail.Fields.Add
                    ("http://schemas.microsoft.com/cdo/configuration/smtpserverport",
                                  "465");
                myMail.Fields.Add
                    ("http://schemas.microsoft.com/cdo/configuration/sendusing",
                                  "2");

            }

            string DestinationFilePath1 = @"C:\Documents and Settings\Melek.Babur\Desktop\FFMPEG Thumbnail\CrystalReport1.rpt";
           
            //try
            //{

            //    SendEmail("v.cortexreportmail" + "@gmail.com", "ipvnipvn", "v.cortexreportmail@gmail.com", "subject", "My message",pFormat,DestinationFilePath1);
            //   // lblError.Text = "Mail sent successfully.";
            //}
            //catch (Exception ex)
            //{
            //   // lblError.Text = ex.Message;
            //}
            ///

            try
            {
                if (firsttimemailsent)
                {
                    myMail.Fields.Add
                   ("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
                    //Use 0 for anonymous
                    myMail.Fields.Add
                    ("http://schemas.microsoft.com/cdo/configuration/sendusername",
                        "v.cortexreportmail@gmail.com");
                    myMail.Fields.Add
                    ("http://schemas.microsoft.com/cdo/configuration/sendpassword",
                         "ipvnipvn");
                    myMail.Fields.Add
                    ("http://schemas.microsoft.com/cdo/configuration/smtpusessl",
                         "true");

                    myMail.From = "v.cortexreportmail@gmail.com";
                    //  myMail.To = "v.cortexreportmail@gmail.com";
                }
                firsttimemailsent = false;
                myMail.To = ToRichTextBox.Text;
                myMail.Subject = SubjectRichTextBox.Text;
                //myMail.BodyFormat ;
                myMail.Body = MessageBodyRichTextBox.Text;

                if (!fileattached)
                {
                    if (DestinationFilePath1.Trim() != "")
                    {
                        //Attachment MyAttachment = new Attachment(DestinationFilePath1);

                        MailAttachment MyAttachment = new MailAttachment(DestinationFilePath1);

                        myMail.Attachments.Add(MyAttachment);
                        // System.Web.Mail.SmtpMail.SmtpServer = "smtp.gmail.com:465";
                        // myMail.Priority = System.Web.Mail.MailPriority.High;
                    }
                }
                    System.Web.Mail.SmtpMail.SmtpServer = "smtp.gmail.com:465";
                    myMail.Priority = System.Web.Mail.MailPriority.High;
                    System.Web.Mail.SmtpMail.Send(myMail);
                    //return true;
                    EmailSentlabel.Visible = true;

                
            }
            catch (Exception ex)
            {
                EmailSentlabel.Visible = true;
                EmailSentlabel.ForeColor = Color.DarkRed;
                EmailSentlabel.Text = "Problem: Report could not send";
            }
        
         
        }
        */
        #endregion email configurations

        #region email cancel and attachment methods
        /*     private void CancelEmailButton_Click(object sender, EventArgs e)
        {
            EmailSentlabel.Visible = false;
            SubjectRichTextBox.Text = "";
            ToRichTextBox.Text = "";
            fileattached = false;
            myMail.Attachments.Clear();
            MessageBodyRichTextBox.Text = "";
            AttachementRichTextBox.Text = "";
            fileattached = false;
            morethanoneattachment = false;

        }

         private void AttachFileButton_Click(object sender, EventArgs e)
        {
            fileattached = true;
            //AttachementRichTextBox.Enabled = true;
            string report;
            try
            {
                
                                              
                // Displays an OpenFileDialog so the user can select a Cursor.
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Cursor Files|*.*";
                openFileDialog1.Title = "Select a File";

                // Show the Dialog.
                // If the user clicked OK in the dialog and
                // a .CUR file was selected, open it.
                //if (openFileDialog1.ShowDialog() == DialogResult.OK)
                //{
                //    // Assign the cursor in the Stream to the Form's Cursor property.
                //    //this.Cursor = new Cursor(openFileDialog1.OpenFile());
                //}\


                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    {
                    //System.IO.StreamReader sr = new
                    //System.IO.StreamReader(openFileDialog1.FileName);
                    //MessageBox.Show(sr.ReadToEnd());
                    report = openFileDialog1.FileName;
                    reportname = openFileDialog1.SafeFileName;
                      //sr.Close();
                    if (morethanoneattachment)
                    {
                        reportname = ", "+ reportname;
                    }
                    }
                    morethanoneattachment = true;
                  //  AttachementRichTextBox.Text=("");            
                    AttachementRichTextBox.Text += reportname;
            }
                // else if ok was not clicked 
                else
                {
                    return;
                }

                // The stream monitor thread needs to know the new streamdir
                try
                {
                    MailAttachment MyAttachment = new MailAttachment(report);

                    myMail.Attachments.Add(MyAttachment);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred report attachment!\n" + ex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Choose report exception.\n" + ex);
            }
        }
    */
        #endregion email

        private void MaximizeWindowCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                string stream = this.vs.path.StreamPath;
                Process myProc = new Process();
                string myCmd = @"C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe";
                string myArgs = "-vvv \"" + stream + "\"";
                ProcessStartInfo myStart = new ProcessStartInfo(myCmd, myArgs);
                myStart.UseShellExecute = false;
                myProc.StartInfo = myStart;
                myProc.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }


        #region Multiple Videos selected - PDF file
        //Create PDF File-Melek
        private bool CreateMultiplePDFFile(string rname, VStream rs, Document doc)
        {
            try
            {
                string reviewmenureportname, pdfFilePath, profiledate, s_profile, s_PMOSscore,
                       s_Trough, s_BadScenes, s_BlankScenes, s_videobitrate,
                       s_comment, s_finaldecision, s_encodingmode, s_resolution, s_processedduration,
                       imageFilePath, logoFilePath, s_overallbitrate, s_path,
                       s_clipname, s_FileSizeinMBString, s_tracks, s_qwarnings, cmt2;
                ListViewItem lvi = new ListViewItem();
                Chunk chnkk = new Chunk("\n");
                iTextSharp.text.Image jpg;
                //if (((GenerateReportResultsViewClicked == true) || (GenerateReportButtonReviewViewClicked==true)) && (reportdatedisplayed == false))
                if (GenerateReportButtonReviewViewClicked == true && reportdatedisplayed == false) //Many clips selected many reports generated
                {
                    reportdatedisplayed = true;
                    reportname = rs.StreamFileName;

                    pdfFilePath = RepDirTextBox.Text + reportname + ".pdf";
                    PdfWriter wri = PdfWriter.GetInstance(doc, new FileStream(pdfFilePath, FileMode.Create));
                    doc.Open();//Open Document to write
                }


                imageFilePath = AppDomain.CurrentDomain.BaseDirectory + "Images\\" + rs.StreamName + ".jpg";

                if (File.Exists(imageFilePath))
                {
                    //jpg= global::QA.Properties.Resources.Nothumb;                  
                    jpg = iTextSharp.text.Image.GetInstance(imageFilePath);
                }
                else //this case was added for the files which have< 2 seconds duration but with the latest configuration these files are not supported  
                {
                    jpg = iTextSharp.text.Image.GetInstance(AppDomain.CurrentDomain.BaseDirectory + "Nothumb.jpg");
                }

                if (!docopened)
                {

                    //string logoFilePath =AppDomain.CurrentDomain.BaseDirectory + "Images\\Path 1 logo.JPG";
                    logoFilePath = AppDomain.CurrentDomain.BaseDirectory + "Path 1 logo.JPG";

                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoFilePath);
                    logo.SpacingAfter = 3f;//1f
                    //logo.ScaleToFit(100f, 80f);
                    logo.Alignment = Element.ALIGN_RIGHT;
                    //logo.Url("http://www.path1.com");
                    logo.SpacingAfter = 30f;
                    doc.Add(logo);
                    //doc.Add(chnkk);
                    //Paragraph vcortextitle = new Paragraph(("v.Cortex Report", FontFactory.GetFont(FontFactory.HELVETICA, 12, iTextSharp.text.Font.BOLD));
                    iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA, 12, iTextSharp.text.Font.BOLD);
                    Paragraph vcortextitle = new Paragraph("                                               v.Cortex Report", font);
                    vcortextitle.Alignment = Element.ALIGN_CENTER;
                    doc.Add(new Phrase(vcortextitle));
                    //vcortextitle.Alignment = Element.ALIGN_CENTER;
                    //Chunk ckk = new Chunk("                                                   v.Cortex Report", FontFactory.GetFont(FontFactory.HELVETICA, 12, iTextSharp.text.Font.BOLD));
                    //Paragraph paragraph = new Paragraph("v.Cortex Report");
                    //paragraph.SpacingAfter = 30f;
                    //paragraph.Font = FontFactory.GetFont(FontFactory.HELVETICA, 12, iTextSharp.text.Font.BOLD);
                    //paragraph.Alignment = Element.ALIGN_CENTER;
                    //doc.Add(paragraph);
                    //doc.Add(new Phrase(ckk));

                    //Report name entered by user 

                    if (GenerateReportResultsViewClicked == true)
                    {
                        reviewmenureportname = ReportNameResultsViewtextBox.Text;
                        // GenerateReportResultsViewClicked = false;
                    }
                    else
                    {
                        reviewmenureportname = ReportNametextBoxReviewMenu.Text;
                    }
                    Chunk r_name = new Chunk(reviewmenureportname, FontFactory.GetFont(FontFactory.HELVETICA, 12, iTextSharp.text.Font.BOLD));
                    doc.Add(chnkk);
                    doc.Add(new Phrase(r_name));
                    docopened = true;
                    GenerateReportResultsViewClicked = false;
                    GenerateReportButtonReviewViewClicked = false;
                }

                if (reportdatedisplayed == true)
                {
                    DateTime rep_tim = DateTime.Now;
                    // doc.Add(new Paragraph("Report Created: "));
                    profiledate = "Report Generated: " + rep_tim.ToString();
                    Paragraph parag = new Paragraph(profiledate);
                    doc.Add(parag);
                    reportdatedisplayed = false;
                }
                //else
                //{

                //}

                doc.Add(chnkk);
                //Give space before image
                jpg.SpacingBefore = 200f;
                //Give some space after the image
                //jpg.SpacingAfter = 100f;
                jpg.Alignment = Element.ALIGN_LEFT;
                //Resize image depend upon your need
                //jpg.ScaleToFit(280f, 260f);
                //doc.Add(paragraph); // add paragraph to the document
                doc.Add(jpg); //add an image to the created pdf document
                //Give some space after the image

                // jpg.SpacingAfter = 3f;

                s_clipname = "Clip Name: " + rs.StreamFileName;

                Chunk ckk2 = new Chunk(s_clipname, FontFactory.GetFont(FontFactory.HELVETICA, 12, iTextSharp.text.Font.NORMAL));

                //Color cc = new Color();
                //if (rs.Final_Decision.String == "Fail") { cc = Color.Red; }
                //else cc = Color.Green;
                //BaseColor colr = new BaseColor(cc);
                //if (rs.Final_Decision.String == "Fail") { colr = BaseColor.RED; }
                //else colr = BaseColor.GREEN;
                // ckk.SetBackground(colr);

                doc.Add(new Phrase(ckk2));

                s_finaldecision = "Final decision: " + rs.Final_Decision.String;
                doc.Add(new Paragraph(s_finaldecision));
                s_PMOSscore = "Average PMOS Score: " + rs.avMOS;
                doc.Add(new Paragraph(s_PMOSscore));
                s_profile = "Applied Profile: " + rs.QualityProfile;
                doc.Add(new Paragraph(s_profile));

                // doc.Add(chnkk);
                if (rs.Comment == "")
                    s_comment = "N/A";
                else
                    s_comment = rs.Comment;

                cmt2 = "User Decision Comment: " + s_comment;
                doc.Add(new Paragraph(cmt2));

                //  doc.Add(chnkk);

                #region alerts list
                if (rs != null && ((rs.sum.AggAlertScnA.Count != 0) || (rs.sum.AlertScnA.Count != 0) || (rs.sum.AggBSA.Count != 0)))
                {
                    doc.Add(chnkk);

                    s_qwarnings = "Qaulity Warnings: ";
                    doc.Add(new Paragraph(s_qwarnings));

                    for (int i = 0; i < rs.sum.QualityWarningA.Count; i++)
                    {
                        string w = rs.sum.QualityWarningA[i].Detail;
                        doc.Add(new Paragraph(w));
                    }
                    s_BadScenes = "Bad Percent: " + rs.BadPerc;
                    doc.Add(new Paragraph(s_BadScenes));
                    if ((rs.TroughWarning == "_") || (rs.TroughWarning == "N/A")) { /*s_Trough = "Trough Warning: N/A";*/ } else { s_Trough = "Trough Warning: " + rs.TroughWarning; doc.Add(new Paragraph(s_Trough)); }


                    if ((rs.BlankWarning == "_") || (rs.BlankWarning == "N/A")) { /*s_BlankScenes = "Blank Scenes Warning: N/A";*/ } else { s_BlankScenes = "Blank Scenes Warning: " + rs.BlankWarning; doc.Add(new Paragraph(s_BlankScenes)); }


                    doc.Add(chnkk);

                    if (rs.sum.AlertScnA.Count != 0)
                    {
                        Paragraph paragraph = new Paragraph("Alerts List");
                        doc.Add(paragraph);
                        string title = "#      Type                 Start                  Duration             Comment";
                        doc.Add(new Paragraph(title));

                        List<AlertScene> tempa = new List<AlertScene>();
                        AlertScene AlScn;

                        if (rs.sum.qualityProfile.aggregatedBadSceneDetector.Enabled == true)
                            if (ShowAggBSBox.Checked == true)
                                tempa = rs.sum.AggAlertScnA;
                            else
                                tempa = rs.sum.AlertScnA;
                        else
                            tempa = rs.sum.AlertScnA;

                        for (int i = 0; i < tempa.Count; i++)
                        {
                            ListViewItem lviItem = new ListViewItem();

                            try
                            {
                                AlScn = (AlertScene)tempa[i];
                                lviItem.Text = Convert.ToString(i);
                                lviItem.Tag = AlScn;
                                lviItem.SubItems.Add(AlScn.Type);
                                lviItem.SubItems.Add(Global.frameToDurationString((int)AlScn.Start, rs.cv.format.frameRate, true));
                                lviItem.SubItems.Add(Global.frameToDurationString((int)AlScn.DurationInFrames, rs.cv.format.frameRate, true));

                                if ((AlScn.Type == "Bad Scene") || (AlScn.Type == "bad scene"))
                                {
                                    double mosav = Math.Round(AlScn.Value, 2);
                                    if (mosav > rs.sum.qualityProfile.badSceneDetector.MosThreshold) mosav = rs.sum.qualityProfile.badSceneDetector.MosThreshold;
                                    lviItem.SubItems.Add("mos=" + Convert.ToString(Math.Round(mosav, 2)));
                                    string bsinfo = lviItem.Text.ToString() + "      " + AlScn.Type + "       " + Global.frameToDurationString((int)AlScn.Start, rs.cv.format.frameRate, true) + "       " + Global.frameToDurationString((int)AlScn.DurationInFrames, rs.cv.format.frameRate, true) + "        MOS: " + Convert.ToString(Math.Round(mosav, 2));
                                    doc.Add(new Paragraph(bsinfo));
                                }
                                else if (AlScn.Type == "Agg Bad Scene")
                                {
                                    double mosav = Math.Round(AlScn.Value, 2);
                                    string absinfo = lviItem.Text.ToString() + "   " + AlScn.Type + "       " + Global.frameToDurationString((int)AlScn.Start, rs.cv.format.frameRate, true) + "       " + Global.frameToDurationString((int)AlScn.DurationInFrames, rs.cv.format.frameRate, true) + "        MOS: " + Convert.ToString(Math.Round(mosav, 2));
                                    doc.Add(new Paragraph(absinfo));

                                }
                                else if (AlScn.Type == "Black")
                                {
                                    double mosav = Math.Round(AlScn.Value, 2);
                                    string absinfo = lviItem.Text.ToString() + "         " + AlScn.Type + "       " + Global.frameToDurationString((int)AlScn.Start, rs.cv.format.frameRate, true) + "       " + Global.frameToDurationString((int)AlScn.DurationInFrames, rs.cv.format.frameRate, true) + "      Int: " + Convert.ToString(Math.Round(mosav, 2));
                                    doc.Add(new Paragraph(absinfo));

                                }
                                else if (AlScn.Type == "White")
                                {

                                    double mosav = Math.Round(AlScn.Value, 2);
                                    string absinfo = lviItem.Text.ToString() + "            " + AlScn.Type + "       " + Global.frameToDurationString((int)AlScn.Start, rs.cv.format.frameRate, true) + "       " + Global.frameToDurationString((int)AlScn.DurationInFrames, rs.cv.format.frameRate, true) + "      Int: " + Convert.ToString(Math.Round(mosav, 2));
                                    doc.Add(new Paragraph(absinfo));

                                }

                                else if (AlScn.Type == "Frozen")
                                {

                                    doc.Add(new Paragraph(lviItem.Text.ToString()));

                                }

                                else if (AlScn.Type == "Trough")
                                {

                                    double mosav = Math.Round(AlScn.Value, 2);
                                    string absinfo = lviItem.Text.ToString() + "      " + AlScn.Type + "             " + Global.frameToDurationString((int)AlScn.Start, rs.cv.format.frameRate, true) + "       " + Global.frameToDurationString((int)AlScn.DurationInFrames, rs.cv.format.frameRate, true) + "        Int: " + Convert.ToString(Math.Round(mosav, 2));
                                    doc.Add(new Paragraph(absinfo));

                                }
                                else if (AlScn.Type == "Field order")
                                {
                                    string commnt = "                                                                     " + AlScn.Comment;
                                    doc.Add(new Paragraph(commnt));

                                }
                            }

                            catch (Exception ex)
                            {
                                Console.WriteLine("Problem updating the badscene arraylist view\n" + ex);
                                return false;
                            }
                        }
                    }

                }
                #endregion end of alerts list

                s_path = "Clip Path: " + rs.path.StreamPath;
                s_FileSizeinMBString = "Clip Size : " + rs.format.overall.FileSizeinMBString;
                s_encodingmode = "Video Specs " + rs.VideoSpecsSummaryString;
                s_processedduration = "Processed Video Duration (hr:min:sec): " + rs.ProcessedDurationString;
                s_tracks = "Tracks : " + rs.format.overall.nbVideoStreams + " video, " + rs.format.overall.nbAudioStreams + " audio.";


                //s_BlankScenes = "Blank Scenes Warning: " + rs.BlankWarning;
                s_resolution = "Resolution Label: " + rs.ResolutionLabel;
                s_overallbitrate = "Overall Bit Rate: " + rs.format.OverallBitrateString;
                s_videobitrate = "Video Bit Rate: " + rs.VideoBitrateReported + " Mbps";
                //s_totaldecoderereports = "Decoder: "+rs.TotalDecoderWarnings() + " decoding warnings reported.";
                //s_demuxerrors ="Container: "+ rs.demuxReport.ErrorPackets + " packet warnings.";
                //s_packetloss = "Packet loss: Reconstructed frames : " + rs.videoModelCheck.recStats.affectedFrames + " ( " + Math.Round((double)rs.videoModelCheck.recStats.affectedFrames / (double)rs.videoModelCheck.processedDurationInFrames, 2) + "%).";

                doc.Add(chnkk);
                doc.Add(new Paragraph(s_path));
                doc.Add(new Paragraph(s_encodingmode));
                doc.Add(new Paragraph(s_tracks));

                doc.Add(new Paragraph(s_FileSizeinMBString));

                doc.Add(new Paragraph(s_overallbitrate));
                doc.Add(new Paragraph(s_videobitrate));
                //doc.Add(new Paragraph(s_resolution));
                doc.Add(new Paragraph(s_processedduration));


                doc.Add(chnkk);
                //Paragraph parend = new Paragraph("------------------------------------------------------------------------------------------------------------------------ ");
                //doc.Add(parend);
                //doc.Add(new Paragraph(s_packetloss));
                //doc.Add(new Paragraph(s_totaldecoderereports));
                //doc.Add(new Paragraph(s_demuxerrors));
                //Paragraph par = new Paragraph(" ");
                //par.SpacingBefore = 10f;

                //////////////   par.Alignment = Element.ALIGN_CENTER;
                ////////////Anchor anchor1 = new Anchor("Help", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 255)));
                ////////////anchor1.Reference = "http://www.path1.com";
                ////////////anchor1.Name = "left";
                ////////////par.Add(anchor1);
                ////////////par.Add("/");

                ////////////Anchor anchor2 = new Anchor("Contact", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 255)));
                ////////////anchor2.Reference = "http://www.path1.com";
                ////////////anchor2.Name = "middle";
                ////////////par.Add(anchor2);
                ////////////par.Add("/");

                ////////////Anchor anchor3 = new Anchor("Whatever we want to show", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 255)));
                ////////////anchor3.Reference = "http://www.path1.com";
                ////////////anchor3.Name = "middle";
                ////////////par.Add(anchor3);
                ////////////par.Alignment = Element.ALIGN_CENTER;
                ////////////doc.Add(par);

                ////////////doc.AddAuthor("Path1");
                ////////////doc.AddTitle("v.Cortex Report");
                ////////////doc.AddSubject("This report is created by v.Cortex");

                // par.Add(new Chunk("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n"));
                //  doc.Add(par);
                //Anchor anchor2 = new Anchor("please jump to a local destination", iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 12, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 0, 255)));
                //anchor2.Reference = "#top";
                //doc.Add(anchor2);

                //Phrase p1 = new Phrase();
                //Chunk ck2 = new Chunk("Stream Info");
                //p1.Add(ck2);
                //Chunk ck3 = new Chunk("Video Info");
                //p1.Add(ck3);
                //Chunk ck4 = new Chunk("Audio Info");
                //p1.Add(ck4);
                //string sFilePath = AppDomain.CurrentDomain.BaseDirectory + "Images\\FiveMinAd.jpg";
                //iTextSharp.text.Image sJPG = iTextSharp.text.Image.GetInstance(sFilePath);
                //sJPG.Alignment = Element.ALIGN_CENTER;
                //Chunk ck5 = new Chunk(sJPG, 200, 100);
                //p1.Add(ck);
                //doc.Add(p1);

                //ColumnText ct = new ColumnText(wri.DirectContent);
                //ct.SetSimpleColumn(doc.Left, doc.Bottom, doc.Right, doc.Top);
                //for (int i = 0; i < 10; i++)
                //{
                //    ct.AddElement(new Phrase(i + ". hello"));
                //}

                //int status = ct.Go();
                //while (ColumnText.HasMoreText(status))
                //{
                //    doc.NewPage();
                //    //ct.YLine(doc.Top());
                //    status = ct.Go();
                //}
                //  iTextSharp.text.pdf.


            }
            catch (DocumentException docEx)
            {
                //handle pdf document exception if any
                MessageBox.Show(docEx.Message);
                return false;
            }
            catch (IOException ioEx)
            {
                // handle IO exception
                MessageBox.Show(ioEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                // ahndle other exception if occurs
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //Close document and write

            }
            return true;
        }
        //End of PDF file
        #endregion Review Menu PDF file


        private void button8_Click(object sender, EventArgs e)
        {
            SaveReportAstextBox.Text = "";
            ReportNameResultsViewtextBox.Text = "";
        }


        private void ClearButtonReviewMenu_Click(object sender, EventArgs e)
        {
            ReportNametextBoxReviewMenu.Text = "";
        }

        private void SlowdownButton_Click(object sender, EventArgs e)//Melek
        {
            if (this.currentState != PlayState.Paused)
                PauseClip();
            SetRate(10.5);
            // playbutton_Click("", EventArgs.Empty);
            if (vs.path.StreamIsAvailable && vs.Final_Decision.String != "Not made") //Melek- play the video if the decision is different than not made 
            {
                PlaybackState(true);
                OpenStream(vs.path.StreamPath);
            }

        }

        private void ResDirTextBox_TextChanged(object sender, EventArgs e)
        {


        }

        private void StreamFoldersBox_CellEnter(object sender, DataGridViewCellEventArgs e) //Melek - combobox 3 times click disable
        {
            StreamFoldersBox.BeginEdit(false);
            if (e.ColumnIndex == 4 || e.ColumnIndex == 5)// the combobox column index
            {
                if (this.StreamFoldersBox.EditingControl != null
                    && this.StreamFoldersBox.EditingControl is ComboBox)
                {
                    ComboBox cmb = this.StreamFoldersBox.EditingControl as ComboBox;
                    cmb.DroppedDown = true;
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {            
           ChooseRepDir();
        }

        private void Titlelabel_Click(object sender, EventArgs e)
        {

        }
          
    }

}
