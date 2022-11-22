//*********************************************************************
//xToolkit
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Xarial.XToolkit.Reflection;

namespace Xarial.XToolkit.Wpf.Dialogs
{
    internal class AdvancedFolderBrowseDialog : IDisposable
    {
        private static readonly MethodInfo m_CreateVistaDialogMethod;
        private static readonly MethodInfo m_OnBeforeVistaDialogMethod;
        private static readonly MethodInfo m_GetOptionsMethod;
        private static readonly MethodInfo m_SetOptionsMethod;

        private static readonly MethodInfo m_AdviceMethod;
        private static readonly MethodInfo m_ShowMethod;
        private static readonly MethodInfo m_UnadviceMethod;

        private static readonly ConstructorInfo m_VistaDialogEventsConstructor;

        private static object CreateVistaDialog(OpenFileDialog dlg) => TypeExtension.InvokeMethod<object>(dlg, m_CreateVistaDialogMethod);
        private static void OnBeforeVistaDialog(OpenFileDialog dlg, object vistaDlg) => TypeExtension.InvokeMethod(dlg, m_OnBeforeVistaDialogMethod, vistaDlg);
        private static uint GetOptions(OpenFileDialog dlg) => TypeExtension.InvokeMethod<uint>(dlg, m_GetOptionsMethod);
        private static void SetOptions(object vistaDlg, uint opts) => TypeExtension.InvokeMethod(vistaDlg, m_SetOptionsMethod, opts);
        private static object NewVistaDialogEvents(OpenFileDialog dlg) => m_VistaDialogEventsConstructor.Invoke(new object[] { dlg });
        private static void Advice(object vistaDlg, object vistaDlgEvents, out uint cookie)
        {
            var adviceParams = new object[] { vistaDlgEvents, 0u };

            TypeExtension.InvokeMethod(vistaDlg, m_AdviceMethod, adviceParams);

            cookie = (uint)adviceParams[1];
        }
        private static int Show(object vistaDlg, IntPtr owner) => TypeExtension.InvokeMethod<int>(vistaDlg, m_ShowMethod, owner);
        private static void Unadvise(object vistaDlg, uint cookie) => TypeExtension.InvokeMethod(vistaDlg, m_UnadviceMethod, cookie);

        static AdvancedFolderBrowseDialog()
        {
            var systemWindowsFormsAssm = typeof(Form).Assembly;

            var fileDialogClrType = systemWindowsFormsAssm.GetType("System.Windows.Forms.FileDialogNative+IFileDialog");
            var vistaDlgEventsClrType = systemWindowsFormsAssm.GetType("System.Windows.Forms.FileDialog+VistaDialogEvents");

            m_CreateVistaDialogMethod = typeof(OpenFileDialog).FindMethod("CreateVistaDialog");
            m_OnBeforeVistaDialogMethod = typeof(FileDialog).FindMethod("OnBeforeVistaDialog");
            m_GetOptionsMethod = typeof(FileDialog).FindMethod("GetOptions");
            m_SetOptionsMethod = fileDialogClrType.FindMethod("SetOptions");

            m_AdviceMethod = fileDialogClrType.FindMethod("Advise");
            m_ShowMethod = fileDialogClrType.FindMethod("Show");
            m_UnadviceMethod = fileDialogClrType.FindMethod("Unadvise");

            m_VistaDialogEventsConstructor = vistaDlgEventsClrType.GetConstructor(new Type[] { typeof(FileDialog) });
        }

        private const int VISTA_MAJOR_VERSION = 6;

        private readonly OpenFileDialog m_Dlg;
        private readonly FolderBrowserDialog m_FallbackDlg;

        public bool Multiselect
        {
            get => m_Dlg.Multiselect;
            set => m_Dlg.Multiselect = value;
        }

        public string InitialDirectory
        {
            get => m_Dlg.InitialDirectory;
            set => m_Dlg.InitialDirectory = string.IsNullOrEmpty(value) ? Environment.CurrentDirectory : value;
        }

        public string Title
        {
            get => m_Dlg.Title;
            set => m_Dlg.Title = value ?? "Select a folder";
        }

        public string FolderName => m_Dlg.FileName;

        public string[] FolderNames => m_Dlg.FileNames;

        public AdvancedFolderBrowseDialog()
        {
            m_Dlg = new OpenFileDialog()
            {
                Filter = "Folders|" + Environment.NewLine,
                AddExtension = false,
                CheckFileExists = false,
                DereferenceLinks = true,
            };

            m_FallbackDlg = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
            };
        }

        public DialogResult ShowDialog() => ShowDialog(IntPtr.Zero);

        public DialogResult ShowDialog(IntPtr owner)
        {
            DialogResult res;

            if (!TryShowVistaDialog(owner, out res))
            {
                m_FallbackDlg.Description = Title;
                m_FallbackDlg.SelectedPath = InitialDirectory;

                res = m_FallbackDlg.ShowDialog(new Win32Window(owner));

                m_Dlg.FileName = m_FallbackDlg.SelectedPath;
            }

            return res;
        }

        private bool TryShowVistaDialog(IntPtr owner, out DialogResult res)
        {
            if (Environment.OSVersion.Version.Major >= VISTA_MAJOR_VERSION)
            {
                try
                {
                    const int S_OK = 0;
                    const int FOS_PICKFOLDERS = 0x00000020;

                    var vistaDlg = CreateVistaDialog(m_Dlg);
                    OnBeforeVistaDialog(m_Dlg, vistaDlg);
                    var opts = GetOptions(m_Dlg);
                    SetOptions(vistaDlg, opts | FOS_PICKFOLDERS);

                    var vistaDlgEvents = NewVistaDialogEvents(m_Dlg);

                    Advice(vistaDlg, vistaDlgEvents, out var cookie);

                    try
                    {
                        var isOk = Show(vistaDlg, owner) == S_OK;

                        res = isOk ? DialogResult.OK : DialogResult.Cancel;
                        return true;
                    }
                    finally
                    {
                        Unadvise(vistaDlg, cookie);
                    }
                }
                catch
                {
                    res = DialogResult.Cancel;
                    return false;
                }
            }
            else
            {
                res = DialogResult.Cancel;
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            m_Dlg.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
