﻿/*
 * The BSD license
 * Copyright (c) 2014, lgchan
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of lgchan, nor the names of its contributors may be 
 *       used to endorse or promote products derived from this software without
 *       specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE REGENTS AND CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;
using System.Security;

namespace SuperHashingPasswordGenerator
{

    /// <summary>
    /// my salt wrapper
    /// </summary>
    public class SHPGSalt
    {
        public string Alias { get; set; }
        public string SaltString { get; set; }
        public bool OnceOnly { get; set; }

        public SHPGSalt()
        {
            this.Alias = "";
            this.SaltString = "";
            this.OnceOnly = false;
        }

        public SHPGSalt(string alias, string salt, bool once)
        {
            this.Alias = alias;
            this.SaltString = salt;
            this.OnceOnly = once;
        }

        public SimpleSaltAppender GetSaltAppender()
        {
            return new SimpleSaltAppender(this.SaltString, this.OnceOnly);
        }
    }

    public class SHPGSalts : List<SHPGSalt>
    {
        public SHPGSalts()
        {
        }
        public new void Add(SHPGSalt salt)
        {
            base.Add(salt);
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool windowInitilized = false;
        private List<SHPGSalt> salts = new List<SHPGSalt>();

        public MainWindow()
        {
            InitializeComponent();
            windowInitilized = true;
            var versioninfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Title = this.statusTextBlock.Text = "SHPG by logchan ver " + String.Format("{0}.{1}", versioninfo.ProductMajorPart, versioninfo.ProductMinorPart);

            salts.Add(new SHPGSalt("default", "", false));
            this.saltDataGrid.DataContext = salts;
        }

        private void doHashButton_Click(object sender, RoutedEventArgs e)
        {
            ProceedHashing();
        }

        private bool GetHashingCount(out int hashingCount)
        {
            hashingCount = 0;
            if (hashFixedNumberRadio.IsChecked.HasValue && hashFixedNumberRadio.IsChecked.Value)
            {
                return Int32.TryParse(this.hashingTimesBox.Text, out hashingCount);
            }
            else
            {
                int strlen = this.stringLengthToCalculateBox.Text.Length;

                int div = 1;
                if (Int32.TryParse(this.stringLengthDivisor.Text, out div))
                {
                    if (div >= 1)
                    {
                        hashingCount = strlen / div;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private void ProceedHashing()
        {
            // check hashing count
            int hashingcount = 0;
            if (!GetHashingCount(out hashingcount))
            {
                MessageBox.Show("The number you entered is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (hashingcount <= 0)
            {
                MessageBox.Show("No, you can not hash zero or negative times.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (hashingcount > 100)
            {
                MessageBox.Show("Please don't hash so many times. It is meaningless.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            HashingPasswordGenerator hpg = new HashingPasswordGenerator(new MD5Hasher(), new MixUpperLowerCasePostHashing());

            foreach (SHPGSalt salt in salts)
            {
                hpg.AddSaltProcessor(salt.GetSaltAppender());
            }

            hpg.TotalHashingTimes = (UInt32)hashingcount;

            string hash, posthash;
            hpg.DoHashing(secretBox.Password, out hash, out posthash);

            this.hashingResultBox.Text = hash;
            this.hashingResultUpperCasedBox.Text = hash.ToUpper();
            this.hashingResultMixedCasedBox.Text = posthash;

            SetStatus(String.Format("Hashed {0} time(s) successfully.", hashingcount), Colors.Green);
        }

        private void ShowFeatureUnavailableMessageBox()
        {
            MessageBox.Show("Sorry, this feature will be added in the future.\n(It's done when it's done.)", "Feature Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetStatus(String text, Color color)
        {
            this.statusTextBlock.Text = text;
            this.statusTextBlock.Foreground = new SolidColorBrush(color);
        }

        private void exportSaltsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureUnavailableMessageBox();
        }

        private void importSaltsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureUnavailableMessageBox();
        }

        private void CopyHashWithNotification(TextBox textBox, int start = 0, int length = 0)
        {
            if (length <= 0) length = textBox.Text.Length;
            else if (start + length > textBox.Text.Length)
            {
                MessageBox.Show("Hash copy failed: requiring longer text than hashing result");
                return;
            }

            textBox.Focus();
            textBox.Select(start, length);

            if (CopyHash(textBox, start, length))
            {
                MessageBox.Show("Hash copied to clipboard.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to copy hash to clipboard." + Environment.NewLine + "(Luckily, I have selected the hash for you, just use Ctrl+C)", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// copy a segment of hashing result to clipboard
        /// </summary>
        /// <param name="textBox">the TextBox containing the hashing result</param>
        /// <param name="startIndex">start index</param>
        /// <param name="length">length</param>
        /// <returns></returns>
        private bool CopyHash(TextBox textBox, int startIndex, int length)
        {
            try
            {
                Clipboard.SetDataObject(textBox.Text.Substring(startIndex, length), true);
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void copyHashingResultButtonHandler(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            switch(fe.Name)
            {
                // All
                case "copyResultButton":
                    CopyHashWithNotification(hashingResultBox);
                    break;
                case "copyUpperResultButton":
                    CopyHashWithNotification(hashingResultUpperCasedBox);
                    break;
                case "copyMixedResultButton":
                    CopyHashWithNotification(hashingResultMixedCasedBox);
                    break;
                // Front 16
                case "copyResultF16Button":
                    CopyHashWithNotification(hashingResultBox, 0, 16);
                    break;
                case "copyUpperResultF16Button":
                    CopyHashWithNotification(hashingResultUpperCasedBox, 0, 16);
                    break;
                case "copyMixedResultF16Button":
                    CopyHashWithNotification(hashingResultMixedCasedBox, 0, 16);
                    break;
                // Middle 16
                case "copyResultM16Button":
                    CopyHashWithNotification(hashingResultBox, 8, 16);
                    break;
                case "copyUpperResultM16Button":
                    CopyHashWithNotification(hashingResultUpperCasedBox, 8, 16);
                    break;
                case "copyMixedResultM16Button":
                    CopyHashWithNotification(hashingResultMixedCasedBox, 8, 16);
                    break;
                // Last 16
                case "copyResultL16Button":
                    CopyHashWithNotification(hashingResultBox, 16, 16);
                    break;
                case "copyUpperResultL16Button":
                    CopyHashWithNotification(hashingResultUpperCasedBox, 16, 16);
                    break;
                case "copyMixedResultL16Button":
                    CopyHashWithNotification(hashingResultMixedCasedBox, 16, 16);
                    break;
                default:
                    break;
            }
        }

        private void hideHashButton_Checked(object sender, RoutedEventArgs e)
        {
            // I know this is stupid, haha
            if (!windowInitilized) return;
            this.hashingResultBox.Background = Brushes.Black;
            this.hashingResultUpperCasedBox.Background = Brushes.Black;
            this.hashingResultMixedCasedBox.Background = Brushes.Black;
        }

        private void showHashButton_Checked(object sender, RoutedEventArgs e)
        {
            // yeah, this is also stupid
            if (!windowInitilized) return;
            this.hashingResultBox.Background = Brushes.White;
            this.hashingResultUpperCasedBox.Background = Brushes.White;
            this.hashingResultMixedCasedBox.Background = Brushes.White;
        }

    }
}
