﻿using FlexConfirmMail.Config;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace FlexConfirmMail.Dialog
{
    public partial class MainDialog : Window
    {
        private Outlook.MailItem _mail;
        private ConfigData _config;

        public MainDialog()
        {
            InitializeComponent();
        }

        public MainDialog(ConfigData config, Outlook.MailItem mail)
        {
            InitializeComponent();
            _config = config;
            _mail = mail;
            LoadMail(mail)
        }

        private void LoadMail(Outlook.MailItem mail)
        {
            var trusted = new List<RecipientInfo>();
            var ext = new List<RecipientInfo>();

            foreach (Outlook.Recipient recp in mail.Recipients)
            {
                HashSet<string> hsTrusted = _config.GetHashSet(ConfigFile.TrustedDomains);
                RecipientInfo info = new RecipientInfo(recp);
                if (hsTrusted.Contains(info.Domain) || info.Domain == RecipientInfo.DOMAIN_EXCHANGE)
                {
                    trusted.Add(info);
                }
                else
                {
                    ext.Add(info);
                }
            }
            RenderAddressList(trusted, true);
            RenderAddressList(ext, false);

            var all = trusted.Concat(ext).ToList();
            CheckSafeBcc(all);
            CheckUnsafeDomains(all);
            CheckUnsafeFiles(mail);

            foreach (Outlook.Attachment item in mail.Attachments)
            {
                spFile.Children.Add(GetCheckBox($"[添付ファイル] {item.FileName}", item.FileName));
            }

            /* Show the subject string in title bar */
            Title = $"{mail.Subject} - FlexConfirmMail";
        }

        private void CheckUnsafeDomains(List<RecipientInfo> list)
        {
            HashSet<string> hsUnsafe = _config.GetHashSet(ConfigFile.UnsafeDomains);
            HashSet<string> done = new HashSet<string>();

            foreach (RecipientInfo info in list)
            {
                if (done.Contains(info.Domain))
                {
                    continue;
                }

                if (hsUnsafe.Contains(info.Domain))
                {
                    spFile.Children.Add(GetWarnCheckBox(
                        $"[警告] 注意が必要なドメイン（{info.Domain}）が宛先に含まれています。",
                        "このドメインは誤送信の可能性が高いため、再確認を促す警告を出してします。"
                    ));
                }
                done.Add(info.Domain);
            }
        }

        private void CheckUnsafeFiles(Outlook.MailItem mail)
        {
            HashSet<string> unsafeFiles = _config.GetHashSet(ConfigFile.UnsafeFiles);

            foreach (Outlook.Attachment item in mail.Attachments)
            {
                foreach (string word in unsafeFiles)
                {

                    if (item.FileName.Contains(word))
                    {
                        spFile.Children.Add(GetWarnCheckBox(
                            $"[警告] 注意が必要なファイル名（{word}）が含まれています。",
                            $"添付ファイル「{item.FileName}」に注意が必要な単語が含まれているため、再確認を促す警告を出しています。"
                        ));
                        break;
                    }

                }
            }
        }

        private void CheckSafeBcc(List<RecipientInfo> list)
        {
            if (!_config.GetBool(ConfigOption.SafeBccEnabled))
            {
                return;
            }

            int threshold = _config.GetInt(ConfigOption.SafeBccThreshold);
            if (threshold < 1)
            {
                return;
            }

            var domains = new HashSet<string>();
            foreach (RecipientInfo info in list)
            {
                if (info.IsSMTP && info.Type != "Bcc" && !domains.Contains(info.Domain))
                {
                    domains.Add(info.Domain);
                }
            }
            if (domains.Count >= threshold)
            {
                spFile.Children.Add(GetWarnCheckBox(
                    $"[警告] To・Ccに{threshold}件以上のドメインが含まれています。",
                    @"宛先に多数のドメインが検知されました。
ToおよびCcに含まれるメールアドレスはすべての受取人が確認できるため、
アナウンスなどを一斉送信する場合はBccを利用して宛先リストを隠します。"
                ));
            }
        }

        private void RenderAddressList(List<RecipientInfo> list, bool trusted)
        {
            var domains = new HashSet<string>();
            var sp = trusted ? spTrusted : spExt;
            CheckBox cb;

            list.Sort();

            foreach (RecipientInfo info in list)
            {
                if (!domains.Contains(info.Domain))
                {
                    sp.Children.Add(GetDomainLabel(info.Domain));
                    domains.Add(info.Domain);
                }
                if (trusted)
                {
                    cb = GetCheckBox($"{info.Type,-3}: {info.Address}", info.Help);
                }
                else
                {
                    cb = GetWarnCheckBox($"{info.Type,-3}: {info.Address}", info.Help);
                }
                sp.Children.Add(cb);

            }
        }

        private Label GetDomainLabel(string title)
        {
            Label label = new Label();
            label.Content = title;
            label.FontWeight = FontWeights.Bold;
            label.Padding = new Thickness(0, 4, 0, 4);
            return label;
        }

        private CheckBox GetCheckBox(string title, string help)
        {
            CheckBox cb = new CheckBox();
            cb.Content = title;
            cb.ToolTip = help;
            cb.Margin = new Thickness(7, 2, 0, 2);
            cb.Click +=  CheckBox_Click;
            cb.MouseEnter += CheckBox_MouseEnter;
            cb.MouseLeave += CheckBox_MouseLeave;
            return cb;
        }

        private CheckBox GetWarnCheckBox(string title, string help)
        {
            CheckBox cb = GetCheckBox(title, help);
            cb.Foreground = System.Windows.Media.Brushes.Firebrick;
            cb.FontWeight = FontWeight.FromOpenTypeWeight(500);
            return cb;
        }

        private static bool IsAllChecked(StackPanel sp)
        {
            foreach (UIElement e in sp.Children)
            {
                if (e is CheckBox && ((CheckBox)e).IsChecked != true)
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckBox_MouseEnter(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Foreground == System.Windows.Media.Brushes.Firebrick)
            {
                cb.Foreground = System.Windows.Media.Brushes.RosyBrown;
            }
            else
            {
                cb.Foreground = System.Windows.Media.Brushes.SteelBlue;
            }
        }

        private void CheckBox_MouseLeave(object sender, RoutedEventArgs e)
        {

            CheckBox cb = (CheckBox)sender;
            if (cb.Foreground == System.Windows.Media.Brushes.RosyBrown)
            {
                cb.Foreground = System.Windows.Media.Brushes.Firebrick;
            }
            else
            {
                cb.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ButtonOK.IsEnabled = IsAllChecked(spTrusted) && IsAllChecked(spExt) && IsAllChecked(spFile);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
