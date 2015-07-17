
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.ApplicationSettings;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace _15Win8
{

    public sealed partial class MainPage : Page
    {

        private DataTransferManager dataTransferManager;
        private string imgFormat;




        public MainPage()
        {
            this.InitializeComponent();


            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
            App.Current.UnhandledException += new UnhandledExceptionEventHandler(App_Exception);
            Window.Current.SizeChanged += OnWindowSizeChanged;


        }

        private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {

     
            switch (DetermineState())
            {
                case "Snapped_Detail":
                    btnSelectImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    btnImageFromCamera.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    imgVisible.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    imgLogo.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    mainGrid.Margin = new Thickness(20, 20, 20, 20);
                    break;
                case "FullScreenPortrait_Detail":
                    btnSelectImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    btnImageFromCamera.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    imgVisible.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    imgLogo.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    mainGrid.Margin = new Thickness(50, 50, 50, 50);
                    break;
                default:
                    btnSelectImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    btnImageFromCamera.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    imgVisible.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    imgLogo.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    mainGrid.Margin = new Thickness(50, 50, 50, 50);
                    break;
            }



        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                //Register this page as a share source.
                this.dataTransferManager = DataTransferManager.GetForCurrentView();
                this.dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.DataRequested);
            }
            catch { }
        }

        void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {

            try
            {

                SettingsCommand shareQCommand = new SettingsCommand("shareHTML", "Share", new UICommandInvokedHandler(onShareQCommand));
                args.Request.ApplicationCommands.Add(shareQCommand);

                SettingsCommand showAboutCommand = new SettingsCommand("showAbout", "Help", new UICommandInvokedHandler(onshowAboutCommand));
                args.Request.ApplicationCommands.Add(showAboutCommand);
            }
            catch { }

        }

        private void onShareQCommand(IUICommand cmd)
        {
            DataTransferManager.ShowShareUI();
        }

        private void onshowAboutCommand(IUICommand cmd)
        {
            InfoFlyout iFlyout = new InfoFlyout();
            iFlyout.Show();
        }

         private void App_Exception(object sender, UnhandledExceptionEventArgs e)
        {
             
         //   showToast(e.Message);
        }


        private async void DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {

       
                string htmlContent = @"<html><body><div style='text-align:center;'>";
                htmlContent = htmlContent + @"<a href=""http://www.alexalex.ru/myfreeware.html""><img src='puzzle15.png'></a></div><br />";

                htmlContent = htmlContent + @"<p>I has found funny free app. Try it also: <a href=""http://alexalex.ru/myfreeware.html"">" + "Puzzle 4x4" + @" </a>" + @"</p></body></html>";

                var htmlFormat = Windows.ApplicationModel.DataTransfer.HtmlFormatHelper.CreateHtmlFormat(htmlContent);

                DataPackage requestData = e.Request.Data;
                requestData.Properties.Title = "Something funny fot you";
                requestData.Properties.Description = "You could find funny apps on this site! ;) ";
                requestData.SetText("I has found funny app on this site! ;)");
  
                requestData.SetHtmlFormat(htmlFormat);

                try
                {
                    StorageFile file = null;

                    try
                    {
                        var uri = new System.Uri("ms-appx:///puzzle15.png");
                        file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                    }
                    catch { }

                    if (file != null) requestData.ResourceMap["puzzle15.png"] = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(file);

                }
                catch { }


            }



        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

            try
            {
                // Unregister this page as a share source.
                this.dataTransferManager.DataRequested -= new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.DataRequested);
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
            catch { }
        }

        public void showToast(string elem0, string elem1)
        {

            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(elem0));
            stringElements[1].AppendChild(toastXml.CreateTextNode(elem1));

            ToastNotification toast = new ToastNotification(toastXml);

            toast.Activated += ToastActivated;
            toast.Dismissed += ToastDismissed;
            toast.Failed += ToastFailed;

            ToastNotificationManager.CreateToastNotifier().Show(toast);

        }

        private void ToastFailed(ToastNotification sender, ToastFailedEventArgs args) { }
        private void ToastDismissed(ToastNotification sender, ToastDismissedEventArgs args) { }
        private void ToastActivated(ToastNotification sender, object args) { }



        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            getImageFromFolder();
        }


        async void getImageFromFolder()
        {

            var picker = new FileOpenPicker();

            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.CommitButtonText = "Select image";
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

           

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            if (file.FileType == ".jpg")
            {
                imgFormat = "jpg";
            }
            else
            {
                imgFormat = "png";
            }

            processFile(file);

        }

        string createHTML()
        {

            StringBuilder txthtml = new StringBuilder("<!DOCTYPE HTML PUBLIC " + (char)34 + "-//W3C//DTD HTML 4.01 Transitional//EN" + (char)34);
            txthtml.Append((char)34 + "http://www.w3.org/TR/html4/loose.dtd" + (char)34 + ">");
            txthtml.AppendLine();
            txthtml.Append("<html><head><title>Funny Fifteen</title>");
            txthtml.AppendLine();
            txthtml.Append("<meta name=" + (char)34 + "description" + (char)34 + " content=" + (char)34 + "play in amazing game funny fifteen" + (char)34 + ">");
            txthtml.AppendLine();
            txthtml.Append("<meta name=" + (char)34 + "keywords" + (char)34 + " content=" + (char)34 + "game,fifteen,puzzle,puzle,15" + (char)34 + ">");
            txthtml.AppendLine();
            txthtml.Append("<script type=" + (char)34 + "text/javascript" + (char)34 + "><!--");
            txthtml.AppendLine();
            txthtml.Append("ltr=new Array();");
            txthtml.AppendLine();
            txthtml.Append("tme=0;");
            txthtml.Append("sss=111;xxww=" + (char)34 + (char)34 + ";xw=0;q=0;tt2=150;tt=177;");
            txthtml.AppendLine();
            txthtml.Append("strokaed=" + (char)34 + (char)34 + ";stroka=" + (char)34 + "0123456789qwerty" + (char)34 + ";stroka2=" + (char)34 + "0123456789qwerty" + (char)34 + ";");
            txthtml.AppendLine();
            txthtml.Append("stroka3=" + (char)34 + (char)34 + ";stroka4=" + (char)34 + (char)34 + ";stroka5=" + (char)34 + (char)34 + ";stroka1=" + (char)34 + (char)34 + ";num=0;n=57;lng=0;lng1=0;");
            txthtml.AppendLine();
            txthtml.Append("function whatnumber(n){");
            txthtml.AppendLine();
            txthtml.Append("if (tme==" + (char)34 + "1" + (char)34 + "){");
            txthtml.AppendLine();
            txthtml.Append("if ((ltr[n-1]==" + (char)34 + "y" + (char)34 + ")&&(n!=4)&&(n!=8)&&(n!=12)){");
            txthtml.AppendLine();
            txthtml.Append("tt=" + (char)34 + "a" + (char)34 + "+n;tt2=" + (char)34 + "a" + (char)34 + "+(n-1);nwe=document.getElementById(tt).src;document.getElementById(tt).src=document.getElementById(tt2).src;document.getElementById(tt2).src=nwe;ltr[n-1]=ltr[n];ltr[n]=" + (char)34 + "y" + (char)34 + ";");
            txthtml.AppendLine();
            txthtml.Append("}else{}");
            txthtml.AppendLine();
            txthtml.Append("if ((ltr[n+1]==" + (char)34 + "y" + (char)34 + ")&&(n!=3)&&(n!=7)&&(n!=11)){");
            txthtml.AppendLine();
            txthtml.Append("tt=" + (char)34 + "a" + (char)34 + "+n;tt2=" + (char)34 + "a" + (char)34 + "+(n+1);nwe=document.getElementById(tt).src;document.getElementById(tt).src=document.getElementById(tt2).src;document.getElementById(tt2).src=nwe;ltr[n+1]=ltr[n];ltr[n]=" + (char)34 + "y" + (char)34 + ";");
            txthtml.AppendLine();
            txthtml.Append("}else{}");
            txthtml.AppendLine();
            txthtml.Append("if (ltr[n-4]==" + (char)34 + "y" + (char)34 + "){");
            txthtml.AppendLine();
            txthtml.Append("tt=" + (char)34 + "a" + (char)34 + "+n;tt2=" + (char)34 + "a" + (char)34 + "+(n-4);nwe=document.getElementById(tt).src;document.getElementById(tt).src=document.getElementById(tt2).src;document.getElementById(tt2).src=nwe;ltr[n-4]=ltr[n];ltr[n]=" + (char)34 + "y" + (char)34 + ";");
            txthtml.AppendLine();
            txthtml.Append("}else{}");
            txthtml.AppendLine();
            txthtml.Append("if (ltr[n+4]==" + (char)34 + "y" + (char)34 + "){");
            txthtml.Append("tt=" + (char)34 + "a" + (char)34 + "+n;tt2=" + (char)34 + "a" + (char)34 + "+(n+4);nwe=document.getElementById(tt).src;document.getElementById(tt).src=document.getElementById(tt2).src;document.getElementById(tt2).src=nwe;ltr[n+4]=ltr[n];ltr[n]=" + (char)34 + "y" + (char)34 + ";");
            txthtml.AppendLine();
            txthtml.Append("}else{}");
            txthtml.AppendLine();
            txthtml.Append("strokaed=" + (char)34 + (char)34 + ";q=0;");
            txthtml.AppendLine();
            txthtml.Append("do{strokaed=strokaed+ltr[q];q=q+1;} while (q<16)");
            txthtml.AppendLine();
            txthtml.Append("if (strokaed==" + (char)34 + "0123456789qwerty" + (char)34 + "){tme=0;alert(" + (char)34 + "My congratulations!" + (char)34 + ");}else{}");
            txthtml.AppendLine();
            txthtml.Append("}else{}}");
            txthtml.AppendLine();
            txthtml.Append("function startt(){");
            txthtml.AppendLine();
            txthtml.Append("tme=1;n=30;stroka2=" + (char)34 + "0123456789qwerty" + (char)34 + ";stroka1=" + (char)34 + (char)34 + ";lng=0;lng1=0;");
            txthtml.AppendLine();
            txthtml.Append("nn=15;");
            txthtml.AppendLine();
            txthtml.Append("do{ltr[lng]=stroka2.charAt(lng);lng=lng+1;} while (lng<16)");
            txthtml.AppendLine();
            txthtml.Append("while (lng1<500){");
            txthtml.AppendLine();
            txthtml.Append("num=Math.round(Math.random()*3);");
            txthtml.AppendLine();
            txthtml.Append("if ((num==0)&&(nn!=4)&&(nn!=8)&&(nn!=12)&&(nn>0)){");
            txthtml.AppendLine();
            txthtml.Append("ltr[nn]=ltr[nn-1];ltr[nn-1]=" + (char)34 + "y" + (char)34 + ";nn=nn-1;");
            txthtml.AppendLine();
            txthtml.Append("}else {}");
            txthtml.AppendLine();
            txthtml.Append("if ((num==1)&&(nn!=3)&&(nn!=7)&&(nn!=11)&&(nn<15)){");
            txthtml.Append("ltr[nn]=ltr[nn+1];ltr[nn+1]=" + (char)34 + "y" + (char)34 + ";nn=nn+1;");
            txthtml.Append("}else {}");
            txthtml.AppendLine();
            txthtml.Append("if ((num==2)&&(nn>4)){");
            txthtml.AppendLine();
            txthtml.Append("ltr[nn]=ltr[nn-4];ltr[nn-4]=" + (char)34 + "y" + (char)34 + ";nn=nn-4;");
            txthtml.AppendLine();
            txthtml.Append("}else {}");
            txthtml.AppendLine();
            txthtml.Append("if ((num==3)&&(nn<12)){");
            txthtml.AppendLine();
            txthtml.Append("ltr[nn]=ltr[nn+4];ltr[nn+4]=" + (char)34 + "y" + (char)34 + ";nn=nn+4;");
            txthtml.AppendLine();
            txthtml.Append("}else {}");
            txthtml.AppendLine();
            txthtml.Append("lng1=lng1+1;}");
            txthtml.AppendLine();
            txthtml.Append("xxww=" + (char)34 + (char)34 + ";");
            txthtml.AppendLine();
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "0" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "01."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "1" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "02."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "2" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "03."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "3" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "04."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "4" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "05."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "5" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "06."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "6" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "07."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "7" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "08."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "8" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "09."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "9" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "010."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "q" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "011."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "w" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "012."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "e" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "013."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "r" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "014."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "t" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "015."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");
            txthtml.Append("xw=0;while(xw<=15){if(ltr[xw]==" + (char)34 + "y" + (char)34 + "){xxww=" + (char)34 + "a" + (char)34 + "+xw;document.getElementById(xxww).src=" + (char)34 + "016."+imgFormat + (char)34 + ";}else{}xw=xw+1;}");

            txthtml.Append("}");
            txthtml.AppendLine();
            txthtml.Append("function swmu()");
            txthtml.Append("{document.getElementById('onld').style.visibility=" + (char)34 + "visible" + (char)34 + ";}");
            txthtml.AppendLine();
            txthtml.Append("//--></script>");
            txthtml.AppendLine();
            txthtml.Append("<style type=" + (char)34 + "text/css" + (char)34 + "><!--");
            txthtml.AppendLine();
            txthtml.Append("a{text-decoration:none}");
            txthtml.AppendLine();
            txthtml.Append("BODY{background-color:rgb(0,0,0);}");
            txthtml.AppendLine();
            txthtml.Append("BODY{scrollbar-face-color:#BFDEAE;scrollbar-arrow-color:#8A7777;scrollbar-track-color:#405669;scrollbar-shadow-color:#EEDD77;scrollbar-highlight-color:#FFFFFF;}");
            txthtml.AppendLine();
            txthtml.Append("body{cursor:default}");
            txthtml.Append("table{cursor:default}");
            txthtml.Append("a{cursor:default}");
            txthtml.Append("p{font-size:16pt}");
            txthtml.AppendLine();
            txthtml.Append("#kom{font-size:16pt}");
            txthtml.AppendLine();
            txthtml.Append("#st{font-size:22pt}");
            txthtml.AppendLine();
            txthtml.Append("//--></style></head>");
            txthtml.AppendLine();
            txthtml.Append("<body text=" + (char)34 + "#888800" + (char)34 + " alink=" + (char)34 + "#FFEE77" + (char)34 + " link=" + (char)34 + "#FFFF00" + (char)34 + " vlink=" + (char)34 + "#EEFF00" + (char)34 + " onload=" + (char)34 + "swmu()" + (char)34 + ">");
            txthtml.AppendLine();
            txthtml.Append("<center>");
            txthtml.AppendLine();
            txthtml.Append("<table border=" + (char)34 + "0" + (char)34 + " id=" + (char)34 + "io" + (char)34 + " bgcolor=" + (char)34 + "#FFFFFF" + (char)34 + " cellpadding=" + (char)34 + "0" + (char)34 + " cellspacing=" + (char)34 + "0" + (char)34 + ">");
            txthtml.AppendLine();
            txthtml.Append("<tr>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(0)" + (char)34 + " width=75><img src=" + (char)34 + "01."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a0" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(1)" + (char)34 + " width=75><img src=" + (char)34 + "02."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a1" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(2)" + (char)34 + " width=75><img src=" + (char)34 + "03."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a2" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(3)" + (char)34 + " width=75><img src=" + (char)34 + "04."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a3" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("</tr><tr>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(4)" + (char)34 + " width=75><img src=" + (char)34 + "05."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a4" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(5)" + (char)34 + " width=75><img src=" + (char)34 + "06."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a5" + (char)34 + "></td>");
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(6)" + (char)34 + " width=75><img src=" + (char)34 + "07."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a6" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(7)" + (char)34 + " width=75><img src=" + (char)34 + "08."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a7" + (char)34 + "></td>");
            txthtml.Append("</tr><tr>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(8)" + (char)34 + " width=75><img src=" + (char)34 + "09."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a8" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(9)" + (char)34 + " width=75><img src=" + (char)34 + "010."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a9" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(10)" + (char)34 + " width=75><img src=" + (char)34 + "011."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a10" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(11)" + (char)34 + " width=75><img src=" + (char)34 + "012."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a11" + (char)34 + "></td>");
            txthtml.Append("</tr><tr>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(12)" + (char)34 + " width=75><img src=" + (char)34 + "013."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a12" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(13)" + (char)34 + " width=75><img src=" + (char)34 + "014."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a13" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.AppendLine(); txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(14)" + (char)34 + " width=75><img src=" + (char)34 + "015."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a14" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("<td align=center onclick=" + (char)34 + "whatnumber(15)" + (char)34 + " width=75><img src=" + (char)34 + "016."+imgFormat + (char)34 + " alt=" + (char)34 + (char)34 + " id=" + (char)34 + "a15" + (char)34 + "></td>");
            txthtml.AppendLine();
            txthtml.Append("</tr></table>");

            txthtml.Append("<br>");
            txthtml.AppendLine();
            txthtml.Append("<table id=" + (char)34 + "onld" + (char)34 + " cellpadding=0 cellspacing=0 style=" + (char)34 + "visibility:hidden" + (char)34 + " bgcolor=black><tr><td>");
            txthtml.AppendLine();
            txthtml.Append("<p id="+(char)34+"st"+(char)34+" onclick=" + (char)34 + "startt()" + (char)34 + " title=" + (char)34 + "Старт! Начать с начала!" + (char)34 + ">");
            txthtml.AppendLine();
            txthtml.Append("<font color=" + (char)34 + "#EDF072" + (char)34 + "><b>&nbsp Начать игру &nbsp</b></font></p>");
            txthtml.AppendLine();
            txthtml.Append("</td></tr></table>");
            txthtml.Append("</center>");
            txthtml.AppendLine();
            txthtml.Append("<br>");
            txthtml.AppendLine();
            txthtml.Append("<table border=0 cellpadding=0 cellspacing=0 bgcolor=black><tr><td>");
            txthtml.AppendLine();
            txthtml.Append("<p id=" + (char)34 + "kom" + (char)34 + ">&nbsp Создано программой <a href=" + (char)34 + "http://www.alexalex.ru/" + (char)34 + "> " + (char)34 + "Пазл Пятнашки " + (char)34 + " &nbsp</a>");
            txthtml.Append("</p></td></tr></table>");
            txthtml.AppendLine();
            txthtml.Append("</body></html>");
            txthtml.Append("");

            return txthtml.ToString();

        }

        async void cropImage(IRandomAccessStream inputStream, uint bw, uint bh, uint w, uint h, string fname, StorageFolder folder)
        {

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(inputStream);

            InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
            BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);

            BitmapBounds bounds = new BitmapBounds();
            bounds.Height = h;
            bounds.Width = w;
            bounds.X = bw;
            bounds.Y = bh;
            
            enc.BitmapTransform.Bounds = bounds;

            // write out to the stream
            try
            {
                await enc.FlushAsync();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }

 
            ras.Seek(0);
            // render the stream to the screen
            BitmapImage bImg = new BitmapImage();
            bImg.SetSource(ras);

            imgHidden.Source = bImg; // image element in xaml


            if (folder != null)
            {

                StorageFile file1 = await folder.CreateFileAsync(fname + "."+imgFormat, CreationCollisionOption.ReplaceExisting);

                using (var fileStream1 = await file1.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await RandomAccessStream.CopyAndCloseAsync(ras.GetInputStreamAt(0), fileStream1.GetOutputStreamAt(0));
                }

            }

        }

        private void btnImageFromCamera_Click(object sender, RoutedEventArgs e)
        {
            ImageFromCamera();
        }


        async void ImageFromCamera()
        {
            imgFormat = "jpg";
            var dialog = new CameraCaptureUI();
            dialog.PhotoSettings.AllowCropping = true;

            var file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file != null)
            {
                processFile(file);
            }

        }




        async void processFile(StorageFile file)
        {

            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            BitmapImage image = new BitmapImage();
            image.SetSource(stream);
            
            imgVisible.Source = image;
            imgVisible.Stretch = Stretch.Uniform;

            uint w;
            uint h;
            w = (uint)image.PixelWidth;
            h = (uint)image.PixelHeight;

            if ((w < 100) || (h < 100))
            {
                showToast("Image is too small!", "Please select larger image.");
                return;
            }

            h = h / 4;
            w = w / 4;

            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.CommitButtonText = "Select folder for game";


            var fp = await folderPicker.PickSingleFolderAsync();

            if (fp == null)
            {
                showToast("Oky!", "You should select folder to create a game.");
                return;
            }

            int n;
            n = 1;
            for (int j = 0; j <= 3; j++)
            {
                for (int i = 0; i <= 3; i++)
                {
                    if (n < 16)
                    {
                        cropImage(stream, (uint)(w * i), (uint)(h * j), w, h, "0" + n.ToString(), fp);
                        n++;
                    }
                }
            }

            createBlank(fp, h, w);

            string allhtml;
            allhtml = createHTML();

            StorageFile sampleFile = await fp.CreateFileAsync("puzzle.html", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, allhtml, Windows.Storage.Streams.UnicodeEncoding.Utf8);

            await Windows.System.Launcher.LaunchFileAsync(sampleFile);

        }

        async void createBlank(StorageFolder sf, uint h, uint w)
        {

            var uri = new Uri("ms-appx:///blank."+imgFormat);
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);

            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            BitmapImage image = new BitmapImage();
            image.SetSource(stream);
            imgHidden.Source = image;
            imgHidden.Stretch = Stretch.None;

            cropImage(stream, 0, 0, w, h, "016", sf);

        }

        private void AppBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

   

        private string DetermineState()
        {
            string visualState = "FullScreenLandscape";
            var windowWidth = Window.Current.Bounds.Width;
            var windowHeight = Window.Current.Bounds.Height;

            if (windowWidth <= 500)
            {
                visualState = "Snapped" + "_Detail";
            }
            else if (windowWidth <= 1366)
            {
                if (windowWidth < windowHeight)
                {
                    visualState = "FullScreenPortrait" + "_Detail";
                }
                else
                {
                    visualState = "FilledOrNarrow";
                }
            }

            return visualState;
        }

        private void btnTutorial_Click(object sender, RoutedEventArgs e)
        {
            InfoFlyout iFlyout = new InfoFlyout();
            iFlyout.Show();
        }


    }
}
