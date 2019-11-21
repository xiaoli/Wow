using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace AntiAFK
{
    public delegate void CodeFoundEventHandler(object sender, EventArgs e);

    /// <summary>
    /// ActivateDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ActivateDialog : Window
    {
        public ActivateDialog()
        {
            InitializeComponent();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            OnCodeFound();
        }

        public string ActivateCode
        {
            get { return codeTextBox.Text; }
        }

        public event CodeFoundEventHandler CodeFound;

        protected virtual void OnCodeFound()
        {
            CodeFoundEventHandler codeFound = this.CodeFound;
            if (codeFound != null) codeFound(this, EventArgs.Empty);
        }

    }
}
