using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;

[assembly: ExportRenderer(typeof(Beyond.WebBrowser), typeof(Beyond.iOS.WebBrowserRenderer))]
namespace Beyond.iOS {
  public class WebBrowserRenderer : ViewRenderer<WebBrowser, UIWebView> {
    private class RendererDelegate : WebBrowser.IWebBrowserDelegate {
      private WebBrowserRenderer renderer;
      public RendererDelegate(WebBrowserRenderer renderer) {
        this.renderer = renderer;
      }

      public Task<string> InvokeScriptAsync(string script) {
        return Task.FromResult(renderer.WebView.EvaluateJavascript(script));
      }

      private async Task PerformNavigationAsync(Action navigation) {
        var tcs = new TaskCompletionSource<object>();
        EventHandler loadFinished = (sender, args) => {
          tcs.TrySetResult(null);
        };
        EventHandler<UIWebErrorArgs> loadError = (sender, args) => {
          tcs.TrySetException(new NSErrorException(args.Error));
        };
        renderer.WebView.LoadFinished += loadFinished;
        renderer.WebView.LoadError += loadError;
        navigation();
        await tcs.Task;
        renderer.WebView.LoadFinished -= loadFinished;
        renderer.WebView.LoadError -= loadError;
      }

      public Task LoadPageAsync(Uri source, string rawHtml) {
        var url = source != null ? new NSUrl(source.AbsoluteUri) : null;
        return PerformNavigationAsync(() => {
          if (rawHtml == null) {
            renderer.WebView.LoadRequest(new NSUrlRequest(url));
          } else {
            renderer.WebView.LoadHtmlString(rawHtml, url);
          }
        });
      }

      public bool CanGoBack {
        get { return renderer.WebView.CanGoBack; }
      }

      public bool CanGoForward {
        get { return renderer.WebView.CanGoForward; }
      }

      public Task GoBackAsync() {
        return PerformNavigationAsync(renderer.WebView.GoBack);
      }

      public Task GoForwardAsync() {
        return PerformNavigationAsync(renderer.WebView.GoForward);
      }

      public event Action UpdateProperties;

      public void OnUpdateProperties() {
        var up = UpdateProperties;
        if (up != null) {
          up();
        }
      }
    }

    public WebBrowserRenderer() {
    }

    private RendererDelegate rendererDelegate;

    private void UpdateProperties(object sender, object args) {
      rendererDelegate.OnUpdateProperties();
    }

    protected override void OnElementChanged(ElementChangedEventArgs<WebBrowser> e) {
      base.OnElementChanged(e);
      if (e.OldElement != null || Element == null) {
        return;
      }
      if (WebView != null) {
        WebView.LoadError -= UpdateProperties;
        WebView.LoadFinished -= UpdateProperties;
        WebView.LoadStarted -= UpdateProperties;
      }
      WebView = new UIWebView();
      SetNativeControl(WebView);
      rendererDelegate = new RendererDelegate(this);
      WebView.LoadError += UpdateProperties;
      WebView.LoadFinished += UpdateProperties;
      WebView.LoadStarted += UpdateProperties;
      e.NewElement.BrowserDelegate = rendererDelegate;
    }
    protected override void OnElementPropertyChanged(object sender,
        PropertyChangedEventArgs e) {
      base.OnElementPropertyChanged(sender, e);
    }

    private UIWebView WebView { get; set; }
  }
}