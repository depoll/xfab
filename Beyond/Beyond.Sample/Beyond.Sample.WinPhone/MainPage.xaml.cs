using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Xamarin.Forms;


namespace Beyond.Sample.WinPhone {
  public partial class MainPage : PhoneApplicationPage {
    public MainPage() {
      InitializeComponent();

      Forms.Init();
      Content = Beyond.Sample.App.GetMainPage().ConvertPageToUIElement(this);
    }
  }
}
