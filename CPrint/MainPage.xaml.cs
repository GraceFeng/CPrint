using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CPrint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PrintManager printmgr = PrintManager.GetForCurrentView();

        private PrintDocument printDoc = null;

        private RotateTransform rottrf = new RotateTransform();

        private PrintTask task = null;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(1800, 700);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            printmgr.PrintTaskRequested += Printmgr_PrintTaskRequested;
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Printmgr_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();

            task = args.Request.CreatePrintTask("Print", OnPrintTaskSourceRequrested);
            task.Options.MediaSize = PrintMediaSize.IsoA4;

            task.Options.Orientation = PrintOrientation.Landscape;
            task.Options.MediaType = PrintMediaType.MultiPartForm;
            task.Completed += PrintTask_Completed;
            deferral.Complete();
        }

        private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
        }

        private async void OnPrintTaskSourceRequrested(PrintTaskSourceRequestedArgs args)
        {
            var def = args.GetDeferral();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
              () =>
              {
                  args.SetSource(printDoc?.DocumentSource);
              });
            def.Complete();
        }

        private async void appbar_Printer_Click(object sender, RoutedEventArgs e)
        {
            if (printDoc != null)
            {
                printDoc.GetPreviewPage -= OnGetPreviewPage;
                printDoc.Paginate -= PrintDic_Paginate;
                printDoc.AddPages -= PrintDic_AddPages;
            }
            this.printDoc = new PrintDocument();

            printDoc.GetPreviewPage += OnGetPreviewPage;

            printDoc.Paginate += PrintDic_Paginate;

            printDoc.AddPages += PrintDic_AddPages;

            bool showPrint = await PrintManager.ShowPrintUIAsync();
        }

        private void PrintDic_AddPages(object sender, AddPagesEventArgs e)
        {
            foreach (var item in MyPrintPages.Items)
            {
                var rect = item as Rectangle;
                printDoc.AddPage(rect);
            }
            printDoc.AddPagesComplete();
        }

        public async Task<IEnumerable<FrameworkElement>> GetWebPages(WebView webView, Windows.Foundation.Size page)
        {
            // ask the content its width
            var widthString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollWidth.toString()" });
            int contentWidth;

            if (!int.TryParse(widthString, out contentWidth))
            {
                throw new Exception(string.Format("failure/width:{0}", widthString));
            }

            webView.Width = contentWidth;

            // ask the content its height
            var heightString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });
            int contentHeight;

            if (!int.TryParse(heightString, out contentHeight))
            {
                throw new Exception(string.Format("failure/height:{0}", heightString));
            }

            webView.Height = contentHeight;

            // how many pages will there be?
            double scale = page.Width / contentWidth;
            double scaledHeight = (contentHeight * scale);
            double pageCount = (double)scaledHeight / page.Height;
            pageCount = pageCount + ((pageCount > (int)pageCount) ? 1 : 0);

            // create the pages
            var pages = new List<Windows.UI.Xaml.Shapes.Rectangle>();

            for (int i = 0; i < (int)pageCount; i++)
            {
                var translateY = -page.Height * i;

                var rectanglePage = new Windows.UI.Xaml.Shapes.Rectangle
                {
                    Height = page.Height,
                    Width = page.Width,
                    Margin = new Thickness(5),
                    Tag = new TranslateTransform { Y = translateY },
                };

                rectanglePage.Loaded += (async (s, e) =>
                {
                    var subRectangle = s as Windows.UI.Xaml.Shapes.Rectangle;
                    var subBrush = await GetWebViewBrush(webView);
                    subBrush.Stretch = Stretch.UniformToFill;
                    subBrush.AlignmentY = AlignmentY.Top;
                    subBrush.Transform = subRectangle.Tag as TranslateTransform;
                    subRectangle.Fill = subBrush;
                });

                pages.Add(rectanglePage);
            }

            return pages;
        }

        public async Task<WebViewBrush> GetWebViewBrush(WebView webView)
        {
            // resize width to content
            double originalWidth = webView.Width;
            var widthString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollWidth.toString()" });
            int contentWidth;

            if (!int.TryParse(widthString, out contentWidth))
            {
                throw new Exception(string.Format("failure/width:{0}", widthString));
            }

            webView.Width = contentWidth;

            // resize height to content
            double originalHeight = webView.Height;
            var heightString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });
            int contentHeight;

            if (!int.TryParse(heightString, out contentHeight))
            {
                throw new Exception(string.Format("failure/height:{0}", heightString));
            }

            webView.Height = contentHeight;

            // create brush
            var originalVisibilty = webView.Visibility;
            webView.Visibility = Windows.UI.Xaml.Visibility.Visible;

            WebViewBrush brush = new WebViewBrush
            {
                SourceName = webView.Name,
                Stretch = Stretch.Uniform
            };

            brush.Redraw();

            // reset, return
            webView.Width = originalWidth;
            webView.Height = originalHeight;
            webView.Visibility = originalVisibilty;

            return brush;
        }

        private void PrintDic_Paginate(object sender, PaginateEventArgs e)
        {
            PrintTaskOptions opt = task.Options;
            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }

        private void OnGetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            // printDoc.SetPreviewPage(e.PageNumber, sp_PrintArea);
            printDoc.SetPreviewPage(e.PageNumber, webView);
        }

        private async void BtnListboxheight_Click(object sender, RoutedEventArgs e)
        {
            //await new Windows.UI.Popups.MessageDialog(ListEmployee.Height.ToString()).ShowAsync();
        }

        private void GetCurrentView_Click(object sender, RoutedEventArgs e)
        {
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //ApplicationView.GetForCurrentView().TryResizeView(new Size { Width = 210, Height = 297 });
            // Txtblock.Text = "Now width=210 height=297";
        }

        private async void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            MyWebViewRectangle.Fill = await GetWebViewBrush(webView);
            MyPrintPages.ItemsSource = await GetWebPages(webView, new Windows.Foundation.Size(842, 595));
        }

        private async void Savetoimage_Clicked(object sender, RoutedEventArgs e)
        {
            var piclib = Windows.Storage.KnownFolders.PicturesLibrary;
            foreach (var item in MyPrintPages.Items)
            {
                var rect = item as Rectangle;
                RenderTargetBitmap renderbmp = new RenderTargetBitmap();
                await renderbmp.RenderAsync(rect);
                var pixels = await renderbmp.GetPixelsAsync();
                var file = await piclib.CreateFileAsync("webview.png", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    byte[] bytes = pixels.ToArray();
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                     BitmapAlphaMode.Ignore,
                                     (uint)rect.Width, (uint)rect.Height,
                                     0, 0, bytes);
                    await encoder.FlushAsync();
                }
            }
        }
    }
}