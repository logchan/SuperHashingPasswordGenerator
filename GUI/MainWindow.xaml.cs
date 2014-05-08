/*
 * MainWindow.xaml.cs
 * 
 * lgchan, 2014
 * 
 * Main window code of super hashing password generator
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

        /***** Event handlers *****/

        private void doHashButton_Click(object sender, RoutedEventArgs e)
        {
            ProceedHashing();
        }

        private void exportSaltsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureUnavailableMessageBox();
        }

        private void importSaltsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureUnavailableMessageBox();
        }

        private void copyHashingResultButtonHandler(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            switch(fe.Name)
            {
                // Copy All
                case "copyResultButton":
                    CopyHashWithSelectionAndNotification(hashingResultBox);
                    break;
                case "copyUpperResultButton":
                    CopyHashWithSelectionAndNotification(hashingResultUpperCasedBox);
                    break;
                case "copyMixedResultButton":
                    CopyHashWithSelectionAndNotification(hashingResultMixedCasedBox);
                    break;
                // Front 16
                case "copyResultF16Button":
                    CopyHashWithSelectionAndNotification(hashingResultBox, 0, 16);
                    break;
                case "copyUpperResultF16Button":
                    CopyHashWithSelectionAndNotification(hashingResultUpperCasedBox, 0, 16);
                    break;
                case "copyMixedResultF16Button":
                    CopyHashWithSelectionAndNotification(hashingResultMixedCasedBox, 0, 16);
                    break;
                // Middle 16
                case "copyResultM16Button":
                    CopyHashWithSelectionAndNotification(hashingResultBox, 8, 16);
                    break;
                case "copyUpperResultM16Button":
                    CopyHashWithSelectionAndNotification(hashingResultUpperCasedBox, 8, 16);
                    break;
                case "copyMixedResultM16Button":
                    CopyHashWithSelectionAndNotification(hashingResultMixedCasedBox, 8, 16);
                    break;
                // Last 16
                case "copyResultL16Button":
                    CopyHashWithSelectionAndNotification(hashingResultBox, 16, 16);
                    break;
                case "copyUpperResultL16Button":
                    CopyHashWithSelectionAndNotification(hashingResultUpperCasedBox, 16, 16);
                    break;
                case "copyMixedResultL16Button":
                    CopyHashWithSelectionAndNotification(hashingResultMixedCasedBox, 16, 16);
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
            this.hashingResultBox.Background = (VisualBrush)this.FindResource("hashingGroupingBackground");
            this.hashingResultUpperCasedBox.Background = (VisualBrush)this.FindResource("hashingGroupingBackground");
            this.hashingResultMixedCasedBox.Background = (VisualBrush)this.FindResource("hashingGroupingBackground");
        }

        /***** Helper Functions *****/

        /// <summary>
        /// Gather information and do the hashing
        /// </summary>
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

        /// <summary>
        /// Get the hashing count input by user
        /// </summary>
        /// <param name="hashingCount">the total number of times of hashing</param>
        /// <returns>true if success, false if failed</returns>
        private bool GetHashingCount(out int hashingCount)
        {
            hashingCount = 0;
            // check which kind of hashing count is selected
            if (hashFixedNumberRadio.IsChecked.HasValue && hashFixedNumberRadio.IsChecked.Value)
            {
                // give a number
                return Int32.TryParse(this.hashingTimesBox.Text, out hashingCount);
            }
            else
            {
                // calculate the length of string, 
                int strlen = this.stringLengthToCalculateBox.Text.Length;
                // and then divide it by some integer
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

        /// <summary>
        /// Wrapper of CopyHash. Add value check, notification (with messagebox), and select the copied text
        /// </summary>
        /// <param name="textBox">the TextBox containing the hashing result</param>
        /// <param name="start">start index</param>
        /// <param name="length">length</param>
        private void CopyHashWithSelectionAndNotification(TextBox textBox, int start = 0, int length = 0)
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
        /// Show a messagebox to tell the user that the feature is not yet available
        /// </summary>
        private void ShowFeatureUnavailableMessageBox()
        {
            MessageBox.Show("Sorry, this feature will be added in the future.\n(It's done when it's done.)", "Feature Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Set the status textblock
        /// </summary>
        /// <param name="text">The text of the status</param>
        /// <param name="color">The color of the text</param>
        private void SetStatus(String text, Color color)
        {
            this.statusTextBlock.Text = text;
            this.statusTextBlock.Foreground = new SolidColorBrush(color);
        }

        /// <summary>
        /// copy a segment of hashing result to clipboard
        /// </summary>
        /// <param name="textBox">the TextBox containing the hashing result</param>
        /// <param name="startIndex">start index</param>
        /// <param name="length">length</param>
        /// <returns>true if succeeded</returns>
        private bool CopyHash(TextBox textBox, int startIndex, int length)
        {
            try
            {
                Clipboard.SetDataObject(textBox.Text.Substring(startIndex, length), true);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
