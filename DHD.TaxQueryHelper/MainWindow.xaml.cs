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
using System.Drawing;


namespace DHD.TaxQueryHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<PathBookmark>? _pathBookmarks;
        private List<String>? _fpdm;

        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                Config.Load();

                CTRL_PickDate.SelectedDate = DateTime.Now.Date;
                CTRL_MainBrowser.SourceChanged += CTRL_MainBrowser_SourceChanged;
                CTRL_MainBrowser.NavigationStarting += CTRL_MainBrowser_NavigationStarting;
                LoadFPDM();
                LoadBookmark();
                InitializeAsync();

                // 加载版本号
                String? ver = System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString();
                if (!String.IsNullOrEmpty(ver))
                {
                    this.Title = $"{this.Title} - {ver}";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("程序启动异常：" + ex.Message + ex.StackTrace);
            }
        }

        #region 浏览器相关

        private async void InitializeAsync()
        {
            await CTRL_MainBrowser.EnsureCoreWebView2Async();
        }

        private void CTRL_MainBrowser_SourceChanged(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void CoreWebView2_SourceChanged(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {

            //throw new NotImplementedException();
        }

        private void CTRL_MainBrowser_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            String uri = e.Uri;
            CTRL_URL.Text = uri?.ToString();
        }

        private void OnClick_OpenDev(object sender, RoutedEventArgs e)
        {
            if (CTRL_MainBrowser != null && CTRL_MainBrowser.CoreWebView2 != null)
            {
                CTRL_MainBrowser.CoreWebView2.OpenDevToolsWindow();
            }
        }

        private void OnClick_Go(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CTRL_MainBrowser != null && CTRL_MainBrowser.CoreWebView2 != null)
                {
                    CTRL_MainBrowser.CoreWebView2.Navigate(CTRL_URL.Text);
                }
            }
            catch (Exception ex)
            {
                WriteConsole($"【打开地址】发生异常：{ex.Message},{ex.StackTrace}", MessageType.Error);
            }
        }

        #endregion

        #region 查询部分

        private void OnClick_FillInput(object sender, RoutedEventArgs e)
        {
            FillInput();
        }

        private void OnClick_Query(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void Query()
        {
            // 填充校验码
            SetValueByID("yzm", CTRL_VerCode.Text);

            // 执行查询
            CTRL_MainBrowser.ExecuteScriptAsync("$('#checkfp').click();");
        }

        private async void FillInput()
        {
            SetValueByID("fpdm", CTRL_FPDM.Text);
            await CTRL_MainBrowser.ExecuteScriptAsync($"$('#fpdm').blur();");
            SetValueByID("fphm", CTRL_TXT_FPHM.Text);
            await CTRL_MainBrowser.ExecuteScriptAsync($"$('#fphm').blur();");
            SetValueByID("kprq", CTRL_TXT_KPRQ.Text);
            await CTRL_MainBrowser.ExecuteScriptAsync($"$('#kprq').blur();");
            SetValueByID("kjje", CTRL_TXT_KPJE.Text);
            await CTRL_MainBrowser.ExecuteScriptAsync($"$('#kjje').blur();");
        }

        #endregion

        #region Command绑定

        private void CommandBinding_FillInput(object sender, ExecutedRoutedEventArgs e)
        {
            FillInput();
        }

        private void CommandBinding_Query(object sender, ExecutedRoutedEventArgs e)
        {
            Query();
        }
        private void CommandBinding_Caputre(object sender, ExecutedRoutedEventArgs e)
        {
            Capture();
        }

        #endregion

        #region 截图与信息保存

        private void OnClick_CaptureAndSave(object sender, RoutedEventArgs e)
        {
            Capture();
        }

        private void Capture()
        {
            try
            {
                if (String.IsNullOrEmpty(CTRL_TXT_FPHM.Text))
                {
                    MessageBox.Show("发票号码为空！");
                    return;
                }
                if (String.IsNullOrEmpty(CTRL_TXT_KPRQ.Text))
                {
                    MessageBox.Show("开票日期为空！");
                    return;
                }
                String saveFileName = $"{CTRL_TXT_KPRQ.Text}_{CTRL_TXT_FPHM.Text}.jpg";

                String saveFolder = GetSaveFolder();


                WriteConsole(CTRL_SELECT_SavePath.SelectedIndex.ToString(), MessageType.Information);

                if (!System.IO.Directory.Exists(saveFolder))
                {
                    MessageBox.Show("指定的截图保存路径无效！");
                    return;
                }

                if (!saveFolder.EndsWith("\\"))
                {
                    saveFolder = saveFolder + "\\";
                }

                System.Windows.Point p = CTRL_BrowserContainer.PointToScreen(new System.Windows.Point(0d, 0d));
                Int32 sX = (Int32)p.X;
                Int32 sY = (Int32)p.Y;

                Int32 imgWidth = (Int32)CTRL_BrowserContainer.ActualWidth - 50;
                Int32 imgHeight = (Int32)CTRL_BrowserContainer.ActualHeight;


                WriteConsole($"浏览器容器宽：{imgWidth}，高：{imgHeight}", MessageType.Information);


                using (Bitmap bitMap = new Bitmap(imgWidth, imgHeight))
                {
                    using (Graphics g = Graphics.FromImage(bitMap))
                    {
                        g.CopyFromScreen(sX, sY, 0, 0, new System.Drawing.Size(imgWidth, imgHeight));
                        bitMap.Save($"{saveFolder}{saveFileName}");
                    }
                }

                WriteConsole($"截图保存成功：{saveFolder}{saveFileName}", MessageType.Success);

            }
            catch (Exception ex)
            {
                WriteConsole($"截图保存发生异常：{ex.Message}{ex.StackTrace}", MessageType.Error);
            }
        }

        /// <summary>
        /// 保存发票信息文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClick_SaveTaxInfo(object sender, RoutedEventArgs e)
        {
            try
            {

                String savePath = $"{GetSaveFolder()}\\{CTRL_TXT_KPRQ.Text}_{CTRL_TXT_FPHM.Text}.txt";
                savePath = savePath.Replace("\\\\", "\\");

                WriteConsole(savePath, MessageType.Warning);

                // 检测发票查询结果是专票还是普票
                String idSuffix = "pp";
                String t = await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#fpcc_zp').text();");
                if (!String.IsNullOrEmpty(t) && t.Contains("专用"))
                {
                    idSuffix = "zp";
                }

                String scriptTemplate = "$(document.getElementById('dialog-body').contentWindow.document.body).find('#{0}_" + idSuffix + "').text();";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"发票代码：{CTRL_FPDM.Text}");
                sb.AppendLine($"发票号码：{CTRL_TXT_FPHM.Text}");
                sb.AppendLine($"开票日期：{CTRL_TXT_KPRQ.Text}");
                sb.AppendLine($"购买方名称：{await ExScriptWithResult(String.Format(scriptTemplate, "gfmc"))}");
                sb.AppendLine($"购买方税号：{await ExScriptWithResult(String.Format(scriptTemplate, "gfsbh"))}");
                sb.AppendLine($"购买方地址电话：{await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#gfdzdh_" + idSuffix + "').text();")}");
                sb.AppendLine($"购买方开户行以及账号：{await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#gfyhzh_" + idSuffix + "').text();")}");
                sb.AppendLine($"总额小写：{await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#jshjxx_" + idSuffix + "').text();")}");
                sb.AppendLine($"总额大写：{await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#jshjdx_" + idSuffix + "').text();")}");
                sb.AppendLine($"销售方名称：{await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#xfmc_" + idSuffix + "').text();")}");
                sb.AppendLine($"销售方税号：{await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#xfsbh_" + idSuffix + "').text();")}");
                sb.AppendLine($"销售方地址电话：{await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#xfdzdh_" + idSuffix + "').text();")}");
                sb.AppendLine($"销售方开户行以及账号：{await ExScriptWithResult("$(document.getElementById('dialog-body').contentWindow.document.body).find('#xfyhzh_" + idSuffix + "').text();")}");

                System.IO.File.WriteAllText(savePath, sb.ToString());

                WriteConsole(sb.ToString(), MessageType.Warning);
                WriteConsole($"发票信息保存成功！{savePath}");
            }
            catch (Exception ex)
            {
                WriteConsole($"【保存发票信息】发生异常：{ex.Message},{ex.StackTrace}", MessageType.Error);
            }
        }

        #endregion

        #region 界面数据设置

        private void OnChange_SelectDateKPRQ(object sender, SelectionChangedEventArgs e)
        {
            CTRL_TXT_KPRQ.Text = CTRL_PickDate.SelectedDate?.ToString("yyyyMMdd");
        }


        private async void OnClick_RefreshVerCode(object sender, RoutedEventArgs e)
        {
            await CTRL_MainBrowser.ExecuteScriptAsync("$('#yzm_img').click();");
        }

        private void OnClick_OpenInExplorer(object sender, RoutedEventArgs e)
        {
            String saveFolder = GetSaveFolder();
            System.Diagnostics.Process.Start("explorer.exe", saveFolder);
        }

        private String GetSaveFolder()
        {
            String saveFolder = String.Empty;
            if (CTRL_SELECT_SavePath.SelectedIndex == -1) // 用户自行输入
            {
                saveFolder = CTRL_SELECT_SavePath.Text;
            }
            else
            {
                PathBookmark pb = (PathBookmark)CTRL_SELECT_SavePath.SelectedItem;
                saveFolder = pb.Path;
            }
            return saveFolder;
        }

        private void OnClick_SelectSaveFolder(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "选择截图保存目录！";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CTRL_SELECT_SavePath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void LoadFPDM()
        {
            _fpdm = Config.FPDM;
            if (_fpdm != null && _fpdm.Count > 0)
            {
                CTRL_FPDM.ItemsSource = _fpdm;
            }
        }

        private void OnClick_AddFPDMToConfig(object sender, RoutedEventArgs e)
        {
            String fpdm = CTRL_FPDM.Text;
            if (String.IsNullOrEmpty(fpdm))
            {
                WriteConsole("不能添加空的发票代码！", MessageType.Warning);
                return;
            }

            if (CTRL_FPDM.SelectedIndex == -1)
            {
                if (Config.AddFPDM(fpdm))
                {
                    WriteConsole("已添加到常用发票代码中！", MessageType.Success);
                }
                else
                {
                    WriteConsole("发票代码添加失败！", MessageType.Error);
                }
            }
            else
            {
                WriteConsole("此发票代码已存在！", MessageType.Information);
            }
        }

        #region 保存路径书签

        private void LoadBookmark()
        {
            _pathBookmarks = Config.PathBookmarks;
            if (_pathBookmarks == null)
                _pathBookmarks = new List<PathBookmark>();
            _pathBookmarks.Add(new PathBookmark()
            {
                Name = "桌面",
                Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            });

            _pathBookmarks.Add(new PathBookmark()
            {
                Name = "文档",
                Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            });

            CTRL_SELECT_SavePath.ItemsSource = _pathBookmarks;
            CTRL_SELECT_SavePath.SelectedIndex = 0;
        }

        private void OnClick_AddToBookmark(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CTRL_SELECT_SavePath.SelectedIndex == -1)
                {
                    String path = CTRL_SELECT_SavePath.Text;
                    if (System.IO.Directory.Exists(path))
                    {
                        PathBookmark pb = new PathBookmark();
                        pb.Path = path;
                        pb.Name = System.IO.Path.GetFileName(path);
                        Config.AddPathBookmark(pb);
                        LoadBookmark();
                        WriteConsole("添加书签成功！", MessageType.Success);
                    }
                    else
                    {
                        WriteConsole($"路径{path}不存在，无法保存到书签！", MessageType.Error);
                    }
                }
                else
                {
                    WriteConsole("已经存在的不需要添加书签！", MessageType.Information);
                }

            }
            catch (Exception ex)
            {
                WriteConsole($"书签添加异常：{ex.Message}{ ex.StackTrace}", MessageType.Error);
            }
        }

        #endregion

        #endregion

        #region 脚本执行

        /// <summary>
        /// 测试脚本按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClick_RunScript(object sender, RoutedEventArgs e)
        {
            try
            {
                Object o = await CTRL_MainBrowser.CoreWebView2.ExecuteScriptAsync(CTRL_TXT_Script.Text);
                if (o != null)
                {
                    WriteConsole(o.ToString());
                }
                else
                {
                    WriteConsole("执行脚本未获取到任何信息！");
                }
            }
            catch (Exception ex)
            {
                WriteConsole($"【运行脚本】发生异常：{ex.Message},{ex.StackTrace}", MessageType.Error);
            }
        }

        /// <summary>
        /// 根据ID设置值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        private void SetValueByID(String id, String value)
        {
            CTRL_MainBrowser.ExecuteScriptAsync($"$('#{id}').val('{value}')");

        }

        private async Task<String> ExScriptWithResult(String script)
        {
            var o = await CTRL_MainBrowser.CoreWebView2.ExecuteScriptAsync(script);
            if (!String.IsNullOrEmpty(o))
            {
                o = o.Trim('"');
            }
            return o;
        }

        #endregion

        #region 公共方法
        private void WriteConsole(String? msg)
        {
            WriteConsole(msg, MessageType.Normal);
        }

        private void WriteConsole(String? msg, MessageType msgType)
        {
            if (String.IsNullOrEmpty(msg)) return;

            msg = $"[{DateTime.Now.ToString("HH:mm:ss")}]\t{msg}";

            Paragraph p = new Paragraph(new Run(msg));
            p.FontSize = 12;
            p.LineHeight = 3;
            p.Foreground = ConsoleMessageForeColor(msgType);
            CTRL_Console.Document.Blocks.Add(p);

        }

        private System.Windows.Media.Brush ConsoleMessageForeColor(MessageType msgType)
        {
            switch (msgType) {
                case MessageType.Normal:
                    return System.Windows.Media.Brushes.Black;
                case MessageType.Error:
                    return System.Windows.Media.Brushes.Red;
                case MessageType.Warning:
                    return System.Windows.Media.Brushes.Orange;
                case MessageType.Information:
                    return System.Windows.Media.Brushes.Blue;
                case MessageType.Success:
                    return System.Windows.Media.Brushes.Green;
                default:
                    return System.Windows.Media.Brushes.Black;
            }
        }

        private enum MessageType { 
            Normal,
            Information,
            Error,
            Warning,
            Success
        }

        #endregion

    }
}
