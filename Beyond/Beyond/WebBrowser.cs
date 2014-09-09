using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Beyond {
  public class WebBrowser : View {
    internal interface IWebBrowserDelegate {
      Task<string> InvokeScriptAsync(string script);
      Task LoadPageAsync(Uri source, string rawHtml);
      bool CanGoBack { get; }
      bool CanGoForward { get; }
      Task GoBackAsync();
      Task GoForwardAsync();
      event Action UpdateProperties;
    }

    private TaskCompletionSource<IWebBrowserDelegate> browserDelegateTcs =
      new TaskCompletionSource<IWebBrowserDelegate>();
    private IWebBrowserDelegate browserDelegate;
    internal IWebBrowserDelegate BrowserDelegate {
      get {
        return browserDelegate;
      }
      set {
        if (browserDelegate != null) {
          browserDelegate.UpdateProperties -= UpdateProperties;
        }
        browserDelegate = value;
        if (browserDelegate != null) {
          browserDelegate.UpdateProperties += UpdateProperties;
          browserDelegateTcs.TrySetResult(browserDelegate);
        } else {
          browserDelegateTcs = new TaskCompletionSource<IWebBrowserDelegate>();
        }
      }
    }

    private Task<IWebBrowserDelegate> GetBrowserDelegateAsync() {
      return browserDelegateTcs.Task;
    }

    public WebBrowser() {
    }

    public Uri Source {
      get {
        return (Uri)this.GetValue(SourceProperty);
      }
      set {
        this.SetValue(SourceProperty, value);
      }
    }

    public bool CanGoBack {
      get {
        return (bool)GetValue(CanGoBackProperty);
      }
      private set {
        SetValue(CanGoBackProperty, value);
      }
    }

    public bool CanGoForward {
      get {
        return (bool)GetValue(CanGoForwardProperty);
      }
      private set {
        SetValue(CanGoForwardProperty, value);
      }
    }

    public async Task GoBackAsync() {
      if (!CanGoBack) {
        throw new InvalidOperationException("Cannot go back.");
      }
      await (await GetBrowserDelegateAsync()).GoBackAsync();
    }

    public async Task GoForwardAsync() {
      if (!CanGoForward) {
        throw new InvalidOperationException("Cannot go forward.");
      }
      await (await GetBrowserDelegateAsync()).GoForwardAsync();
    }

    private async Task LoadPageAsync(Uri uri, string rawHtml) {
      await (await GetBrowserDelegateAsync()).LoadPageAsync(uri, rawHtml);
    }

    public async Task<string> InvokeScriptAsync(string script) {
      return await (await GetBrowserDelegateAsync()).InvokeScriptAsync(script);
    }

    public Task NavigateAsync(Uri uri) {
      return LoadPageAsync(uri, null);
    }

    public Task NavigateToStringAsync(string rawHtml) {
      return LoadPageAsync(new Uri("about:blank"), rawHtml);
    }

    private void UpdateProperties() {
      CanGoForward = BrowserDelegate.CanGoForward;
      CanGoBack = BrowserDelegate.CanGoBack;
    }

    private void OnSourceChanged(Uri oldValue, Uri newValue) {
      var _ = LoadPageAsync(newValue, null);
    }

    public static BindableProperty SourceProperty = BindableProperty.Create<WebBrowser, Uri>(
      wb => wb.Source,
      default(Uri),
      propertyChanged: (bindable, oldValue, newValue) => ((WebBrowser)bindable).OnSourceChanged(oldValue, newValue));
    public static BindableProperty CanGoBackProperty = BindableProperty.Create<WebBrowser, bool>(
      wb => wb.CanGoBack,
      default(bool),
      coerceValue: (bindable, value) => ((WebBrowser)bindable).BrowserDelegate.CanGoBack);
    public static BindableProperty CanGoForwardProperty = BindableProperty.Create<WebBrowser, bool>(
      wb => wb.CanGoForward,
      default(bool),
      coerceValue: (bindable, value) => ((WebBrowser)bindable).BrowserDelegate.CanGoForward);
  }
}
