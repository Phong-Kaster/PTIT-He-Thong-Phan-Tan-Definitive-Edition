using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SharedClass;

namespace ClientSide
{
    public partial class FormClient : DevExpress.XtraEditors.XtraForm
    {
        /*khoi tao socket phia server*/
        private static Socket clientSide = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static string clientRequest = "";



        /***********************************************************************
         * Step 1: vong lap while gui ket noi lien tuc toi  
         * Step 2: ket noi toi server bang IP Loopback address toi port 100
         * Step 3: khi nao ma ket noi duoc thi xoa man hinh di ghi bang "connected"
         ***********************************************************************/
        private static void LoopConnect()
        {
            //int trial = 0;
            while (!clientSide.Connected)
            {
                //trial++;
                try
                {
                    clientSide.Connect(IPAddress.Any, 100);
                    Console.WriteLine("IP : " + IPAddress.Loopback);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("LoopConnect error");
                    Console.WriteLine(ex.Message);
                }
            }
        }



        private static void SendData()
        {
            while (true)
            {

                //Thread.Sleep(500);
                byte[] dataSent = Encoding.ASCII.GetBytes(clientRequest);
                clientSide.Send(dataSent);

                byte[] buffer = new byte[1024];
                int dataReceivedSize = clientSide.Receive(buffer);

                byte[] dataReceived = new byte[dataReceivedSize];
                Array.Copy(buffer, dataReceived, dataReceivedSize);

                String response = Encoding.ASCII.GetString(dataReceived);
                Console.WriteLine("<--------<<Response: " + response);
            }
        }



        public FormClient()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            clientRequest = txtRequest.Text;

            //Thread.Sleep(500);
            byte[] dataSent = Encoding.ASCII.GetBytes(clientRequest);
            clientSide.Send(dataSent);

            byte[] buffer = new byte[1024];
            int dataReceivedSize = clientSide.Receive(buffer);

            byte[] dataReceived = new byte[dataReceivedSize];
            Array.Copy(buffer, dataReceived, dataReceivedSize);

            string response = Encoding.ASCII.GetString(dataReceived);
            //response = response.Replace("|", "\n\n\n\n");
            Console.WriteLine(response);
            //txtResponse.Text = response;


        }

        private void button4_Click(object sender, EventArgs e)
        {
            while (!clientSide.Connected)
            {
                //trial++;
                try
                {
                    clientSide.Connect(IPAddress.Loopback, 100);
                    Console.WriteLine("IP : " + IPAddress.Loopback);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("LoopConnect error");
                    Console.WriteLine(ex.Message);
                }
            }
            txtStatus.Text = "Connected";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtRequest.Text = fbd.SelectedPath;
            }

        }



        private void btnConnect_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            /* while (!clientSide.Connected)
             {
                 //trial++;
                 try
                 {
                     clientSide.Connect(IPAddress.Parse(txtHost.Text), 100);
                     //clientSide.Connect(IPAddress.Loopback, 100);
                     Console.WriteLine("IP : " + IPAddress.Loopback);

                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine("LoopConnect error");
                     Console.WriteLine(ex.Message);
                     MessageBox.Show(ex.ToString(), this.Name);
                 }

             }
             txtStatus.Text = "Connected";*/
            while (!clientSide.Connected)
            {
                try
                {
                    /*Step 2*/
                    clientSide.Connect(IPAddress.Parse(txtHost.Text), 100);
                    Console.WriteLine("IP : " + IPAddress.Loopback);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("LoopConnect error");
                    MessageBox.Show(this,"Lỗi kết nối. Vui lòng xem lại ip","Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    txtStatus.Text = "Disconnected";
                    break;
                }
                txtStatus.Text = "Connected";
            }
        }


        private void btnSend_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //Xoá console
            directoryView.Nodes.Clear();

            try
            {
                //kt người dùng có chọn option dowload ko
                if (!btnDownload.Checked)
                {
                    clientRequest = txtRequest.Text;

                    //GỬI ĐƯỜNG DẪN
                    if (Regex.IsMatch(clientRequest, @"^[a-zA-Z]:\\[\\\S|*\S]?.*$"))
                    {
                        //Cú pháp filter*all*C:\Users
                        String syntax = sendRequestWithFilesTypesOptions() + clientRequest;
                        byte[] dataSent = Encoding.UTF8.GetBytes(syntax);
                        clientSide.Send(dataSent);
                        clientSide.ReceiveTimeout=30000;

                        byte[] buffer = new byte[999999999];
                        int dataReceivedSize = clientSide.Receive(buffer);

                        byte[] dataReceived = new byte[dataReceivedSize];
                        Array.Copy(buffer, dataReceived, dataReceivedSize);

                        //PHAT
                        Dir r = (Dir)ByteArrayToObject(dataReceived);
                        if (isCollectionEmpty(r)) directoryView.Nodes.Add("Not Found");
                        else LoadDirectory(r);
                    }

                    else //GỬI LỆNH KHÔNG PHẢI ĐƯỜNG DẪN
                    {
                        //Thread.Sleep(500);
                        byte[] dataSent = Encoding.ASCII.GetBytes(clientRequest);
                        clientSide.Send(dataSent);

                        byte[] buffer = new byte[99999999];
                        int dataReceivedSize = clientSide.Receive(buffer);

                        byte[] dataReceived = new byte[dataReceivedSize];
                        Array.Copy(buffer, dataReceived, dataReceivedSize);

                        //PHONG
                        string response = Encoding.UTF8.GetString(dataReceived);
                        response = response.Replace("\r\n", "#");
                        string[] result = response.Split('#');
                        Console.WriteLine(response);

                        for (int i = 0; i < result.Length; i++)
                        {
                            directoryView.Nodes.Add(result[i]);
                        }
                        //txtResponse.Text = response;
                    }
                }
                //}
                else //Nếu người dùng chọn Download
                {
                    //txtResponse.Text = "";
                    if (!Directory.Exists(txtPathDest.Text))
                    {
                        directoryView.Nodes.Add("Destination path does not exist");
                    }
                    clientRequest = "*" + txtRequest.Text;// thêm cờ vào rồi gửi lên cho server
                    byte[] dataSent = Encoding.ASCII.GetBytes(clientRequest);
                    clientSide.Send(dataSent);

                    directoryView.Nodes.Add("It Works and looks for files");
                    //Nhận dữ liệu từ server
                    byte[] clientData = new byte[600000000];
                    int receiveByteLen = clientSide.Receive(clientData);

                    //phần tách dữ liệu theo như thứ tự đã được đặt trước ở server
                    directoryView.Nodes.Add("Receiving file ....");
                    // lấy chiều dài của tên file
                    int fNameLen = BitConverter.ToInt32(clientData, 0);

                    //tiếp theo là lấy tên file
                    string fName = Encoding.ASCII.GetString(clientData, 4, fNameLen);

                    //tiến hành tạo tên file trên đường dẫn đích mà người dùng nhập vào
                    BinaryWriter write = new BinaryWriter(File.Open(txtPathDest.Text + "/" + fName, FileMode.Append));

                    //Nạo dữ liệu vào file
                    write.Write(clientData, 4 + fNameLen, receiveByteLen - 4 - fNameLen);

                    directoryView.Nodes.Add("Saving file....");
                    write.Close();
                }
            }
            catch (Exception ex)
            {
                directoryView.Nodes.Add("Not Available");
                //MessageBox.Show(ex.ToString(), this.Name);
            }
        }

        /* ---------------------- Tuấn -----------------------------------
         */
        private string sendRequestWithFilesTypesOptions()
        {
            string requestWithFileTypeFilter = "filtered";
            if (checkBoxAll.Checked)
            {
                requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "all";
            }
            else
            {
                if (textBoxExtensions.Text.Trim() != "")
                {
                    //requestWithFileTypeFilter = "exFiltered";
                    string[] split = textBoxExtensions.Text.Trim().Split(';');
                    //Lọc theo định dạng nhập tay
                    for (int i = 0; i < split.Length; i++)
                    {
                        requestWithFileTypeFilter += "*" + split[i];
                    }
                }

                if (checkBoxFolder.Checked)
                {
                    requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "folder";
                }

                if (checkBoxSound.Checked)
                {
                    requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "sound";
                }
                if (checkBoxVideo.Checked)
                {
                    requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "video";
                }
                if (checkBoxText.Checked)
                {
                    requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "text";
                }
                if (checkBoxImage.Checked)
                {
                    requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "image";
                }
                if (checkBoxCompressed.Checked)
                {
                    requestWithFileTypeFilter = requestWithFileTypeFilter + "*" + "compressed";
                }
            }
            //}

            return requestWithFileTypeFilter + "*";
            /**/
        }
        /**/
        private void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.txtRequest.Text = "";
            directoryView.Nodes.Clear();
        }

        private void btnExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void btnDownload_CheckedChanged(object sender, EventArgs e)
        {
            if (btnDownload.Checked)
            {
                txtPathDest.Visible = true;
            }
            else
            {
                txtPathDest.Visible = false;
            }
        }

        /* ------------------ Tuấn ----------------------*/
        private void checkBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAll.Checked)
            {
                checkBoxFolder.Checked = false;
                checkBoxText.Checked = false;
                checkBoxSound.Checked = false;
                checkBoxVideo.Checked = false;
                checkBoxImage.Checked = false;
                checkBoxCompressed.Checked = false;
            }
        }

        private void checkBoxSound_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSound.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxVideo_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxVideo.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxText_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxText.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxFolder.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxImage_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxImage.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }

        private void checkBoxCompressed_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCompressed.Checked)
            {
                checkBoxAll.Checked = false;
            }
        }


        //PHAT
        public void LoadDirectory(Dir directoryCollection)
        {
            //Trong tree view cấu trúc hiển thị của nó sẽ theo dạng node cha, node con
            //Tạo node cha đầu tiên
            TreeNode tds = directoryView.Nodes.Add(directoryCollection.Name);
            tds.ImageIndex = 0;
            tds.StateImageIndex = 0;
            tds.SelectedImageIndex = 0;
            //Gắn tag vào cho node cha đó, tag này sẽ là đường dẫn đầy đủ của file trên server
            tds.Tag = directoryCollection.Path;

            //Load tất cả các file con bên trong thư mục cha hiện tại
            LoadFiles(directoryCollection, tds);

            //Load tất cả các thư mục con bên trong thư mục cha hiện tại
            LoadSubDirectories(directoryCollection, tds);
        }

        private void LoadSubDirectories(Dir parrentDirectory, TreeNode td)
        {
            // Lặp qua tất cả các đường dẫn bên trong đường dẫn cha
            foreach (Dir subdirectory in parrentDirectory.SubDirectories)
            {
                //Thực hiện các bước như ở trên LoadDirectories
                TreeNode tds = td.Nodes.Add(subdirectory.Name);
                tds.ImageIndex = 0;
                tds.StateImageIndex = 0;
                tds.SelectedImageIndex = 0;
                tds.Tag = subdirectory.Path;
                LoadFiles(subdirectory, tds);
                LoadSubDirectories(subdirectory, tds);
            }
        }

        private void LoadFiles(Dir parrentDirectory, TreeNode td)
        {
            // Lặp qua các file con trong thư mục cha 
            foreach (FileDir file in parrentDirectory.SubFiles)
            {
                //Đưa tên file vào danh sách hiển thị
                TreeNode tds = td.Nodes.Add(file.Name);
                //Gắn tag cho node đó trong tree view là đường dẫn đầy đủ của file
                tds.Tag = file.Path;

                /*tds.StateImageIndex = 1;
                tds.ImageIndex = 1;
                tds.SelectedImageIndex = 1;*/
                assignIconForFile(file.Path, tds);
            }
        }

        //Đổi mảng byte thành object
        private object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = (object)binForm.Deserialize(memStream);
            return obj;
        }

        private bool isCollectionEmpty(Dir directoryCollection)
        {
            //Kiểm tra object nhận được có chứa thông tin không?
            // Nếu không chứa thông tin thì sẽ trả về true và hiện ra là Not Available
            if (directoryCollection.Name == null) return true;
            if (directoryCollection.Path == null) return true;
            return false;
        }

        private void directoryView_MouseMove(object sender, MouseEventArgs e)
        {
            // Lấy node mà con trỏ chuột đang trỏ hiện tại trong treeview  
            TreeNode theNode = this.directoryView.GetNodeAt(e.X, e.Y);

            // Tạo một tooltip nếu con trỏ chuột đang trỏ vào node đó,
            // tooltip này hiển thị ra đường dẫn đầy đủ của file đó
            if (theNode != null && theNode.Tag != null)
            {
                // Đổi tooltip nếu con trỏ chuột đổi trỏ sang node khác
                if (theNode.Tag.ToString() != toolTip.GetToolTip(this.directoryView))
                    toolTip.SetToolTip(this.directoryView, theNode.Tag.ToString());

            }
            else     // Con trỏ chuột đang không trỏ vào bất kì node nào thì xoá tooltip
            {
                toolTip.SetToolTip(this.directoryView, "");
            }
        }

        private static List<string> soundExtensions = new List<string>(new string[] { "mp3", "m4p", "m4a", "flac" });
        private static List<string> videoExtensions = new List<string>(new string[] { "mp4", "mkv", "webm", "flv" });
        private static List<string> textExtensions = new List<string>(new string[] { "txt", "doc", "docx" });
        private static List<string> imageExtensions = new List<string>(new string[] { "jpg", "jpeg", "png", "bmp" });
        private static List<string> compressedExtensions = new List<string>(new string[] { "7z", "rar", "zip" });

        private void assignIconForFile(string path, TreeNode tds)
        {
            string fileType = path.Substring(path.LastIndexOf(".") + 1).ToLower();

            if (textExtensions.Contains(fileType))
            {
                tds.StateImageIndex = 1;
                return;
            }
            else if (soundExtensions.Contains(fileType))
            {
                tds.StateImageIndex = 2;
                return;
            }
            else if (imageExtensions.Contains(fileType))
            {
                tds.StateImageIndex = 3;
                return;
            }
            else if (videoExtensions.Contains(fileType))
            {
                tds.StateImageIndex = 4;
                return;
            }

            else if (compressedExtensions.Contains(fileType))
            {
                tds.StateImageIndex = 5;
                return;
            }
            else
            {
                tds.StateImageIndex = 6;
                return;
            }

        }
    }
}
