﻿using FlexConfirmMail.Dialog;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Interop;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace FlexConfirmMail
{
    public partial class ThisAddIn
    {

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            this.Application.ItemSend += new Microsoft.Office.Interop.Outlook.ApplicationEvents_11_ItemSendEventHandler(ThisAddIn_ItemSend);
            ShowBanner();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        private void ThisAddIn_ItemSend(object Item, ref bool Cancel)
        {
            Outlook.MailItem mail = (Outlook.MailItem)Item;

            // Some users reported that Intel Graphic + Win10 causes
            // a blank screen. Diable Hardware Accerelation.
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            // First of all, enable Cancel flag. This makes Outlook
            // NOT to send the mail even if something goes awry.
            Cancel = true;

            try
            {
                if (DoCheck(mail)) {
                    Cancel = false;
                    QueueLogger.Log("Check finished [send=yes]");
                }
                else
                {
                    QueueLogger.Log("Check finished [send=no]");
                }
            }
            catch (System.Exception e)
            {
                QueueLogger.Log(e);
            }

            // Never send a message with a debug build!
#if DEBUG
            Cancel = true;
#endif
        }

        private void ShowBanner()
        {
            try
            {
                QueueLogger.Log($"Start {Global.AppName}");
                QueueLogger.Log($"* Version {Global.Version} {Global.Edition}");
                QueueLogger.Log($"* Outlook {Application.Version}");
                QueueLogger.Log($"* {System.Environment.OSVersion.VersionString}");
            }
            catch (System.Exception e)
            {
                QueueLogger.Log(e);
            }
        }

        private bool DoCheck(Outlook.MailItem mail)
        {
            QueueLogger.Log($"===== Start DoCheck() ======");

            Config config = new Config();

            if (Global.EnableGPO)
            {
                config.Merge(Loader.LoadFromReg(RegistryPath.DefaultPolicy));
            }
            config.Merge(Loader.LoadFromDir(StandardPath.GetUserDir()));

            MainDialog mainDialog = new MainDialog(config, mail);
            if (mainDialog.SkipConfirm())
            {
                return true;
            }

            if (mainDialog.ShowDialog() == true)
            {
                if (!config.CountEnabled)
                {
                    return true;
                }

                if (new CountDialog(config).ShowDialog() == true)
                {
                    return true;
                }
            }
            return false;
        }

        protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new Ribbon();
        }

        #region VSTO で生成されたコード

        /// <summary>
        /// デザイナーのサポートに必要なメソッドです。
        /// このメソッドの内容をコード エディターで変更しないでください。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
