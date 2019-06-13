using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.IO;
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
using static Dropbox.Api.TeamLog.AccessMethodLogInfo;
using static Dropbox.Api.UsersCommon.AccountType;

namespace DropboxApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            //var task = Task.Run((Func<Task>)Run);
            //task.Wait();




        }
        public static UploadFile SaveAsFile(MainWindow window)
        {
            Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog();
            fileDialog.Filter = "";
                    UploadFile file = new UploadFile();
            if (fileDialog.ShowDialog() == true)
            {
                using (var stream = new StreamReader(fileDialog.FileName))
                {
                    var content = stream.ReadLine();
                    var name = System.IO.Path.GetFileName(fileDialog.FileName);
                    file.Name = name;
                    file.Content = content;
                }
                    return file;
                
            }
            return file;
        }

        async Task Upload(DropboxClient dbx, string folder, string file, string content)
        {
            if(content != null)
            {
            using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var updated = await dbx.Files.UploadAsync(
                    folder + "/" + file,
                    WriteMode.Overwrite.Instance,
                    body: mem);
            }
            }
        }


        private async void uploadButtonClick(object sender, RoutedEventArgs e)
        {
            using (var dbx = new DropboxClient("KX2MmyobnMAAAAAAAAAAC1wRsoNbasdAQnB0TMiPCYrbzg8eyFys_uBSQMTGA6lx"))
            {
                var file = SaveAsFile(this);
                await Upload(dbx, "/MyFolder",file.Name,file.Content);
            }
        }
        async Task Download(DropboxClient dbx, string folder, string file)
        {
            using (var response = await dbx.Files.DownloadAsync(folder + "/" + file))
            {
                MessageBox.Show(await response.GetContentAsStringAsync());
            }
        }
       

        private Task SendToDatabase()
        {
            string login = loginTextBox.Text;
            string password = passwordTextBox.Text;
            return Task.Run(() =>
            {
                using (var context = new DropboxContext())
                {
                    User user = new User()
                    { Login = login, Password = password };

                    context.Users.Add(user);

                    context.SaveChanges();
                }
            });
        }
        private async void RegistrationButtonClick(object sender, RoutedEventArgs e)
        {
            if (loginTextBox.Text != "" && passwordTextBox.Text != "")
            {

                await SendToDatabase();

            }
            else
            {
                MessageBox.Show("Заполните поля!");
            }
        }


        private Task<bool> CheckUser()
        {
            string login = loginTextBox.Text;
            string password = passwordTextBox.Text;
            return Task.Run(() =>
            {
                using (var context = new DropboxContext())
                {
                    var users = context.Users.ToList();

                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i].Login == login && users[i].Password == password)
                        {
                            return true;
                        }
                    }
                }
                return false;
            });
        }
        private async void AuthorizationButtonClick(object sender, RoutedEventArgs e)
        {
            if (loginTextBox.Text != "" && passwordTextBox.Text != "")
            {
                if (await CheckUser())
                {
                    MessageBox.Show("Welcome!");
                    loginTextBox.Visibility = Visibility.Hidden;
                    passwordTextBox.Visibility = Visibility.Hidden;
                    registrationButton.Visibility = Visibility.Hidden;
                    authorizationButton.Visibility = Visibility.Hidden;
                    loginLabel.Visibility = Visibility.Hidden;
                    passwordLabel.Visibility = Visibility.Hidden;

                    uploadButton.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Not found");
                }
            }
            else
            {
                MessageBox.Show("Заполните поля!");
            }
        }


    }
}
