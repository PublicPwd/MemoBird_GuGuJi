﻿using MemoBird_GuGu.Classes;
using MemoBird_GuGu.Utils;
using MemoBird_GuGu.Utils.WebApi;
using MemoBird_GuGu.Windows;
using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MemoBird_GuGu.Pages
{
    public partial class Page_Text : Page
    {
        public Page_Text()
        {
            InitializeComponent();
            RichTextBox_Content.Focus();
            ComboBox_DeviceList.ItemsSource = DeviceList.Details;
            FillFontFamily();
            FillFontSize();
        }

        #region Private Function

        /// <summary>
        /// 打印文字
        /// </summary>
        private void PrintPaper()
        {
            try
            {
                string memobirdID = ComboBox_DeviceList.SelectedValue.ToString();
                string str;
                string content;

                bool editModeIsNormal = ComboBox_EditMode.SelectedIndex == 0 ? true : false;
                if (editModeIsNormal)
                {
                    content = "T:" + Convert.ToBase64String(Encoding.Default.GetBytes(new TextRange(RichTextBox_Content.Document.ContentStart, RichTextBox_Content.Document.ContentEnd).Text));
                    str = WebApiHelper.PrintPaper(content, memobirdID);
                }
                else
                {
                    TextRange textRange = new TextRange(RichTextBox_Content.Document.ContentStart, RichTextBox_Content.Document.ContentEnd);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        textRange.Save(stream, DataFormats.Xaml);
                        content = Parsing.XamlToHtml(Encoding.UTF8.GetString(stream.ToArray()));
                        content = Convert.ToBase64String(Encoding.Default.GetBytes(content));
                    }
                    str = WebApiHelper.PrintPaperFromHtml(content, memobirdID);
                    content = "T:" + Convert.ToBase64String(Encoding.Default.GetBytes(textRange.Text));
                }

                if (Parsing.GetValueFromJsonString(str, "showapi_res_code") == "1")
                {
                    FileX.SaveHistory(memobirdID, content);
                }
                else
                {
                    MessageBox.Show(FindResource("printfail").ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                new TextRange(RichTextBox_Content.Document.ContentStart, RichTextBox_Content.Document.ContentEnd).Text = string.Empty;
                GC.Collect();
            }
        }

        /// <summary>
        /// 往下拉框中填充字体
        /// </summary>
        private void FillFontFamily()
        {
            var fontFamily = new InstalledFontCollection().Families;
            fontFamily.ToList().ForEach(f =>
            {
                ComboBox_FontFamily.Items.Add(f.Name);
            });
        }

        /// <summary>
        /// 往下拉框中填充字体大小
        /// </summary>
        private void FillFontSize()
        {
            for (int i = 15; i < 385; i++)
            {
                ComboBox_FontSize.Items.Add(i);
            }
        }

        #endregion

        #region Event Handlers

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBox_DeviceList.Items.Count == 0)
            {
                MessageBox.Show(FindResource("pleaseadddevice").ToString());
                return;
            }
            if (new TextRange(RichTextBox_Content.Document.ContentStart, RichTextBox_Content.Document.ContentEnd).Text.Length == 0)
            {
                MessageBox.Show(FindResource("pleaseaddcontent").ToString());
                return;
            }
            new Window_Tip(FindResource("printing").ToString()).Show();
            PrintPaper();
        }

        private void ComboBox_EditMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_EditMode.SelectedIndex == 0)
            {
                Grid_Advance.Visibility = Visibility.Hidden;
                RichTextBox_Content.SelectAll();
                RichTextBox_Content.Selection.ClearAllProperties();
            }
            else
            {
                Grid_Advance.Visibility = Visibility.Visible;
            }
        }

        private void ComboBox_FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RichTextBox_Content.Selection.IsEmpty)
            {
                return;
            }
            RichTextBox_Content.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, ComboBox_FontFamily.SelectedItem.ToString());
        }

        private void ComboBox_FontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RichTextBox_Content.Selection.IsEmpty)
            {
                return;
            }
            RichTextBox_Content.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, double.Parse(ComboBox_FontSize.SelectedItem.ToString()));
        }

        #endregion
    }
}