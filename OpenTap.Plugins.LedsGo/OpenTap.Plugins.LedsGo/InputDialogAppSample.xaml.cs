using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;
using Keysight.Ccl.Wsl.UI;

namespace InputDialog
{
	public partial class InputDialogSample : WslDialog
	{
		private MessageBoxResult _result;
		public InputDialogSample(string question, string defaultAnswer = "")
		{
			InitializeComponent();
			lblQuestion.Content = question;
			txtAnswer.Text = defaultAnswer;
		}

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
			_result = MessageBoxResult.OK;
			this.Close();
		}

		private void btnDialogCancel_Click(object sender, RoutedEventArgs e)
		{
			_result = MessageBoxResult.Cancel;
			this.Close();
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			txtAnswer.SelectAll();
			txtAnswer.Focus();
		}

		public string Answer
		{
			get { return txtAnswer.Text; }
		}

		public MessageBoxResult DialogResult
		{
			get { return _result; }
		}
	}
}
