using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace yiyifs_wpf_winform_client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Bt_selectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "C# Corner Open File Dialog";
            fdlg.InitialDirectory = @"c:\";   //@是取消转义字符的意思
            fdlg.Filter = "All files（*.*）|*.*";
            /*
             * FilterIndex 属性用于选择了何种文件类型,缺省设置为0,系统取Filter属性设置第一项
             * ,相当于FilterIndex 属性设置为1.如果你编了3个文件类型，当FilterIndex ＝2时是指第2个.
             */
            fdlg.FilterIndex = 2;
            /*
             *如果值为false，那么下一次选择文件的初始目录是上一次你选择的那个目录，
             *不固定；如果值为true，每次打开这个对话框初始目录不随你的选择而改变，是固定的  
             */
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog().HasValue)
            {
                Task.Run(() => {
                    //MessageBox.Show(System.IO.Path.GetFileNameWithoutExtension(fdlg.FileName));
                    var res = MyUploader(fdlg.FileName, "http://192.168.101.4:9007/api/up");
                    this.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(res);
                    });
                   
                });
                
            }
        }

        string MyUploader(string strFileToUpload, string strUrl)
        {
            string strFileFormName = "file";
            Uri oUri = new Uri(strUrl);
            string strBoundary = "----------" + DateTime.Now.Ticks.ToString("x");
            // The trailing boundary string
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");
            // The post message header
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(strBoundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(strFileFormName);
            sb.Append("\"; filename=\"");
            sb.Append(System.IO.Path.GetFileName(strFileToUpload));
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("Transfer-Encoding: ");
            sb.Append("chunked");
            sb.Append("\r\n");
            sb.Append("\r\n");
            string strPostHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);
            // The WebRequest
            HttpWebRequest oWebrequest = (HttpWebRequest)WebRequest.Create(oUri);
            oWebrequest.ContentType = "multipart/form-data; boundary=" + strBoundary;
            oWebrequest.Method = "POST";
            oWebrequest.ProtocolVersion = HttpVersion.Version11;
            // This is important, otherwise the whole file will be read to memory anyway...
            oWebrequest.AllowWriteStreamBuffering = false;
            // Get a FileStream and set the final properties of the WebRequest
            FileStream oFileStream = new FileStream(strFileToUpload, FileMode.Open, FileAccess.Read);
            oWebrequest.SendChunked = true;
            oWebrequest.Timeout = (int)new TimeSpan(24, 0, 0).TotalMilliseconds;
            Stream oRequestStream = oWebrequest.GetRequestStream();
            // Write the post header
            oRequestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            // Stream the file contents in small pieces (4096 bytes, max).
            byte[] buffer = new Byte[4096];
            int bytesRead = 0;
            while ((bytesRead = oFileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                oRequestStream.Write(buffer, 0, bytesRead);
            }
            oFileStream.Close();
            // Add the trailing boundary
            oRequestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            WebResponse oWResponse = oWebrequest.GetResponse();
            Stream s = oWResponse.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            String sReturnString = sr.ReadToEnd();
            // Clean up
            oFileStream.Close();
            oRequestStream.Close();
            s.Close();
            sr.Close();
            return sReturnString;
        }

    }
}
