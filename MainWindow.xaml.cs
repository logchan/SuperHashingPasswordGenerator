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
        private List<SHPGSalt> salts = new List<SHPGSalt>();

        public MainWindow()
        {
            InitializeComponent();
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

            HashingPasswordGenerator hpg = new HashingPasswordGenerator(new MD5Hasher(), new DoNothingPostHashing());

            foreach (SHPGSalt salt in salts)
            {
                hpg.AddSaltProcessor(salt.GetSaltAppender());
            }

            hpg.TotalHashingTimes = (UInt32)hashingcount;

            string hash, posthash;
            hpg.DoHashing(secretBox.Password, out hash, out posthash);

            this.hashingResultBox.Text = hash;
            this.hashingResultUpperCasedBox.Text = posthash.ToUpper();

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

    }
}
