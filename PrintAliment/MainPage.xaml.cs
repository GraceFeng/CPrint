using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PrintAliment
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        PrintDocument printDocument = null;
        public MainPage()
        {
            this.InitializeComponent();
        }

        //private void OnPrintDocumentPaginate(object sender, PaginateEventArgs e)
        //{
        //    // Construct an instance of the page to print, and tell Windows that there is only 1 page
        //    this.printPage = new PrintPage();

        //    printDocument.SetPreviewPageCount(1, PreviewPageCountType.Final);
        //}

        //private void OnPrintDocumentGetPreviewPage(object sender, GetPreviewPageEventArgs e)
        //{
        //    // Give Windows a reference to the page to print for preview
        //    this.printDocument.SetPreviewPage(e.PageNumber, this.printPage);
        //}
    }
}
