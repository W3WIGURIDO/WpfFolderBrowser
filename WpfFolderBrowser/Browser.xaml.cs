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
using System.IO;
using Microsoft.Win32;

namespace WpfFolderBrowser
{
    /// <summary>
    /// Browser.xaml の相互作用ロジック
    /// </summary>
    public partial class Browser : Window
    {
        private ViewerProp viewerProp = new ViewerProp();
        private List<string> addressHistory = new List<string>();
        private int addressIndex = 0;
        public string FolderName
        {
            get
            {
                return viewerProp.FolderName;
            }
            set
            {
                viewerProp.FolderName = value;
            }
        }
        public string Address
        {
            get
            {
                return viewerProp.Address;
            }
            set
            {
                viewerProp.Address = value;
            }
        }
        public string FullName
        {
            get
            {
                string addressRoot = System.IO.Path.GetPathRoot(Address);
                if (System.IO.Path.Equals(addressRoot, FolderName))
                {
                    return Address;
                }
                else if (System.IO.Path.Equals(addressRoot, Address))
                {
                    return Address + FolderName;
                }
                else
                {
                    return Address + @"\" + FolderName;
                }
            }
        }
        public bool AccessForbiddenVisibility { get; set; } = false;


        private bool result = false;

        private static BitmapImage folderIcon = new BitmapImage(new Uri(@"Resources/shell32_3_0.png", UriKind.Relative));
        private static BitmapImage desktopIcon = new BitmapImage(new Uri(@"Resources/shell32_34_0.png", UriKind.Relative));
        private static BitmapImage documentsIcon = new BitmapImage(new Uri(@"Resources/imageres_107_0.png", UriKind.Relative));
        private static BitmapImage musicIcon = new BitmapImage(new Uri(@"Resources/imageres_103_0.png", UriKind.Relative));
        private static BitmapImage picturesIcon = new BitmapImage(new Uri(@"Resources/imageres_108_0.png", UriKind.Relative));
        private static BitmapImage videosIcon = new BitmapImage(new Uri(@"Resources/imageres_178_0.png", UriKind.Relative));
        private static BitmapImage downloadIcon = new BitmapImage(new Uri(@"Resources/imageres_175_0.png", UriKind.Relative));
        private static BitmapImage cDriveIcon = new BitmapImage(new Uri(@"Resources/imageres_31_0.png", UriKind.Relative));
        private static BitmapImage driveIcon = new BitmapImage(new Uri(@"Resources/imageres_30_0.png", UriKind.Relative));
        private static BitmapImage pcIcon = new BitmapImage(new Uri(@"Resources/shell32_15_0.png", UriKind.Relative));
        private static BitmapImage quickIcon = new BitmapImage(new Uri(@"Resources/shell32_318_0.png", UriKind.Relative));

        private SolidColorBrush transparent = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        private static double iconSize = 24;
        private Dictionary<Button, string> pathDic = new Dictionary<Button, string>();
        private Dictionary<Button, string> sidePathDic = new Dictionary<Button, string>();
        private Dictionary<Button, List<Button>> sideParentDic = new Dictionary<Button, List<Button>>();

        private static Uri desktopUri = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        private static Uri documentsUri = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        private static Uri musicUri = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        private static Uri picturesUri = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        private static Uri videosUri = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
        private static Uri downloadUri;

        public Browser()
        {
            InitializeComponent();
            this.DataContext = viewerProp;
        }

        private void SetAddress(string path)
        {
            Address = path;
            CountUpIndex();
            addressHistory.Add(path);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public new bool? ShowDialog()
        {
            if (string.IsNullOrEmpty(Address))
            {
                Address = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            addressHistory.Add(Address);

            if (Directory.Exists(Address))
            {
                ViewFolderList(Address);
                CreateSidePanel();
                base.ShowDialog();
                return result;
            }
            else
            {
                return false;
            }
        }

        private StackPanel AddSide(string dir, Button owner, bool ownerIsChild)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = GetFolderName(dir),
                Width = double.NaN
            };

            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            stackPanel.Children.Add(GetFolderImage(dir));
            stackPanel.Children.Add(textBlock);

            Button button = new Button()
            {
                Background = transparent,
                BorderBrush = transparent,
                Content = stackPanel,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            button.Click += SelectSideFolder;
            button.Click += SideParent_Click;
            StackPanel parentPanel = new StackPanel();
            parentPanel.Children.Add(button);

            if (owner != null)
            {
                if (ownerIsChild)
                {
                    parentPanel.Margin = new Thickness(5, 0, 0, 0);
                    sideParentDic.GetOrDefault(owner)?.Add(button);
                    parentPanel.Visibility = Visibility.Hidden;
                    parentPanel.Height = 0;
                    owner.AddOwnerChildren(parentPanel);
                }
                else
                {
                    parentPanel.Margin = new Thickness(owner.Margin.Left + 5, 0, 0, 0);
                    sideParentDic.GetOrDefault(owner)?.Add(button);
                    sidePanel.Children.Add(parentPanel);
                }
            }
            else
            {
                stackPanel.Margin = new Thickness(0);
            }

            sidePathDic.TryAdd(button, dir);
            return parentPanel;
        }

        private Button AddSideParent(string name, bool isChild)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = name,
                Width = double.NaN
            };

            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            BitmapImage bitmapImage;
            if (name.CompareTo("PC") == 0)
            {
                bitmapImage = pcIcon;
            }
            else
            {
                bitmapImage = quickIcon;
            }
            Image image = new Image()
            {
                Source = bitmapImage,
                Width = iconSize,
                Height = iconSize
            };
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);
            Thickness thickness;
            if (isChild)
            {
                thickness = new Thickness(5, 0, 0, 0);
            }
            else
            {
                thickness = new Thickness(0);
            }

            Button button = new Button()
            {
                Background = transparent,
                BorderBrush = transparent,
                Content = stackPanel,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = thickness
            };

            button.Click += SideParent_Click;
            sidePanel.Children.Add(button);
            sideParentDic.TryAdd(button, new List<Button>());

            return button;
        }

        private void SideParent_Click(object sender, RoutedEventArgs e)
        {
            Button senderButton = (Button)sender;
            List<Button> buttons;
            if (sideParentDic.ContainsKey(senderButton))
            {
                buttons = sideParentDic[senderButton];
            }
            else
            {
                sideParentDic.TryAdd(senderButton, new List<Button>());
                string path = sidePathDic.GetOrDefault(senderButton);
                Dictionary<DirectoryInfo, bool> infoList = CreateInfoList(path);
                foreach (KeyValuePair<DirectoryInfo, bool> keyValuePair in infoList)
                {
                    StackPanel stackPanel = AddSide(keyValuePair.Key.FullName, senderButton, true);
                    if (keyValuePair.Value)
                    {
                        stackPanel.Opacity = 0.5;
                    }
                }
                buttons = sideParentDic[senderButton];
            }
            foreach (Button button in buttons)
            {
                if (button.Parent is StackPanel parentPanel)
                {
                    if (parentPanel.Visibility == Visibility.Visible)
                    {
                        parentPanel.Visibility = Visibility.Hidden;
                        parentPanel.Height = 0;
                    }
                    else
                    {
                        parentPanel.Visibility = Visibility.Visible;
                        parentPanel.Height = double.NaN;
                    }
                }
            }
        }

        private void CreateSidePanel()
        {
            List<string> sideList = new List<string>() {
                desktopUri.LocalPath,
                documentsUri.LocalPath,
                picturesUri.LocalPath,
                videosUri.LocalPath,
                musicUri.LocalPath
            };
            List<string> quickList = new List<string>();
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\BannerStore\OptIn");
                string download = Environment.ExpandEnvironmentVariables(registryKey.GetValue("Location1")?.ToString());
                downloadUri = new Uri(download);
                string quick = registryKey.GetValue("Location0")?.ToString();
                registryKey?.Dispose();

                sideList.Insert(0, download);

                Shell32.Shell shell = new Shell32.Shell();
                Shell32.Folder folder = shell.NameSpace("shell:" + quick);
                foreach (Shell32.FolderItem item in folder?.Items())
                {
                    quickList.Add(item.Path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                string[] drives = Environment.GetLogicalDrives();
                foreach (string drive in drives)
                {
                    DriveInfo driveInfo = new DriveInfo(drive);
                    if (driveInfo.IsReady)
                    {
                        sideList.Add(drive);
                    }
                    else
                    {
                        if (AccessForbiddenVisibility)
                        {
                            sideList.Add(drive);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Button pcButton = AddSideParent("PC", false);
            foreach (string sidePath in sideList)
            {
                AddSide(sidePath, pcButton, false);
            }
            sidePanel.Children.Add(new Grid()
            {
                Height = 15,
                IsHitTestVisible = false,
                Focusable = false
            });
            Button quickButton = AddSideParent("クイック アクセス", false);
            foreach (string sidePath in quickList)
            {
                if (Directory.Exists(sidePath))
                {
                    AddSide(sidePath, quickButton, false);
                }
            }
        }

        private Image GetFolderImage(string directoryPath)
        {
            Uri dirUri = new Uri(directoryPath);
            if (dirUri.Equals(desktopUri))
            {
                return new Image()
                {
                    Source = desktopIcon,
                    Width = iconSize,
                    Height = iconSize
                };
            }
            else if (dirUri.Equals(documentsUri))
            {
                return new Image()
                {
                    Source = documentsIcon,
                    Width = iconSize,
                    Height = iconSize
                };
            }
            else if (dirUri.Equals(musicUri))
            {
                return new Image()
                {
                    Source = musicIcon,
                    Width = iconSize,
                    Height = iconSize
                };
            }
            else if (dirUri.Equals(videosUri))
            {
                return new Image()
                {
                    Source = videosIcon,
                    Width = iconSize,
                    Height = iconSize
                };
            }
            else if (dirUri.Equals(picturesUri))
            {
                return new Image()
                {
                    Source = picturesIcon,
                    Width = iconSize,
                    Height = iconSize
                };
            }
            else if (downloadUri?.Equals(dirUri) == true)
            {
                return new Image()
                {
                    Source = downloadIcon,
                    Width = iconSize,
                    Height = iconSize
                };
            }
            else
            {
                if (System.IO.Path.GetPathRoot(directoryPath).CompareTo(directoryPath) == 0)
                {
                    if (directoryPath.CompareTo(@"C:\") == 0)
                    {
                        return new Image()
                        {
                            Source = cDriveIcon,
                            Width = iconSize,
                            Height = iconSize
                        };
                    }
                    else
                    {
                        return new Image()
                        {
                            Source = driveIcon,
                            Width = iconSize,
                            Height = iconSize
                        };
                    }
                }
                else
                {
                    return new Image()
                    {
                        Source = folderIcon,
                        Width = iconSize,
                        Height = iconSize
                    };
                }
            }
        }

        private string GetFolderName(string path)
        {
            Uri dirUri = new Uri(path);
            if (dirUri.Equals(desktopUri))
            {
                return "デスクトップ";
            }
            else if (dirUri.Equals(documentsUri))
            {
                return "ドキュメント";
            }
            else if (dirUri.Equals(musicUri))
            {
                return "ミュージック";
            }
            else if (dirUri.Equals(videosUri))
            {
                return "ビデオ";
            }
            else if (dirUri.Equals(picturesUri))
            {
                return "ピクチャ";
            }
            else if (downloadUri?.Equals(dirUri) == true)
            {
                return "ダウンロード";
            }
            else if (System.IO.Path.GetPathRoot(path).CompareTo(path) == 0)
            {
                DriveInfo driveInfo = new DriveInfo(path);
                if (driveInfo.IsReady)
                {
                    return string.Format("{0}({1})", driveInfo.VolumeLabel, path);
                }
                else
                {
                    return path;
                }
            }
            else
            {
                return System.IO.Path.GetFileName(path);
            }
        }

        private Dictionary<DirectoryInfo, bool> CreateInfoList(string path)
        {
            try
            {
                DirectoryInfo parentPathInfo = new DirectoryInfo(path);
                IEnumerable<DirectoryInfo> tmpInfoList = parentPathInfo.EnumerateDirectories();
                Dictionary<DirectoryInfo, bool> infoList = new Dictionary<DirectoryInfo, bool>();
                tmpInfoList.Select(di =>
                {
                    FileAttributes attributes = di.Attributes;
                    bool accessForbidden = attributes.HasFlag(FileAttributes.Hidden) || attributes.HasFlag(FileAttributes.System);
                    if (accessForbidden && !AccessForbiddenVisibility)
                    {
                        return accessForbidden;
                    }
                    else
                    {
                        infoList.Add(di, accessForbidden);
                        return accessForbidden;
                    }
                }).ToArray();
                return infoList;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "メッセージ", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
                return new Dictionary<DirectoryInfo, bool>();
            }
        }

        private void ViewFolderList(string path)
        {
            try
            {
                pathDic.Clear();
                viewer.Children.Clear();
                foreach (KeyValuePair<DirectoryInfo, bool> keyValuePair in CreateInfoList(path))
                {
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = GetFolderName(keyValuePair.Key.FullName),
                        Width = 200,
                        TextWrapping = TextWrapping.Wrap,
                    };

                    StackPanel stackPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    stackPanel.Children.Add(GetFolderImage(keyValuePair.Key.FullName));
                    stackPanel.Children.Add(textBlock);

                    if (keyValuePair.Value && AccessForbiddenVisibility)
                    {
                        stackPanel.Opacity = 0.5;
                    }

                    Button button = new Button()
                    {
                        Background = transparent,
                        BorderBrush = transparent,
                        Content = stackPanel
                    };
                    button.MouseDoubleClick += DoubleClickFolder;
                    button.Click += SelectFolder;
                    viewer.Children.Add(button);
                    pathDic.TryAdd(button, keyValuePair.Key.FullName);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, ex.Message, "メッセージ", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
            }
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            FolderName = System.IO.Path.GetFileName(pathDic[(Button)sender]);
            if (Keyboard.GetKeyStates(Key.Enter) == KeyStates.Down)
            {
                DoubleClickFolder(sender, null);
            }
        }

        private void SelectSideFolder(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            pathDic.TryAdd(button, sidePathDic[button]);
            DoubleClickFolder(sender, null);
        }

        private void DoubleClickFolder(object sender, MouseButtonEventArgs e)
        {
            string path = pathDic[(Button)sender];
            if (Directory.Exists(path))
            {
                ViewFolderList(path);
                SetAddress(path);
                FolderName = string.Empty;
                viewScrollViewer.ScrollToLeftEnd();
            }
        }

        private void UpFolder(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.GetDirectoryName(Address);
            if (Directory.Exists(path))
            {
                ViewFolderList(path);
                SetAddress(path);
                FolderName = string.Empty;
            }
        }

        private void BackFolder(object sender, RoutedEventArgs e)
        {
            string path = addressHistory[--addressIndex];
            if (Directory.Exists(path))
            {
                ViewFolderList(path);
                Address = path;
                FolderName = string.Empty;
                reButton.IsEnabled = true;
                if (addressIndex <= 0)
                {
                    backButton.IsEnabled = false;
                }
            }
        }

        private void CountUpIndex()
        {
            addressIndex++;
            if (addressHistory.Count > addressIndex)
            {
                addressHistory.RemoveRange(addressIndex, addressHistory.Count - (addressIndex));
            }
            backButton.IsEnabled = true;
            reButton.IsEnabled = false;
        }

        private void ReDoFolder(object sender, RoutedEventArgs e)
        {
            string path = addressHistory[++addressIndex];
            if (Directory.Exists(path))
            {
                ViewFolderList(path);
                Address = path;
                FolderName = string.Empty;
                backButton.IsEnabled = true;
                if (addressHistory.Count <= addressIndex + 1)
                {
                    reButton.IsEnabled = false;
                }
            }
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Directory.Exists(addressBox.Text))
                {
                    if (addressBox.Text.EndsWith(@"\"))
                    {
                        addressBox.Text = addressBox.Text.Substring(0, addressBox.Text.Length - 1);
                    }
                    ViewFolderList(addressBox.Text);
                    SetAddress(addressBox.Text);
                }
                else
                {
                    addressBox.Text = Address;
                }
            }
        }

        private void ClickSelect(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FolderName))
            {
                if (Directory.Exists(FullName))
                {
                    result = true;
                    this.Close();
                }
                else
                {
                    CustomMessageBox.Show(this, "指定したフォルダが存在しません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
                }
            }
            else if (!string.IsNullOrEmpty(Address))
            {
                if (Directory.Exists(Address))
                {
                    if (System.IO.Path.Equals(System.IO.Path.GetPathRoot(Address), Address))
                    {
                        FolderName = Address;
                    }
                    else
                    {
                        string parent = System.IO.Path.GetDirectoryName(Address);
                        string name = System.IO.Path.GetFileName(Address);
                        FolderName = name;
                        Address = parent;
                    }
                    result = true;
                    this.Close();
                }
            }
        }

        private void Button_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.IsEnabled)
                {
                    button.Opacity = 1;
                }
                else
                {
                    button.Opacity = 0.5;
                }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scrollSize = 15;
            if (e.Delta > 0)
            {
                viewScrollViewer.ScrollToHorizontalOffset(viewScrollViewer.ContentHorizontalOffset - scrollSize);
            }
            else
            {
                viewScrollViewer.ScrollToHorizontalOffset(viewScrollViewer.ContentHorizontalOffset + scrollSize);
            }
        }

        private void NewFolder_Click(object sender, RoutedEventArgs e)
        {
            NameInputWindow nameInputWindow = new NameInputWindow() { Title = "フォルダ名を入力", Owner = this };
            if (nameInputWindow.ShowDialog() == true)
            {
                try
                {
                    Directory.CreateDirectory(Address + @"\" + nameInputWindow.NameText);
                    ViewFolderList(Address);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(this, ex.Message, "例外");
                }
            }
        }
    }
}
